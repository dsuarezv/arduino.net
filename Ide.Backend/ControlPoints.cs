using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class ControlPointInfo
    {
        public int LineNumber = 1;
        public string SourceFileName = "default.cpp";        
    }

    public class BreakpointInfo: ControlPointInfo
    {
        public int HitCount = 0;
    }

    
    public class TracepointInfo: ControlPointInfo
    {
        public string SymbolToTrace;
    }
}
