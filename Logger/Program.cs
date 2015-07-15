using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhysioDetectors;

namespace Logger
{
    class Program
    {
        static void Main(string[] args)
        {
            IDetector zeph = new Zephyr();
            zeph.OnData += zeph_OnData;
            zeph.ConnectDevice("COM5");

            Console.Read();
        }

        static void zeph_OnData(object data)
        {
           
        }
    }
}
