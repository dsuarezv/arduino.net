using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace arduino.net
{
    public interface ICaptureMonitor
    {
        string MonitorName { get; }

        void Setup(CapturePointInfo capture);
    }
}
