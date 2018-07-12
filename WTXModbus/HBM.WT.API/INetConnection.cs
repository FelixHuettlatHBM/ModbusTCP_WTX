using System;

namespace HBM.WT.API  
{
    /// <summary>
    /// Define the common communication-interface
    /// </summary>
    public interface INetConnection
    {

        T Read<T>(object index);

        void Write<T>(object index, T data);

        void WriteArray2Reg(ushort index, ushort[] data);

        void WriteWord2Reg(ushort index, ushort data);

        event EventHandler BusActivityDetection;

        event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent;

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
