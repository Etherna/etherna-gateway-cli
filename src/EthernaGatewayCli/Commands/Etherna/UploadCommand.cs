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
using Etherna.Sdk.Users.Gateway.Services;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna
{
    public class UploadCommand(
        Assembly assembly,
        IAuthenticationService authService,
        IFileService fileService,
        IGatewayService gatewayService,
        IIoService ioService,
        IPostageBatchService postageBatchService,
        IServiceProvider serviceProvider)
        : CommandBase<UploadCommandOptions>(assembly, ioService, serviceProvider)
    {
        // Consts.
        private const int UploadMaxRetry = 10;
        private readonly TimeSpan UploadRetryTimeSpan = TimeSpan.FromSeconds(5);

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
            var batchDepth = await postageBatchService.CalculatePostageBatchDepthAsync(paths);
            
            // Identify postage batch to use.
            var postageBatchId = await postageBatchService.GetUsablePostageBatchAsync(
                batchDepth,
                Options.NewPostageTtl,
                Options.NewPostageAutoPurchase,
                Options.UsePostageBatchId is null ? (PostageBatchId?)null : new PostageBatchId(Options.UsePostageBatchId),
                Options.NewPostageLabel);

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
    }
}