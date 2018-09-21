using HBM.WT.API.WTX.Modbus;
using Newtonsoft.Json.Linq;
using System;

namespace HBM.WT.API  
{
    /// <summary>
    /// Define the common communication-interface
    /// </summary>
    public interface INetConnection
    {
        event EventHandler BusActivityDetection;
        event EventHandler<DataEvent> RaiseDataEvent;

        void Connect();
     
        int Read(object index);

        void Write(object index, int data);

        void WriteArray(ushort index, ushort[] data);

        void Disconnect();

        int NumofPoints     { get; set; }
        bool IsConnected    { get; set; }
        string IpAddress    { get; set; }
        int SendingInterval { get; set; }

    }

    /*
    /// <summary>
    /// Common Exception-type wich thrown if catch a Exception from spezific Interface
    /// </summary>
    public class InterfaceException : Exception
    {
        public InterfaceException(Exception innerException, uint error) : base(innerException.Message, innerException)
        {
            Error = error;
        }

        public InterfaceException(uint error)
        {
            Error = error;
        }

        public uint Error { get; }
    }
    */

    
    
}
