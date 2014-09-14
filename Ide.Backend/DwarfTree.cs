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
        private Dictionary<string, DwarfCompileUnit> mCompileUnits = new Dictionary<string, DwarfCompileUnit>();

        public DwarfTree(DwarfTextParser parser)
        {
            mParser = parser;

            BuildTree();
        }

        
        public DwarfSubprogram GetFunctionAt(int programCounter)
        {
            foreach (var cu in mCompileUnits.Values)
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
                case "compile_unit": result = CreateCompileUnit(node); break;
                case "base_type": break;
                case "const_type": break;
                case "pointer_type": break;
                case "class_type": break;
                case "subprogram": result = CreateSubProgram(node); break;
                case "formal_parameter": break;
                case "variable": break;
            }

            if (result != null) 
            {
                mIndexById[result.Id] = result;
            }

            return result;
        }


        private DwarfCompileUnit CreateCompileUnit(DwarfParserNode node)
        {
            var result = new DwarfCompileUnit()
            {
                Id = node.Id,
                Name = node.GetAttr("name").GetStringValue()
            };

            if (node.Children != null)
            { 
                foreach (var childNode in node.Children)
                {
                    var child = ParseNode(childNode);
                    Add(child).To(result);
                }
            }

            mCompileUnits.Add(result.Name, result);

            return result;
        }

        private DwarfSubprogram CreateSubProgram(DwarfParserNode node)
        {
            var result = new DwarfSubprogram()
            {
                Id = node.Id,
                Name = node.GetAttr("name").GetStringValue(),
                DeclarationFile = node.GetAttr("decl_file").GetIntValue(),
                DeclarationLine = node.GetAttr("decl_line").GetIntValue(),
                HighPc = node.GetAttr("high_pc").GetIntValue(),
                LowPc = node.GetAttr("low_pc").GetIntValue(),
                LinkageName = node.GetAttr("MIPS_linkage_name").GetStringValue(),
            };

            if (node.Children != null)
            { 
                foreach (var childNode in node.Children)
                {
                    var child = ParseNode(childNode);
                    Add(child).To(result);
                }
            }

            return result;
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
