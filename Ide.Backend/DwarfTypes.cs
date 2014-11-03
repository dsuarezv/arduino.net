using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    // __ Base Types __________________________________________________________


    public class DwarfBaseType : DwarfNamedObject
    {
        public int ByteSize;
        public int Encoding;
        public List<DwarfMember> Members;


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

        public virtual string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            long intValue = GetIntFromBytes(rawValue);

            return GetValueRepresentation(intValue);
        }

        private string GetValueRepresentation(long intValue)
        {
            switch (ByteSize)
            {
                case 1: return ((byte)intValue).ToString();
                case 2: return ((Int16)intValue).ToString();
                case 4: return ((Int32)intValue).ToString();
            }

            return intValue.ToString();
        }

        public static DwarfBaseType GetTypeFromIndex(Dictionary<int, DwarfObject> index, int key)
        {
            DwarfObject result;

            index.TryGetValue(key, out result);

            return result as DwarfBaseType;
        }


        protected long GetIntFromBytes(byte[] value)
        {
            int numBytes = (ByteSize == -1) ? value.Length : ByteSize;
            long intValue = 0;

            for (int i = 0; i < numBytes; ++i)
            {
                intValue += ((long)(value[i])) << i * 8;
            }

            return intValue;
        }

        public override string ToString()
        {
            return Name;
        }
    }


    public class DwarfPointerType: DwarfBaseType
    {
        private int mTypeId;
        private DwarfMember mPointedSymbol;


        public DwarfBaseType PointedSymbolType;


        public DwarfPointerType()
        {
            Members = new List<DwarfMember>();

            mPointedSymbol = new DwarfPointerMember() { Name = "->" };
            Members.Add(mPointedSymbol);
        }

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            mTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            if (mTypeId == -1) return;

            PointedSymbolType = GetTypeFromIndex(index, mTypeId);
            mPointedSymbol.Type = PointedSymbolType;
            mPointedSymbol.ByteSize = -1;
        }

        public override string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            var pointerValue = GetIntFromBytes(rawValue);
            if (pointerValue == 0) return "<null>";

            return string.Format("0x{0:x}", pointerValue);
                
        }

        public override string ToString()
        {
            return PointedSymbolType.Name + "*";
        }
    }


    public class DwarfStructType : DwarfBaseType
    {
        private int mSiblingId;

        public DwarfStructType()
        {
            Members = new List<DwarfMember>();
        }

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            mSiblingId = node.GetAttr("sibling").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            if (mSiblingId == -1) return;

            // There is a sibling. Have to check the spec to see what it points to.
        }

        public override string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            return "<struct>";
        }

        public override string ToString()
        {
            return "struct " + Name;
        }
    }


    public class DwarfMember : DwarfBaseType
    {
        protected string mLocationString;
        private int mTypeId;

        public DwarfLocation Location;
        public DwarfBaseType Type;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            mLocationString = node.GetAttr("data_member_location").RawValue;
            mTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            if (mTypeId > -1) Type = DwarfBaseType.GetTypeFromIndex(index, mTypeId);
            if (mLocationString != null) Location = DwarfLocation.Get(parser, mLocationString);
        }

        public override string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            return Type.GetValueRepresentation(debugger, rawValue);
        }

        public virtual byte[] GetMemberRawValue(IDebugger debugger, byte[] parentRawValue)
        {
            return Location.GetValue(parentRawValue, Type);
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    public class DwarfPointerMember: DwarfMember
    {
        public override byte[] GetMemberRawValue(IDebugger debugger, byte[] rawValue)
        {
            if (debugger.Status != DebuggerStatus.Break) return null;
            if (Type == null) return null;

            // Create a new Location Program to get the pointer value
            var pointerValue = GetIntFromBytes(rawValue);
            if (pointerValue == 0) return null;

            var program = new List<string>();
            program.Add(string.Format("DW_OP_addr: {0:x}", pointerValue));
            var pointerTargetLocation = new DwarfLocation() { RawLocationProgram = program };

            return pointerTargetLocation.GetValue(debugger, Type);
        }
    }


    public class DwarfClassType : DwarfBaseType
    {
        private int mSiblingId;

        public int Declaration;

        public DwarfClassType()
        {
            Members = new List<DwarfMember>();
        }

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            Declaration = node.GetAttr("declaration").GetIntValue();
            mSiblingId = node.GetAttr("sibling").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            if (mSiblingId == -1) return;

            // The sibling points to the structure containing the virtual pointer table. 
        }

        public override string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            return "<class>";
        }

        public override string ToString()
        {
            return "class " + Name;
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
