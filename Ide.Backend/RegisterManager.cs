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

            UpdateWordRegister("SP", 35, 36);
            UpdateWordRegister("PC", 33, 34);
        }

        public void UpdateWordRegister(string name, int indexLow, int indexHigh)
        {
            mRegisters[name] = (int)( (mRegistersPacket[indexHigh] << 8) | mRegistersPacket[indexLow]);
        }
    }

}
