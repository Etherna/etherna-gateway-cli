// Copyright 2024-present Etherna SA
// This file is part of Etherna Gateway CLI.
// 
// Etherna Gateway CLI is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Gateway CLI is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Gateway CLI.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.BeeNet.Models;
using Etherna.CliHelper.Models.Commands;
using Etherna.CliHelper.Services;
using Etherna.GatewayCli.Services;
using Etherna.Sdk.Gateway.GenClients;
using System;
using System.IO;
using System.Reflection;
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
            Assembly assembly,
            IAuthenticationService authService,
            IFileService fileService,
            IGatewayService gatewayService,
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(assembly, ioService, serviceProvider)
        {
            this.authService = authService;
            this.fileService = fileService;
            this.gatewayService = gatewayService;
        }
        
        // Properties.
        public override string CommandArgsHelpString => "SOURCE [SOURCE ...]";
        public override string Description => "Upload files and directories to Swarm";

        // Methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length < 1)
                throw new ArgumentException("Upload requires 1 or more arguments");
            var paths = commandArgs;

            // Authenticate user.
            await authService.SignInAsync();
            
            // Search files and calculate required postage batch depth.
            var batchDepth = await gatewayService.CalculatePostageBatchDepthAsync(paths);
            
            // Identify postage batch to use.
            var postageBatchId = await GetUsablePostageBatchIdAsync(batchDepth);

            // Upload file.
            foreach (var path in paths)
            {
                IoService.WriteLine($"Uploading {path}...");
                
                var uploadSucceeded = false;
                SwarmHash hash = default!;
                for (int i = 0; i < UploadMaxRetry && !uploadSucceeded; i++)
                {
                    try
                    {
                        if(File.Exists(path)) //is a file
                        {
                            await using var fileStream = File.OpenRead(path);
                            var mimeType = fileService.GetMimeType(path);
                        
                            hash = await gatewayService.UploadFileAsync(
                                postageBatchId,
                                fileStream,
                                Path.GetFileName(path),
                                mimeType,
                                Options.PinResource);
                        }
                        else if (Directory.Exists(path)) //is a directory
                        {
                            hash = await gatewayService.UploadDirectoryAsync(
                                postageBatchId,
                                path,
                                Options.PinResource);
                        }
                        else //invalid path
                            throw new InvalidOperationException($"Path {path} is not valid");
                        
                        IoService.WriteLine($"Hash: {hash}");
                        
                        uploadSucceeded = true;
                    }
#pragma warning disable CA1031
                    catch (Exception e)
                    {
                        IoService.WriteErrorLine($"Error uploading {path}");
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
                    IoService.WriteErrorLine($"Can't upload \"{path}\" after {UploadMaxRetry} retries");
                else if (Options.OfferDownload)
                {
#pragma warning disable CA1031
                    try
                    {
                        await gatewayService.FundResourceDownloadAsync(hash);
                        IoService.WriteLine($"Resource traffic funded");
                    }
                    catch (Exception e)
                    {
                        IoService.WriteErrorLine($"Error funding resource traffic");
                        IoService.WriteLine(e.ToString());
                    }
#pragma warning restore CA1031
                }
            }
        }

        // Helpers.
        private async Task<PostageBatchId> GetUsablePostageBatchIdAsync(int batchDepth)
        {
            if (Options.UsePostageBatchId is null)
            {
                //create a new postage batch
                var chainPrice = await gatewayService.GetChainPriceAsync();
                var amount = PostageBatch.CalculateAmount(chainPrice, Options.NewPostageTtl);
                var bzzPrice = PostageBatch.CalculatePrice(amount, batchDepth);

                IoService.WriteLine($"Required postage batch Depth: {batchDepth}, Amount: {amount.ToPlurString()}, BZZ price: {bzzPrice}");

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
                PostageBatch postageBatch;
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
                if (!postageBatch.IsUsable)
                {
                    IoService.WriteErrorLine($"Postage batch \"{Options.UsePostageBatchId}\" is not usable.");
                    throw new InvalidOperationException();
                }
                
                Console.WriteLine("Attention! Provided postage batch will be used without requirements checks!");
                //See: https://etherna.atlassian.net/browse/ESG-269
                // // verify if it has available space
                // if (gatewayService.CalculatePostageBatchByteSize(postageBatch) -
                //     gatewayService.CalculateRequiredPostageBatchSpace(contentByteSize) < 0)
                // {
                //     IoService.WriteErrorLine($"Postage batch \"{Options.UsePostageBatchId}\" has not enough space.");
                //     throw new InvalidOperationException();
                // }
                
                return postageBatch.Id;
            }
        }
    }
}