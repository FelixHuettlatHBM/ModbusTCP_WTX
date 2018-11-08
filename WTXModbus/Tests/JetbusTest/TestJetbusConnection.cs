using System;

namespace HBM.Weighing.API.WTX.Jet
{
    using Hbm.Devices.Jet;
    using HBM.Weighing.API;
    using JetbusTest;
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

        DisconnectionFail,
        DisconnectionSuccess,

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

        NetGrossValueStringComment_4D_Fail,
        NetGrossValueStringComment_4D_Success,

        NetGrossValueStringComment_3D_Fail,
        NetGrossValueStringComment_3D_Success,

        NetGrossValueStringComment_2D_Fail,
        NetGrossValueStringComment_2D_Success,

        NetGrossValueStringComment_1D_Fail,
        NetGrossValueStringComment_1D_Success,

        ReadFail_Attributes,
        ReadSuccess_Attributes,

        StatusStringComment_Fail,
        StatusStringComment_Success,

        ReadFail_DataReceived,
        ReadSuccess_DataReceived,

    }

    public class TestJetbusConnection : INetConnection, IDisposable
    {
        private Behavior behavior;
        private List<string> messages;
        private bool connected;

        public event EventHandler BusActivityDetection;
        public event EventHandler<DataEvent> RaiseDataEvent;

        private int _mTimeoutMs;

        public Dictionary<string, JToken> _dataBuffer;

        private AutoResetEvent _mSuccessEvent = new AutoResetEvent(false);
                
        private Exception _mException = null;

        private string IP;
        private int interval;

        private JToken[] JTokenArray;
        private ushort[] DataUshortArray;
        private string[] DataStrArray;

        private TestJetPeer _peer;

        // Constructor with all parameters possible from class 'JetbusConnection' - Without ssh certification.
        //public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000) : base(ipAddr, user, passwd, certificationCallback, timeoutMs = 5000)

        public TestJetbusConnection(Behavior behavior, string ipAddr, string user, string passwd, RemoteCertificateValidationCallback certificationCallback, int timeoutMs = 5000)
        {
            //IJetConnection jetConnection = new WebSocketJetConnection(_uri, RemoteCertificationCheck);

            _peer = new TestJetPeer(behavior, this);

            this.connected = false;
            this.behavior = behavior;
            this.messages = new List<string>();

            _dataBuffer = new Dictionary<string, JToken>();

            this._mTimeoutMs = 5000; // values of 5000 according to the initialization in class JetBusConnection. 

            //ConnectOnPeer(user, passwd, timeoutMs);
            FetchAll();
        }

        public Behavior GetBehavior
        {
            get
            {
                return this.behavior;
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

        public int Read(object index)
        {
            try
            {
                return Convert.ToInt32(ReadObj(index));
            }
            catch (FormatException)
            {
                throw new Exception("Invalid data format");
            }
        }

        protected JToken ReadObj(object index)
        {
            switch (this.behavior)
            {             
                case Behavior.CalibrationSuccess:

                    if (_dataBuffer.ContainsValue(1801543519))
                        _dataBuffer["6002/02"] = 1634168417;  // = command 'on go', in exection.
                    else
                       if (_dataBuffer.ContainsValue(1634168417))
                        _dataBuffer["6002/02"] = 1801543519;  // = command ok, done. 

                    return _dataBuffer["6002/02"];

                case Behavior.CalibrationFail:

                    if (_dataBuffer.ContainsValue(1801543519))
                        _dataBuffer["6002/02"] = 1634168417;  // = command 'on go', in exection.
                    else
                       if (_dataBuffer.ContainsValue(1634168417))
                        _dataBuffer["6002/02"] = 1801543519;  // = command ok, done. 

                    return _dataBuffer["6002/02"];

                case Behavior.MeasureZeroSuccess:

                    if (_dataBuffer.ContainsValue(1801543519))
                        _dataBuffer["6002/02"] = 1634168417;  // = command 'on go', in exection.
                    else
                        if (_dataBuffer.ContainsValue(1634168417))
                        _dataBuffer["6002/02"] = 1801543519;  // = command ok, done. 

                    return _dataBuffer["6002/02"];
                    
                case Behavior.MeasureZeroFail:

                    if (_dataBuffer.ContainsValue(1801543519))
                        _dataBuffer["6002/02"] = 1634168417;  // = command 'on go', in exection.
                    else
                        if (_dataBuffer.ContainsValue(1634168417))
                        _dataBuffer["6002/02"] = 1801543519;  // = command ok, done. 

                    return _dataBuffer["6002/02"];
                    
                default:
                    break;               
            }
           
            this.ConvertJTokenToStringArray();

            if (this.behavior != Behavior.ReadFail_DataReceived)
                RaiseDataEvent?.Invoke(this, new DataEvent(DataUshortArray, DataStrArray));

            return _dataBuffer[index.ToString()];
            
        }


        private void ConvertJTokenToStringArray()
        {
            JTokenArray = _dataBuffer.Values.ToArray();
            DataUshortArray = new ushort[JTokenArray.Length];
            DataStrArray = new string[JTokenArray.Length];

            for (int i = 0; i < JTokenArray.Length; i++)
            {
                JToken JTokenElement = JTokenArray[i];
                DataStrArray[i] = JTokenElement.ToString();
            }

        }

        public Dictionary<string, int> getData()
        {
            this.FetchAll();
            
            Dictionary<string, int> newDict = new Dictionary<string, int>();

            foreach (var element in _dataBuffer)
            {
                int i = 0;

                if (int.TryParse(element.Value.ToString(), out i))
                    newDict.Add(element.Key, Convert.ToInt32(element.Value.ToString()));
            }

            return newDict;
        }

        public Dictionary<string, JToken> getDataBuffer
        {
            get
            {
                return this._dataBuffer;
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

            //this.OnFetchData(this.simulateJTokenInstance("123", "add", 123));

            Matcher matcher = new Matcher();
            FetchId id;

            _peer.Fetch(out id, matcher, OnFetchData, null, 500); // Onfetch = null (given by 'JetBusConnection'), timeoutms=500;

            bool success = true;

            this.ConvertJTokenToStringArray();

            if (this.behavior != Behavior.ReadFail_DataReceived)
                RaiseDataEvent?.Invoke(this, new DataEvent(DataUshortArray, DataStrArray));

            BusActivityDetection?.Invoke(this, new LogEvent("Fetch-All success: " + success + " - buffersize is " + _dataBuffer.Count));            
        }

        protected virtual void WaitOne(int timeoutMultiplier = 1)
        {
            if (!_mSuccessEvent.WaitOne(_mTimeoutMs * timeoutMultiplier))
            {

                this.connected = false;
                //
                // Timeout-Exception
                //

                throw new TimeoutException("Interface Timeout - signal-handler will never reset");
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
        public void OnFetchData(JToken data)
        {
            string path = data["path"].ToString();
            string Event = data["Event"].ToString();
            lock (_dataBuffer)
            {
                switch (Event)
                {
                    case "add":
                        _dataBuffer.Add(path, data["value"]);

                        break;

                    case "fetch":
                        _dataBuffer[path] = data["value"];

                        break;

                    case "change":
                        _dataBuffer[path] = data["value"];

                        break;
                }
                
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
            switch (this.behavior)
            {
                case Behavior.DisconnectionFail:
                    connected = true;
                    break;

                case Behavior.DisconnectionSuccess:
                    connected = false;
                    break;

                default:
                    connected = true;
                    break;
            }
        }

        public void Write(object index, int data)
        {
            switch (behavior)
            {
                case Behavior.WriteTareSuccess:
                    // The specific path and specific value for taring is added to the buffer _dataBuffer
                    _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", "change", data)["value"]);
                    break;

                case Behavior.WriteTareFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                case Behavior.WriteGrossSuccess:
                    // The specific path and specific value for gross is added to the buffer _dataBuffer
                    _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", "change", data)["value"]);
                    break;

                case Behavior.WriteGrossFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                case Behavior.WriteZeroSuccess:
                    // The specific path and specific value for gross is added to the buffer _dataBuffer
                    _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", "change", data)["value"]);
                    break;

                case Behavior.WriteZeroFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                case Behavior.CalibrationSuccess:
                    // For Calibration : The specific path and specific value for calibration is added to the buffer _dataBuffer
                    if (index.Equals("6152/00"))
                        _dataBuffer.Add("6152/00", simulateJTokenInstance("6152/00", "change", data)["value"]);
                    if (index.Equals("6002/01"))
                        _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", "change", data)["value"]);

                    break;

                case Behavior.CalibrationFail:
                    // A wrong value is added at the specific path to the buffer _dataBuffer
                    if (index.Equals("6002/01"))
                        _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", "change", 0)["value"]);
                    break;

                case Behavior.MeasureZeroSuccess:
                    // For setting to zero(=Measure zero) : The specific path and specific value for calibration is added to the buffer _dataBuffer
                    if (index.Equals("6002/01"))
                        _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", "change", data)["value"]);
                    break;

                case Behavior.MeasureZeroFail:
                    // A wrong value is added at the specific path to the buffer _dataBuffer
                    if (index.Equals("6002/01"))
                        _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", "change", 0)["value"]);
                    break;

                case Behavior.CalibratePreloadCapacitySuccess:

                    if (index.Equals("2110/06"))
                        _dataBuffer.Add("2110/06", simulateJTokenInstance("2110/06", "change", data)["value"]);

                    if (index.Equals("2110/07"))
                        _dataBuffer.Add("2110/07", simulateJTokenInstance("2110/07", "change", data)["value"]);

                    break;

                case Behavior.CalibratePreloadCapacityFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                default:
                    break; 

            }
        }

        public JToken simulateJTokenInstance(string pathParam, string eventParam, int data)
        {

            FetchData fetchInstance = new FetchData
            {
                path = pathParam,    // For path  = "6014/01" (f.e.)
                Event = eventParam,  // For event = "add" || "change" || "fetch" 
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


// To method Read(..) in 'TestJetBusConnection.cs':
/*
 *                 case Behavior.g_UnitValue_Success:

                    _dataBuffer["6014/01"] = 0x004B0000;
                    return _dataBuffer["6014/01"];
               break;

                case Behavior.g_UnitValue_Fail:
                    return _dataBuffer[""];
                   break;

                case Behavior.kg_UnitValue_Success:
                    
                    if (_dataBuffer.ContainsKey(index.ToString()))
                    {
                        _dataBuffer[index.ToString()] = 0x00020000;
                        return _dataBuffer[index.ToString()];
                    }

                    _dataBuffer["6014/01"] = 0x00020000;
                    return _dataBuffer["6014/01"];
                    break;

                case Behavior.kg_UnitValue_Fail:
                        return _dataBuffer[""];
                       break;

                case Behavior.t_UnitValue_Success:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                    {
                        _dataBuffer[index.ToString()] = 0x004C0000;
                        return _dataBuffer[index.ToString()];
                    }                   
                    break;

                case Behavior.t_UnitValue_Fail:
                    return _dataBuffer[""];
                    break;


                case Behavior.lb_UnitValue_Success:
                    _dataBuffer["6014/01"]= 0x00A60000;
                    return _dataBuffer["6014/01"]; 
                    break;

                case Behavior.lb_UnitValue_Fail:
                    return _dataBuffer[""];
                    break;

*/