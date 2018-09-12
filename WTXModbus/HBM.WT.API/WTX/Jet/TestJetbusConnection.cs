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

        ReadFail_WEIGHING_DEVICE_1_WEIGHT_STATUS,
        ReadSuccess_WEIGHING_DEVICE_1_WEIGHT_STATUS,

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

        ReadFail_Decimals,
        ReadSuccess_Decimals,

        ReadFail_FillingProcessSatus,
        ReadSuccess_FillingProcessSatus,

        ReadFail_DosingResult,
        ReadSuccess_DosingResult,

        ReadFail_NumberDosingResults,
        ReadSuccess_NumberDosingResults,

        ReadFail_Unit,
        ReadSuccess_Unit,

        t_UnitValue_Fail,
        t_UnitValue_Success,

        kg_UnitValue_Fail,
        kg_UnitValue_Success,

        g_UnitValue_Fail,
        g_UnitValue_Success,

        lb_UnitValue_Fail,
        lb_UnitValue_Success,

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

                case Behavior.ReadSuccess_WEIGHING_DEVICE_1_WEIGHT_STATUS:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;
                case Behavior.ReadFail_WEIGHING_DEVICE_1_WEIGHT_STATUS:
                    //throw new InterfaceException(new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                    return _mTokenBuffer[""];
                    break;

                case Behavior.ReadSuccess_Decimals:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_Decimals:
                    return _mTokenBuffer[""];
                    break;

                case Behavior.ReadSuccess_DosingResult:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_DosingResult:
                    return _mTokenBuffer[""];
                    break;

                case Behavior.ReadSuccess_FillingProcessSatus:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_FillingProcessSatus:
                    return _mTokenBuffer[""];
                    break;

                case Behavior.ReadSuccess_NumberDosingResults:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_NumberDosingResults:
                    return _mTokenBuffer[""];

                case Behavior.ReadSuccess_Unit:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_Unit:
                    return _mTokenBuffer[""];

                case Behavior.t_UnitValue_Success:
                    if (_mTokenBuffer.ContainsKey(index.ToString()))
                        return _mTokenBuffer[index.ToString()];
                    break;

                case Behavior.t_UnitValue_Fail:
                    return _mTokenBuffer[""];

                default:
                    break;

            }
   
            this.ConvertJTokenToStringArray();

            RaiseDataEvent?.Invoke(this, new DataEvent(DataUshortArray, DataStrArray));

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

            this.ConvertJTokenToStringArray();

            RaiseDataEvent?.Invoke(this, new DataEvent(DataUshortArray, DataStrArray));

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
                    _mTokenBuffer.Add("6144/00", simulateJTokenInstance("6144/00", 12345)["value"]);   // Read 'gross value'

                    _mTokenBuffer.Add("601A/01", simulateJTokenInstance("601A/01", 12345)["value"]);    // Read 'net value'
    
                    _mTokenBuffer.Add("6153/00", simulateJTokenInstance("6153/00", 12345)["value"]);   // Read 'weight moving'
              
                    _mTokenBuffer.Add("6012/01", simulateJTokenInstance("6012/01", 12345)["value"]);  // Read 'weight moving'

                    _mTokenBuffer.Add("6013/01", simulateJTokenInstance("6013/01", 12345)["value"]);
                    _mTokenBuffer.Add("SDO", simulateJTokenInstance("SDO", 12345)["value"]);
                    _mTokenBuffer.Add("FRS1", simulateJTokenInstance("FRS1", 12345)["value"]);
                    _mTokenBuffer.Add("NDS", simulateJTokenInstance("FRS1", 12345)["value"]);

                    _mTokenBuffer.Add("6014/01", simulateJTokenInstance("6014/01", 0x4C0000)["value"]);    // Read Unit, prefix or fixed parameters - for t.

                // Hex and bin. for Unit testing: 

                // A6 = lb = 0x530000 = 10100110000000000000000
                // 02 = kg = 0x20000  = 100000000000000000
                // 4B = g  = 0x4B0000 = 10010110000000000000000
                // 4C = t  = 0x4C0000 = 10011000000000000000000

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
