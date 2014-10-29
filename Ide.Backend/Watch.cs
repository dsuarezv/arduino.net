using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class Watch
    {
        private List<Watch> mChildren = new List<Watch>();
        private byte[] mRawValue;
        private IDebugger mDebugger;
        private IWatchProvider mWatchProvider;


        public string Name { get; set; }
        public string Type { get; private set; }
        public string Value { get; private set; }

        
        public Watch(IDebugger debugger, IWatchProvider provider)
        {
            mDebugger = debugger;
            mWatchProvider = provider;
        }


        public IList<Watch> Children
        {
            get { return mChildren; }
        }


        public void Update()
        {
            if (Name == null) return;

            var pc = mDebugger.RegManager.Registers["PC"];
            var function = mWatchProvider.GetFunctionAt(pc);
            if (function == null) 
            {
                Value = "<current context not found>";
                return;
            }

            UpdateValue(function);
        }

        private void UpdateValue(DwarfSubprogram function)
        {
            var variable = mWatchProvider.GetSymbol(Name, function);
            if (variable == null) 
            {
                Value = "<not in current context>";
                return;
            }

            Type = variable.Type.Name;

            mRawValue = variable.GetValue(mDebugger);
            if (mRawValue == null) 
            {
                Value = "<symbol has no location debug information>";
                return;
            }

            Value = variable.GetValueRepresentation(mDebugger, mRawValue);

            HandleChildren();
        }

        private void HandleChildren()
        {
            

            
        }


        public static string GetWatchValue(IDebugger debugger, IWatchProvider provider, string symbolName)
        {
            var pc = debugger.RegManager.Registers["PC"];
            var function = provider.GetFunctionAt(pc);
            if (function == null) return symbolName + ": <current context not found>\n";

            return GetWatchValue(debugger, provider, function, symbolName);
        }

        public static string GetWatchValue(IDebugger debugger, IWatchProvider provider, string functionName, string symbolName)
        {
            var function = provider.GetFunctionByName(functionName);
            if (function == null) return symbolName + ": <context not found>\n";

            return GetWatchValue(debugger, provider, function, symbolName);
        }

        public static string GetWatchValue(IDebugger debugger, IWatchProvider provider, DwarfSubprogram function, string symbolName)
        {
            var symbol = provider.GetSymbol(symbolName, function);
            if (symbol == null) return symbolName + ": <not in current context>\n";

            var val = symbol.GetValue(debugger);
            if (val == null) return symbolName + ": <symbol has no location debug information>\n";

            return string.Format("{0}: {1}\n", symbolName, symbol.GetValueRepresentation(debugger, val));
        }

        
    }

    
    public interface IWatchProvider
    {
        DwarfLocatedObject GetSymbol(string symbolName, DwarfSubprogram function);
        DwarfSubprogram GetFunctionAt(int programCounter);
        DwarfSubprogram GetFunctionByName(string functionName);
    }
}
