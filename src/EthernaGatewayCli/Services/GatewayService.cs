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

using Etherna.Sdk.GeneratedClients.Gateway;
using Etherna.Sdk.Users;
using System;
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
        private readonly IEthernaUserGatewayClient ethernaGatewayClient;
        
        // Constructor.
        public GatewayService(
            IEthernaUserGatewayClient ethernaGatewayClient)
        {
            this.ethernaGatewayClient = ethernaGatewayClient;
        }

        // Methods.
        public async Task<long> CalculateAmountAsync(TimeSpan ttl)
        {
            var currentPrice = await GetCurrentChainPriceAsync();
            return (long)(ttl.TotalSeconds * currentPrice / CommonConsts.GnosisBlockTime.TotalSeconds);
        }

        public async Task<string> CreatePostageBatchAsync(long amount, int batchDepth, string? label)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            if (batchDepth < MinBatchDepth)
                throw new ArgumentException($"Postage depth must be at least {MinBatchDepth}");
            
            // Start creation.
            var bzzPrice = amount * Math.Pow(2, batchDepth) / BzzDecimalPlacesToUnit;
            Console.WriteLine($"Creating postage batch... Depth: {batchDepth}, Amount: {amount}, BZZ price: {bzzPrice}");
            var batchReferenceId = await ethernaGatewayClient.UsersClient.BatchesPostAsync(batchDepth, amount, label);

            // Wait until created batch is available.
            Console.Write("Waiting for batch created... (it may take a while)");

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

            Console.WriteLine(". Done");

            await WaitForBatchUsableAsync(batchId);

            return batchId;
        }
        
        public async Task<string> CreatePostageBatchFromContentAsync(long contentByteSize, TimeSpan ttlPostageStamp, string? label, bool autoPurchase)
        {
            //calculate batch depth
            var batchDepth = 17;
            while (Math.Pow(2, batchDepth) * ChunkByteSize < contentByteSize * 1.2) //keep 20% of tolerance
                batchDepth++;

            //calculate amount
            var amount = await CalculateAmountAsync(ttlPostageStamp);
            var bzzPrice = amount * Math.Pow(2, batchDepth) / BzzDecimalPlacesToUnit;

            Console.WriteLine($"Required postage batch Depth: {batchDepth}, Amount: {amount}, BZZ price: {bzzPrice}");

            if (!autoPurchase)
            {
                bool validSelection = false;

                while (validSelection == false)
                {
                    Console.WriteLine($"Confirm the batch purchase? Y to confirm, N to deny [Y|n]");

                    switch (Console.ReadKey())
                    {
                        case { Key: ConsoleKey.Y }:
                        case { Key: ConsoleKey.Enter }:
                            validSelection = true;
                            break;
                        case { Key: ConsoleKey.N }:
                            throw new InvalidOperationException("Batch purchase denied");
                        default:
                            Console.WriteLine("Invalid selection");
                            break;
                    }
                }
            }

            //create batch
            var batchId = await CreatePostageBatchAsync(amount, batchDepth, label);

            Console.WriteLine($"Created postage batch: {batchId}");

            return batchId;
        }
        
        public async Task<long> GetCurrentChainPriceAsync() =>
            (await ethernaGatewayClient.SystemClient.ChainstateAsync()).CurrentPrice;

        public Task<PostageBatchDto> GetPostageBatchInfoAsync(string batchId) =>
            ethernaGatewayClient.UsersClient.BatchesGetAsync(batchId);
        
        // Helpers.
        private async Task WaitForBatchUsableAsync(string batchId)
        {
            // Wait until created batch is usable.
            Console.Write("Waiting for batch being usable... (it may take a while)");

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

            Console.WriteLine(". Done");
        }
    }
}