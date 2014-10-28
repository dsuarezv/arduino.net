using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace arduino.net
{
    public class DwarfLocation
    {
        // Summary of location programs supported: 
        // Reference to a location list: 
        //    "0x0	(location list)"
        //    "0x18	(location list)"
        // Direct value: 
        //    "5 byte block: 3 7 1 80 0 	(DW_OP_addr: 800107)"
        //    "5 byte block: 3 0 0 0 0 	(DW_OP_addr: 0)"
        //    "2 byte block: 23 0 	(DW_OP_plus_uconst: 0)"
        //    "2 byte block: 23 1 	(DW_OP_plus_uconst: 1)"
        //    "2 byte block: 23 2 	(DW_OP_plus_uconst: 2)"
        //    "2 byte block: 8c 4 	(DW_OP_breg28: 4)"
        //    "2 byte block: 90 20 	(DW_OP_regx: 32)"
        //    "5 byte block: 3 1a 1 80 0 	(DW_OP_addr: 80011a)"
        //    "6 byte block: 66 93 1 67 93 1 	(DW_OP_reg22; DW_OP_piece: 1; DW_OP_reg23; DW_OP_piece: 1)"
        //    "6 byte block: 64 93 1 65 93 1 	(DW_OP_reg20; DW_OP_piece: 1; DW_OP_reg21; DW_OP_piece: 1)"
        //    "2 byte block: 23 5 	(DW_OP_plus_uconst: 5)"

        private static Regex LocationReferenceRegExpr = new Regex(@"0x([a-f0-9]+)\s\(location list\)");
        private static Regex InlineLocationRegExpr = new Regex(@"[0-9]+ byte block:\s[0-9a-f\s]*\(([\w\s:;]+)\)");
        
        private static Regex OpRegisterRegExpr = new Regex(@"DW_OP_reg(?<reg>[0-9]+)|DW_OP_regx:\s(?<reg>[0-9]+)");
        private static Regex OpRegisterOffsetRegExpr = new Regex(@"DW_OP_breg(?<reg>[0-9]+):\s+(?<offset>[0-9]+)");
        private static Regex OpConstantOffsetRegExpr = new Regex(@"DW_OP_plus_uconst: (?<offset>[0-9]+)");
        private static Regex OpAddressRegExpr = new Regex(@"DW_OP_addr: ([0-9a-f]+)");

        internal List<string> RawLocationProgram;

        public DwarfLocation()
        { 
            
        }

        public DwarfLocation(string rawLocationProgram)
        {
            RawLocationProgram = DwarfParserLocation.GetProgramEntries(rawLocationProgram);
        }
        
        public static DwarfLocation Get(DwarfTextParser parser, string locationString)
        {
            var result = new DwarfLocation();
            result.SetupLocationProgram(parser, locationString);

            return result;
        }


        public byte[] GetValue(IDebugger debugger, DwarfBaseType type)
        {
            return RunProgram(debugger, type);
        }

        public byte[] GetValue(byte[] buffer, DwarfBaseType type)
        {
            return RunProgram(buffer, type);
        }
        

        public void SetupLocationProgram(DwarfTextParser parser, string locationString)
        {
            var locationReferenceMatch = LocationReferenceRegExpr.Match(locationString);
            if (locationReferenceMatch.Success)
            {
                var locationId = locationReferenceMatch.Groups[1].GetHexValue();
                parser.Locations.TryGetValue(locationId, out RawLocationProgram);
                return;
            }

            var inlineLocationMatch = InlineLocationRegExpr.Match(locationString);
            if (inlineLocationMatch.Success)
            {
                RawLocationProgram = DwarfParserLocation.GetProgramEntries(inlineLocationMatch.Groups[1].Value);
                return;
            }            
        }


        // __ Program parsing _________________________________________________


        private byte[] RunProgram(IDebugger debugger, DwarfBaseType type)
        {
            var result = new List<byte>();

            foreach (var op in RawLocationProgram)
            {
                if (RegisterOp(op, debugger, result)) continue;
                if (AddressOp(op, debugger, result, type)) continue;
                if (RegisterOffsetOp(op, debugger, result, type)) continue;
            }

            return result.ToArray();
        }

        private bool RegisterOp(string op, IDebugger debugger, List<byte> result)
        {
            var m = OpRegisterRegExpr.Match(op);
            if (!m.Success) return false;

            int register = m.Groups["reg"].GetIntValue();
            if (register < 1 || register > 32) return false;

            var registerName = string.Format("r{0:00}", register);
            var val = debugger.RegManager.Registers[registerName];

            result.Add((byte)val);

            return true;
        }

        private bool RegisterOffsetOp(string op, IDebugger debugger, List<byte> result, DwarfBaseType type)
        {
            var m = OpRegisterOffsetRegExpr.Match(op);
            if (!m.Success) return false;

            int register = m.Groups["reg"].GetIntValue();
            if (register < 1 || register > 32) return false;

            string addressRegister;

            switch (register)
            { 
                case 26: addressRegister = "X"; break;
                case 28: addressRegister = "Y"; break;
                case 30: addressRegister = "Z"; break;
                default: return false;
            }

            var offset = m.Groups["offset"].GetIntValue();
            var stackPointer = debugger.RegManager.Registers[addressRegister];
            var address = stackPointer + offset;
            var size = (type != null) ? (byte)type.ByteSize : (byte)2;
            var value = debugger.GetTargetMemDump(address, size);

            result.AddRange(value);

            return true;
        }

        private bool AddressOp(string op, IDebugger debugger, List<byte> result, DwarfBaseType type)
        {
            var m = OpAddressRegExpr.Match(op);
            if (!m.Success) return false;

            var address = m.Groups[1].GetHexValue();
            var size = (type != null) ? (byte)type.ByteSize : (byte)2;

            var value = debugger.GetTargetMemDump(address, size);

            result.AddRange(value);

            return true;
        }


        // __ Struct program parsing __________________________________________

        
        private byte[] RunProgram(byte[] buffer, DwarfBaseType type)
        {
            var result = new List<byte>();

            foreach (var op in RawLocationProgram)
            {
                if (ConstantOffsetOp(op, buffer, result, type)) continue;
            }

            return result.ToArray();
        }

        private bool ConstantOffsetOp(string op, byte[] buffer, List<byte> result, DwarfBaseType type)
        {
            // this is DW_TAG_member element. It describes members in a struct, so they represent an offset from a 
            // known address (that of the parent).

            var m = OpConstantOffsetRegExpr.Match(op);
            if (!m.Success) return false;

            var offset = m.Groups["offset"].GetIntValue();
            var size = type.ByteSize;

            if (offset == -1) return false;

            for (int i = 0; i < size; ++i)
            {
                result.Add(buffer[i + offset]);
            }

            return true;
        }


    }
}
