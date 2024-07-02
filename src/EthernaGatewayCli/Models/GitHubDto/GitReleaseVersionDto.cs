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

using System;

namespace Etherna.GatewayCli.Models.GitHubDto
{
    public class GitReleaseVersionDto
    {
        // Properties.
        public string Assets_url { get; set; } = default!;
        public DateTime Created_at { get; set; }
        public bool Draft { get; set; }
        public string Html_url { get; set; } = default!;
        public int Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool Prerelease { get; set; }
        public DateTime Published_at { get; set; }
        public string Tag_name { get; set; } = default!;
        public string Url { get; set; } = default!;
    }
}