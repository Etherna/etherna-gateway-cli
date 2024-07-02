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

using Etherna.CliHelper.Services;
using Etherna.GatewayCli.Models.GitHubDto;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Utilities
{
    public static class EthernaVersionControl
    {
        // Fields.
        private static Version? _currentVersion;

        // Properties.
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public static Version CurrentVersion
        {
            get
            {
                if (_currentVersion is null)
                {
                    var assemblyVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ??
                        throw new InvalidOperationException("Invalid assembly version");
                    _currentVersion = new Version(assemblyVersion);
                }
                return _currentVersion;
            }
        }

        // Public methods.
        public static async Task<bool> CheckNewVersionAsync(
            IIoService ioService)
        {
            ArgumentNullException.ThrowIfNull(ioService, nameof(ioService));
            
            // Get current version.
            ioService.WriteLine($"Etherna Gateway CLI (v{CurrentVersion})");
            ioService.WriteLine();

            // Get last version form github releases.
            try
            {
                using HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "EthernaImportClient");
                var gitUrl = "https://api.github.com/repos/Etherna/etherna-gateway-cli/releases";
                var response = await httpClient.GetAsync(gitUrl);
                var gitReleaseVersionsDto = await response.Content.ReadFromJsonAsync<List<GitReleaseVersionDto>>();

                if (gitReleaseVersionsDto is null || gitReleaseVersionsDto.Count == 0)
                    return false;

                var lastVersion = gitReleaseVersionsDto
                    .Select(git => new
                    {
                        Version = new Version(git.Tag_name.Replace("v", "", StringComparison.OrdinalIgnoreCase)),
                        Url = git.Html_url
                    })
                    .OrderByDescending(v => v.Version)
                    .First();

                if (lastVersion.Version > CurrentVersion)
                {
                    ioService.WriteLine(
                        $"""
                         A new release is available: {lastVersion.Version}
                         Upgrade now, or check out the release page at:
                           {lastVersion.Url}
                         """);
                    return true;
                }
                else
                    return false;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                ioService.WriteErrorLine(
                    $"""
                     Unable to check last version on GitHub
                     Error: {ex.Message}
                     """);
                return false;
            }
        }
    }
}