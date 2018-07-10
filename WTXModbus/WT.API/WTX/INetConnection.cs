using HBM.WT.API;
using System;

namespace HBM.WT.API  // Ohne WTX!!
{
    /// <summary>
    /// Define the common communication-interface
    /// </summary>
    public interface INetConnection
    {

        T Read<T>(object index);

        void Write<T>(object index, T data);

        event EventHandler BusActivityDetection;

        event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent;

        void ResetDevice();
    }

    /// <summary>
    /// Common Exception-type wich thrown if catch a Exception from spezific Interface
    /// </summary>
    public class InterfaceException : Exception
    {
        private uint m_Error;

        public InterfaceException(Exception innerException, uint error) 
            :base(innerException.Message, innerException) {

            m_Error = error;
        }

        public InterfaceException(uint error) {
            m_Error = error;
        }

        public uint Error { get { return m_Error; } }
    }


    
    
}
