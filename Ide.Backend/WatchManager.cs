using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;


namespace arduino.net
{
    public class WatchManager: IPersistenceListener
    {
        private IDebugger mDebugger;
        private ObservableCollection<string> mSymbolNames = new ObservableCollection<string>();


        public ObservableCollection<string> SymbolNames
        {
            get { return mSymbolNames; }
        }


        public WatchManager(IDebugger debugger)
        {
            mDebugger = debugger;
            SessionSettings.RegisterPersistenceListener(this);
        }


        public IList<SymbolInfo> GetValues()
        {
            if (mDebugger.Status != DebuggerStatus.Break) return null;

            var currentFunction = GetCurrentFunction();
            if (currentFunction == null) return null;
            
            ObservableCollection<SymbolInfo> result = new ObservableCollection<SymbolInfo>();
            if (mSymbolNames == null) return result;

            foreach (var name in mSymbolNames)
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

            return new SymbolInfo(mDebugger, name, symbol.Type, val) { IsRoot = true };
        }
        
        private DwarfSubprogram GetCurrentFunction()
        {
            var pc = mDebugger.RegManager.Registers["PC"];
            return IdeManager.Dwarf.GetFunctionAt(pc);
        }


        // __ IPersistenceListener ____________________________________________


        public object GetObjectToPersist()
        {
            return mSymbolNames;
        }

        public void RestorePersistedObject(object obj)
        {
            mSymbolNames = obj as ObservableCollection<string>;
        }
    }

    
    public interface IDwarfProvider
    {
        DwarfSubprogram GetFunctionAt(int programCounter);
        DwarfSubprogram GetFunctionByName(string functionName);
        DwarfLocatedObject GetSymbol(string symbolName, DwarfSubprogram function);        
    }
}
