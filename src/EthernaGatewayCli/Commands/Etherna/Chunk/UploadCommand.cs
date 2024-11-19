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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna.Chunk
{
    public class UploadCommand(
        Assembly assembly,
        IAuthenticationService authService,
        IGatewayService gatewayService,
        IIoService ioService,
        IPostageBatchService postageBatchService,
        IServiceProvider serviceProvider)
        : CommandBase<UploadCommandOptions>(assembly, ioService, serviceProvider)
    {
        // Consts.
        private ushort ChunkBatchMaxSize = 500;
        private const int UploadMaxRetry = 10;
        private readonly TimeSpan UploadRetryTimeSpan = TimeSpan.FromSeconds(5);

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
            var batchId = await postageBatchService.GetUsablePostageBatchAsync(
                batchDepth,
                Options.NewPostageTtl,
                Options.NewPostageAutoPurchase,
                Options.UsePostageBatchId is null ? (PostageBatchId?)null : new PostageBatchId(Options.UsePostageBatchId),
                Options.NewPostageLabel);
            
            IoService.WriteLine($"Start uploading {chunkFiles.Length} chunks...");
            
            int totalUploaded = 0;
            var uploadStartDateTime = DateTime.UtcNow;
            for (int retry = 0; retry < UploadMaxRetry && totalUploaded < chunkFiles.Length; retry++)
            {
                try
                {
                    var lastUpdateDateTime = DateTime.UtcNow;

                    // Iterate on chunk batches.
                    while(totalUploaded < chunkFiles.Length)
                    {
                        var now = DateTime.UtcNow;
                        if (now - lastUpdateDateTime > TimeSpan.FromSeconds(1))
                        {
                            PrintProgressLine(
                                "Uploading chunks",
                                totalUploaded,
                                chunkFiles.Length,
                                uploadStartDateTime);
                            lastUpdateDateTime = now;
                        }
                        
                        var chunkBatchFiles = chunkFiles.Skip(totalUploaded).Take(ChunkBatchMaxSize).ToArray();
                        
                        List<SwarmChunk> chunkBatch = [];
                        foreach (var chunkFile in chunkBatchFiles)
                            chunkBatch.Add(SwarmChunk.BuildFromSpanAndData(
                                Path.GetFileNameWithoutExtension(chunkFile),
                                await File.ReadAllBytesAsync(chunkFile)));
                        
                        await gatewayService.ChunksBulkUploadAsync(
                            chunkBatch.ToArray(),
                            batchId);
                        retry = 0;

                        totalUploaded += chunkBatchFiles.Length;
                    }
                    IoService.WriteLine();
                }
                catch (Exception e) when (
                    e is HttpRequestException
                        or InvalidOperationException
                        or WebSocketException
                        or OperationCanceledException)
                {
                    IoService.WriteErrorLine($"Error uploading chunks");
                    IoService.WriteLine(e.ToString());

                    if (retry + 1 < UploadMaxRetry)
                    {
                        Console.WriteLine("Retry...");
                        await Task.Delay(UploadRetryTimeSpan);
                    }
                }
            }
            
            IoService.WriteLine($"Uploaded {totalUploaded} chunks successfully.");
        }
        
        private void PrintProgressLine(string message, long uploadedChunks, long totalChunks, DateTime startDateTime)
        {
            // Calculate ETA.
            var elapsedTime = DateTime.UtcNow - startDateTime;
            TimeSpan? eta = null;
            var progressStatus = (double)uploadedChunks / totalChunks;
            if (progressStatus != 0)
            {
                var totalRequiredTime = TimeSpan.FromSeconds(elapsedTime.TotalSeconds / progressStatus);
                eta = totalRequiredTime - elapsedTime;
            }

            // Print update.
            var strBuilder = new StringBuilder();

            strBuilder.Append(CultureInfo.InvariantCulture,
                $"{message} ({(progressStatus * 100):N2}%) {uploadedChunks} chunks of {totalChunks}.");

            if (eta is not null)
                strBuilder.Append(CultureInfo.InvariantCulture, $" ETA: {eta:hh\\:mm\\:ss}");

            strBuilder.Append('\r');

            IoService.Write(strBuilder.ToString());
        }
    }
}