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

using Microsoft.AspNetCore.StaticFiles;
using MimeDetective;
using System.Linq;

namespace Etherna.GatewayCli.Services
{
    public class FileService : IFileService
    {
        // Consts.
        const string DefaultContentType = "application/octet-stream";

        // Fields.
        private readonly FileExtensionContentTypeProvider extensionMimeTypeProvider = new();
        private readonly ContentInspector contentMimeTypeProvider = new ContentInspectorBuilder
        {
            Definitions = MimeDetective.Definitions.Default.All()
        }.Build();

        // Methods.
        public string GetMimeType(string filePath)
        {
            var contentType = GetMimeTypeFromExtension(filePath);
            return contentType == DefaultContentType ? GetMimeTypeFromContent(filePath) : contentType;
        }

        public string GetMimeTypeFromContent(string filePath)
        {
            var result = contentMimeTypeProvider.Inspect(filePath);
            var mimeTypes = result.ByMimeType().Select(mtm => mtm.MimeType);
            return mimeTypes.FirstOrDefault() ?? DefaultContentType;
        }

        public string GetMimeTypeFromExtension(string filePath) =>
            extensionMimeTypeProvider.TryGetContentType(filePath, out string? contentType)
                ? contentType
                : DefaultContentType;
    }
}