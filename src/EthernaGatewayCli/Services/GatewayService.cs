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

using Etherna.BeeNet.Hashing.Postage;
using Etherna.BeeNet.Models;
using Etherna.BeeNet.Services;
using Etherna.CliHelper.Services;
using Etherna.GatewayCli.Services.Options;
using Etherna.Sdk.Gateway.GenClients;
using Etherna.Sdk.Users.Gateway.Clients;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Services
{
    public class GatewayService(
        IChunkService chunkService,
        IEthernaUserGatewayClient ethernaGatewayClient,
        IFileService fileService,
        IIoService ioService,
        IOptions<GatewayServiceOptions> options)
        : IGatewayService
    {
        // Const.
        private readonly TimeSpan BatchCheckTimeSpan = new(0, 0, 0, 5);
        private readonly TimeSpan BatchCreationTimeout = new(0, 0, 10, 0);
        private readonly TimeSpan BatchUsableTimeout = new(0, 0, 10, 0);
        
        // Fields.
        private readonly GatewayServiceOptions options = options.Value;

        // Methods.
        public async Task<int> CalculatePostageBatchDepthAsync(Stream fileStream, string fileContentType, string fileName) =>
            (await chunkService.EvaluateSingleFileUploadAsync(fileStream, fileContentType, fileName))
            .PostageStampIssuer.Buckets.RequiredPostageBatchDepth;

        public async Task<int> CalculatePostageBatchDepthAsync(byte[] fileData, string fileContentType, string fileName) =>
            (await chunkService.EvaluateSingleFileUploadAsync(fileData, fileContentType, fileName))
            .PostageStampIssuer.Buckets.RequiredPostageBatchDepth;

        [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of \'IEnumerable\' collection")]
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public async Task<int> CalculatePostageBatchDepthAsync(IEnumerable<string> paths)
        {
            ArgumentNullException.ThrowIfNull(paths, nameof(paths));
            if (!paths.Any())
                throw new ArgumentOutOfRangeException(nameof(paths), "Empty file paths");
            
            ioService.Write("Calculating required postage batch depth... ");

            var stampIssuer = new PostageStampIssuer(PostageBatch.MaxDepthInstance);
            UploadEvaluationResult lastResult = null!;
            foreach (var path in paths)
            {
                if (File.Exists(path)) //is a file
                {
                    await using var fileStream = File.OpenRead(path);
                    var mimeType = fileService.GetMimeType(path);
                    var fileName = Path.GetFileName(path);

                    lastResult = await chunkService.EvaluateSingleFileUploadAsync(
                        fileStream,
                        mimeType,
                        fileName,
                        postageStampIssuer: stampIssuer);
                }
                else if (Directory.Exists(path)) //is a directory
                {
                    lastResult = await chunkService.EvaluateDirectoryUploadAsync(
                        path,
                        postageStampIssuer: stampIssuer);
                }
                else //invalid path
                    throw new InvalidOperationException($"Path {path} is not valid");
            }

            ioService.WriteLine("Done");

            return lastResult.PostageStampIssuer.Buckets.RequiredPostageBatchDepth;
        }

        public async Task<PostageBatchId> CreatePostageBatchAsync(BzzBalance amount, int batchDepth, string? label)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            if (batchDepth < PostageBatch.MinDepth)
                throw new ArgumentException($"Postage depth must be at least {PostageBatch.MinDepth}");
            
            // Start creation.
            var bzzPrice = PostageBatch.CalculatePrice(amount, batchDepth);
            ioService.WriteLine($"Creating postage batch...");

            PostageBatchId? batchId = null;
            if (options.UseBeeApi)
            {
                batchId = await ethernaGatewayClient.BeeClient.BuyPostageBatchAsync(amount, batchDepth, label);
            }
            else
            {
                var batchReferenceId = await ethernaGatewayClient.BuyPostageBatchAsync(amount, batchDepth, label);

                // Wait until created batch is available.
                ioService.Write("Waiting for batch created... (it may take a while)");

                var batchStartWait = DateTime.UtcNow;
                do
                {
                    //timeout throw exception
                    if (DateTime.UtcNow - batchStartWait >= BatchCreationTimeout)
                    {
                        var ex = new InvalidOperationException("Batch not avaiable after timeout");
                        ex.Data.Add("BatchReferenceId", batchReferenceId);
                        throw ex;
                    }

                    try
                    {
                        batchId = await ethernaGatewayClient.TryGetNewPostageBatchIdFromPostageRefAsync(batchReferenceId);
                    }
                    catch (EthernaGatewayApiException)
                    {
                        //waiting for batchId available
                        await Task.Delay(BatchCheckTimeSpan);
                    }
                } while (!batchId.HasValue);

                ioService.WriteLine(". Done");
            }

            await WaitForBatchUsableAsync(batchId.Value);

            return batchId.Value;
        }

        public Task<TagInfo> CreateTagAsync(PostageBatchId postageBatchId) =>
            ethernaGatewayClient.BeeClient.CreateTagAsync(postageBatchId);

        public Task FundResourceDownloadAsync(SwarmHash hash) =>
            ethernaGatewayClient.FundResourceDownloadAsync(hash);

        public Task FundResourcePinningAsync(SwarmHash hash) =>
            ethernaGatewayClient.FundResourcePinningAsync(hash);

        public async Task<BzzBalance> GetChainPriceAsync()
        {
            if (options.UseBeeApi)
                return (await ethernaGatewayClient.BeeClient.GetChainStateAsync()).CurrentPrice;
            return (await ethernaGatewayClient.GetChainStateAsync()).CurrentPrice;
        }

        public Task<ChunkUploaderWebSocket> GetChunkUploaderWebSocketAsync(
            PostageBatchId batchId,
            TagId? tagId = null,
            CancellationToken cancellationToken = default) =>
            ethernaGatewayClient.BeeClient.GetChunkUploaderWebSocketAsync(batchId, tagId, cancellationToken);

        public Task<PostageBatch> GetPostageBatchInfoAsync(PostageBatchId batchId)
        {
            if (options.UseBeeApi)
            {
                return ethernaGatewayClient.BeeClient.GetPostageBatchAsync(batchId);
            }
            else
            {
                return ethernaGatewayClient.GetPostageBatchAsync(batchId);
            }
        }

        public async Task<PostageBatchId> GetUsablePostageBatchIdAsync(
            int requiredBatchDepth,
            PostageBatchId? usePostageBatchId,
            TimeSpan newPostageTtl,
            bool newPostageAutoPurchase,
            string? newPostageLabel)
        {
            if (usePostageBatchId is null)
            {
                //create a new postage batch
                var chainPrice = await GetChainPriceAsync();
                var amount = PostageBatch.CalculateAmount(chainPrice, newPostageTtl);
                var bzzPrice = PostageBatch.CalculatePrice(amount, requiredBatchDepth);

                ioService.WriteLine($"Required postage batch Depth: {requiredBatchDepth}, Amount: {amount.ToPlurString()}, BZZ price: {bzzPrice}");

                if (!newPostageAutoPurchase)
                {
                    bool validSelection = false;

                    while (validSelection == false)
                    {
                        ioService.WriteLine($"Confirm the batch purchase? Y to confirm, N to deny [Y|n]");

                        switch (ioService.ReadKey())
                        {
                            case { Key: ConsoleKey.Y }:
                            case { Key: ConsoleKey.Enter }:
                                validSelection = true;
                                break;
                            case { Key: ConsoleKey.N }:
                                throw new InvalidOperationException("Batch purchase denied");
                            default:
                                ioService.WriteLine("Invalid selection");
                                break;
                        }
                    }
                }

                //create batch
                var postageBatchId = await CreatePostageBatchAsync(amount, requiredBatchDepth, newPostageLabel);

                ioService.WriteLine($"Created postage batch: {postageBatchId}");

                return postageBatchId;
            }
            else
            {
                //get info about existing postage batch
                PostageBatch postageBatch;
                try
                {
                    postageBatch = await GetPostageBatchInfoAsync(usePostageBatchId.Value);
                }
                catch (EthernaGatewayApiException e) when (e.StatusCode == 404)
                {
                    ioService.WriteErrorLine($"Unable to find postage batch \"{usePostageBatchId}\".");
                    throw;
                }
                
                //verify if it is usable
                if (!postageBatch.IsUsable)
                {
                    ioService.WriteErrorLine($"Postage batch \"{usePostageBatchId}\" is not usable.");
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

        public Task<SwarmHash> UploadFileAsync(
            PostageBatchId batchId,
            Stream content,
            string? name,
            string? contentType,
            bool pinResource) =>
            ethernaGatewayClient.UploadFileAsync(
                batchId,
                content,
                name: name,
                contentType: contentType,
                swarmDeferredUpload: true,
                swarmPin: pinResource);

        public Task<SwarmHash> UploadDirectoryAsync(
            PostageBatchId batchId,
            string directoryPath,
            bool pinResource) =>
            ethernaGatewayClient.UploadDirectoryAsync(
                batchId,
                directoryPath,
                swarmDeferredUpload: true,
                swarmPin: pinResource);

        // Helpers.
        private async Task WaitForBatchUsableAsync(PostageBatchId batchId)
        {
            // Wait until created batch is usable.
            ioService.Write("Waiting for batch being usable... (it may take a while)");

            var batchStartWait = DateTime.UtcNow;
            bool batchIsUsable;
            do
            {
                //timeout throw exception
                if (DateTime.UtcNow - batchStartWait >= BatchUsableTimeout)
                {
                    var ex = new InvalidOperationException("Batch not usable after timeout");
                    ex.Data.Add("BatchId", batchId);
                    throw ex;
                }

                batchIsUsable = (await GetPostageBatchInfoAsync(batchId)).IsUsable;

                //waiting for batch usable
                if (!batchIsUsable)
                    await Task.Delay(BatchCheckTimeSpan);
            } while (!batchIsUsable);

            ioService.WriteLine(". Done");
        }
    }
}