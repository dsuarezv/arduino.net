using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class DwarfTree
    {
        private DwarfTextParser mParser;
        //private Dictionary<string, DwarfNamedObject> mIndexByName = new Dictionary<string, DwarfNamedObject>();
        private Dictionary<int, DwarfObject> mIndexById = new Dictionary<int, DwarfObject>();
        private List<DwarfCompileUnit> mCompileUnits = new List<DwarfCompileUnit>();
        private DwarfCompileUnit mCurrentCompileUnit;


        public DwarfTree(DwarfTextParser parser)
        {
            mParser = parser;

            BuildTree();
        }

        
        public DwarfSubprogram GetFunctionAt(int programCounter)
        {
            foreach (var cu in mCompileUnits)
            { 
                foreach (var sub in cu.Subprograms)
                { 
                    if (sub.LowPc <= programCounter && sub.HighPc >= programCounter)
                    {
                        return sub;
                    }
                }
            }

            return null;
        }

        public DwarfSubprogram GetFunctionByName(string funcName)
        {
            foreach (var cu in mCompileUnits)
            {
                foreach (var sub in cu.Subprograms)
                {
                    if (sub.Name == funcName)
                    {
                        return sub;
                    }
                }
            }

            return null;
        }

        public DwarfLocatedObject GetSymbol(string symbolName, DwarfSubprogram currentFunc)
        {
            // Search in this function first
            DwarfLocatedObject result;

            if (currentFunc == null) return null;

            if (currentFunc.Variables.TryGetValue(symbolName, out result))
            {
                return result;
            }

            DwarfVariable globalVar;

            if (currentFunc.Parent.Variables.TryGetValue(symbolName, out globalVar))
            {
                return globalVar;
            }

            return null;            
        }

        public byte[] GetSymbolValue(string symbolName, DwarfSubprogram currentFunc, Debugger debugger)
        {
            var symbol = GetSymbol(symbolName, currentFunc);

            if (symbol == null) return null;

            return symbol.GetValue(debugger);
        }
        
        
        private void BuildTree()
        { 
            // First pass: fill objects with parsed values
            foreach (var n in mParser.TopNodes)
            { 
                ParseNode(n);
            }

            // Second pass: update object references
            foreach (var o in mIndexById.Values)
            {
                o.SetupReferences(mParser, mIndexById);
            }
        }

        private DwarfObject ParseNode(DwarfParserNode node)
        {
            DwarfObject result = null;

            switch (node.TagType)
            {
                case "typedef": break;
                case "compile_unit": result = CreateCompileUnit(node); break;
                case "base_type": result = SetupNewObject(node, new DwarfBaseType()); break;
                case "const_type": result = SetupNewObject(node, new DwarfConstType()); break;
                case "pointer_type": result = SetupNewObject(node, new DwarfPointerType()); break;
                case "class_type": result = SetupNewObject(node, new DwarfClassType()); break;
                case "structure_type": break;
                case "subprogram": result = CreateSubProgram(node); break;
                case "formal_parameter": result = SetupNewObject(node, new DwarfFormalParameter()); break;
                case "variable": result = SetupNewObject(node, new DwarfVariable()); break;
            }

            return result;
        }

        private DwarfCompileUnit CreateCompileUnit(DwarfParserNode node)
        {
            var result = new DwarfCompileUnit();
            mCurrentCompileUnit = result;
            SetupNewObject(node, result);
            mCompileUnits.Add(result);

            return result;
        }

        private DwarfSubprogram CreateSubProgram(DwarfParserNode node)
        {
            var result = new DwarfSubprogram();
            result.Parent = mCurrentCompileUnit;
            SetupNewObject(node, result);

            return result;
        }

        private DwarfObject SetupNewObject(DwarfParserNode node, DwarfObject obj)
        {
            obj.FillAttributes(node);

            if (node.Children != null)
            {
                foreach (var childNode in node.Children)
                {
                    var child = ParseNode(childNode);
                    Add(child).To(obj);
                }
            }

            mIndexById.Add(obj.Id, obj);

            return obj;
        }




        // __ Collection dispatching __________________________________________


        private CollectionDispatcher Add(DwarfObject target)
        {
            return new CollectionDispatcher(target);
        }

        private class CollectionDispatcher
        {
            private DwarfObject mTarget;
            

            public CollectionDispatcher(DwarfObject target)
            {
                mTarget = target;
            }

            public void To(DwarfObject container)
            {
                if (container is DwarfCompileUnit) { To((DwarfCompileUnit)container); return; }
                if (container is DwarfSubprogram) { To((DwarfSubprogram)container); return; }
            }

            public void To(DwarfCompileUnit container)
            {
                if (!IsValidInput(container)) return;


                var subprogram = mTarget as DwarfSubprogram;
                if (subprogram != null)
                {
                    container.Subprograms.Add(subprogram);
                    return;
                }

                var variable = mTarget as DwarfVariable;
                if (variable != null)
                {
                    container.Variables.Add(variable.Name, variable);
                    return;
                }

                var type = mTarget as DwarfLocatedObject;
                if (type != null) 
                {
                    container.Types.Add(type.Name, type);
                    return;
                }
            }

            public void To(DwarfSubprogram container)
            {
                if (!IsValidInput(container)) return;

                var variable = mTarget as DwarfVariable;
                if (variable != null) 
                {
                    container.Variables.Add(variable.Name, variable);
                    return;
                }

                var param = mTarget as DwarfFormalParameter;
                if (param != null)
                {
                    container.Variables.Add(param.Name, param);
                    return;
                }
            }

            
            // __ Helpers _____________________________________________________


            private bool IsValidInput(DwarfObject container)
            {
                if (container == null || mTarget == null) return false;

                var named = mTarget as DwarfNamedObject;
                if (named != null && named.Name == null) return false;

                return true;
            }
        }
    }

    
}
