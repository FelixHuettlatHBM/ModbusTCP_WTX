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
    /// Use this class to handle a connection via Ethernet.
    /// </summary>
    public class JetBusConnection : INetConnection, IDisposable
    {
        #region member
        protected JetPeer _peer;

        private Dictionary<string, JToken> _dataJTokenBuffer = new Dictionary<string, JToken>();
        Dictionary<string, int> _dataIntegerBuffer = new Dictionary<string, int>();

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
            string _uri = "wss://" + IPAddress + ":443/jet/canopen";

            IJetConnection jetConnection = new WebSocketJetConnection(_uri, RemoteCertificationCheck);
            _peer = new JetPeer(jetConnection);

            this._user = User;
            this._password = Password;
            this._timeoutMs = TimeoutMs;
            this._ipaddress = IPAddress;
        }

        // Constructor with ssh certification
        public JetBusConnection(string IPAddress, int TimeoutMs = 5000)
        {
            string _uri = "wss://" + IPAddress + ":443/jet/canopen";
            IJetConnection jetConnection = new WebSocketJetConnection(_uri, RemoteCertificationCheck);

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
            
            BusActivityDetection?.Invoke(this, new LogEvent("Fetch-All success: " + success + " - buffersize is " + _dataJTokenBuffer.Count));

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
            int i = 0;

            lock (_dataJTokenBuffer)
            {

                switch (data["event"].ToString())
                {
                    case "add":

                        _dataJTokenBuffer.Add(path, data["value"]);
                    
                        if (int.TryParse(data["value"].ToString(), out i))      // checks if the data is a number, which can be converted to an integer at the path. 
                            _dataIntegerBuffer.Add(path, Convert.ToInt32(data["value"].ToString()));
                        break;

                    case "fetch":

                        _dataJTokenBuffer[path] = data["value"];

                        if (int.TryParse(data["value"].ToString(), out i))      // checks if the data is a number, which can be converted to an integer at the path. 
                            _dataIntegerBuffer[path] = Convert.ToInt32(data["value"].ToString());
                        break;

                    case "change":

                        _dataJTokenBuffer[path] = data["value"];

                        if (int.TryParse(data["value"].ToString(), out i))      // checks if the data is a number, which can be converted to an integer at the path. 
                            _dataIntegerBuffer[path] = Convert.ToInt32(data["value"].ToString());
                        
                        break;
                }

                this.ConvertJTokenToStringArray();

                RaiseDataEvent?.Invoke(this, new DataEvent(DataUshortArray, DataStrArray));

                BusActivityDetection?.Invoke(this, new LogEvent(data.ToString()));
            }
        }
     
        public Dictionary<string, int> getData()
        {
            return _dataIntegerBuffer;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual JToken ReadObj(object index) {

            //lock (_dataJTokenBuffer)

                if (_dataJTokenBuffer.ContainsKey(index.ToString())) {

                    this.ConvertJTokenToStringArray();
                   
                    RaiseDataEvent?.Invoke(this, new DataEvent(DataUshortArray,DataStrArray));

                    return _dataJTokenBuffer[index.ToString()];
                }
                else {

                    throw new Exception("Object does not exist in the object dictionary");
                }
        }

        private void ConvertJTokenToStringArray()
        {
            JTokenArray = _dataJTokenBuffer.Values.ToArray();
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
            lock (_dataJTokenBuffer) {
                int i = 0;
                foreach (var item in _dataJTokenBuffer) {
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
        public bool RemoteCertificationCheck(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            
            try
            {
                X509Certificate2 clientCertificate = new X509Certificate2(CertificateToByteArray(),"");
                
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

        private byte[] CertificateToByteArray()
        {
            const string input = 
                
                "MIIECzCCAvOgAwIBAgIJAPTJN5RGpzbRMA0GCSqGSIb3DQEBBQUAMIGbMQswCQYDVQQGEwJERTELMAkGA1UECAw" +
                "CSEUxEjAQBgNVBAcMCURhcm1zdGFkdDErMCkGA1UECgwiSG90dGluZ2VyIEJhbGR3aW4gTWVzc3RlY2huaWsgR21iSDELMAkGA1UECwwCV" +
                "1QxFDASBgNVBAMMC3d3dy5oYm0uY29tMRswGQYJKoZIhvcNAQkBFgxpbmZvQGhibS5jb20wHhcNMTcwNDA2MTU1NzQzWhcNMjcwNDA0MTU1N" +
                "zQzWjCBmzELMAkGA1UEBhMCREUxCzAJBgNVBAgMAkhFMRIwEAYDVQQHDAlEYXJtc3RhZHQxKzApBgNVBAoMIkhvdHRpbmdlciBCYWxkd2luIE1" +
                "lc3N0ZWNobmlrIEdtYkgxCzAJBgNVBAsMAldUMRQwEgYDVQQDDAt3d3cuaGJtLmNvbTEbMBkGCSqGSIb3DQEJARYMaW5mb0BoYm0uY29tMIIBIjA" +
                "NBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr + 51SnSoQX1M3aOUwaD8dcFIoac9peaiRsIOUqwJGn58g + n53aevw54sFyvffcJnzZVAFEP" +
                "Ech1oQCNowsNDnNr4UL / NaO4C4GsJX5bmdia6nGj7IWLMeQzs + 0gfqPWmO / OsJVnwrp9h6 / SxuIz5n04l7ESupSBnXfifb9RGA0encHt" +
                "ZK0LxHRD9sxyhNYKDKW76hgDK / EZ5HU2YjKS0y58 + PU15AV + vQ5srJFMC + KNHveWF4xgj528r3C25FWpVtW5Dqd937OrSAS5truGxPBz" +
                "enWx3PHw6zRPvBbOApTNWLfbcp90mF8 / 9wFi94PG + EokaYSoF0xyT2G6fAVs3qQIDAQABo1AwTjAdBgNVHQ4EFgQUZC39SAkffhRee1x / 7" +
                "TbnrQJQ / jMwHwYDVR0jBBgwFoAUZC39SAkffhRee1x / 7TbnrQJQ / jMwDAYDVR0TBAUwAwEB / zANBgkqhkiG9w0BAQUFAAOCAQEACRI28" +
                "UaB6KNtDVee + waz + SfNd3tm / RspwRarJBlf9dcbpcxolvp9UxCoWkyf9hkmJPEyJzJltuan08DkqmschD36saOJcVRh6BfLNBD8DkFTavP" +
                "0Q2BKb8FvJpdacNTRK542sJHSk5gr6imwnD5EAj + OT24UpH5rwq5Esu5TYFLdSuYfRXw6puTION / fqqTKVK9Au0TdFPgZ4Fppym4fInQ0jJQ" +
                "hcGSWMs3yomPqftwitRwv5 / p8hLtf3yNIkk9OnBwPpT7QxXxw4Zs0Jvl / VBFuNwbeD12ur3RKbMyCn9W0RjaMrYpKnAjik3IlSqDYZ0XDMwZ" +
                "0oQiOFy / a6bR4Vw =="     
                ;

            // Invoke GetBytes method.
            byte[] _byteArray = Encoding.ASCII.GetBytes(input);

            return _byteArray;
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

        public Dictionary<string, JToken> getDataBuffer
        {
            get
            {
                return _dataJTokenBuffer;
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

        public JetBusException(JToken token)
        {
            _mError = int.Parse(token["error"]["code"].ToString());

            _mMessage = token["error"]["message"].ToString();
        }

        public int Error
        {
            get
            {
                return _mError;
            }
        }

        public override string Message
        {
            get {
                return _mMessage + " [ 0x" + _mError.ToString("X") + " ]";
            }
        }
    }

}
