using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arduino.net
{
    public class ComportAdapter
    {
        public static ConfigSection Get(string[] listOfPorts)
        {
            var result = new ConfigSection();

            foreach (string port in listOfPorts)
            {
                result.GetSub(port)["name"] = port;
            }

            return result;
        }
    }
}
