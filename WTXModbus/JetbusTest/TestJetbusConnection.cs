using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


// Look also on the tests on GitHub at a related project for SharpJet : https://github.com/gatzka/SharpJet/tree/master/SharpJetTests

namespace JetbusTest
{
    using HBM.WT.API;
    using System;
    using System.Collections.Generic;

    public enum Behavior
    {
        ConnectionFail,
        ConnectionSuccess
    }

    public class TestJetbusConnection : INetConnection
    {
        private Behavior behavior;
        private List<string> messages;
        private bool connected;

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        public TestJetbusConnection(Behavior behavior)
        {
            this.behavior = behavior;
            this.messages = new List<string>();
        }

        public void Connect()
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

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public int Read(object index)
        {
            throw new NotImplementedException();
        }

        public void Write(object index, int data)
        {
            throw new NotImplementedException();
        }

        public void WriteArray(ushort index, ushort[] data)
        {
            throw new NotImplementedException();
        }
    }
}
