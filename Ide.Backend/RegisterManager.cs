using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class RegisterManager
    {
        public const int PacketSize = 38;


        private byte[] mRegistersPacket;
        private SortedDictionary<string, int> mRegisters;
        
        public SortedDictionary<string, int> Registers
        {
            get { return mRegisters; }
        }

        public RegisterManager()
        { 
            mRegisters = new SortedDictionary<string, int>();
        }

        public void UpdateRegisters(byte[] registersPacket)
        {
            if (registersPacket == null || registersPacket.Length != PacketSize) return;

            mRegistersPacket = registersPacket;

            mRegisters["SREG"] = mRegistersPacket[0];

            for (int i = 0; i < 32; ++i)
            {
                mRegisters["r" + i.ToString("00")] = mRegistersPacket[i + 1];
            }

            // TODO: on the atmega2560, the PC is 3 bytes. Have to account for that here and in the MCU side.
            UpdateWordRegister("SP", 35, 36);
            //UpdateWordRegister("PC", 34, 33);
            UpdatePc(33, 34);

            UpdateWordRegister("X", 27, 28);
            UpdateWordRegister("Y", 29, 30);
            UpdateWordRegister("Z", 31, 32);
        }

        private void UpdateWordRegister(string name, int indexLow, int indexHigh)
        {
            mRegisters[name] = (int)( (int)(mRegistersPacket[indexHigh]) << 8 | mRegistersPacket[indexLow]);
        }

        private void UpdatePc(int indexLow, int indexHigh)
        {
            // Aparently the program counter is different (atmega328P). 
            // It is stored in opposite order to the other registers.
            // Also, it is stored with the last bit stripped. So it has to 
            // be left-shifted once to get the real address (multiply x2).
            // The reasoning is that all instructions are stored in 2 or 4 bytes.
            int high = mRegistersPacket[indexLow] << 8;
            int low = mRegistersPacket[indexHigh];
            int val = (high | low) << 1;
            mRegisters["PC"] = val;
        }
    }

}
