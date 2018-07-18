using Newtonsoft.Json.Linq;
using System;

namespace HBM.WT.API  
{
    /// <summary>
    /// Define the common communication-interface
    /// </summary>
    public interface INetConnection
    {
        int Read(object index);

        void Write(object index, int data);

        void WriteArray(ushort index, ushort[] data);
        
        event EventHandler BusActivityDetection;

        event EventHandler<DataEvent> RaiseDataEvent;

    }

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


    
    
}
