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
        

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ReadGrossTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ReadFail).ExpectedResult = 0; // ""
                yield return new TestCaseData(Behavior.ReadSuccess).ExpectedResult = 1; // "6144 / 00"
            }
        }

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0;            
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadGrossTestCases")]
        public void testReadGrossValue(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            //_jetTestConnection.FetchAll();

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.GrossValue;
        
            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6144/00"));

        }

        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }

    }
}
