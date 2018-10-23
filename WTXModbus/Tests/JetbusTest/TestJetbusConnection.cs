using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;


// Look also on the tests on GitHub at a related project for SharpJet : https://github.com/gatzka/SharpJet/tree/master/SharpJetTests

namespace HBM.WT.API.WTX.Jet
{
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

        private Dictionary<string, JToken> _dataBuffer;

        private AutoResetEvent _mSuccessEvent = new AutoResetEvent(false);
                
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

            _dataBuffer = new Dictionary<string, JToken>();

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
                throw new Exception("Invalid data format");
            }
        }

        protected JToken ReadObj(object index)
        {
            
            switch (this.behavior)
            {
                case Behavior.ReadGrossValueSuccess:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;
                case Behavior.ReadGrossValueFail:
                    return _dataBuffer[""];
                    break;

                case Behavior.ReadNetValueSuccess:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;
                case Behavior.ReadNetValueFail:
                    return _dataBuffer[""];
                    break;

                case Behavior.ReadSuccess_WEIGHING_DEVICE_1_WEIGHT_STATUS:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;
                case Behavior.ReadFail_WEIGHING_DEVICE_1_WEIGHT_STATUS:
                    return _dataBuffer[""];
                    break;

                case Behavior.ReadSuccess_Decimals:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_Decimals:
                    return _dataBuffer[""];
                    break;

                case Behavior.ReadSuccess_DosingResult:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_DosingResult:
                    return _dataBuffer[""];
                    break;

                case Behavior.ReadSuccess_FillingProcessSatus:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_FillingProcessSatus:
                    return _dataBuffer[""];
                    break;

                case Behavior.ReadSuccess_NumberDosingResults:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_NumberDosingResults:
                    return _dataBuffer[""];

                case Behavior.ReadSuccess_Unit:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;

                case Behavior.ReadFail_Unit:
                    return _dataBuffer[""];

                case Behavior.t_UnitValue_Success:
                    if (_dataBuffer.ContainsKey(index.ToString()))
                        return _dataBuffer[index.ToString()];
                    break;

                case Behavior.t_UnitValue_Fail:
                    return _dataBuffer[""];

                case Behavior.NetGrossValueStringComment_4D_Success:
                    return _dataBuffer[""];
                    break;

                case Behavior.NetGrossValueStringComment_4D_Fail:
                    return "";
                    break; 

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

            this.OnFetchData(this.simulateJTokenInstance("123",123));
            
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
        protected void OnFetchData(JToken data)
        {
            string path = data["path"].ToString();
            lock (_dataBuffer)
            {  
                    _dataBuffer.Add("6144/00", simulateJTokenInstance("6144/00", 1)["value"]);   // Read 'gross value'
                    _dataBuffer.Add("601A/01", simulateJTokenInstance("601A/01", 1)["value"]);   // Read 'net value'
                    _dataBuffer.Add("6153/00", simulateJTokenInstance("6153/00", 1)["value"]);   // Read 'weight moving detection'        
                    _dataBuffer.Add("6012/01", simulateJTokenInstance("6012/01", 1)["value"]);   // Read 'Weighing device 1 (scale) weight status'
              
                    _dataBuffer.Add("SDO", simulateJTokenInstance("SDO", 1)["value"]);
                    _dataBuffer.Add("FRS1", simulateJTokenInstance("FRS1", 1)["value"]);
                    _dataBuffer.Add("NDS", simulateJTokenInstance("NDS", 1)["value"]);

                    _dataBuffer.Add("6014/01", simulateJTokenInstance("6014/01", 0x4C0000)["value"]);    // Read Unit, prefix or fixed parameters - for t.
                
                    _dataBuffer.Add("6013/01", simulateJTokenInstance("6013/01", 4)["value"]);   // Read 'Weight decimal point', f.e. = 4.
                    _dataBuffer.Add("IM1", simulateJTokenInstance("IM1", 1)["value"]);
                    _dataBuffer.Add("IM2", simulateJTokenInstance("IM2", 1)["value"]);
                    _dataBuffer.Add("IM3", simulateJTokenInstance("IM3", 1)["value"]);
                    _dataBuffer.Add("IM4", simulateJTokenInstance("IM4", 1)["value"]);

                    _dataBuffer.Add("OM1", simulateJTokenInstance("OM1", 1)["value"]); 
                    _dataBuffer.Add("OM2", simulateJTokenInstance("OM2", 1)["value"]);
                    _dataBuffer.Add("OM3", simulateJTokenInstance("OM3", 1)["value"]);
                    _dataBuffer.Add("OM4", simulateJTokenInstance("OM4", 1)["value"]);

                    _dataBuffer.Add("OS1", simulateJTokenInstance("OS1", 1)["value"]); 
                    _dataBuffer.Add("OS2", simulateJTokenInstance("OS2", 1)["value"]);
                    _dataBuffer.Add("OS3", simulateJTokenInstance("OS3", 1)["value"]);
                    _dataBuffer.Add("OS4", simulateJTokenInstance("OS4", 1)["value"]);

                    _dataBuffer.Add("CFT", simulateJTokenInstance("CFT", 1)["value"]);
                    _dataBuffer.Add("FFT", simulateJTokenInstance("FFT", 1)["value"]);
                    _dataBuffer.Add("TMD", simulateJTokenInstance("TMD", 1)["value"]);
                    _dataBuffer.Add("UTL", simulateJTokenInstance("UTL", 1)["value"]);
                    _dataBuffer.Add("LTL", simulateJTokenInstance("LTL", 1)["value"]);
                    _dataBuffer.Add("MSW", simulateJTokenInstance("MSW", 1)["value"]);
                    _dataBuffer.Add("EWT", simulateJTokenInstance("EWT", 1)["value"]);
                    _dataBuffer.Add("TAD", simulateJTokenInstance("TAD", 1)["value"]);
                    _dataBuffer.Add("CBT", simulateJTokenInstance("CBT", 1)["value"]);
                    _dataBuffer.Add("CBK", simulateJTokenInstance("CBK", 1)["value"]);
                    _dataBuffer.Add("FBK", simulateJTokenInstance("FBK", 1)["value"]);
                    _dataBuffer.Add("FBT", simulateJTokenInstance("FBT", 1)["value"]);
                    _dataBuffer.Add("SYD", simulateJTokenInstance("SYD", 1)["value"]);
                    _dataBuffer.Add("VCT", simulateJTokenInstance("VCT", 1)["value"]);
                    _dataBuffer.Add("EMD", simulateJTokenInstance("EMD", 1)["value"]);
                    _dataBuffer.Add("CFD", simulateJTokenInstance("CFD", 1)["value"]);
                    _dataBuffer.Add("FFD", simulateJTokenInstance("FFD", 1)["value"]);
                    _dataBuffer.Add("SDM", simulateJTokenInstance("SDM", 1)["value"]);
                    _dataBuffer.Add("SDS", simulateJTokenInstance("SDS", 1)["value"]);
                    _dataBuffer.Add("RFT", simulateJTokenInstance("RFT", 1)["value"]);

                    _dataBuffer.Add("MDT", simulateJTokenInstance("MDT", 1)["value"]);
                    _dataBuffer.Add("FFM", simulateJTokenInstance("FFM", 1)["value"]);
                    _dataBuffer.Add("OSN", simulateJTokenInstance("OSN", 1)["value"]);
                    _dataBuffer.Add("FFL", simulateJTokenInstance("FFL", 1)["value"]);
                    _dataBuffer.Add("DL1", simulateJTokenInstance("DL1", 1)["value"]);

                //StatusStringComment:

                _dataBuffer.Add("6002/02", simulateJTokenInstance("6002/02", 1801543519)["value"]);

                //Limit value status:

                _dataBuffer.Add("2020/25", simulateJTokenInstance("2020/25", 0xA)["value"]);   // 0xA(hex)=1010(binary)

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
            switch(behavior)
            {
                case Behavior.WriteTareSuccess:
                    // The specific path and specific value for taring is added to the buffer _dataBuffer
                    _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);
                    break;

                case Behavior.WriteTareFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                case Behavior.WriteGrossSuccess:
                    // The specific path and specific value for gross is added to the buffer _dataBuffer
                    _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);
                    break;

                case Behavior.WriteGrossFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                case Behavior.WriteZeroSuccess:
                    // The specific path and specific value for gross is added to the buffer _dataBuffer
                    _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);
                    break;

                case Behavior.WriteZeroFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                case Behavior.CalibrationSuccess:
                    // The specific path and specific value for calibration is added to the buffer _dataBuffer
                    if (index.Equals("6152/00"))
                        _dataBuffer.Add("6152/00", simulateJTokenInstance("6152/00", data)["value"]);

                    if (index.Equals("6002/01"))
                        _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);

                    break;

                case Behavior.CalibrationFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                case Behavior.MeasureZeroSuccess:
                    _dataBuffer.Add("6002/01", simulateJTokenInstance("6002/01", data)["value"]);
                    break;

                case Behavior.MeasureZeroFail:
                    // No path and no value is added to the buffer _dataBuffer
                    break;

                case Behavior.CalibratePreloadCapacitySuccess:

                    if (index.Equals("2110/06"))
                        _dataBuffer.Add("2110/06", simulateJTokenInstance("2110/06", data)["value"]);

                    if (index.Equals("2110/07"))
                        _dataBuffer.Add("2110/07", simulateJTokenInstance("2110/07", data)["value"]);

                    break;

                case Behavior.CalibratePreloadCapacityFail:
                    // No path and no value is added to the buffer _dataBuffer
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
