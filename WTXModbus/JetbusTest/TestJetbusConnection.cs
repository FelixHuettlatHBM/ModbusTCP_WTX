using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


// Look also on the tests on GitHub at a related project for SharpJet : https://github.com/gatzka/SharpJet/tree/master/SharpJetTests

namespace HBM.WT.API.WTX.Jet
{
    using Hbm.Devices.Jet;
    using HBM.WT.API;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Threading;

    public enum Behavior
    {
        ConnectionFail,
        ConnectionSuccess,

        ReadFail,
        ReadSuccess,
    }

    public class TestJetbusConnection : JetBusConnection
    {
        private Behavior behavior;
        private List<string> messages;
        private bool connected;

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        private int _mTimeoutMs;

        private Dictionary<string, JToken> _mTokenBuffer;

        private AutoResetEvent _mSuccessEvent = new AutoResetEvent(false);

        // Constructor with all parameters possible from class 'JetbusConnection' - Without ssh certification.
        //public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) : base(ipAddr, user, passwd, certificationCallback, timeoutMs = 5000)

        public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) : base(ipAddr, user, passwd, certificationCallback, timeoutMs)
        {
            this.behavior = behavior;
            this.messages = new List<string>();

            _mTokenBuffer = new Dictionary<string, JToken>();

            this._mTimeoutMs = 5000; // values of 5000 according to the initialization in class JetBusConnection. 

            //ConnectOnPeer(user, passwd, timeoutMs);
            //FetchAll();
        }

        protected JToken ReadObj(object index)
        {

            switch(this.behavior)
            {
                case Behavior.ReadSuccess:
                    if(_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;
                case Behavior.ReadFail:
                    //throw new InterfaceException(new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                    return _mTokenBuffer[""];
                    break;

                default:
                    break;

            }

            return 0; 
        }

        public Dictionary<string, JToken> getTokenBuffer
        {
            get
            {
                return this._mTokenBuffer;
            }
        }


        public override void FetchAll()
        {
            Matcher matcher = new Matcher();
            FetchId id;

            MPeer.Fetch(out id, matcher, OnFetchData, delegate (bool success, JToken token) {
                if (!success)
                {                  
                    JetBusException exception = new JetBusException(token);
                }
                //
                // Wake up the waiting thread where call the konstruktor to connect the session
                //
                _mSuccessEvent.Set();

                BusActivityDetection?.Invoke(this, new LogEvent("Fetch-All success: " + success + " - buffersize is " + _mTokenBuffer.Count));

                //BusActivityDetection?.Invoke(this, new NetConnectionEventArgs<string>(EventArgType.Message, "Fetch-All success: " + success + " - buffersize is: " + _mTokenBuffer.Count));

            }, _mTimeoutMs);
            WaitOne(3);
        }

        /// <summary>
        /// Event with callend when raced a Fetch-Event by a other Peer.
        /// For testing it must be filled with pseudo data be tested in the UNIT tests. 
        /// </summary>
        /// <param name="data"></param>
        protected override void OnFetchData(JToken data)
        {
            string path = data["path"].ToString();
            lock (_mTokenBuffer)
            {              
                _mTokenBuffer.Add("6144 / 00", data["value"]);
                
                /*
                switch (data["event"].ToString())
                {
                    case "add": _mTokenBuffer.Add(path, data["value"]); break;
                    case "fetch": _mTokenBuffer[path] = data["value"]; break;
                    case "change":
                        _mTokenBuffer[path] = data["value"];
                        break;
                }
                */

                BusActivityDetection?.Invoke(this, new LogEvent(data.ToString()));
            }
        }




        public new void Connect()
        {
            switch (this.behavior)
            {
                case Behavior.ConnectionFail:
                    connected = false;
                    break;

                case Behavior.ConnectionSuccess:
                    connected = true;
                    break;

                default:
                    connected = true;
                    break;
            }
        }



        public bool isConnected()
        {
            return this.connected;
        }

        public new void Disconnect()
        {
            throw new NotImplementedException();
        }

        public new int Read(object index)
        {
            throw new NotImplementedException();
        }

        public new void Write(object index, int data)
        {
            throw new NotImplementedException();
        }

        public new void WriteArray(ushort index, ushort[] data)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string json)
          { 
            messages.Add(json);
        }

}
}
