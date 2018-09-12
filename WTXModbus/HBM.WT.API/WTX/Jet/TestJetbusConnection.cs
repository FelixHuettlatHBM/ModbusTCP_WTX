using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;


// Look also on the tests on GitHub at a related project for SharpJet : https://github.com/gatzka/SharpJet/tree/master/SharpJetTests

namespace HBM.WT.API.WTX.Jet
{
    using Hbm.Devices.Jet;
    using HBM.WT.API;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Security;
    using System.Threading;

    public enum Behavior
    {
        ConnectionFail,
        ConnectionSuccess,

        ReadGrossValueFail,
        ReadGrossValueSuccess,

        ReadNetValueFail,
        ReadNetValueSuccess,

        ReadWeightMovingFail,
        ReadWeightMovingSuccess,

        WriteTareFail,
        WriteTareSuccess,

        WriteGrossFail,
        WriteGrossSuccess,

        WriteZeroFail,
        WriteZeroSuccess,

        CalibrationFail,
        CalibrationSuccess,

        CalibratePreloadCapacityFail,
        CalibratePreloadCapacitySuccess,

        MeasureZeroFail,
        MeasureZeroSuccess,
    }

    public class TestJetbusConnection : INetConnection, IDisposable
    {
        private Behavior behavior;
        private List<string> messages;
        private bool connected;

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        private int _mTimeoutMs;

        private Dictionary<string, JToken> _mTokenBuffer;

        private AutoResetEvent _mSuccessEvent = new AutoResetEvent(false);

        protected JetPeer MPeer;
        
        private Exception _mException = null;

        private string IP;
        private int interval;

        private JToken[] JTokenArray;
        private ushort[] DataUshortArray;
        private string[] DataStrArray;

        // Constructor with all parameters possible from class 'JetbusConnection' - Without ssh certification.
        //public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) : base(ipAddr, user, passwd, certificationCallback, timeoutMs = 5000)

        public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000)
        {
            this.connected = false;
            this.behavior = behavior;
            this.messages = new List<string>();

            _mTokenBuffer = new Dictionary<string, JToken>();

            this._mTimeoutMs = 5000; // values of 5000 according to the initialization in class JetBusConnection. 

            //ConnectOnPeer(user, passwd, timeoutMs);
            FetchAll();
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

        public int Read(object index)
        {
            try
            {
                return Convert.ToInt32(ReadObj(index));
            }
            catch (FormatException)
            {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }

        protected JToken ReadObj(object index)
        {
            
            switch (this.behavior)
            {
                case Behavior.ReadGrossValueSuccess:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;
                case Behavior.ReadGrossValueFail:
                    //throw new InterfaceException(new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                    return _mTokenBuffer[""];
                    break;

                case Behavior.ReadNetValueSuccess:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;
                case Behavior.ReadNetValueFail:
                    //throw new InterfaceException(new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                    return _mTokenBuffer[""];
                    break;

                case Behavior.ReadWeightMovingSuccess:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;
                case Behavior.ReadWeightMovingFail:
                    //throw new InterfaceException(new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                    return _mTokenBuffer[""];
                    break;

                default:
                    break;

            }


            this.ConvertJTokenToStringArray();

            return _mTokenBuffer[index.ToString()];
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

        public Dictionary<string, JToken> getTokenBuffer
        {
            get
            {
                return this._mTokenBuffer;
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

        public bool IsConnected
        {
            get
            {
                return this.connected;
            }

            set
            {
                this.connected = value;
            }
        }

        public string IpAddress
        {
            get
            {
                return this.IP;
            }

            set
            {
                this.IP = value;
            }
        }

       
        public void FetchAll()
        {

            this.OnFetchData(this.simulateJTokenInstance("123",123));
            
            bool success = true;

            BusActivityDetection?.Invoke(this, new LogEvent("Fetch-All success: " + success + " - buffersize is " + _mTokenBuffer.Count));            
        }

        protected virtual void WaitOne(int timeoutMultiplier = 1)
        {
            if (!_mSuccessEvent.WaitOne(_mTimeoutMs * timeoutMultiplier))
            {

                this.connected = false;
                //
                // Timeout-Exception
                //
                throw new InterfaceException(new TimeoutException("Interface Timeout - signal-handler will never reset"), 0x1);
            }
            if (_mException != null)
            {
                Exception exception = _mException;
                _mException = null;
                throw exception;
            }
        }

        /// <summary>
        /// Event with callend when raced a Fetch-Event by a other Peer.
        /// For testing it must be filled with pseudo data be tested in the UNIT tests. 
        /// </summary>
        /// <param name="data"></param>
        protected void OnFetchData(JToken data)
        {
            string path = data["path"].ToString();
            lock (_mTokenBuffer)
            {  
                //if(this.behavior==Behavior.ReadGrossValueSuccess)
                    _mTokenBuffer.Add("6144/00", simulateJTokenInstance("6144/00", 12345)["value"]);

                //if (this.behavior == Behavior.ReadNetValueSuccess)
                    _mTokenBuffer.Add("601A/01", simulateJTokenInstance("601A/01", 12345)["value"]);

                //if (this.behavior == Behavior.ReadWeightMovingSuccess)
                    _mTokenBuffer.Add("6153/00", simulateJTokenInstance("6153/00", 12345)["value"]);

                //if(this.behavior == Behavior.ReadWeightMovingSuccess)
                _mTokenBuffer.Add("6012/01", simulateJTokenInstance("6012/01", 12345)["value"]);

                BusActivityDetection?.Invoke(this, new LogEvent(data.ToString()));
            }
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

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Write(object index, int data)
        {
            switch(behavior)
            {
                case Behavior.WriteTareSuccess:
                    // The specific path and specific value for taring is added to the buffer _mTokenBuffer
                    _mTokenBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);
                    break;

                case Behavior.WriteTareFail:
                    // No path and no value is added to the buffer _mTokenBuffer
                    break;

                case Behavior.WriteGrossSuccess:
                    // The specific path and specific value for gross is added to the buffer _mTokenBuffer
                    _mTokenBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);
                    break;

                case Behavior.WriteGrossFail:
                    // No path and no value is added to the buffer _mTokenBuffer
                    break;

                case Behavior.WriteZeroSuccess:
                    // The specific path and specific value for gross is added to the buffer _mTokenBuffer
                    _mTokenBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);
                    break;

                case Behavior.WriteZeroFail:
                    // No path and no value is added to the buffer _mTokenBuffer
                    break;

                case Behavior.CalibrationSuccess:
                    // The specific path and specific value for calibration is added to the buffer _mTokenBuffer
                    if (index.Equals("6152/00"))
                        _mTokenBuffer.Add("6152/00", simulateJTokenInstance("6152/00", data)["value"]);

                    if (index.Equals("6002/01"))
                        _mTokenBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);

                    break;

                case Behavior.CalibrationFail:
                    // No path and no value is added to the buffer _mTokenBuffer
                    break;

                case Behavior.MeasureZeroSuccess:
                    _mTokenBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);
                    break;

                case Behavior.MeasureZeroFail:
                    // No path and no value is added to the buffer _mTokenBuffer
                    break;

                case Behavior.CalibratePreloadCapacitySuccess:

                    if (index.Equals("2110/06"))
                        _mTokenBuffer.Add("2110/06", simulateJTokenInstance("2110/06", data)["value"]);

                    if (index.Equals("2110/07"))
                        _mTokenBuffer.Add("2110/07", simulateJTokenInstance("2110/07", data)["value"]);

                    break;

                case Behavior.CalibratePreloadCapacityFail:
                    // No path and no value is added to the buffer _mTokenBuffer
                    break;

                default:
                    break; 

            }
        }

        public JToken simulateJTokenInstance(string index, int data)
        {

            FetchData fetchInstance = new FetchData
            {
                path = index,
                Event = "change",  // measure zero
                value = data,

            };

            return JToken.FromObject(fetchInstance);
        }


        public void WriteArray(ushort index, ushort[] data)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string json)
        {
            messages.Add(json);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public class FetchData
        {
            public string path { get; set; }
            public string Event { get; set; }
            public int value { get; set; }
        }


    }
}
