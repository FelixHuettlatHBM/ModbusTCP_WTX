using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Hbm.Devices.Jet;
using System.Threading;
using System.Net.Security;
using HBM.WT.API.COMMON;
using HBM.WT.API.WTX;

namespace HBM.WT.API.WTX.Jet
{
    /// <summary>
    /// Use this class du handle a connection over Ethernet.
    /// </summary>
    public class JetBusConnection : INetConnection, IDisposable {
        #region member
        protected JetPeer m_Peer;

        private Dictionary<string, JToken> m_TokenBuffer = new Dictionary<string, JToken>();

        private AutoResetEvent m_SuccessEvent = new AutoResetEvent(false);
        private Exception m_Exception = null;
        private int m_TimeoutMS;

        public event EventHandler BusActivityDetection;
        public event EventHandler<NetConnectionEventArgs<ushort[]>> RaiseDataEvent;

        #endregion

        #region constructors

        public JetBusConnection(string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMS = 5000) {
            IJetConnection jetConnection = new WebSocketJetConnection(ipAddr, certificationCallback);
            m_Peer = new JetPeer(jetConnection);

            ConnectOnPeer(user, passwd, timeoutMS);
            FetchAll();
        }

        public JetBusConnection(string ipAddr, RemoteCertificateValidationCallback certificationCallback, int timeoutMS = 5000) {
            IJetConnection jetConnection = new WebSocketJetConnection(ipAddr, NetConnectionSecurity.RemoteCertificationCheck);
            m_Peer = new JetPeer(jetConnection);

            ConnectOnPeer(timeoutMS);
            FetchAll();
        }

        public JetBusConnection(string ipAddr, string user, string passwd, int timeoutMS = 5000) 
            : this(ipAddr, user, passwd, NetConnectionSecurity.RemoteCertificationCheck, timeoutMS){ }

        public JetBusConnection(string ipAddr, int timeoutMS = 5000) 
            : this(ipAddr, NetConnectionSecurity.RemoteCertificationCheck, timeoutMS) { }

        #endregion

        #region support functions

        protected virtual void ConnectOnPeer(int timeoutMS = 5000) {
            m_Peer.Connect(delegate (bool connected) {
                if (!connected) {
                    m_Exception = new Exception("Connection failed.");
                }
                m_SuccessEvent.Set();
            }, timeoutMS);
            m_TimeoutMS = timeoutMS;

            // 
            // Das WaitOne und der Timeout bezieht sich auf die gesamte Routine einschließlich aller
            // Instruktionen in die Callbacks der Connect-Methode
            //
            WaitOne();
        }

        protected virtual void ConnectOnPeer(string user, string passwd, int timeoutMS = 5000) {
            m_Peer.Connect(delegate (bool connected) {
                if (connected) {
                    m_Peer.Authenticate(user, passwd, delegate (bool success, JToken token) {
                        if (!success) {
                            JetBusException exception = new JetBusException(token);
                            m_Exception = new InterfaceException(exception, (uint)exception.Error);
                        }
                        m_SuccessEvent.Set();
                    }, m_TimeoutMS);
                }
                else {
                    m_Exception = new Exception("Connection failed");
                    m_SuccessEvent.Set();
                }
            }, timeoutMS);
            m_TimeoutMS = timeoutMS;
            WaitOne(2);
        }

        protected virtual void FetchAll() {
            Matcher matcher = new Matcher();
            FetchId id;
            m_Peer.Fetch(out id, matcher, OnFetchData, delegate (bool success, JToken token) {
                if (!success) {
                    JetBusException exception = new JetBusException(token);
                    m_Exception = new InterfaceException(exception, (uint)exception.Error);
                }
                //
                // Wake up the waiting thread where call the konstruktor to connect the session
                //
                m_SuccessEvent.Set();
                BusActivityDetection?.Invoke(this, new NetConnectionEventArgs<string>(EventArgType.Message, "Fetch-All success: " + success + " - buffersize is: " + m_TokenBuffer.Count));
            }, m_TimeoutMS);
            WaitOne(3);
        }

        protected virtual void WaitOne(int timeoutMultiplier = 1) {
            if (!m_SuccessEvent.WaitOne(m_TimeoutMS * timeoutMultiplier)) {
                //
                // Timeout-Exception
                //
                throw new InterfaceException(new TimeoutException("Interface Timeout - signal-handler will never reset"), 0x1);
            }
            if (m_Exception != null) {
                Exception exception = m_Exception;
                m_Exception = null;
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
            lock (m_TokenBuffer) {
                switch (data["event"].ToString()) {
                    case "add": m_TokenBuffer.Add(path, data["value"]); break;
                    case "fetch": m_TokenBuffer[path] = data["value"]; break;
                    case "change":
                        m_TokenBuffer[path] = data["value"];
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
            lock (m_TokenBuffer) {
                if (m_TokenBuffer.ContainsKey(index.ToString())) {
                    return m_TokenBuffer[index.ToString()];
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

        public int ReadINT(object index) {
            try {
                return Convert.ToInt32(ReadObj(index));
            }
            catch (FormatException) {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }

        public long ReadDINT(object index) {
            try {
                return Convert.ToInt64(ReadObj(index));
            }
            catch (FormatException) {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }

        public string ReadASC(object index) {
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
                JObject request = m_Peer.Set(path.ToString(), value, delegate (bool success, JToken token) {
                    if (!success) {
                        JetBusException exception = new JetBusException(token);
                        m_Exception = new InterfaceException(exception, (uint)exception.Error);
                    }
                    m_SuccessEvent.Set();
                    BusActivityDetection?.Invoke(this, new NetConnectionEventArgs<string>(EventArgType.Message, "Set data: " + success));
                }, m_TimeoutMS);
            }
            catch (Exception e) {
                throw new InterfaceException(e, 0x01);
            }
            WaitOne();
        }

        public void WriteINT(object index, int data) {
            JValue value = new JValue(data);
            SetData(index, value);
        }

        public void WriteDINT(object index, long data) {
            JValue value = new JValue(data);
            SetData(index, value);
        }
        public void WriteASC(object index, string data) {
            JValue value = new JValue(data);
            SetData(index, value);
        }
        #endregion

        public void ResetDevice() {
            throw new NotImplementedException();
        }

        public string BufferToString() {
            StringBuilder sb = new StringBuilder();
            lock (m_TokenBuffer) {
                int i = 0;
                foreach (var item in m_TokenBuffer) {
                    sb.Append(i.ToString("D3")).Append(" # ").Append(item).Append("\r\n");
                    i++;
                }
            }
            return sb.ToString();
        }

        public void Disconnect() {
            m_Peer.Disconnect();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) { 
                if (disposing) {
                    // dispose managed state (managed objects).
                    m_SuccessEvent.Close();
                    m_SuccessEvent.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class JetBusException : Exception
    {
        private int m_Error;
        private string m_Message;

        public JetBusException(JToken token) {
            m_Error = int.Parse(token["error"]["code"].ToString());
            m_Message = token["error"]["message"].ToString();
        }

        public int Error { get { return m_Error; } }

        public override string Message {
            get {
                return m_Message + " [ 0x" + m_Error.ToString("X") + " ]";
            }
        }
    }
}
