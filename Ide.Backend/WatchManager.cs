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
        private ObservableCollection<SymbolInfo> mSymbols = new ObservableCollection<SymbolInfo>();


        public ObservableCollection<SymbolInfo> Symbols
        {
            get { return mSymbols; }
        }


        public WatchManager(IDebugger debugger)
        {
            mDebugger = debugger;
            SessionSettings.RegisterPersistenceListener(this);
        }


        public void Refresh(DwarfTree dwarf)
        {
            var currentFunc = GetCurrentFunction(dwarf);

            foreach (var s in mSymbols)
            {
                s.Refresh(currentFunc, dwarf);
            }
        }

        public void Reset()
        {
            mSymbols.Clear();
        }

        public SymbolInfo GetInmmediateValue(string name, DwarfTree dwarf)
        {
            var symbol = new SymbolInfo(mDebugger, name) { IsRoot = true };
            symbol.Refresh(GetCurrentFunction(dwarf), dwarf);

            return symbol;
        }

        public DwarfSubprogram GetCurrentFunction(DwarfTree dwarf)
        {
            if (!mDebugger.RegManager.Registers.ContainsKey("PC")) return null;

            var pc = mDebugger.RegManager.Registers["PC"];
            return dwarf.GetFunctionAt(pc);
        }

        public SymbolInfo AddSymbol(string name)
        {
            var result = new SymbolInfo(mDebugger, name) { IsRoot = true };

            mSymbols.Add(result);

            return result;
        }


        // __ IPersistenceListener ____________________________________________


        public object GetObjectToPersist()
        {
            List<string> result = new List<string>();

            foreach (var s in mSymbols) result.Add(s.SymbolName);

            return result;
        }

        public void RestorePersistedObject(object obj)
        {
            var symbolNames = obj as List<string>;
            if (symbolNames == null) return;

            foreach (var s in symbolNames) AddSymbol(s);
        }
    }

    
    public interface IDwarfProvider
    {
        DwarfSubprogram GetFunctionAt(int programCounter);
        DwarfSubprogram GetFunctionByName(string functionName);
        DwarfLocatedObject GetSymbol(string symbolName, DwarfSubprogram function);        
    }
}
