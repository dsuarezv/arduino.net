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
        //private IDwarfProvider mDwarf;
        private ObservableCollection<string> mWatchNames = new ObservableCollection<string>();


        public ObservableCollection<string> WatchNames
        {
            get { return mWatchNames; }
        }


        public WatchManager(IDebugger debugger)
        {
            mDebugger = debugger;
        }

        public IList<SymbolInfo> GetValues()
        {
            var currentFunction = GetCurrentFunction();
            if (currentFunction == null) return null;
            
            List<SymbolInfo> result = new List<SymbolInfo>();

            foreach (var name in mWatchNames)
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

        
        

        //public static string GetWatchValue(IDebugger debugger, IWatchProvider provider, string symbolName)
        //{
        //    var pc = debugger.RegManager.Registers["PC"];
        //    var function = provider.GetFunctionAt(pc);
        //    if (function == null) return symbolName + ": <current context not found>\n";

        //    return GetWatchValue(debugger, provider, function, symbolName);
        //}

        //public static string GetWatchValue(IDebugger debugger, IWatchProvider provider, string functionName, string symbolName)
        //{
        //    var function = provider.GetFunctionByName(functionName);
        //    if (function == null) return symbolName + ": <context not found>\n";

        //    return GetWatchValue(debugger, provider, function, symbolName);
        //}

        //public static string GetWatchValue(IDebugger debugger, IWatchProvider provider, DwarfSubprogram function, string symbolName)
        //{
        //    var symbol = provider.GetSymbol(symbolName, function);
        //    if (symbol == null) return symbolName + ": <not in current context>\n";

        //    var val = symbol.GetValue(debugger);
        //    if (val == null) return symbolName + ": <symbol has no location debug information>\n";

        //    return string.Format("{0}: {1}\n", symbolName, symbol.GetValueRepresentation(debugger, val));
        //}
    }

    
    public interface IDwarfProvider
    {
        DwarfSubprogram GetFunctionAt(int programCounter);
        DwarfSubprogram GetFunctionByName(string functionName);
        DwarfLocatedObject GetSymbol(string symbolName, DwarfSubprogram function);        
    }
}
