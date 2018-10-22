// <copyright file="JetBusConnection.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// HBM.WT.API, a library to communicate with HBM weighing technology devices  
//
// The MIT License (MIT)
//
// Copyright (C) Hottinger Baldwin Messtechnik GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>

using Hbm.Devices.Jet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
        protected JetPeer _peer;

        private Dictionary<string, JToken> _mTokenBuffer = new Dictionary<string, JToken>();

        private AutoResetEvent _mSuccessEvent = new AutoResetEvent(false);
        private Exception _mException = null;

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        private bool _connected;

        private string _ipaddress;
        private int interval;

        private JToken[] JTokenArray;
        private ushort[] DataUshortArray;
        private string[] DataStrArray;

        private string _password;
        private string _user;
        private int _timeoutMs;

        #endregion

        #region constructors

        // Constructor without ssh certification. 
        public JetBusConnection(string IPAddress, string User, string Password, int TimeoutMs = 5000)
        {
            IJetConnection jetConnection = new WebSocketJetConnection(IPAddress, RemoteCertificationCheck);
            _peer = new JetPeer(jetConnection);
            this._user = User;
            this._password = Password;
            this._timeoutMs = TimeoutMs;
            this._ipaddress = IPAddress;
        }

        // Constructor with ssh certification
        public JetBusConnection(string IPAddress, int TimeoutMs = 5000)
        {
            IJetConnection jetConnection = new WebSocketJetConnection(IPAddress, RemoteCertificationCheck);
            _peer = new JetPeer(jetConnection);
            this._timeoutMs = TimeoutMs;
        }
        #endregion

        #region support functions

        public void Connect()
        {
            ConnectPeer(this._user, this._password, this._timeoutMs);
            FetchAll();
        }


        public void DisconnectDevice()
        {
            _peer.Disconnect();
        }


        public bool IsConnected
        {
            get
            {
                return _connected;
            } 
        }


        public string[] getStringData
        {
            get
            {
                return this.DataStrArray;
            }
        }

        private void OnAuthenticate(bool success, JToken token)
        {           
            if (!success)
            {

                this._connected = false;
                JetBusException exception = new JetBusException(token);
                _mException = new Exception(exception.Error.ToString());
            }
            _mSuccessEvent.Set();
        }
                     

        private void OnConnect(bool connected)
        {
            if (!connected)
            {
                _mException = new Exception("Connection failed.");
            }

            this._connected = true;
            _mSuccessEvent.Set();
        }


        private void OnConnectAuhtenticate(bool connected)
        {
            if (connected)
            {
                this._connected = true;

                _peer.Authenticate(this._user, this._password, OnAuthenticate, this._timeoutMs);
            }
            else
            {
                this._connected = false;
                _mException = new Exception("Connection failed");
                _mSuccessEvent.Set();
            }

        }


        private void ConnectPeer(int timeoutMs)
        {
            _peer.Connect(OnConnect, timeoutMs);
            WaitOne();
        }


        private void ConnectPeer(string User, string Password, int TimeoutMs)
        {
            this._user = User;
            this._password = Password;

            _peer.Connect(OnConnectAuhtenticate, TimeoutMs);
            WaitOne(2);
        }


        private void OnFetch(bool success, JToken token)
        {
            if (!success)
            {

                this._connected = false;

                JetBusException exception = new JetBusException(token);
                _mException = new Exception(exception.Error.ToString());
            }
            //
            // Wake up the waiting thread where call the construktor to connect the session
            //

            this._connected = true;
            _mSuccessEvent.Set();

            BusActivityDetection?.Invoke(this, new LogEvent("Fetch-All success: " + success + " - buffersize is " + _mTokenBuffer.Count));

        }


        public virtual void FetchAll()
        {
            Matcher matcher = new Matcher();
            FetchId id;

            _peer.Fetch(out id, matcher, OnFetchData, OnFetch , this._timeoutMs);
            WaitOne(3);
        }
        
        protected virtual void WaitOne(int timeoutMultiplier = 1)
        {
            if (!_mSuccessEvent.WaitOne(_timeoutMs * timeoutMultiplier))
            {

                this._connected = false;
  
                // Timeout-Exception
                throw new Exception("Jet interface Timeout");
            }


            this._connected = true; 

            if (_mException != null)
            {
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
        protected virtual void OnFetchData(JToken data)
        {
            string path = data["path"].ToString();

            lock (_mTokenBuffer)
            {

                switch (data["event"].ToString())
                {
                    case "add":
                        _mTokenBuffer.Add(path, data["value"]);
                        break;

                    case "fetch":
                        _mTokenBuffer[path] = data["value"];
                        break;

                    case "change":
                        _mTokenBuffer[path] = data["value"];
                        break;
                }

                this.ConvertJTokenToStringArray();

                RaiseDataEvent?.Invoke(this, new DataEvent(DataUshortArray, DataStrArray));

                BusActivityDetection?.Invoke(this, new LogEvent(data.ToString()));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual JToken ReadObj(object index) {

            lock (_mTokenBuffer)
            {
                if (_mTokenBuffer.ContainsKey(index.ToString())) {

                    this.ConvertJTokenToStringArray();
                   
                    RaiseDataEvent?.Invoke(this, new DataEvent(DataUshortArray,DataStrArray));

                    return _mTokenBuffer[index.ToString()];
                }
                else {

                    throw new Exception("Object does not exist in the object dictionary");
                }
            }
        }

        private void ConvertJTokenToStringArray()
        {
            JTokenArray = _mTokenBuffer.Values.ToArray();
            DataUshortArray = new ushort[JTokenArray.Length];
            DataStrArray = new string[JTokenArray.Length];

            for (int i = 0; i < JTokenArray.Length; i++)
            {
                JToken JTokenElement = JTokenArray[i];

                DataStrArray[i] = JTokenElement.ToString();
            }
        }
       
        public int Read(object index)
        {
            try
            {
                return Convert.ToInt32(ReadObj(index));
            }
            catch (FormatException)
            {
                throw new FormatException("Invalid data format");
            }
        }

        public void Write(object index, int value) {
            JValue jValue = new JValue(value);
            SetData(index, jValue);
        }

        public int ReadInt(object index) {
            try {
                return Convert.ToInt32(ReadObj(index));
            }
            catch (FormatException) {
                throw new FormatException("Invalid data format");
            }
        }

        public long ReadDint(object index) {
            try {
                return Convert.ToInt64(ReadObj(index));
            }
            catch (FormatException) {
                throw new FormatException("Invalid data format");
            }
        }

        public string ReadAsc(object index) {
            return ReadObj(index).ToString();
        }

        #endregion


        #region Write functions
        private void OnSet(bool success, JToken token)
        {
           if (!success)
           {
                JetBusException exception = new JetBusException(token);
                _mException = new Exception(exception.Error.ToString());
           }
            
           _mSuccessEvent.Set();
            
           BusActivityDetection?.Invoke(this, new LogEvent("Set data" + success ));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        protected virtual void SetData(object path, JValue value)
        {
            try
            {
                JObject request = _peer.Set(path.ToString(), value, OnSet, this._timeoutMs);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }


        public void WriteInt(object index, int data)
        {
            JValue value = new JValue(data);
            SetData(index, value);
        }


        public void WriteDint(object index, long data)
        {
            JValue value = new JValue(data);
            SetData(index, value);
        }


        public void WriteAsc(object index, string data)
        {
            JValue value = new JValue(data);
            SetData(index, value);
        }
        #endregion



        public string BufferToString()
        {
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


        public void Disconnect()
        {
            _peer.Disconnect();
            this._connected = false;
        }


        /// <summary>
        /// RemoteCertificationCheck:
        /// Callback-Method wich is called from SslStream. Is a customized implementation of a certification-check.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool RemoteCertificationCheck(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                X509Certificate2 clientCertificate = new X509Certificate2("ca-cert.crt");
                SslStream sslStream = (sender as SslStream);

                if (sslPolicyErrors == SslPolicyErrors.None || sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    foreach (X509ChainElement item in chain.ChainElements)
                    {
                        item.Certificate.Export(X509ContentType.Cert);
                        //
                        // If one of the included status-flags is not posiv then the cerficate-check
                        // failed. Except the "untrusted root" because it is a self-signed certificate
                        //
                        foreach (X509ChainStatus status in item.ChainElementStatus)
                        {
                            if (status.Status != X509ChainStatusFlags.NoError
                                && status.Status != X509ChainStatusFlags.UntrustedRoot
                                 && status.Status != X509ChainStatusFlags.NotTimeValid)
                            {

                                return false;
                            }
                        }
                        //
                        // compare the certificate in the chain-collection. If on of the certificate at
                        // the path to root equal, are the check ist positive
                        //
                        if (clientCertificate.Equals(item.Certificate))
                        {
                            return true;
                        }
                    }
                }
                // TODO: to reactivate the hostename-check returning false.
                return true;
            }
            catch (Exception)
            {
                // If thrown any exception then is the certification-check failed
                return false;
            }
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
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.

            Dispose(true);

            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public void WriteArray(ushort index, ushort[] data)
        {
            throw new NotImplementedException();
        }


        public Dictionary<string, JToken> getTokenBuffer
        {
            get
            {
                return _mTokenBuffer;
            }
        }
        
        public int NumofPoints 
        {
            get
            {
                return 38;
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        
        public string IpAddress
        {
            get
            {
                return this._ipaddress;
            }
        }

        public int SendingInterval
        {
            get
            {
                return this.interval;
            }
            set
            {
                this.interval = value;
            }
        }

        /*
        public ushort[] getData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        */

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

        public override string Message
        {
            get {
                return _mMessage + " [ 0x" + _mError.ToString("X") + " ]";
            }
        }
    }

}
