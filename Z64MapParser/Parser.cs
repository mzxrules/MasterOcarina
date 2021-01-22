using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace mzxrules.SymbolParser
{
    public class Parser
    {
        //static Regex getSymbol = new Regex(" {16}0x([0-9a-f]{16}) {16}(.+)", RegexOptions.Compiled);
        static Regex getSymbol = new Regex(@" {16}0x([0-9a-f]{16}) {16}(\w+)$", RegexOptions.Compiled);

        //lines starting with .. denote file start
        //
        public void Parse(string path)
        {
            Dictionary<string, Dictionary<N64Ptr, string>> file_addr_sym = new Dictionary<string, Dictionary<N64Ptr, string>>();
            Dictionary<string, Dictionary<string, N64Ptr>> file_sym_addr = new Dictionary<string, Dictionary<string, N64Ptr>>();
            
            var lines = File.ReadAllLines(path);
            List<string> testOut = new List<string>();
            bool GetSym = false;
            string file = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("OUTPUT"))
                {
                    break;
                }
                if (line.StartsWith(".."))
                {
                    GetSym = true;
                    file = GetName(line);
                    if (!file_addr_sym.ContainsKey(file))
                    {
                        file_addr_sym.Add(file, new Dictionary<N64Ptr, string>());
                        file_sym_addr.Add(file, new Dictionary<string, N64Ptr>());
                        testOut.Add(file);
                    }
                    continue;
                }
                else if (!GetSym)
                {
                    continue;
                }
                GetSym = true;
                var match = getSymbol.Match(line);
                if (match.Success)
                {
                    N64Ptr ptr = long.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                    string sym = match.Groups[2].Value;
                    if (!file_addr_sym[file].ContainsKey(ptr))
                    {
                        file_addr_sym[file].Add(ptr, sym);
                    }
                    else
                    {
                        testOut.Add($"  {ptr}  {sym}");
                    }
                    if (!file_sym_addr[file].ContainsKey(sym))
                    {
                        file_sym_addr[file].Add(sym, ptr);
                    }
                    else
                    {
                        testOut.Add($"  {ptr}  {sym}");
                    }    
                }
            }

            File.WriteAllLines("test.txt", testOut);
        }

        private string GetName(string line)
        {
            string name = line.Substring(2);
            var index = name.IndexOf(' ');
            if (index > 0)
            {
                name = name.Substring(0, index);
            }
            if(name.EndsWith(".bss"))
            {
                name = name.Substring(0, name.Length - 4);
            }
            return name;
        }
    }
}
