/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
 * 
 * Author : Felix Huettl 
 * 
 *  */


namespace Hbm.Devices.WTXModbus
{

    /// <summary>
    /// This interface contains the basic methods to realize a communication between a device and its master.
    /// 
    /// Method "Connect()" should establish a connection to the device and method "Is_connected()" should return 
    /// whether there is a successful connection to the device or not.
    /// 
    /// The method ReadRegister() should read the register from the device. 
    /// The method WriteRegister(data) should write data to the register. 
    /// The method Close() should close the connection to the device.
    /// 
    /// There are several more ways to implement this, therefore another interface "Communication_Device_Interface" 
    /// is derived from this interface to define the methods more precise. 
    /// 
    /// </summary>
    interface ICommunication
    {
        void Connect();

        bool is_connected{ get;}

        void ReadRegister();

        void WriteRegister(ushort data);

        void Close();
    }
}
