using Hbm.Wt.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hbm.Wt.CommonNetLib.Utils
{
    public interface IModbusConnection
    {
        bool is_connected { get; }
        ushort StartAdress { get; set; }
        ushort getNumOfPoints { get; set; }
        string IP_Adress { get; set; }
        int Sending_interval { get; set; }
        int Port { get; set; }

        void Connect();

        void Read();

        void Write(ushort index, ushort data);

        void Write(ushort index, ushort[] data);

        void ResetDevice();     // = Close()

        //event EventHandler<MessageEvent<ushort>> RaiseDataEvent;
        event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent;


    }
}
