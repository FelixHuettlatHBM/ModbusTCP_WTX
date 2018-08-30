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


        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable WriteTareTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteFail).Returns(false);
                yield return new TestCaseData(Behavior.WriteSuccess).Returns(true);
            }
        }

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0;
        }



        [Test, TestCaseSource(typeof(ReadTests), "WriteTareTestCases")]
        public bool WriteTareTest(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            _jetTestConnection.Write("6002/01", 1701994868);

            if (_jetTestConnection.getTokenBuffer.ContainsKey("6002/01"))
                return true;

            else
                return false;

            //Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6002/01"));

        }

        private void OnConnect(bool obj)
        {
            //Callback, do for example something ... 
        }
    }
}

