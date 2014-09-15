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

    public class DwarfBaseType: DwarfNamedObject
    {
        public int ByteSize;
        public int Encoding;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            ByteSize = node.GetAttr("byte_size").GetIntValue();
            Encoding = node.GetAttr("encoding").GetIntValue();
        }

        public static DwarfBaseType GetTypeFromIndex(Dictionary<int, DwarfObject> index, int key)
        {
            DwarfObject result;

            index.TryGetValue(key, out result);
            
            return  result as DwarfBaseType;
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
            HighPc = node.GetAttr("high_pc").GetIntValue();
            LowPc = node.GetAttr("low_pc").GetIntValue();
            LinkageName = node.GetAttr("MIPS_linkage_name").GetStringValue();
            External = node.GetAttr("external").GetBoolValue();
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
            throw new NotImplementedException();

            // This method should interpret the location program and send the needed traceQueries 
            // to the debugger until it finds the described value.
        }

        public virtual string GetValueRepresentation(byte[] value)
        {
            throw new NotImplementedException();

            // using the Type, interpret the bytes and return a printable representation.
        }

        public override void FillAttributes(DwarfParserNode node)
        {
 	        base.FillAttributes(node);
             
            mLocationString = node.GetAttr("location").GetStringValue();
            mTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            if (mTypeId == -1) return;

            Type = DwarfBaseType.GetTypeFromIndex(index, mTypeId);
        }
    }

    public class DwarfFormalParameter : DwarfLocatedObject
    {
        
    }
    
    public class DwarfVariable: DwarfLocatedObject
    {
        public string ConstValue;
    }


    public class DwarfLocation: DwarfObject
    {
        public List<string> LocationProgram;
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
