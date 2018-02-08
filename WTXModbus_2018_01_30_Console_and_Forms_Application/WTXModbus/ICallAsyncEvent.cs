/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
 * 
 * Author : Felix Huettl 
 * 
 *  */

using System;
using System.ComponentModel;

namespace Hbm.Devices.WTXModbus
{
    /// <summary>
    /// This is the interface for the methods of the data communication between the classes of the Device and the GUI. 
    /// Besides the interface IDevice_Values, this interface is also derived from WTX120.
    /// It should contain an asynchronous communication allowing to read/write and update the GUI at the same time. In the class
    /// GUI the asynchronous call is started (after the button is clicked) and its corresponding callback method is implemented. 
    /// 
    /// In the method Asyn_Call(command_param, callback_param) an asynchronous invocation should be started. In the derived class
    /// from this interface a Backgroundworker is used (see also in class WTX120). Parameter "command_param" commits the command
    /// and "callback_param" commits the corresponding callback method in the GUI.
    /// 
    /// The methods Read_DoWork(sender,e) and Write_DoWork(sender,e) should call a method to read the register of the device(slave)
    /// in an asynchronous way (for example by the Backgroundworker, see also in class WTX120). 
    /// 
    /// The methods Write_Completed(sender,e) and Read_Completed(sender,e) commit the interface IDevice_Values via the callback method to
    /// the GUI (see also in class WTX120). 
    /// 
    /// The HandleDataEvent(sender,e) is used to get the data from class Modbus_TCP via an event-based call. In that way the data 
    /// is called from "e.Message" to the interface IDeviceValues. 
    /// 
    /// </summary>
    interface ICallAsyncEvent
    {

        void HandleDataEvent(object sender, MessageEvent e);

        void Async_Call(ushort command_param, Action<IDeviceValues> callback_param);

        void Read_DoWork(object sender, DoWorkEventArgs e);

        //IDevice_Values read_data(BackgroundWorker worker);

        void Read_Completed(object sender, RunWorkerCompletedEventArgs e);

        void Write_DoWork(object sender, DoWorkEventArgs e);

        void Write_Completed(object sender, RunWorkerCompletedEventArgs e);
    }
}
