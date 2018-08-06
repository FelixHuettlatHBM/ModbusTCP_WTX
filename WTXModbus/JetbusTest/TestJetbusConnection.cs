using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


// Look also on the tests on GitHub at a related project for SharpJet : https://github.com/gatzka/SharpJet/tree/master/SharpJetTests

namespace HBM.WT.API.WTX.Jet
{
    using HBM.WT.API;
    using System;
    using System.Collections.Generic;
    using System.Net.Security;

    public enum Behavior
    {
        ConnectionFail,
        ConnectionSuccess
    }

    public class TestJetbusConnection : JetBusConnection
    {
        private Behavior behavior;
        private List<string> messages;
        private bool connected;

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        // Constructor with all parameters possible from class 'JetbusConnection' - Without ssh certification.
        //public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) : base(ipAddr, user, passwd, certificationCallback, timeoutMs = 5000)

        public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) : base(ipAddr, user, passwd, certificationCallback, timeoutMs)
        {
            this.behavior = behavior;
            this.messages = new List<string>();
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
    }
}
