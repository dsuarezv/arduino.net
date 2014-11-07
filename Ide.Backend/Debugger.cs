using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Threading;


namespace arduino.net
{
    public class Debugger: IDisposable, IDebugger
    {
        private int mDebuggerBytesCount = 0;
        private string mComPort;
        private SerialPort mSerialPort;
        private BreakPointManager mBreakPoints = new BreakPointManager();
        private RegisterManager mRegisters = new RegisterManager();
        private ConcurrentQueue<byte> mReceivedCharsQueue = new ConcurrentQueue<byte>();
        private BlockingCollection<CaptureData> mReceivedCapturesQueue = new BlockingCollection<CaptureData>(new ConcurrentQueue<CaptureData>());
        private byte[] mTraceQueryBuffer;
        private DebuggerStatus mStatus = DebuggerStatus.Stopped;
        private BreakPointInfo mLastBreakPoint;


        public event TargetConnectedDelegate TargetConnected;
        public event BreakPointDelegate BreakPointHit;
        public event ByteDelegate SerialCharReceived;
        public event StatusChangedDelegate StatusChanged;
        public event CaptureAnswerReceivedDelegate CaptureReceived;

        public string ComPort
        {
            get 
            { 
                return mComPort; 
            }
            set
            {
                if (mComPort == value) return;

                Detach();

                mComPort = value;
            }
        }

        public ConcurrentQueue<byte> ReceivedCharsQueue
        {
            get { return mReceivedCharsQueue; }
        }

        public BlockingCollection<CaptureData> ReceivedCapturesQueue
        {
            get { return mReceivedCapturesQueue; }
        }

        public BreakPointManager BreakPoints
        {
            get { return mBreakPoints; }
        }

        public RegisterManager RegManager
        {
            get { return mRegisters; }
        }

        public DebuggerStatus Status
        {
            get { return mStatus; }
        }

        public bool IsAttached
        {
            get { return mSerialPort != null; }
        }

        public BreakPointInfo LastBreakpoint
        {
            get { return mLastBreakPoint; }
        }


        // __ Public API ______________________________________________________


        public string[] GetAvailableComPorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Run()
        {
            switch (Status)
            {
                case DebuggerStatus.Break:
                    TargetContinue();
                    break;

                case DebuggerStatus.Running:
                    break;

                case DebuggerStatus.Stopped:
                    IdeManager.Debugger.Attach();
                    IdeManager.Debugger.TargetContinue();
                    break;
            }
        }

        public void Stop()
        {
            Detach();
        }


        private void Attach()
        {
            if (mSerialPort != null) return;

            mSerialPort = new SerialPort(mComPort, 115200);
            mSerialPort.Open();

            LaunchReadingThread();
        }

        private void Detach()
        {
            SetStatus(DebuggerStatus.Stopped);

            if (mSerialPort == null) return;

            mSerialPort.Close();
        }

        public void Dispose()
        {
            mReceivedCapturesQueue.Dispose();

            DisposeSerial();
        }

        private void DisposeSerial()
        {
            mSerialPort.Dispose();
            mSerialPort = null;
        }

        public void TouchProjectFilesAffectedByDebugging(Project p)
        {
            List<string> filesToTouch = new List<string>();

            filesToTouch.Add(p.GetSketchFileName());

            foreach (var bi in mBreakPoints.BreakPoints)
            {
                var f = Path.Combine(p.ProjectPath, bi.SourceFileName);
                if (!filesToTouch.Contains(f)) filesToTouch.Add(f);
            }

            foreach (var f in filesToTouch)
            {
                if (File.Exists(f)) File.SetLastWriteTime(f, DateTime.Now);
            }
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

        private void TargetContinue()
        {
            if (mSerialPort == null) Attach();

            mSerialPort.BaseStream.WriteByte(255);
            mSerialPort.BaseStream.WriteByte((byte)DebuggerPacketType.Continue);
            mDebuggerBytesCount += 2;
            SetStatus(DebuggerStatus.Running);
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
            catch (Exception ex)
            {
                DisposeSerial();
            }
        }

        private enum DebuggerPacketType 
        { 
            Connect = 254,
            Break = 253,
            TraceQuery = 250,
            TraceAnswer = 249,
            CaptureAnswer = 248,
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

            var typeByte = r.ReadByte();
            DebuggerPacketType type = (DebuggerPacketType)typeByte;
            mDebuggerBytesCount += 2;

            switch (type)
            {
                case DebuggerPacketType.Connect: 
                    OnTargetInit(); 
                    break;
                
                case DebuggerPacketType.Break:
                    int breakpointIndex = r.ReadByte();
                    mRegisters.UpdateRegisters(r.ReadBytes(RegisterManager.PacketSize));
                    mDebuggerBytesCount += RegisterManager.PacketSize;
                    OnTargetBreak(breakpointIndex); 
                    break;
                
                case DebuggerPacketType.TraceAnswer:
                    int size = r.ReadByte();
                    if (size == 0) break;
                    byte[] memdump = r.ReadBytes(size);
                    mDebuggerBytesCount += size + 1;
                    OnTargetTraceAnswer(memdump);
                    break;

                case DebuggerPacketType.CaptureAnswer:
                    int id = r.ReadByte();
                    int value = r.ReadInt32();
                    mDebuggerBytesCount += 5;
                    OnTargetCaptureAnswer(id, value);
                    break;

                default: 
                    // It was not a packet for the debugger. Send to the char receiver the bytes consumed so far.
                    OnSerialCharReceived(b);
                    OnSerialCharReceived(typeByte);
                    mDebuggerBytesCount -= 2;
                    return;
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
            mDebuggerBytesCount += p.Length;
        }


        private void OnTargetInit()
        {
            SetStatus(DebuggerStatus.Running);

            if (TargetConnected != null)
            {
                ThreadPool.QueueUserWorkItem((a) => TargetConnected(this));
            }
        }

        private void OnTargetBreak(int breakpointId)
        {
            BreakPointInfo br = null;

            SetStatus(DebuggerStatus.Break);

            br = mBreakPoints[breakpointId];
            if (br != null) br.HitCount++;

            mLastBreakPoint = br;

            if (BreakPointHit != null) 
            {
                ThreadPool.QueueUserWorkItem( a => BreakPointHit(this, br) );
            }
        }

        private void OnTargetTraceAnswer(byte[] memdump)
        {
            mTraceQueryBuffer = memdump;
        }

        private void OnTargetCaptureAnswer(int id, int value)
        {
            mReceivedCapturesQueue.Add(new CaptureData() { TimeStamp = DateTime.Now, Id = id, Value = value });

            if (CaptureReceived == null) return;

            CaptureReceived(this, id, value);
        }

        private void OnSerialCharReceived(byte b)
        {
            mReceivedCharsQueue.Enqueue(b);

            if (SerialCharReceived == null) return;

            SerialCharReceived(this, b);
        }


        // __ Status __________________________________________________________


        private void SetStatus(DebuggerStatus status)
        {
            if (mStatus != status)
            { 
                if (StatusChanged != null) ThreadPool.QueueUserWorkItem( a => StatusChanged(this, status) );
            }

            mStatus = status;
        }
    }


    public delegate void TargetConnectedDelegate(object sender);

    public delegate void BreakPointDelegate(object sender, BreakPointInfo breakpoint);

    public delegate void BreakpointMovedDelegate(object sender, BreakPointInfo breakpoint, int oldBreakpointLine);

    public delegate void ByteDelegate(object sender, byte b);

    public delegate void StatusChangedDelegate(object sender, DebuggerStatus newState);
    
    public delegate void CaptureAnswerReceivedDelegate(object sender, int captureId, int value);

    public enum DebuggerStatus
    { 
        Stopped, 
        Running, 
        Break
    }

    
    public interface IDebugger
    {
        RegisterManager RegManager { get; }
        DebuggerStatus Status { get; }

        byte[] GetTargetMemDump(Int32 address, byte size);
    }
}
