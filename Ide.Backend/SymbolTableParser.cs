using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoIDE.net
{
    public class SymbolTableParser
    {
        private string mElfFile;
        private Dictionary<string, Symbol> mSymbols = new Dictionary<string, Symbol>();

        public Dictionary<string, Symbol> Symbols
        {
            get { return mSymbols; }
        }

        public SymbolTableParser(string elfFile)
        {
            mElfFile = elfFile;
            
            Parse();
        }


        public void Parse()
        {
            foreach (string s in ObjectDumper.GetSymbolTable(mElfFile))
            {
                ProcessSymbolLine(s);
            }
        }

        private void ProcessSymbolLine(string s)
        {
            string[] parts = s.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            Symbol symbol = null;

            if (parts.Length == 5)
            {
                symbol = new Symbol() 
                { 
                    Name = parts[4],
                    MemoryAddress = Convert.ToInt32(parts[0], 16),
                    Size = Convert.ToInt32(parts[3], 16),
                    Type = parts[2],
                    Lgw = parts[1]

                };
            }
            else if (parts.Length == 6)
            {
                symbol = new Symbol()
                {
                    Name = parts[5],
                    MemoryAddress = Convert.ToInt32(parts[0], 16),
                    Size = Convert.ToInt32(parts[4], 16),
                    Type = parts[3],
                    Lgw = parts[1],
                    O = parts[2]
                };
            }

            if (symbol == null) return;
            if (symbol.O == null || symbol.O != "O") return;

            mSymbols.Add(symbol.Name, symbol);
        }
    }

    public class Symbol
    {
        public int MemoryAddress;
        public string Name;
        public string Type;
        public int Size;
        public string Lgw;
        public string O;
        //public object Value;
        public byte[] RawValue;

        public object GetInterpretedValue()
        {
            if (RawValue == null) return "NULL";

            if (Size == 1) return RawValue[0];
            if (Size == 2) return (Int16)(RawValue[0] << 8 | RawValue[1]);

            return "Unknown";
        }
    }
}
