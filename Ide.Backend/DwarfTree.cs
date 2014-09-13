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
        private Dictionary<string, DwarfNamedObject> mIndexByName = new Dictionary<string, DwarfNamedObject>();
        private Dictionary<int, DwarfObject> mIndexById = new Dictionary<int, DwarfObject>();
        private Dictionary<string, DwarfCompileUnit> mCompileUnits = new Dictionary<string, DwarfCompileUnit>();

        public DwarfTree(DwarfTextParser parser)
        {
            mParser = parser;

            BuildTree();
        }

        
        public DwarfSubprogram GetFunctionAt(int programCounter)
        {
            throw new NotImplementedException();
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
                case "compile_unit": break;
                case "base_type": break;
                case "const_type": break;
                case "pointer_type": break;
                case "class_type": break;
                case "subprogram": break;
                case "formal_parameter": break;
                case "variable": break;
            }

            if (result != null) 
            {
                mIndexById[result.Id] = result;
            }

            return result;
        }
    }
}
