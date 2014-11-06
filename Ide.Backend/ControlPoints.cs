using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    [Serializable]
    public class ControlPointInfo
    {
        public int Id = 0;
        public int LineNumber = 1;
        public string SourceFileName = "default.cpp";
        public DateTime LastEditDate = DateTime.Now;

        public bool IsDeployedOnDevice(Compiler compiler)
        {
            return (LastEditDate < compiler.LastSuccessfulDeploymentDate);
        }
    }

    [Serializable]
    public class BreakPointInfo: ControlPointInfo
    {
        public int HitCount = 0;
    }
}
