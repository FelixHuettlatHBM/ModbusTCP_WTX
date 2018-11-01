using Hbm.Devices.Jet;
using HBM.WT.API.WTX.Jet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetbusTest
{

    /// <summary>
    /// Class to simulate the peer. F.e. to simulate when a Fetch-Event is raced by a Peer. 
    /// In method 'Fetch(..)' the data is simulated to be read from the wtx device and this methods commits the data to 'TestJetBusConnection'
    /// for further operations. 
    /// </summary>
    public class TestJetPeer
    {
        private TestJetbusConnection _connection;
        private Behavior behavior;

        public TestJetPeer(Behavior _behaviorParameter, TestJetbusConnection _connectionParameter)
        {
            _connection = _connectionParameter;
            this.behavior = _behaviorParameter; 
        }
        
        // Method to simulate the fetching of data from the wtx device : By adding and changing paths to the data buffer(='_databuffer') and by calling an event in TestJetbusConnection with invoke.  
        public JObject Fetch(out FetchId id, Matcher matcher, Action<JToken> fetchCallback, Action<bool, JToken> responseCallback, double responseTimeoutMs)
        {
            string path="";
            string Event="";
            int data=0;

            if (!_connection._dataBuffer.ContainsKey("6014/01")) // Only if the dictionary does not contain the path "6014/01" the dictionary will be filled: 
            {
                _connection._dataBuffer.Add("6144/00", simulateJTokenInstance("6144/00", "add", 1)["value"]);   // Read 'gross value'
                _connection._dataBuffer.Add("601A/01", simulateJTokenInstance("601A/01", "add", 1)["value"]);   // Read 'net value'
                _connection._dataBuffer.Add("6153/00", simulateJTokenInstance("6153/00", "add", 1)["value"]);   // Read 'weight moving detection'        
                _connection._dataBuffer.Add("6012/01", simulateJTokenInstance("6012/01", "add", 1)["value"]);   // Read 'Weighing device 1 (scale) weight status'

                _connection._dataBuffer.Add("SDO", simulateJTokenInstance("SDO", "add", 1)["value"]);
                _connection._dataBuffer.Add("FRS1", simulateJTokenInstance("FRS1", "add", 1)["value"]);
                _connection._dataBuffer.Add("NDS", simulateJTokenInstance("NDS", "add", 1)["value"]);

                _connection._dataBuffer.Add("6014/01", simulateJTokenInstance("6014/01", "add", 0x4C0000)["value"]);    // Read Unit, prefix or fixed parameters - for t.

                _connection._dataBuffer.Add("6013/01", simulateJTokenInstance("6013/01", "add", 4)["value"]);   // Read 'Weight decimal point', f.e. = 4.
                _connection._dataBuffer.Add("IM1", simulateJTokenInstance("IM1", "add", 1)["value"]);
                _connection._dataBuffer.Add("IM2", simulateJTokenInstance("IM2", "add", 1)["value"]);
                _connection._dataBuffer.Add("IM3", simulateJTokenInstance("IM3", "add", 1)["value"]);
                _connection._dataBuffer.Add("IM4", simulateJTokenInstance("IM4", "add", 1)["value"]);

                _connection._dataBuffer.Add("OM1", simulateJTokenInstance("OM1", "add", 1)["value"]);
                _connection._dataBuffer.Add("OM2", simulateJTokenInstance("OM2", "add", 1)["value"]);
                _connection._dataBuffer.Add("OM3", simulateJTokenInstance("OM3", "add", 1)["value"]);
                _connection._dataBuffer.Add("OM4", simulateJTokenInstance("OM4", "add", 1)["value"]);

                _connection._dataBuffer.Add("OS1", simulateJTokenInstance("OS1", "add", 1)["value"]);
                _connection._dataBuffer.Add("OS2", simulateJTokenInstance("OS2", "add", 1)["value"]);
                _connection._dataBuffer.Add("OS3", simulateJTokenInstance("OS3", "add", 1)["value"]);
                _connection._dataBuffer.Add("OS4", simulateJTokenInstance("OS4", "add", 1)["value"]);

                _connection._dataBuffer.Add("CFT", simulateJTokenInstance("CFT", "add", 1)["value"]);
                _connection._dataBuffer.Add("FFT", simulateJTokenInstance("FFT", "add", 1)["value"]);
                _connection._dataBuffer.Add("TMD", simulateJTokenInstance("TMD", "add", 1)["value"]);
                _connection._dataBuffer.Add("UTL", simulateJTokenInstance("UTL", "add", 1)["value"]);
                _connection._dataBuffer.Add("LTL", simulateJTokenInstance("LTL", "add", 1)["value"]);
                _connection._dataBuffer.Add("MSW", simulateJTokenInstance("MSW", "add", 1)["value"]);
                _connection._dataBuffer.Add("EWT", simulateJTokenInstance("EWT", "add", 1)["value"]);
                _connection._dataBuffer.Add("TAD", simulateJTokenInstance("TAD", "add", 1)["value"]);
                _connection._dataBuffer.Add("CBT", simulateJTokenInstance("CBT", "add", 1)["value"]);
                _connection._dataBuffer.Add("CBK", simulateJTokenInstance("CBK", "add", 1)["value"]);
                _connection._dataBuffer.Add("FBK", simulateJTokenInstance("FBK", "add", 1)["value"]);
                _connection._dataBuffer.Add("FBT", simulateJTokenInstance("FBT", "add", 1)["value"]);
                _connection._dataBuffer.Add("SYD", simulateJTokenInstance("SYD", "add", 1)["value"]);
                _connection._dataBuffer.Add("VCT", simulateJTokenInstance("VCT", "add", 1)["value"]);
                _connection._dataBuffer.Add("EMD", simulateJTokenInstance("EMD", "add", 1)["value"]);
                _connection._dataBuffer.Add("CFD", simulateJTokenInstance("CFD", "add", 1)["value"]);
                _connection._dataBuffer.Add("FFD", simulateJTokenInstance("FFD", "add", 1)["value"]);
                _connection._dataBuffer.Add("SDM", simulateJTokenInstance("SDM", "add", 1)["value"]);
                _connection._dataBuffer.Add("SDS", simulateJTokenInstance("SDS", "add", 1)["value"]);
                _connection._dataBuffer.Add("RFT", simulateJTokenInstance("RFT", "add", 1)["value"]);

                _connection._dataBuffer.Add("MDT", simulateJTokenInstance("MDT", "add", 1)["value"]);
                _connection._dataBuffer.Add("FFM", simulateJTokenInstance("FFM", "add", 1)["value"]);
                _connection._dataBuffer.Add("OSN", simulateJTokenInstance("OSN", "add", 1)["value"]);
                _connection._dataBuffer.Add("FFL", simulateJTokenInstance("FFL", "add", 1)["value"]);
                _connection._dataBuffer.Add("DL1", simulateJTokenInstance("DL1", "add", 1)["value"]);
                _connection._dataBuffer.Add("6002/02", simulateJTokenInstance("6002/02", "add", 1801543519)["value"]); //StatusStringComment
                _connection._dataBuffer.Add("2020/25", simulateJTokenInstance("2020/25", "add", 0xA)["value"]);   // 0xA(hex)=1010(binary) //Limit value status:
             }

            if (this.behavior == Behavior.lb_UnitValue_Fail || this.behavior == Behavior.g_UnitValue_Fail || this.behavior == Behavior.kg_UnitValue_Fail || this.behavior == Behavior.t_UnitValue_Fail)
            {
                path = "6014/01";
                Event = "change";
                data = 0x00000000;
            }

            switch (this.behavior)
            {
                case Behavior.t_UnitValue_Success:
                    path = "6014/01";
                    Event = "change";
                    data = 0x004C0000;
                    break;
                case Behavior.kg_UnitValue_Success:
                    path = "6014/01";
                    Event = "change";
                    data = 0x00020000;
                    break;
                case Behavior.g_UnitValue_Success:
                    path = "6014/01";
                    Event = "change";
                    data = 0x004B0000;
                    break;
                case Behavior.lb_UnitValue_Success:
                    path = "6014/01";
                    Event = "change";
                    data = 0x00A60000;
                    break;
            }

            JToken JTokenobj = simulateJTokenInstance(path,Event,data);

            //fetchCallback?.Invoke(JTokenobj);

            id = null;

            fetchCallback = (JToken x) => _connection.OnFetchData(JTokenobj);

            fetchCallback.Invoke(JTokenobj);

            return JTokenobj.ToObject<JObject>();
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

        public class FetchData
        {
            public string path { get; set; }
            public string Event { get; set; }
            public int value { get; set; }
        }

        /*
        public JObject Authenticate(string user, string password, Action<bool, JToken> responseCallback, double responseTimeoutMs)
        {
        }
        
        public void Connect(Action<bool> completed, double timeoutMs)
        {
        }

        public void Disconnect()
        {           
        }
        */


    }
}
