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
        private List<BreakPointInfo> mBreakPoints = new List<BreakPointInfo>();
        private List<TracepointInfo> mTracepoints = new List<TracepointInfo>();
        private byte[] mTraceQueryBuffer;

        public event BreakPointDelegate BreakPointHit;
        public event BreakPointDelegate BreakPointAdded;
        public event BreakPointDelegate BreakPointRemoved;
        public event ByteDelegate SerialCharReceived;

        public List<BreakPointInfo> BreakPoints
        {
            get { return mBreakPoints; }
        }

        public List<TracepointInfo> TracePoints
        {
            get { return mTracepoints; }
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

        public BreakPointInfo AddBreakpoint(string sourceFile, int lineNumber)
        {
            var result = new BreakPointInfo() { LineNumber = lineNumber, SourceFileName = sourceFile, Id = mBreakPoints.Count };

            mBreakPoints.Add(result);

            if (BreakPointAdded != null) BreakPointAdded(this, result);

            return result;
        }

        public void RemoveBreakPoint(BreakPointInfo br)
        {
            if (!mBreakPoints.Remove(br)) return;

            if (BreakPointRemoved != null) BreakPointRemoved(this, br);
        }

        public List<BreakPointInfo> GetBreakpointsForFile(string fileName)
        {
            List<BreakPointInfo> result = new List<BreakPointInfo>();

            foreach (var br in mBreakPoints)
            {
                if (br.SourceFileName == fileName) result.Add(br);
            }

            return result;
        }


        // __ Debugger commands _______________________________________________


        public byte[] GetTargetMemDump(int address, byte size)
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


        private void SendTraceQuery(int address, byte size)
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
            
        }

        private void OnTargetBreak(int breakpointIndex)
        {
            if (breakpointIndex >= mBreakPoints.Count) return;

            var br = mBreakPoints[breakpointIndex];
            
            br.HitCount++;

            if (BreakPointHit != null) BreakPointHit(this, br);
        }

        private void OnTargetTraceAnswer(byte[] memdump)
        {
            mTraceQueryBuffer = memdump;
        }

        private void OnSerialCharReceived(byte b)
        {
            if (SerialCharReceived != null) SerialCharReceived(this, b);
        }
    }

    public delegate void BreakPointDelegate(object sender, BreakPointInfo breakpoint);

    public delegate void ByteDelegate(object sender, byte b);
}
