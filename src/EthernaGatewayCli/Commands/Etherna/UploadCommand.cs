// Copyright 2024-present Etherna SA
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.BeeNet.InputModels;
using Etherna.GatewayCli.Models.Commands;
using Etherna.GatewayCli.Services;
using Etherna.Sdk.GeneratedClients.Gateway;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna
{
    public class UploadCommand : CommandBase<UploadCommandOptions>
    {
        // Consts.
        private const int UploadMaxRetry = 10;
        private readonly TimeSpan UploadRetryTimeSpan = TimeSpan.FromSeconds(5);
        
        // Fields.
        private readonly IAuthenticationService authService;
        private readonly IFileService fileService;
        private readonly IGatewayService gatewayService;

        // Constructor.
        public UploadCommand(
            IAuthenticationService authService,
            IFileService fileService,
            IGatewayService gatewayService,
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(ioService, serviceProvider)
        {
            this.authService = authService;
            this.fileService = fileService;
            this.gatewayService = gatewayService;
        }
        
        // Properties.
        public override string CommandUsageHelpString => "[OPTIONS] SOURCE_FILE [SOURCE_FILE ...]";
        public override string Description => "Upload a file resource to Swarm";

        // Methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length < 1)
                throw new ArgumentException("Upload requires 1 or more arguments");
            var filePaths = commandArgs;
            
            // Search files and calculate total file size.
            var contentByteSize = 0L;
            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                    throw new InvalidOperationException($"File {filePath} doesn't exist");
 
                var fileInfo = new FileInfo(filePath);
                contentByteSize += fileInfo.Length;
            }

            // Authenticate user.
            await authService.SignInAsync();
            
            // Identify postage batch to use.
            var postageBatchId = await GetUsablePostageBatchIdAsync(contentByteSize);

            // Upload file.
            foreach (var filePath in filePaths)
            {
                IoService.WriteLine($"Uploading {filePath}...");
                
                var uploadSucceeded = false;
                string refHash = default!;
                for (int i = 0; i < UploadMaxRetry && !uploadSucceeded; i++)
                {
                    try
                    {
                        var mimeType = fileService.GetMimeType(filePath);
                        var fileParameterInput = new FileParameterInput(
                            File.Open(filePath, FileMode.Open),
                            Path.GetFileName(filePath),
                            mimeType);
                        
                        refHash = await gatewayService.UploadFileAsync(postageBatchId, fileParameterInput, Options.PinResource);
                        IoService.WriteLine($"Ref hash: {refHash}");
                        
                        uploadSucceeded = true;
                    }
#pragma warning disable CA1031
                    catch (Exception e)
                    {
                        IoService.WriteErrorLine($"Error uploading {filePath}");
                        IoService.WriteLine(e.ToString());
                        
                        if (i + 1 < UploadMaxRetry)
                        {
                            Console.WriteLine("Retry...");
                            await Task.Delay(UploadRetryTimeSpan);
                        }
                    }
#pragma warning restore CA1031
                }

                if (!uploadSucceeded)
                    IoService.WriteErrorLine($"Can't upload \"{filePath}\" after {UploadMaxRetry} retries");
                else if (Options.OfferDownload)
                    await gatewayService.OfferResourceAsync(refHash);
            }
        }

        // Helpers.
        private async Task<string> GetUsablePostageBatchIdAsync(long contentByteSize)
        {
            if (Options.UsePostageBatchId is null)
            {
                //create a new postage batch
                var batchDepth = gatewayService.CalculateDepth(contentByteSize);
                var amount = await gatewayService.CalculateAmountAsync(Options.NewPostageTtl);
                var bzzPrice = gatewayService.CalculateBzzPrice(amount, batchDepth);

                IoService.WriteLine($"Required postage batch Depth: {batchDepth}, Amount: {amount}, BZZ price: {bzzPrice}");

                if (!Options.NewPostageAutoPurchase)
                {
                    bool validSelection = false;

                    while (validSelection == false)
                    {
                        IoService.WriteLine($"Confirm the batch purchase? Y to confirm, N to deny [Y|n]");

                        switch (IoService.ReadKey())
                        {
                            case { Key: ConsoleKey.Y }:
                            case { Key: ConsoleKey.Enter }:
                                validSelection = true;
                                break;
                            case { Key: ConsoleKey.N }:
                                throw new InvalidOperationException("Batch purchase denied");
                            default:
                                IoService.WriteLine("Invalid selection");
                                break;
                        }
                    }
                }

                //create batch
                var postageBatchId = await gatewayService.CreatePostageBatchAsync(amount, batchDepth, Options.NewPostageLabel);

                IoService.WriteLine($"Created postage batch: {postageBatchId}");

                return postageBatchId;
            }
            else
            {
                //get info about existing postage batch
                PostageBatchDto postageBatch;
                try
                {
                    postageBatch = await gatewayService.GetPostageBatchInfoAsync(Options.UsePostageBatchId);
                }
                catch (EthernaGatewayApiException e) when (e.StatusCode == 404)
                {
                    IoService.WriteErrorLine($"Unable to find postage batch \"{Options.UsePostageBatchId}\".");
                    throw;
                }
                
                //verify if it is usable
                if (!postageBatch.Usable)
                {
                    IoService.WriteErrorLine($"Postage batch \"{Options.UsePostageBatchId}\" is not usable.");
                    throw new InvalidOperationException();
                }
                
                //verify if it has available space
                if (gatewayService.CalculatePostageBatchByteSize(postageBatch) -
                    gatewayService.CalculateRequiredPostageBatchSpace(contentByteSize) < 0)
                {
                    IoService.WriteErrorLine($"Postage batch \"{Options.UsePostageBatchId}\" has not enough space.");
                    throw new InvalidOperationException();
                }

                return postageBatch.Id;
            }
        }
    }
}