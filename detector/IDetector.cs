using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysioDetectors
{
    public delegate void PhysData (object data);
    
    public interface IDetector
    {
        event PhysData OnData;

        void ConnectDevice();
        void ConnectDevice(string port);
        bool CheckConnection();

        void Dispose();
    }
}
