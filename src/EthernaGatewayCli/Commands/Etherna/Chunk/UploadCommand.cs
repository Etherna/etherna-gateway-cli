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
using System;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna.Chunk
{
    public class UploadCommand : CommandBase<UploadCommandOptions>
    {
        // Consts.
        private const int UploadMaxRetry = 10;
        private readonly TimeSpan UploadRetryTimeSpan = TimeSpan.FromSeconds(5);
        
        // Fields.
        private readonly IAuthenticationService authService;
        private readonly IGatewayService gatewayService;

        // Constructor.
        public UploadCommand(
            Assembly assembly,
            IAuthenticationService authService,
            IGatewayService gatewayService,
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(assembly, ioService, serviceProvider)
        {
            this.authService = authService;
            this.gatewayService = gatewayService;
        }
        
        // Properties.
        public override string CommandArgsHelpString => "CHUNK_DIR";
        public override string Description => "Upload chunks from a directory";

        // Methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length != 1)
                throw new ArgumentException("Upload requires exactly 1 argument");
            var dirPath = commandArgs[0];

            // Authenticate user.
            await authService.SignInAsync();
            
            // Search chunks and calculate required postage batch depth.
            var chunkFiles = Directory.GetFiles(dirPath, "*.chunk", SearchOption.TopDirectoryOnly);
            using var postageBuckets = new PostageBuckets();
            foreach (var chunkPath in chunkFiles)
            {
                var strHash = Path.GetFileNameWithoutExtension(chunkPath);
                var bucketId = new SwarmHash(strHash).ToBucketId();
                postageBuckets.IncrementCollisions(bucketId);
            }
            var batchDepth = postageBuckets.RequiredPostageBatchDepth;
            
            // Identify postage batch and tag to use.
            var postageBatchId = await gatewayService.GetUsablePostageBatchIdAsync(
                batchDepth,
                Options.UsePostageBatchId is null ? (PostageBatchId?)null : new PostageBatchId(Options.UsePostageBatchId),
                Options.NewPostageTtl,
                Options.NewPostageAutoPurchase,
                Options.NewPostageLabel);
            var tagInfo = await gatewayService.CreateTagAsync(postageBatchId); //necessary to not bypass bee local storage

            // Upload with websocket.
            int totalUploaded = 0;
            for (int i = 0; i < UploadMaxRetry && totalUploaded < chunkFiles.Length; i++)
            {
                // Create websocket.
                using var chunkUploaderWs = await gatewayService.GetChunkUploaderWebSocketAsync(postageBatchId, tagInfo.Id);

                try
                {
                    for (int j = totalUploaded; j < chunkFiles.Length; j++)
                    {
                        var chunkPath = chunkFiles[j];
                        var chunk = SwarmChunk.BuildFromSpanAndData(
                            Path.GetFileNameWithoutExtension(chunkPath),
                            await File.ReadAllBytesAsync(chunkPath));
                        
                        await chunkUploaderWs.SendChunkAsync(chunk, CancellationToken.None);

                        totalUploaded++;
                    }
                }
                catch (Exception e) when (e is WebSocketException or OperationCanceledException)
                {
                    IoService.WriteErrorLine($"Error uploading chunks");
                    IoService.WriteLine(e.ToString());

                    if (i + 1 < UploadMaxRetry)
                    {
                        Console.WriteLine("Retry...");
                        await Task.Delay(UploadRetryTimeSpan);
                    }
                }
                finally
                {
                    await chunkUploaderWs.CloseAsync();
                }
            }
            
            IoService.WriteLine($"Uploaded {totalUploaded} chunks successfully.");
        }
    }
}