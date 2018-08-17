using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Jet;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetbusTest
{

    // Class for testing read functions of JetBusConnection, like 'OnFetchData(JToken data)' and 
    // 'JToken ReadObj(object index)'.
    // In class JetBusConnection at #region read-functions:
    [TestFixture]
    public class ReadTests
    {
        private TestJetbusConnection _jetTestConnection;
        private WtxJet _wtxObj;
        private int testGrossValue;

        private string[] testTokenBuffer;

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ReadGrossTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ReadFail).ExpectedResult = "";
                yield return new TestCaseData(Behavior.ReadSuccess).ExpectedResult = "6144 / 00";
            }
        }

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0;

            testTokenBuffer = new string[10];
        }

        [Test, TestCaseSource(typeof(ConnectTestsJetbus), "ReadGrossTestCases")]
        public void testReadGrossValue(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new HBM.WT.API.WTX.WtxJet((JetBusConnection)_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 5000);

            testGrossValue = _wtxObj.GrossValue;
            
            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6144 / 00"));

        }

        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }

    }
}

/*
 * 

     #region read-functions

       /*
        protected virtual JToken ReadObj(object index)
        {
            lock (_mTokenBuffer)
            {
                if (_mTokenBuffer.ContainsKey(index.ToString()))
                {
                    return _mTokenBuffer[index.ToString()];
                }
                else
                {
                    throw new InterfaceException(
                        new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                }
            }
        }
        */


    /*
/// Event with callend when raced a Fetch-Event by a other Peer.

protected virtual void OnFetchData(JToken data) {
            string path = data["path"].ToString();
            lock (_mTokenBuffer) {
                switch (data["event"].ToString()) {
                    case "add": _mTokenBuffer.Add(path, data["value"]); break;
                    case "fetch": _mTokenBuffer[path] = data["value"]; break;
                    case "change":
                        _mTokenBuffer[path] = data["value"];
                        break;
                }

                BusActivityDetection?.Invoke(this, new LogEvent(data.ToString()));

                // Alternative: 
                //BusActivityDetection?.Invoke(this, new NetConnectionEventArgs<string>(EventArgType.Message, data.ToString()));

                // Äquivalent zu ...
                //if(BusActivityDetection != null){
                //     BusActivityDetection(this, new NetConnectionEventArgs<string>(EventArgType.Message, data.ToString()));
                //}
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual JToken ReadObj(object index) {
            lock (_mTokenBuffer) {
                if (_mTokenBuffer.ContainsKey(index.ToString())) {
                    return _mTokenBuffer[index.ToString()];
                }
                else {
                    throw new InterfaceException(
                        new KeyNotFoundException("Object does not exist in the object dictionary"), 0);
                }
            }
        }
        
        /*
        public T Read<T>(object index) {
            try {
                JToken token = ReadObj(index);
                return (T)Convert.ChangeType(token, typeof(T));
            }
            catch (FormatException) {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }
        

        public int Read(object index)
        {
            try
            {
                return Convert.ToInt32(ReadObj(index));

                //JToken token = ReadObj(index);
                //return token;

                //return (T)Convert.ChangeType(token, typeof(T));
            }
            catch (FormatException)
            {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }



        public void Write(object index, int value)
        {
            JValue jValue = new JValue(value);
            SetData(index, jValue);
        }

        public int ReadInt(object index)
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

        public long ReadDint(object index)
        {
            try
            {
                return Convert.ToInt64(ReadObj(index));
            }
            catch (FormatException)
            {
                throw new InterfaceException(new FormatException("Invalid data format"), 0);
            }
        }

        public string ReadAsc(object index)
        {
            return ReadObj(index).ToString();
        }
*/
