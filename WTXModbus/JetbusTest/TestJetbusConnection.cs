using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


// Look also on the tests on GitHub at a related project for SharpJet : https://github.com/gatzka/SharpJet/tree/master/SharpJetTests

namespace HBM.WT.API.WTX.Jet
{
    using HBM.WT.API;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net.Security;

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

        private Dictionary<string, int> _mTokenBuffer = new Dictionary<string, int>();

        // Constructor with all parameters possible from class 'JetbusConnection' - Without ssh certification.
        //public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) : base(ipAddr, user, passwd, certificationCallback, timeoutMs = 5000)

        public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) : base(ipAddr, user, passwd, certificationCallback, timeoutMs)
        {
            this.behavior = behavior;
            this.messages = new List<string>();
        }


        protected int ReadObj(object index)
        {

            switch(this.behavior)
            {
                case Behavior.ReadSuccess:
                    if(_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;
                case Behavior.ReadFail:
                    throw new InterfaceException(new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                    break;

                default:
                    break;

            }

            return 0; 
        }

        /*
        [Test]
        //public void TestOnFetchData(JToken data)
        public void TestOnFetchData(Behavior dataEvent)
        {
            switch (dataEvent.ToString())
            {
                case "add":

                case "fetch":

                case "change":


            }
        }
        */

        public Dictionary<string, int> getTokenBuffer
        {
            get
            {
                return this._mTokenBuffer;
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
