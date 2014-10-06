using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Threading;


namespace arduino.net
{
    public class Debugger: IDisposable
    {
        private string mComPort;
        private SerialPort mSerialPort;
        private BreakPointManager mBreakPoints = new BreakPointManager();
        private RegisterManager mRegisters = new RegisterManager();
        private ObservableCollection<Watch> mWatches = new ObservableCollection<Watch>();
        private ObservableCollection<TracepointInfo> mTracePoints = new ObservableCollection<TracepointInfo>();
        private ConcurrentQueue<byte> mReceivedCharsQueue = new ConcurrentQueue<byte>();
        private byte[] mTraceQueryBuffer;
        private DebuggerStatus mStatus = DebuggerStatus.Stopped;
        private BreakPointInfo mLastBreakPoint;


        public event TargetConnectedDelegate TargetConnected;
        public event BreakPointDelegate BreakPointHit;
        public event ByteDelegate SerialCharReceived;
        public event StatusChangedDelegate StatusChanged;


        public ConcurrentQueue<byte> ReceivedCharsQueue
        {
            get { return mReceivedCharsQueue; }
        }

        public BreakPointManager BreakPoints
        {
            get { return mBreakPoints; }
        }

        public ObservableCollection<TracepointInfo> TracePoints
        {
            get { return mTracePoints; }
        }

        public RegisterManager RegManager
        {
            get { return mRegisters; }
        }

        public ObservableCollection<Watch> Watches
        {
            get { return mWatches; }
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


        public Debugger(string comPort)
        {
            mComPort = comPort;
        }

        public void Attach()
        {
            if (mSerialPort != null) return;

            mSerialPort = new SerialPort(mComPort, 115200);
            mSerialPort.Open();

            LaunchReadingThread();
        }

        public void Detach()
        {
            SetStatus(DebuggerStatus.Stopped);

            if (mSerialPort == null) return;

            mSerialPort.Close();
        }

        public void Dispose()
        {
            mSerialPort.Dispose();
            mSerialPort = null;
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
            if (mSerialPort == null) Attach();

            mSerialPort.BaseStream.WriteByte(255);
            mSerialPort.BaseStream.WriteByte((byte)DebuggerPacketType.Continue);
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

        private void OnSerialCharReceived(byte b)
        {
            mReceivedCharsQueue.Enqueue(b);

            if (SerialCharReceived != null) 
            {
                //ThreadPool.QueueUserWorkItem( a => SerialCharReceived(this, b) );
                SerialCharReceived(this, b);
            }
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

    public delegate void ByteDelegate(object sender, byte b);

    public delegate void StatusChangedDelegate(object sender, DebuggerStatus newState);
    
    public enum DebuggerStatus
    { 
        Stopped, 
        Running, 
        Break
    }


    
    public class Watch
    {
        public string Name;


        public string GetValue()
        {
            return GetWatchValue(Name);
        }

        public string GetValue(string functionName)
        {
            return GetWatchValue(functionName, Name);
        }

        public string GetValue(DwarfSubprogram function)
        {
            return GetWatchValue(function, Name);
        }


        public static string GetWatchValue(string symbolName)
        {
            var pc = IdeManager.Debugger.RegManager.Registers["PC"];
            var function = IdeManager.Dwarf.GetFunctionAt(pc);
            if (function == null) return symbolName + ": <current context not found>\n";

            return GetWatchValue(function, symbolName);
        }

        public static string GetWatchValue(string functionName, string symbolName)
        {
            var function = IdeManager.Dwarf.GetFunctionByName(functionName);
            if (function == null) return symbolName + ": <context not found>\n";

            return GetWatchValue(function, symbolName);
        }

        public static string GetWatchValue(DwarfSubprogram function, string symbolName)
        {
            var symbol = IdeManager.Dwarf.GetSymbol(symbolName, function);
            if (symbol == null) return symbolName + ": <not in current context>\n";

            var val = symbol.GetValue(IdeManager.Debugger);
            if (val == null) return symbolName + ": <symbol has no location debug information>\n";

            return string.Format("{0}: {1}\n", symbolName, symbol.GetValueRepresentation(IdeManager.Debugger, val));
        }
    }
}
