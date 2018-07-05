using System;

namespace HBM.WT.API.COMMON
{
    public enum EventArgType
    {
        Message,

        Data,
    }

    /// <summary>
    /// Also vom Prinzip her macht es ja Sinn. Es wird ein Event-Type definiert und dort können
    /// dann Argumente beliebigen Typs drin stehen. Das klingt doch eigentlich vom Prinzip
    /// schon mal logisch.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
