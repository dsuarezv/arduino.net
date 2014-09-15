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
        private static Regex ListRegExpr = new Regex(@"0x([a-f0-9]+)\s\(location list\)");
        private static Regex ProgramRegExpr = new Regex(@"[0-9]+ byte block:\s[0-9a-f\s]*\(([\w\s:;]+)\)");
        
        private static Regex OpRegisterRegExpr = new Regex(@"DW_OP_reg(?<reg>[0-9]+)|DW_OP_regx:\s(?<reg>[0-9]+)");
        private static Regex OpRegisterOffsetRegExpr = new Regex(@"DW_OP_breg(?<reg>[0-9]+):\s+(?<offset>[0-9]+)");
        private static Regex OpAddress = new Regex(@"DW_OP_addr: ([0-9a-f]+)");

        private List<string> RawLocationProgram;

        public static DwarfLocation Get(DwarfTextParser parser, string locationString)
        {
            var result = new DwarfLocation();
            result.SetupLocationProgram(parser, locationString);

            return result;
        }


        public byte[] GetValue(Debugger debugger, DwarfBaseType type)
        {
            return RunProgram(debugger, type);
        }


        private void SetupLocationProgram(DwarfTextParser parser, string locationString)
        {
            // Check what kind of location string we have: 
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

            


            var refMatch = ListRegExpr.Match(locationString);
            if (refMatch.Success)
            {
                var locationId = refMatch.Groups[1].GetHexValue();
                parser.Locations.TryGetValue(locationId, out RawLocationProgram);
                return;
            }

            var programRegMatch = ProgramRegExpr.Match(locationString);
            if (programRegMatch.Success)
            {
                RawLocationProgram = DwarfParserLocation.GetProgramEntries(programRegMatch.Groups[1].Value);
                return;
            }            
        }


        // __ Program parsing _________________________________________________


        private byte[] RunProgram(Debugger debugger, DwarfBaseType type)
        {
            var result = new List<byte>();

            // Lists: 
            //    "DW_OP_reg24; DW_OP_piece: 1; DW_OP_reg25; DW_OP_piece: 1"
            //    ""
            //    ""
            //    ""
            //    ""
            //    ""
            //    ""
            //    ""

            foreach (var op in RawLocationProgram)
            {
                if (RegisterOp(op, debugger, result)) continue;
                if (AddressOp(op, debugger, result, type)) continue;
            }

            if (result.Count > 0) return result.ToArray();

            return null;
        }


        private bool RegisterOp(string op, Debugger debugger, List<byte> result)
        {
            var m = OpRegisterRegExpr.Match(op);
            if (!m.Success) return false;

            int register = m.Groups["reg"].GetIntValue();
            if (register < 0 || register > 32) return false;

            var registerName = string.Format("r{0:00}", --register);

            var val = debugger.Registers.Registers[registerName];

            result.Add((byte)val);

            return true;
        }

        private byte RegisterOffsetOp(string op)
        {
            throw new NotImplementedException();
        }

        private bool AddressOp(string op, Debugger debugger, List<byte> result, DwarfBaseType type)
        {
            var m = OpAddress.Match(op);
            if (!m.Success) return false;

            var address = m.Groups[1].GetIntValue();
            var size = (type != null) ? (byte)type.ByteSize : (byte)2;

            var value = debugger.GetTargetMemDump(address, size);

            result.AddRange(value);

            return true;
        }

    }
}
