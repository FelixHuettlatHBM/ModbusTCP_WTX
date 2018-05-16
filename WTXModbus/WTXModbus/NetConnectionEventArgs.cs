using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTXModbus
{
      /*
       * This enum implements 2 types of events : Data and Message. In this Modbus/TCP application we use Data for the reading of
       * the WTX registers. For example a CanBus or JetBus application would require a Message as an event type. 
       * The setting to Data is done in class 'ModbusConnection' when a reading is executed. 
       */

        public enum EventArgType
        {
            Message,

            Data,
        }

    /*
     * This event inherits from EventArgs. An object of this class is created in class 'ModbusConnection' if data should
     * be read. From 'm_Args' you can also get the data from the WTX register once read from the WTX. In this application
     * we get the data from an object of class 'WTX120'(advantage :already interpreted as strings), but the other way would be also possible.
     * 
     * By using this class we can commit arguments of any desired type, because of using generics. 
     * We use for Modbus/TCP ushort as a type. For Jetbus and CanBus we would unsigned int ('uint') as type. 
     */

    public class NetConnectionEventArgs<T> : EventArgs
        {
            private EventArgType m_Type;
            private T m_Args;

            public NetConnectionEventArgs(EventArgType type, T args)
            {
                m_Type = type;
                m_Args = args;
            }

            public EventArgType Type { get { return m_Type; } }

            /*
             * This is an auto-property, which returns and sets the argument. The return type is a generic to allow
             * several cases like for Modbus/TCP, CanBus, JetBus. 
             */
            public T Args
            {
            get
            {
               return m_Args;
            }
            set
            {
                m_Args = value;
            }
            }
        }
}
