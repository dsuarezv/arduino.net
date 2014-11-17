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
            Encoding = node.GetAttr("encoding").GetEncodingValue();
        }

        public virtual string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            switch (Encoding)
            {
                case 1: return "<linear address>";                          // linear machine address
                case 2: return GetBoolValue(rawValue).ToString();  // boolean
                case 3: return "<complex float>";                           // complex floating point number
                case 4: return GetFloatValue(rawValue).ToString(); // float
                case 5: 
                case 6: return GetIntRepresentation(rawValue, true);        // signed binary integer
                case 7: 
                case 8: return GetIntRepresentation(rawValue, false);       // unsigned binary character
                default: return "<unknown encoding>";
            }
        }

        public virtual string GetTypeRepresentation()
        {
            return Name;
        }

        private string GetCharRepresentation(byte[] value, bool signed)
        {
            if (value == null || value.Length != 1) return "<invalid char>";
                
            return new string((char)value[0], 1);
        }

        private string GetIntRepresentation(byte[] value, bool signed)
        {
            long intValue = GetIntValue(value);

            if (signed)
            { 
                switch (ByteSize)
                {
                    case 1: return ((sbyte)intValue).ToString();
                    case 2: return ((Int16)intValue).ToString();
                    case 4: return ((Int32)intValue).ToString();
                }
            }
            else
            {
                switch (ByteSize)
                {
                    case 1: return ((byte)intValue).ToString();
                    case 2: return ((UInt16)intValue).ToString();
                    case 4: return ((UInt32)intValue).ToString();
                }
            }

            return intValue.ToString();
        }

        protected float GetFloatValue(byte[] value)
        {
            if (value.Length != 4 || ByteSize != 4) return -1f;

            return BitConverter.ToSingle(value, 0);
        }

        protected bool GetBoolValue(byte[] value)
        {
            if (ByteSize != 1 || value.Length != 1) return false;  // TODO: throw?

            return value[0] > 0;
        }

        public static DwarfBaseType GetTypeFromIndex(Dictionary<int, DwarfObject> index, int key)
        {
            DwarfObject result;

            index.TryGetValue(key, out result);

            return result as DwarfBaseType;
        }


        protected long GetIntValue(byte[] value)
        {
            int numBytes = (ByteSize == -1) ? value.Length : ByteSize;
            long intValue = 0;

            for (int i = 0; i < numBytes; ++i)
            {
                intValue += ((long)(value[i])) << i * 8;
            }

            return intValue;
        }
    }

    public class DwarfTypeDefType: DwarfBaseType
    {
        private int mTypeId;

        public DwarfBaseType TypeDefType;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);

            mTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            base.SetupReferences(parser, index);

            TypeDefType = GetTypeFromIndex(index, mTypeId);

            if (TypeDefType == null) return;
            ByteSize = TypeDefType.ByteSize;
        }

        public override string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            return TypeDefType.GetValueRepresentation(debugger, rawValue);
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
            mPointedSymbol.MemberType = PointedSymbolType;
            mPointedSymbol.ByteSize = -1;
        }

        public override string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            var pointerValue = GetIntValue(rawValue);
            if (pointerValue == 0) return "<null>";

            return string.Format("0x{0:x}", pointerValue);
        }

        public override string GetTypeRepresentation()
        {
            if (PointedSymbolType == null) return "<null pointer type>";

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
    }


    public class DwarfMember : DwarfBaseType
    {
        protected string mLocationString;
        private int mTypeId;

        public DwarfLocation Location;
        public DwarfBaseType MemberType;

        public override void FillAttributes(DwarfParserNode node)
        {
            base.FillAttributes(node);
            mLocationString = node.GetAttr("data_member_location").RawValue;
            mTypeId = node.GetAttr("type").GetReferenceValue();
        }

        public override void SetupReferences(DwarfTextParser parser, Dictionary<int, DwarfObject> index)
        {
            if (mTypeId > -1) MemberType = DwarfBaseType.GetTypeFromIndex(index, mTypeId);
            if (mLocationString != null) Location = DwarfLocation.Get(parser, mLocationString);
        }

        public override string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            return MemberType.GetValueRepresentation(debugger, rawValue);
        }

        public virtual byte[] GetMemberRawValue(IDebugger debugger, byte[] parentRawValue)
        {
            return Location.GetValue(parentRawValue, MemberType);
        }

        public override string GetTypeRepresentation()
        {
            return MemberType.ToString();
        }
    }

    public class DwarfPointerMember: DwarfMember
    {
        public override byte[] GetMemberRawValue(IDebugger debugger, byte[] rawValue)
        {
            if (debugger.Status != DebuggerStatus.Break) return null;
            if (MemberType == null) return null;

            // Create a new Location Program to get the pointer value
            var pointerValue = GetIntValue(rawValue);
            if (pointerValue == 0) return null;

            var program = new List<string>();
            program.Add(string.Format("DW_OP_addr: {0:x}", pointerValue));
            var pointerTargetLocation = new DwarfLocation() { RawLocationProgram = program };

            return pointerTargetLocation.GetValue(debugger, MemberType);
        }
    }


    public class DwarfClassType : DwarfStructType
    {
        private int mSiblingId;

        public int Declaration;

        public DwarfClassType(): base()
        {
            
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
