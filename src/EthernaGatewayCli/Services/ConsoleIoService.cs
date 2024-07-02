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

namespace Etherna.GatewayCli.Services
{
    public class ConsoleIoService : IIoService
    {
        // Consts.
        private const ConsoleColor ErrorForegroundColor = ConsoleColor.DarkRed;
        
        // Methods.
        public ConsoleKeyInfo ReadKey() => Console.ReadKey();

        public string? ReadLine() => Console.ReadLine();

        public void Write(string? value) => Console.Write(value);
        
        public void WriteError(string value)
        {
            Console.ForegroundColor = ErrorForegroundColor;
            Console.Write(value);
            Console.ResetColor();
        }

        public void WriteErrorLine(string value)
        {
            Console.ForegroundColor = ErrorForegroundColor;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public void WriteLine(string? value = null) => Console.WriteLine(value);
    }
}