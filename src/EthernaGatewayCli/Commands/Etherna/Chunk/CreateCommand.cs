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

using Etherna.BeeNet.Services;
using Etherna.BeeNet.Stores;
using Etherna.CliHelper.Models;
using Etherna.CliHelper.Models.Commands;
using Etherna.CliHelper.Services;
using Etherna.GatewayCli.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna.Chunk
{
    internal sealed class CreateCommand(
        IChunkService chunkService,
        CommandsRegistry commandsRegistry,
        IFileService fileService,
        IIoService ioService,
        IServiceProvider serviceProvider)
        : CommandBase<CreateCommandOptions>(commandsRegistry, ioService, serviceProvider)
    {
        public override string CommandArgsHelpString => "SOURCE OUTPUT_DIR";
        public override string Description => "Create swarm chunks from a file or directory, and save locally";

        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length != 2)
                throw new ArgumentException("Create chunks require exactly 2 arguments");
            
            var sourcePath = commandArgs[0];
            var outputDirPath = commandArgs[1];
            
            // Create chunks.
            UploadEvaluationResult result;
            if (File.Exists(sourcePath)) //is a file
            {
                var fileName = Path.GetFileName(sourcePath);
                var mimeType = fileService.GetMimeType(sourcePath);
                using var stream = File.OpenRead(sourcePath);
                result = await chunkService.UploadSingleFileAsync(
                    stream,
                    mimeType,
                    fileName,
                    chunkStore: new LocalDirectoryChunkStore(outputDirPath, true));
            }
            else if (Directory.Exists(sourcePath)) //is a directory
            {
                result = await chunkService.UploadDirectoryAsync(
                    sourcePath,
                    indexFilename: Options.IndexFilename,
                    errorFilename: null,
                    chunkStore: new LocalDirectoryChunkStore(outputDirPath, true));
            }
            else
            {
                throw new FileNotFoundException("Source path does not exist");
            }
                
            IoService.WriteLine($"Created {result.PostageStampIssuer.Buckets.TotalChunks} chunks");
            IoService.WriteLine($"Root hash: {result.ChunkReference.Hash}");
        }
    }
}