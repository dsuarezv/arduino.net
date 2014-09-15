using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;


namespace arduino.net
{
    public class Debugger: IDisposable
    {
        private SerialPort mSerialPort;
        private BreakPointManager mBreakPoints = new BreakPointManager();
        private RegisterManager mRegisters = new RegisterManager();
        private List<TracepointInfo> mTracepoints = new List<TracepointInfo>();
        private byte[] mTraceQueryBuffer;
        private bool mIsTargetRunning = true;

        public event TargetConnectedDelegate TargetConnected;
        public event BreakPointDelegate BreakPointHit;
        public event ByteDelegate SerialCharReceived;

        public BreakPointManager BreakPoints
        {
            get { return mBreakPoints; }
        }

        public List<TracepointInfo> TracePoints
        {
            get { return mTracepoints; }
        }

        public RegisterManager Registers
        {
            get { return mRegisters; }
        }

        public bool IsTargetRunning
        {
            get { return mIsTargetRunning; }
        }


        public Debugger(string comPort)
        {
            mSerialPort = new SerialPort(comPort, 115200);

            Initialize();
        }

        public void Initialize()
        {
            mSerialPort.Open();

            LaunchReadingThread();
        }

        public void Dispose()
        {
            mSerialPort.Dispose();
        }

       
        // __ Debugger commands _______________________________________________


        public byte[] GetTargetMemDump(Int32 address, byte size)
        {
            mTraceQueryBuffer = null;

            SendTraceQuery(address, size);

            while (mTraceQueryBuffer == null)
            {
                Thread.Sleep(50);
            }

            return mTraceQueryBuffer;
        }


        public void TargetContinue()
        {
            mSerialPort.BaseStream.WriteByte(255);
            mSerialPort.BaseStream.WriteByte((byte)DebuggerPacketType.Continue);
            mIsTargetRunning = true;
        }


        // __ Reading impl ____________________________________________________


        private void LaunchReadingThread()
        {
            Thread t = new Thread(new ThreadStart(ReadWorker));
            t.Name = "SerialPort read";
            t.IsBackground = true;
            t.Start();
        }

        private void ReadWorker()
        {
            try
            {
                using (BinaryReader r = new BinaryReader(mSerialPort.BaseStream))
                {
                    while (true)
                    {
                        ProcessPacket(r);
                    }
                }
            }
            catch (IOException)
            {
                Dispose();
            }
        }

        private enum DebuggerPacketType 
        { 
            Connect = 254,
            Break = 253,
            TraceQuery = 250,
            TraceAnswer = 249,
            Continue = 230
        }

        private void ProcessPacket(BinaryReader r)
        {
            byte b = r.ReadByte();

            if (b != 255)
            {
                OnSerialCharReceived(b);
                return;
            }

            DebuggerPacketType type = (DebuggerPacketType)r.ReadByte();

            switch (type)
            {
                case DebuggerPacketType.Connect: 
                    OnTargetInit(); 
                    break;
                
                case DebuggerPacketType.Break:
                    int breakpointIndex = r.ReadByte();
                    mRegisters.UpdateRegisters(r.ReadBytes(RegisterManager.PacketSize));
                    OnTargetBreak(breakpointIndex); 
                    break;
                
                case DebuggerPacketType.TraceAnswer:
                    int size = r.ReadByte();
                    if (size == 0) break;
                    byte[] memdump = r.ReadBytes(size);
                    OnTargetTraceAnswer(memdump);
                    break;

                default: 
                    break;
            }
        }


        // __ Debugger impl ___________________________________________________


        private void SendTraceQuery(Int32 address, byte size)
        {
            var p = new byte[7];

            p[0] = 255;
            p[1] = (byte)DebuggerPacketType.TraceQuery;
            //p[2] = (byte)(address >> 24);               // BigEndian
            //p[3] = (byte)(address >> 16);
            //p[4] = (byte)(address >> 8);
            //p[5] = (byte)(address);
            p[5] = (byte)(address >> 24);               // LittleEndian
            p[4] = (byte)(address >> 16);
            p[3] = (byte)(address >> 8);
            p[2] = (byte)(address);
            p[6] = size;

            mSerialPort.Write(p, 0, p.Length);
        }


        private void OnTargetInit()
        {
            mIsTargetRunning = true;

            if (TargetConnected != null)
            {
                ThreadPool.QueueUserWorkItem((a) => TargetConnected(this));
            }
        }

        private void OnTargetBreak(int breakpointId)
        {
            BreakPointInfo br = null;

            mIsTargetRunning = false;

            if (breakpointId < mBreakPoints.Count)
            { 
                br = mBreakPoints[breakpointId];
                br.HitCount++;
            }

            if (BreakPointHit != null) 
            {
                ThreadPool.QueueUserWorkItem((a) => BreakPointHit(this, br));
            }
        }

        private void OnTargetTraceAnswer(byte[] memdump)
        {
            mTraceQueryBuffer = memdump;
        }

        private void OnSerialCharReceived(byte b)
        {
            if (SerialCharReceived != null) 
            {
                ThreadPool.QueueUserWorkItem((a) => SerialCharReceived(this, b));
            }
        }
    }

    public delegate void TargetConnectedDelegate(object sender);

    public delegate void BreakPointDelegate(object sender, BreakPointInfo breakpoint);

    public delegate void ByteDelegate(object sender, byte b);
}
