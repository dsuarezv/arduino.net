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

            if (Name == "loop") System.Diagnostics.Debugger.Break();
        }
    }

    public class DwarfLocatedObject: DwarfDeclaredObject
    {
        private string mLocationString;
        private int mTypeId;

        public DwarfLocation Location;
        public DwarfBaseType Type;

        public virtual byte[] GetValue(Debugger debugger)
        {
            if (Location == null) return null;

            return Location.GetValue(debugger, Type);
        }

        public virtual string GetValueRepresentation(Debugger debugger, byte[] value)
        {
            return Type.GetValueRepresentation(debugger, value);
        }

        public override void FillAttributes(DwarfParserNode node)
        {
 	        base.FillAttributes(node);

            mLocationString = node.GetAttr("location").RawValue;
            mTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            //if (Name == "arg3") System.Diagnostics.Debugger.Break();

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


    // __ Base Types __________________________________________________________


    public class DwarfBaseType : DwarfNamedObject
    {
        public int ByteSize;
        public int Encoding;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            ByteSize = node.GetAttr("byte_size").GetIntValue();
            Encoding = node.GetAttr("encoding").GetIntValue();

            if (Name == null) 
            {
                // Special case: apparently "int" is not an indirect string, like everything else.
                var n = node.GetAttr("name").RawValue;
                if (n != null) Name = n.Trim(' ', '\t');
            }
        }

        public virtual string GetValueRepresentation(Debugger debugger, byte[] value)
        {
            long intValue = GetIntFromBytes(value);

            return string.Format("{0} ({1})", intValue, Name);
        }

        public static DwarfBaseType GetTypeFromIndex(Dictionary<int, DwarfObject> index, int key)
        {
            DwarfObject result;

            index.TryGetValue(key, out result);

            return result as DwarfBaseType;
        }


        protected long GetIntFromBytes(byte[] value)
        {
            long intValue = 0;

            for (int i = 0; i < ByteSize; ++i)
            {
                intValue += ((long)(value[i])) << i * 8;
            }

            return intValue;
        }
    }


    public class DwarfPointerType: DwarfBaseType
    {
        private int mTypeId;
        public DwarfBaseType PointerToType;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            mTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            if (mTypeId == -1) return;

            PointerToType = GetTypeFromIndex(index, mTypeId);
        }

        public override string GetValueRepresentation(Debugger debugger, byte[] value)
        {
            if (PointerToType == null) return base.GetValueRepresentation(debugger, value);

            var pointerValue = GetIntFromBytes(value);

            // Null pointer?
            if (pointerValue == 0) return string.Format("{0} ({1} *)", pointerValue, PointerToType.Name);

            // Create a new expression to get the pointer value

            //pointerValue += 0x800000;    // TODO: get this from the config

            var program = new List<string>();
            program.Add(string.Format("DW_OP_addr: {0:X}", pointerValue));
            var pointerTargetLocation = new DwarfLocation() { RawLocationProgram = program };

            var targetRawValue = pointerTargetLocation.GetValue(debugger, PointerToType);

            return string.Format("0x{0:X} ({1} *) -> {2}", pointerValue, PointerToType.Name, 
                PointerToType.GetValueRepresentation(debugger, targetRawValue));
        }
    }

    public class DwarfClassType: DwarfBaseType
    {
        private int mSiblingId;

        public int Declaration;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            Declaration = node.GetAttr("declaration").GetIntValue();
            mSiblingId = node.GetAttr("sibling").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int,DwarfObject> index)
        {
            if (mSiblingId == -1) return;
            
            // The sibling points to the structure definition that describes the class.
        }
    }

    public class DwarfConstType: DwarfBaseType
    {
        private int mConstTypeId;
        public DwarfBaseType ConstType;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            mConstTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            if (mConstTypeId == -1) return;

            ConstType = GetTypeFromIndex(index, mConstTypeId);
        }

    }
}
