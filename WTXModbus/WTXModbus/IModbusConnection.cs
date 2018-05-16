using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTXModbus
{
    /* 
     * This interface presets the methods and auto-properties(set and get)used for establishing a connection via Modbus/TCP to the WTX device,
     * reading from the WTX and writing to the WTX weighting terminal device.
     * The methods and auto-properties are implemented by class 'ModbusConnection'. 
     * 
     * The interface has methods for the connection, for reading, writing(a ushort on a single register and an ushort[]array on multiple registers),
     * for the closing (method 'ResetDevice()') and an eventHandler 'RaiseDataEvent'.
     * 
     * 'RaiseDataEvent' is triggered as soons as the data has been from the WTX register/s. It is callled in class 'WTX120' (method 'UpdateEvent(..)')
     * to convert the ushort values to string values and print them afterwards on the GUI or console.
     */
        public interface IModbusConnection
        {
            event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent;

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
        }
}


