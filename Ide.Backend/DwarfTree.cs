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
        
        
        private void BuildTree()
        { 
            foreach (var n in mParser.TopNodes)
            { 
                ParseNode(n);
            }
        }

        private DwarfObject ParseNode(DwarfParserNode node)
        {
            DwarfObject result = null;

            switch (node.TagType)
            {
                case "compile_unit": 
                    result = SetupNewObject(node, new DwarfCompileUnit());
                    mCompileUnits.Add((DwarfCompileUnit)result);
                    break;
                case "base_type": result = SetupNewObject(node, new DwarfBaseType()); break;
                case "const_type":  break;
                case "pointer_type": break;
                case "class_type": break;
                case "subprogram": result = SetupNewObject(node, new DwarfSubprogram()); break;
                case "formal_parameter": result = SetupNewObject(node, new DwarfFormalParameter()); break;
                case "variable": result = SetupNewObject(node, new DwarfVariable()); break;
            }

            if (result != null) mIndexById.Add(result.Id, result);

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
