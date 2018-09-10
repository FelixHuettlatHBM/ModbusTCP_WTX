using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBM.WT.API
{
    public class DataEvent : EventArgs 
    {
        private ushort[] _ushortArgs;
        private string[] _strArgs;

        public DataEvent(ushort[] _ushortArrayParam, string[] _strArrayParam)
        {
            _ushortArgs = _ushortArrayParam;
            _strArgs = _strArrayParam;
        }
       
        public ushort[] ushortArgs
        {
            get
            {
                return _ushortArgs;
            }
            set
            {
                _ushortArgs = value;
            }
        }

        public string[] strArgs
        {
            get
            {
                return _strArgs;
            }
            set
            {
                _strArgs = value;
            }
        }
    }
}
