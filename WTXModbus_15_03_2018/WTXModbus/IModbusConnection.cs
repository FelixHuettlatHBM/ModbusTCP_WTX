using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTXModbus
{
        public interface IModbusConnection
        {
            bool is_connected { get; }
            ushort StartAdress { get; set; }
            ushort getNumOfPoints { get; set; }
            string IP_Adress { get; set; }
            int Sending_interval { get; set; }
            int Port { get; set; }

            ushort[] getPreviousData { get; }

            void Connect();

            void Read();

            void Write(ushort index, ushort data);

            void Write(ushort index, ushort[] data);

            void ResetDevice();     

            event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent;
        }
}


