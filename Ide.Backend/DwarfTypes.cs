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
            long intValue = 0;

            for (int i = 0; i < ByteSize; ++i)
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

        public override string GetValueRepresentation(IDebugger debugger, byte[] rawValue)
        {
            if (PointerToType == null) return base.GetValueRepresentation(debugger, rawValue);

            var pointerValue = GetIntFromBytes(rawValue);

            // Null pointer?
            if (pointerValue == 0) return string.Format("<null>", pointerValue, PointerToType.Name);

            if (debugger.Status != DebuggerStatus.Break) return "<Arduino is running>";

            // Create a new expression to get the pointer value

            var program = new List<string>();
            program.Add(string.Format("DW_OP_addr: {0:X}", pointerValue));
            var pointerTargetLocation = new DwarfLocation() { RawLocationProgram = program };

            var targetRawValue = pointerTargetLocation.GetValue(debugger, PointerToType);

            return string.Format("0x{0:X} -> {1}", pointerValue, 
                PointerToType.GetValueRepresentation(debugger, targetRawValue));
        }

        public override string ToString()
        {
            return PointerToType.Name + "*";
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
            var result = Type.GetValueRepresentation(debugger, rawValue);
            return result;
            //return string.Format("{0}: {1}", Name, result);
        }

        public byte[] GetMemberRawValue(byte[] value)
        {
            return Location.GetValue(value, Type);
        }

        public override string ToString()
        {
            return Type.ToString();
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
