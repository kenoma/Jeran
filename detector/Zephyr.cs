using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PhysioDetectors
{
    public class Zephyr : IDetector,IDisposable
    {
        #region Constants
        const ushort STXPositionOffset = 0;
        const ushort MessageIDLocationOffset = 1;
        const ushort DLCLocationOffset = 2;
        const ushort ETXPositionOffset = 59;
        const ushort CRCOffset = 58;
        
        const short PacketSize = 60;
        const short STX = 2;
        const short MessageID = 38;
        const short DLC = 55;
        const short ETX = 3;
        #endregion

        
        SerialPort zport;
        Thread stayalive;

        event PhysData OnData;
        event PhysData IDetector.OnData
        {
            add
            {
                OnData += value;
            }
            remove
            {

                OnData -= value;
            }
        }
      

        public void ConnectDevice()
        {
            string[] coms = SerialPort.GetPortNames();

            Console.WriteLine("Checking ports for HxM:");
            Stopwatch sw = new Stopwatch();
            foreach (string com in coms)
            {
                Console.Write(com);

                SerialPort port = new SerialPort(com);
                port.WriteTimeout = 500;
                port.ReadTimeout = 500;
                port.BaudRate = 115200;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.Parity = Parity.None;
                port.Open();
                sw.Restart();
                while (port.BytesToRead != PacketSize && sw.ElapsedMilliseconds < 2000)
                { }

                if (port.BytesToRead != 0)
                {
                    int numberOfBytesInBuffer = port.BytesToRead;
                    byte[] bufferStore = new byte[numberOfBytesInBuffer];
                    port.Read(bufferStore, 0, numberOfBytesInBuffer);
                    if (IsStructureValid(bufferStore))
                        Console.WriteLine("... done!");

                    zport = port;
                    zport.DataReceived += DataReceived;
                    stayalive = new Thread(() =>
                    {
                        while (true)
                        {
                            CheckConnection();
                            Thread.Sleep(3000);
                        }
                    });
                    stayalive.Start();
                    break;
                }
                else
                    Console.WriteLine("...none");
                port.Close();

            }

        }

        public void ConnectDevice(string com)
        {
            try
            {
                Console.Write(com);
                Stopwatch sw = new Stopwatch();
                SerialPort port = new SerialPort(com);
                port.WriteTimeout = 500;
                port.ReadTimeout = 500;
                port.BaudRate = 115200;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.Parity = Parity.None;
                port.Open();
                sw.Restart();
                while (port.BytesToRead != PacketSize && sw.ElapsedMilliseconds < 2000)
                { }

                if (port.BytesToRead != 0)
                {
                    int numberOfBytesInBuffer = port.BytesToRead;
                    byte[] bufferStore = new byte[numberOfBytesInBuffer];
                    port.Read(bufferStore, 0, numberOfBytesInBuffer);
                    if (IsStructureValid(bufferStore))
                        Console.WriteLine("... done!");

                    zport = port;
                    zport.DataReceived += DataReceived;
                    stayalive = new Thread(() =>
                    {
                        while (true)
                        {
                            CheckConnection();
                            Thread.Sleep(3000);
                        }
                    });
                    stayalive.Start();
                }
                else
                {
                    Console.WriteLine("...none");
                    port.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool CheckConnection()
        {
            try
            {
                if (zport == null)
                {
                    Console.WriteLine("Port is null");
                    return false;
                }

                if (zport.IsOpen)
                    return true;
                else
                    if (!string.IsNullOrEmpty(zport.PortName))
                    {
                        Console.WriteLine("Try to reopen port {0}", zport.PortName);
                        zport.Open();
                        if (zport.IsOpen)
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
            }
            catch { }
            return false;
        }

        void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = (SerialPort)sender;
            if (port.BytesToRead >= 60)
            {
                byte[] buffer = new byte[60];
                port.Read(buffer, 0, 60);
                if (IsStructureValid(buffer))
                {
                    HXMdata data = ByteArrayToHXMdata(buffer);
                    //Console.WriteLine(data.RR(0));
                    if (OnData != null && CheckDataValues(data) && buffer[CRCOffset] == CalculateCRC(buffer))
                        OnData(data);
                }
                else
                    port.Close();
            }
        }

        HXMdata ByteArrayToHXMdata(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            HXMdata stuff = (HXMdata)Marshal.PtrToStructure(
                handle.AddrOfPinnedObject(), typeof(HXMdata));
            handle.Free();
            return stuff;
        }

        public void Dispose()
        {
            if (zport != null)
                zport.Close();
            if (stayalive != null && stayalive.IsAlive)
                stayalive.Abort();

        }

        #region ValidationStructure
        static bool IsStructureValid(byte[] bufferData)
        {
            return IsPacketLengthValid(bufferData)
                    && IsSTXMarkerPresent(bufferData)
                    && IsMessageIDPresent(bufferData)
                    && IsDLCMarkerPresent(bufferData)
                    && IsETXMarkerPresent(bufferData);
        }

        private static bool IsETXMarkerPresent(byte[] bufferData)
        {
            if (bufferData[ETXPositionOffset] != ETX)
            {
                var faultDescription = "Invalid ETX was received. Actual Value was " +
                                        bufferData[ETXPositionOffset] + "." +
                                        "Expected value was " + ETX;
                Console.WriteLine(faultDescription);
                return false;
            }

            return true;
        }

        private static bool IsDLCMarkerPresent(byte[] bufferData)
        {
            if (bufferData[DLCLocationOffset] != DLC)
            {
                var faultDescription = "Invalid DLC was received. Actual Value was " +
                                        bufferData[DLCLocationOffset] + "." +
                                        "Expected value was " + DLC;
                Console.WriteLine(faultDescription);
                return false;
            }

            return true;
        }

        private static bool IsMessageIDPresent(byte[] bufferData)
        {
            if (bufferData[MessageIDLocationOffset] != MessageID)
            {
                var faultDescription = "Invalid MessageID was received. Actual Value was " +
                                        bufferData[MessageIDLocationOffset] + "." +
                                        "Expected value was " + MessageID;
                Console.WriteLine(faultDescription);
                return false;
            }

            return true;
        }

        private static bool IsSTXMarkerPresent(byte[] bufferData)
        {
            if (bufferData[STXPositionOffset] != STX)
            {
                var faultDescription = "Invalid Start of Packet Value received. Actual Value was " +
                                        bufferData[STXPositionOffset] + "." +
                                        "Expected value was " + STX;

                Console.WriteLine(faultDescription);
                return false;
            }

            return true;
        }

        private static bool IsPacketLengthValid(byte[] bufferData)
        {
            if (bufferData.Length != PacketSize)
            {
                var faultDescription = "Packet was received with invalid length. Actual length was " + bufferData.Length
                                       + " expected length was " + PacketSize;
                Console.WriteLine(faultDescription);
                return false;
            }

            return true;
        }

        private byte CalculateCRC(byte[] buffer)
        {
            byte currentChecksum = (byte)0;

            for (int index = 3; index < CRCOffset; index++)
            {
                currentChecksum = ChecksumPushByte(currentChecksum, buffer.ElementAt(index));
            }

            return currentChecksum;
        }

        private byte ChecksumPushByte(byte currentChecksum, byte newByte)
        {
            currentChecksum ^= newByte;
            for (var index = 0; index < 8; ++index)
            {
                if ((currentChecksum & 1) == 1)
                {
                    currentChecksum = (byte)(currentChecksum >> 1 ^ 140);
                }
                else
                {
                    currentChecksum >>= 1;
                }
            }

            return currentChecksum;
        }
        
        #endregion

        #region ValidationRanges


        public static bool CheckDataValues(HXMdata data)
        {
            return BatteryValueRange(data) &&
                   HeartRateValueRange(data) &&
                   StridesValueRange(data);
        }

        private static bool BatteryValueRange(HXMdata data)
        {
            if (data.BatteryCharge > 100)
            {
                Console.WriteLine("Battery Charge Reading our of range. Expected value was less than 100. Actuall was {0}", data.BatteryCharge);
                return false;
            }

            return true;
        }

        private static bool HeartRateValueRange(HXMdata data)
        {
            const byte Minimum = 30;
            const byte Maximum = 240;

            if (data.HeartRate  < Minimum ||
                data.HeartRate > Maximum)
            {
                Console.WriteLine("Heart Rate Reading is out of range. Expected value was between 30 and 240. Actuall was {0}", data.HeartRate);
                return false;
            }

            return true;
        }

        private static bool StridesValueRange(HXMdata data)
        {
            const ushort Maximum = 128;

            if (data.Strides > Maximum)
            {
                Console.WriteLine("Strides Reading is out of range. Expected value was less than 255. Actuall was {0}", data.Strides);
                return false;
            }

            return true;
        }

        #endregion

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HXMdata
    {
        public byte STX;
        public byte HxM;
        public byte DLC;

        public ushort FirmwareID;
        public ushort FirmwareVer;
        public ushort HardwareID;
        public ushort HardwareVer;
        public byte BatteryCharge;
        public byte HeartRate;
        public byte HeartBeatNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public ushort[] HeartBeatTimestamp;
        public ushort Reserv_1;
        public ushort Reserv_2;
        public ushort Reserv_3;
        public ushort Distance;
        public ushort InstSpeed;
        public byte Strides;
        public byte Reserv_4;
        public ushort Reserv_5;
        public byte CRC;
        public byte ETX;

        public int RR(int t)
        {
            if (t + 1 < HeartBeatTimestamp.Length)
            {
                if (HeartBeatTimestamp[t + 1] < HeartBeatTimestamp[t])
                    return HeartBeatTimestamp[t] - HeartBeatTimestamp[t + 1];
                else
                    return (65535 - HeartBeatTimestamp[t + 1]) + HeartBeatTimestamp[t];
            }
            return 0;
        }
    }
}
