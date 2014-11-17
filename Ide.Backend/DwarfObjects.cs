using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class DwarfObject
    {
        public int Id;

        public virtual void FillAttributes(DwarfParserNode node)
        {
            Id = node.Id;
        }

        public virtual void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        { 
            
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} {Name}")]
    public class DwarfNamedObject: DwarfObject
    {
        public string Name;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            Name = node.GetAttr("name").GetStringValue();

            //if (Name == "ret") System.Diagnostics.Debugger.Break();
        }
    }

    public class DwarfDeclaredObject: DwarfNamedObject
    {
        public int DeclarationFile;
        public int DeclarationLine;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            DeclarationFile = node.GetAttr("decl_file").GetIntValue();
            DeclarationLine = node.GetAttr("decl_line").GetIntValue();
        }
    }

    public class DwarfCompileUnit: DwarfNamedObject
    {
        public string Ranges;
        public string StatementList;

        public List<DwarfSubprogram> Subprograms = new List<DwarfSubprogram>();
        public Dictionary<string, DwarfVariable> Variables = new Dictionary<string, DwarfVariable>();
        public Dictionary<string, DwarfLocatedObject> Types = new Dictionary<string, DwarfLocatedObject>();


        // Subprograms
        // Typedefs
        // Variables
        
        // BaseTypes
        // StructureTypes
        // ArrayTypes
        // PointerTypes
        // ClassTypes
    }

    public class DwarfSubprogram: DwarfDeclaredObject
    {
        public DwarfCompileUnit Parent;
        public string LinkageName;
        public bool External;
        public int LowPc;
        public int HighPc;

        public Dictionary<string, DwarfLocatedObject> Variables = new Dictionary<string, DwarfLocatedObject>();

        // Formal parameters
        // Variables
        // InlinedSubroutines

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            HighPc = node.GetAttr("high_pc").GetHexValue();
            LowPc = node.GetAttr("low_pc").GetHexValue();
            LinkageName = node.GetAttr("MIPS_linkage_name").GetStringValue();
            External = node.GetAttr("external").GetBoolValue();
        }
    }

    public class DwarfLocatedObject: DwarfDeclaredObject
    {
        protected string mLocationString;
        private int mTypeId;

        public DwarfLocation Location;
        public DwarfBaseType Type;

        public virtual byte[] GetValue(IDebugger debugger)
        {
            if (Location == null) return null;

            return Location.GetValue(debugger, Type);
        }

        public override void FillAttributes(DwarfParserNode node)
        {
 	        base.FillAttributes(node);

            //if (Name == "SuperStruct") System.Diagnostics.Debugger.Break();

            mLocationString = node.GetAttr("location").RawValue;
            mTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            //if (Name == "myStr") System.Diagnostics.Debugger.Break();

            if (mTypeId > -1) Type = DwarfBaseType.GetTypeFromIndex(index, mTypeId);
            if (mLocationString != null) Location = DwarfLocation.Get(parser, mLocationString);
        }
    }

    public class DwarfFormalParameter : DwarfLocatedObject
    {
        
    }
    
    public class DwarfVariable: DwarfLocatedObject
    {
        
    }
}
