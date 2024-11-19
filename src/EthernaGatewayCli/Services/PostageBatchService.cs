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
using Etherna.Sdk.Gateway.GenClients;
using Etherna.Sdk.Users.Gateway.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Services
{
    public class PostageBatchService(
        IChunkService chunkService,
        IFileService fileService,
        IGatewayService gatewayService,
        IIoService ioService)
        : IPostageBatchService
    {
        public async Task<int> CalculatePostageBatchDepthAsync(string[] paths)
        {
            ArgumentNullException.ThrowIfNull(paths, nameof(paths));
            if (paths.Length == 0)
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
        
        public async Task<PostageBatchId> GetUsablePostageBatchAsync(
            int minBatchDepth,
            TimeSpan minBatchTtl,
            bool autoPurchaseNewBatch,
            PostageBatchId? useBatchId,
            string? newBatchLabel)
        {
            if (useBatchId is null)
            {
                //create a new postage batch
                var chainPrice = await gatewayService.GetChainPriceAsync();
                var amount = PostageBatch.CalculateAmount(chainPrice, minBatchTtl);
                var bzzPrice = PostageBatch.CalculatePrice(amount, minBatchDepth);

                ioService.WriteLine($"Required postage batch Depth: {minBatchDepth}, Amount: {amount.ToPlurString()}, BZZ price: {bzzPrice}");

                if (!autoPurchaseNewBatch)
                {
                    var validSelection = false;
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
                var batchId = await gatewayService.CreatePostageBatchAsync(
                    amount,
                    minBatchDepth,
                    newBatchLabel,
                    onWaitingBatchCreation: () => ioService.Write("Waiting for batch created... (it may take a while)"),
                    onBatchCreated: _ => ioService.WriteLine(". Done"),
                    onWaitingBatchUsable: () => ioService.Write("Waiting for batch being usable... (it may take a while)"),
                    onBatchUsable: () => ioService.WriteLine(". Done"));

                ioService.WriteLine($"Created postage batch: {batchId}");

                return batchId;
            }
            else
            {
                //get info about existing postage batch
                PostageBatch postageBatch;
                try
                {
                    postageBatch = await gatewayService.GetPostageBatchInfoAsync(useBatchId.Value);
                }
                catch (EthernaGatewayApiException e) when (e.StatusCode == 404)
                {
                    ioService.WriteErrorLine($"Unable to find postage batch \"{useBatchId}\".");
                    throw;
                }
                
                //verify if it is usable
                if (!postageBatch.IsUsable)
                {
                    ioService.WriteErrorLine($"Postage batch \"{useBatchId}\" is not usable.");
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