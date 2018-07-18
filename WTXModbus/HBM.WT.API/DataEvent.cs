using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBM.WT.API
{
    public class DataEvent : EventArgs 
    {
        private ushort[] _mArgs;

        public DataEvent(ushort[] args)
        {
            _mArgs = args;
        }
       
        public ushort[] Args
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
