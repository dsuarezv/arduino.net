using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;


namespace arduino.net
{
    public class WatchManager
    {
        private IDebugger mDebugger;


        public WatchManager(IDebugger debugger)
        {
            mDebugger = debugger;
        }


        public IList<SymbolInfo> GetValues(IList<string> symbolNames)
        {
            if (mDebugger.Status != DebuggerStatus.Break) return null;

            var currentFunction = GetCurrentFunction();
            if (currentFunction == null) return null;
            
            List<SymbolInfo> result = new List<SymbolInfo>();
            if (symbolNames == null) return result;

            foreach (var name in symbolNames)
            {
                var s = GetSymbol(currentFunction, name);
                if (s != null) result.Add(s);
            }

            return result;
        }

        public SymbolInfo GetInmmediateValue(string name)
        {
            return GetSymbol(GetCurrentFunction(), name);
        }


        private SymbolInfo GetSymbol(DwarfSubprogram currentFunction, string name)
        {
            if (currentFunction == null) return null;

            var symbol = IdeManager.Dwarf.GetSymbol(name, currentFunction);
            if (symbol == null) return null;

            var val = symbol.GetValue(mDebugger);
            if (val == null) return null;

            return new SymbolInfo(mDebugger, name, symbol.Type, val);
        }
        
        private DwarfSubprogram GetCurrentFunction()
        {
            var pc = mDebugger.RegManager.Registers["PC"];
            return IdeManager.Dwarf.GetFunctionAt(pc);
        }
    }

    
    public interface IDwarfProvider
    {
        DwarfSubprogram GetFunctionAt(int programCounter);
        DwarfSubprogram GetFunctionByName(string functionName);
        DwarfLocatedObject GetSymbol(string symbolName, DwarfSubprogram function);        
    }
}
