
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

    public class TestModbusTCPConnection : ModbusTcpConnection
    {
        private Behavior behavior;
        private List<string> messages;
        private bool connected;

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        public TestModbusTCPConnection(Behavior behavior,string ipAddress) : base(ipAddress)
        {
            this.behavior = behavior;
            this.messages = new List<string>();
        }



        public new void Connect()
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
