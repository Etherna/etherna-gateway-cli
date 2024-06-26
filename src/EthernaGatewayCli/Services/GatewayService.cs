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

using Etherna.BeeNet.Clients.GatewayApi;
using Etherna.GatewayCli.Models.Domain;
using Etherna.Sdk.GeneratedClients.Gateway;
using Etherna.Sdk.Users;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Services
{
    public class GatewayService : IGatewayService
    {
        // Const.
        private readonly TimeSpan BatchCheckTimeSpan = new(0, 0, 0, 5);
        private readonly TimeSpan BatchCreationTimeout = new(0, 0, 10, 0);
        private readonly TimeSpan BatchUsableTimeout = new(0, 0, 10, 0);
        private readonly long BzzDecimalPlacesToUnit = (long)Math.Pow(10, 16);
        private const int ChunkByteSize = 4096;
        private const int MinBatchDepth = 17;
        
        // Fields.
        private readonly IBeeGatewayClient beeGatewayClient;
        private readonly IEthernaUserGatewayClient ethernaGatewayClient;
        private readonly IIoService ioService;

        // Constructor.
        public GatewayService(
            IBeeGatewayClient beeGatewayClient,
            IEthernaUserGatewayClient ethernaGatewayClient,
            IIoService ioService)
        {
            this.beeGatewayClient = beeGatewayClient;
            this.ethernaGatewayClient = ethernaGatewayClient;
            this.ioService = ioService;
        }

        // Methods.
        public async Task<long> CalculateAmountAsync(TimeSpan ttl)
        {
            var currentPrice = await GetCurrentChainPriceAsync();
            return (long)(ttl.TotalSeconds * currentPrice / CommonConsts.GnosisBlockTime.TotalSeconds);
        }

        public BzzBalance CalculateBzzPrice(long amount, int depth) =>
            amount * Math.Pow(2, depth) / BzzDecimalPlacesToUnit;

        public int CalculateDepth(long contentByteSize)
        {
            var batchDepth = 17;
            while (Math.Pow(2, batchDepth) * ChunkByteSize < CalculateRequiredPostageBatchSpace(contentByteSize))
                batchDepth++;
            return batchDepth;
        }

        public long CalculatePostageBatchByteSize(PostageBatchDto postageBatch)
        {
            ArgumentNullException.ThrowIfNull(postageBatch, nameof(postageBatch));
            return (long)Math.Pow(2, postageBatch.Depth) * ChunkByteSize;
        }

        public long CalculateRequiredPostageBatchSpace(long contentByteSize) =>
            (long)(contentByteSize * 1.2); //keep 20% of tolerance

        public async Task<string> CreatePostageBatchAsync(long amount, int batchDepth, string? label)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            if (batchDepth < MinBatchDepth)
                throw new ArgumentException($"Postage depth must be at least {MinBatchDepth}");
            
            // Start creation.
            var bzzPrice = CalculateBzzPrice(amount, batchDepth);
            ioService.WriteLine($"Creating postage batch... Depth: {batchDepth}, Amount: {amount}, BZZ price: {bzzPrice}");
            var batchReferenceId = await ethernaGatewayClient.UsersClient.BatchesPostAsync(batchDepth, amount, label);

            // Wait until created batch is available.
            ioService.Write("Waiting for batch created... (it may take a while)");

            var batchStartWait = DateTime.UtcNow;
            string? batchId = null;
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
                    batchId = await ethernaGatewayClient.SystemClient.PostageBatchRefAsync(batchReferenceId);
                }
                catch (EthernaGatewayApiException)
                {
                    //waiting for batchId available
                    await Task.Delay(BatchCheckTimeSpan);
                }
            } while (string.IsNullOrWhiteSpace(batchId));

            ioService.WriteLine(". Done");

            await WaitForBatchUsableAsync(batchId);

            return batchId;
        }

        public Task FundResourcePinningAsync(string hash) =>
            ethernaGatewayClient.ResourcesClient.PinPostAsync(hash);

        public Task FundResourceTrafficAsync(string hash) =>
            ethernaGatewayClient.ResourcesClient.OffersPostAsync(hash);
        
        public async Task<long> GetCurrentChainPriceAsync() =>
            (await ethernaGatewayClient.SystemClient.ChainstateAsync()).CurrentPrice;

        public Task<PostageBatchDto> GetPostageBatchInfoAsync(string batchId) =>
            ethernaGatewayClient.UsersClient.BatchesGetAsync(batchId);

        public Task<string> UploadFileAsync(
            string postageBatchId,
            Stream content,
            string? name,
            string? contentType,
            bool pinResource) =>
            beeGatewayClient.UploadFileAsync(
                postageBatchId,
                content,
                name: name,
                contentType: contentType,
                swarmDeferredUpload: true,
                swarmPin: pinResource);

        // Helpers.
        private async Task WaitForBatchUsableAsync(string batchId)
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

                batchIsUsable = (await GetPostageBatchInfoAsync(batchId)).Usable;

                //waiting for batch usable
                await Task.Delay(BatchCheckTimeSpan);
            } while (!batchIsUsable);

            ioService.WriteLine(". Done");
        }
    }
}