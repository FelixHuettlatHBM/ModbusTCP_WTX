using System;

namespace HBM.WT.API
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
        private readonly EventArgType _mType;
        private T _mArgs;

        public NetConnectionEventArgs(EventArgType type, T args)
        {
            _mType = type;
            _mArgs = args;
        }

        public EventArgType Type { get { return _mType; } }

        public T Args
        {
            get
            {
                return _mArgs;
            }
            set
            {
                _mArgs = value;
            }
        }
    }
}
