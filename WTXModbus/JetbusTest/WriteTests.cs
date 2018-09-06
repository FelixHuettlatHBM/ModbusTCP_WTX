using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Jet;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetbusTest
{
    // Class for testing write functions of JetBusConnection, f.e. 'Write(path,data)' and 
    // 'WriteInt(object index)' and so on.
    [TestFixture]
    public class WriteTests
    {
        private TestJetbusConnection _jetTestConnection;
        private WtxJet _wtxObj;
        private int testGrossValue;


        // Test case source for writing values to the WTX120 device: Taring 
        public static IEnumerable WriteTareTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteTareFail).Returns(false);
                yield return new TestCaseData(Behavior.WriteTareSuccess).Returns(true);
            }
        }

        // Test case source for writing values to the WTX120 device: Set to gross
        public static IEnumerable WriteGrossTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteGrossFail).Returns(false);
                yield return new TestCaseData(Behavior.WriteGrossSuccess).Returns(true);
            }
        }

        // Test case source for writing values to the WTX120 device : Zeroing
        public static IEnumerable WriteZeroTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteZeroFail).Returns(false);
                yield return new TestCaseData(Behavior.WriteZeroSuccess).Returns(true);
            }
        }

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0;
        }


        [Test, TestCaseSource(typeof(WriteTests), "WriteTareTestCases")]
        public bool WriteTareTest(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            _jetTestConnection.Write("6002/01", 1701994868);

            if (_jetTestConnection.getTokenBuffer.ContainsKey("6002/01") && _jetTestConnection.getTokenBuffer.ContainsValue(1701994868))
                return true;

            else
                return false;

        }

        [Test, TestCaseSource(typeof(WriteTests), "WriteGrossTestCases")]
        public bool WriteGrossTest(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            _jetTestConnection.Write("6002/01", 1936683623);

            if (_jetTestConnection.getTokenBuffer.ContainsKey("6002/01") && _jetTestConnection.getTokenBuffer.ContainsValue(1936683623))
                return true;

            else
                return false;

        }

        [Test, TestCaseSource(typeof(WriteTests), "WriteZeroTestCases")]
        public bool WriteZeroTest(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            _jetTestConnection.Write("6002/01", 1869768058);
            
            if (_jetTestConnection.getTokenBuffer.ContainsKey("6002/01") && _jetTestConnection.getTokenBuffer.ContainsValue(1869768058)) 
                return true;

            else
                return false;          

        }

        private void OnConnect(bool obj)
        {
            //Callback, do something ... 
        }
    }
}

