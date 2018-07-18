using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBM.WT.API
{      
    public class LogEvent : EventArgs
    {
        private string _mArgs;

        public LogEvent(string args)
        {
            _mArgs = args;
        }
        
        public string Args
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
