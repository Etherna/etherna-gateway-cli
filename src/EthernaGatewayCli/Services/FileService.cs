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