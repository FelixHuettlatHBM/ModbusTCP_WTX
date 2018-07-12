using Hbm.Devices.Jet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using System.Threading;

namespace HBM.WT.API.WTX.Jet
{
    /// <summary>
    /// Use this class du handle a connection over Ethernet.
    /// </summary>
    public class JetBusConnection : INetConnection, IDisposable
    {
        #region member
        protected JetPeer MPeer;

        private Dictionary<string, JToken> _mTokenBuffer = new Dictionary<string, JToken>();

        private AutoResetEvent _mSuccessEvent = new AutoResetEvent(false);
        private Exception _mException = null;
        private int _mTimeoutMs;

        public event EventHandler BusActivityDetection;
        //public event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent
        public event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent;


        #endregion

        #region constructors

        // Constructor: Without ssh certification. 
        public JetBusConnection(string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) {
            IJetConnection jetConnection = new WebSocketJetConnection(ipAddr, certificationCallback);
            MPeer = new JetPeer(jetConnection);

            ConnectOnPeer(user, passwd, timeoutMs);
            FetchAll();
        }

        // Constructor: With ssh certification as a parameter (NetConnectionSecurity) . 
        public JetBusConnection(string ipAddr, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) {

            IJetConnection jetConnection = new WebSocketJetConnection(ipAddr, NetConnectionSecurity.RemoteCertificationCheck);
            MPeer = new JetPeer(jetConnection);

            ConnectOnPeer(timeoutMs);
            FetchAll();
        }

        public JetBusConnection(string ipAddr, string user, string passwd, int timeoutMs = 5000) 
            : this(ipAddr, user, passwd, NetConnectionSecurity.RemoteCertificationCheck, timeoutMs){ }

        public JetBusConnection(string ipAddr, int timeoutMs = 5000) 
            : this(ipAddr, NetConnectionSecurity.RemoteCertificationCheck, timeoutMs) { }

        #endregion

        #region support functions

        protected virtual void ConnectOnPeer(int timeoutMs = 5000) {
            MPeer.Connect(delegate (bool connected) {
                if (!connected) {
                    _mException = new Exception("Connection failed.");
                }
                _mSuccessEvent.Set();
            }, timeoutMs);
            _mTimeoutMs = timeoutMs;

            // 
            // Das WaitOne und der Timeout bezieht sich auf die gesamte Routine einschließlich aller
            // Instruktionen in die Callbacks der Connect-Methode
            //
            WaitOne();
        }

        protected virtual void ConnectOnPeer(string user, string passwd, int timeoutMs = 5000) {
            MPeer.Connect(delegate (bool connected) {
                if (connected) {
                    MPeer.Authenticate(user, passwd, delegate (bool success, JToken token) {
                        if (!success) {
                            JetBusException exception = new JetBusException(token);
                            _mException = new InterfaceException(exception, (uint)exception.Error);
                        }
                        _mSuccessEvent.Set();
                    }, _mTimeoutMs);
                }
                else {
                    _mException = new Exception("Connection failed");
                    _mSuccessEvent.Set();
                }
            }, timeoutMs);
            _mTimeoutMs = timeoutMs;
            WaitOne(2);
        }

        protected virtual void FetchAll()
        {
            Matcher matcher = new Matcher();
            FetchId id;
            MPeer.Fetch(out id, matcher, OnFetchData, delegate (bool success, JToken token) {
                if (!success) {
                    JetBusException exception = new JetBusException(token);
                    _mException = new InterfaceException(exception, (uint)exception.Error);
                }
                //
                // Wake up the waiting thread where call the konstruktor to connect the session
                //
                _mSuccessEvent.Set();
                BusActivityDetection?.Invoke(this, new NetConnectionEventArgs<string>(EventArgType.Message, "Fetch-All success: " + success + " - buffersize is: " + _mTokenBuffer.Count));  // erstmal rausnehmen 
            }, _mTimeoutMs);
            WaitOne(3);
        }

        protected virtual void WaitOne(int timeoutMultiplier = 1) {
            if (!_mSuccessEvent.WaitOne(_mTimeoutMs * timeoutMultiplier)) {
                //
                // Timeout-Exception
                //
                throw new InterfaceException(new TimeoutException("Interface Timeout - signal-handler will never reset"), 0x1);
            }
            if (_mException != null) {
                Exception exception = _mException;
                _mException = null;
                throw exception;
            }
        }

        #endregion

        #region read-functions

        /// <summary>
        /// Event with callend when raced a Fetch-Event by a other Peer.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnFetchData(JToken data) {
            string path = data["path"].ToString();
            lock (_mTokenBuffer) {
                switch (data["event"].ToString()) {
                    case "add": _mTokenBuffer.Add(path, data["value"]); break;
                    case "fetch": _mTokenBuffer[path] = data["value"]; break;
                    case "change":
                        _mTokenBuffer[path] = data["value"];
                        break;
                }
                BusActivityDetection?.Invoke(this, new NetConnectionEventArgs<string>(EventArgType.Message, data.ToString()));

                // Äquivalent zu ...
                //if(BusActivityDetection != null){
                //     BusActivityDetection(this, new NetConnectionEventArgs<string>(EventArgType.Message, data.ToString()));
                //}
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual JToken ReadObj(object index) {
            lock (_mTokenBuffer) {
                if (_mTokenBuffer.ContainsKey(index.ToString())) {
                    return _mTokenBuffer[index.ToString()];
                }
                else {
                    throw new InterfaceException(
                        new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                }
            }
        }
        
        public T Read<T>(object index) {
            try {
                JToken token = ReadObj(index);
                return (T)Convert.ChangeType(token, typeof(T));
            }
            catch (FormatException) {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }

        public void Write<T>(object index, T value) {
            JValue jValue = new JValue(value);
            SetData(index, jValue);
        }

        public int ReadInt(object index) {
            try {
                return Convert.ToInt32(ReadObj(index));
            }
            catch (FormatException) {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }

        public long ReadDint(object index) {
            try {
                return Convert.ToInt64(ReadObj(index));
            }
            catch (FormatException) {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }

        public string ReadAsc(object index) {
            return ReadObj(index).ToString();
        }

        #endregion

        #region write-functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        protected virtual void SetData(object path, JValue value) {
            //
            // Over the JetPeer.Change(...) will be change the OWN object. Therefore 
            // will be calling the JetPeer.Set(...) to change the foreign object.
            //
            try {
                JObject request = MPeer.Set(path.ToString(), value, delegate (bool success, JToken token) {
                    if (!success) {
                        JetBusException exception = new JetBusException(token);
                        _mException = new InterfaceException(exception, (uint)exception.Error);
                    }
                    _mSuccessEvent.Set();
                    BusActivityDetection?.Invoke(this, new NetConnectionEventArgs<string>(EventArgType.Message, "Set data: " + success));
                }, _mTimeoutMs);
            }
            catch (Exception e) {
                throw new InterfaceException(e, 0x01);
            }
            WaitOne();
        }

        public void WriteInt(object index, int data) {
            JValue value = new JValue(data);
            SetData(index, value);
        }

        public void WriteDint(object index, long data) {
            JValue value = new JValue(data);
            SetData(index, value);
        }
        public void WriteAsc(object index, string data) {
            JValue value = new JValue(data);
            SetData(index, value);
        }
        #endregion

        public void DisconnectDevice() {
            throw new NotImplementedException();
        }

        public string BufferToString() {
            StringBuilder sb = new StringBuilder();
            lock (_mTokenBuffer) {
                int i = 0;
                foreach (var item in _mTokenBuffer) {
                    sb.Append(i.ToString("D3")).Append(" # ").Append(item).Append("\r\n");
                    i++;
                }
            }
            return sb.ToString();
        }

        public void Disconnect() {
            MPeer.Disconnect();
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!_disposedValue) { 
                if (disposing) {
                    // dispose managed state (managed objects).
                    _mSuccessEvent.Close();
                    _mSuccessEvent.Dispose();
                }
                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public void WriteArray2Reg(ushort index, ushort[] data)
        {
            throw new NotImplementedException();
        }

        public void WriteWord2Reg(ushort index, ushort data)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class JetBusException : Exception
    {
        private int _mError;
        private string _mMessage;

        public JetBusException(JToken token) {
            _mError = int.Parse(token["error"]["code"].ToString());
            _mMessage = token["error"]["message"].ToString();
        }

        public int Error { get { return _mError; } }

        public override string Message {
            get {
                return _mMessage + " [ 0x" + _mError.ToString("X") + " ]";
            }
        }
    }
}
