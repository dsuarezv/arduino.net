using System;
using arduino.net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ide.Backend.Tests
{
    [TestClass]
    public class DwarfStructTypeTests
    {
        public static DwarfBaseType IntType = new DwarfBaseType() { ByteSize = 2, Name = "int" };
        public static DwarfBaseType BoolType = new DwarfBaseType() { ByteSize = 1, Name = "bool" };
        public static DwarfBaseType IntPointerType = new DwarfPointerType() { ByteSize = 2, Name = "int *", PointedSymbolType = IntType };

        [TestMethod]
        public void DwarfStructTypeGetValue()
        {
            var t = new DwarfStructType()
            {
                Name = "SuperStruct",
                ByteSize = 5,
                Id = 0x7a
            };

            t.Members.Add(new DwarfMember()
            {
                Name = "myInt1",
                Location = new DwarfLocation("DW_OP_plus_uconst: 0"),
                MemberType = IntType
            });

            t.Members.Add(new DwarfMember()
            {
                Name = "myBool1",
                Location = new DwarfLocation("DW_OP_plus_uconst: 2"),
                MemberType = BoolType
            });

            t.Members.Add(new DwarfMember()
            {
                Name = "myIntPointer1",
                Location = new DwarfLocation("DW_OP_plus_uconst: 3"),
                MemberType = IntPointerType
            });

            var value = new byte[] { 0xff, 0x01, 0x02, 0x03, 0x04 };
            var result = t.GetValueRepresentation(new TestDebugger(), value);

            var lines = result.Split('\n');

            Assert.IsTrue(lines.Length >= 3);
            Assert.IsTrue(lines[0].StartsWith("myInt1"));
            Assert.IsTrue(lines[1].StartsWith("myBool1"));
            Assert.IsTrue(lines[2].StartsWith("myIntPointer1"));
        }
    }


    public class TestDebugger: IDebugger
    {
        private RegisterManager mRegMan = new RegisterManager();

        //public int Address = 0;
        public byte[] Data = new byte[] { 0xCD, 0xCD, 0xCD, 0xCD };        

        public RegisterManager RegManager
        {
            get { return mRegMan; }
        }

        public DebuggerStatus Status
        {
            get;
            set;
        }

        public byte[] GetTargetMemDump(int address, byte size)
        {
            var result = new byte[size];
            
            for (int i = 0; i < size; ++i)
            {
                result[i] = Data[i];
            }

            return result;
        }
    }
}
