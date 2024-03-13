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