
namespace HBM.WT.API.WTX.Modbus
{
    using HBM.WT.API;
    using System;
    using System.Collections.Generic;

    public enum Behavior
    {
         ConnectionFail, 
         ConnectionSuccess
    }

    public class TestModbusTCPConnection : INetConnection
    {
        private Behavior behavior;
        private List<string> messages;
        private bool connected;

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        public TestModbusTCPConnection(Behavior behavior)
        {
            this.behavior = behavior;
            this.messages = new List<string>();
        }

        public void Connect()
        {
            switch(this.behavior)
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
