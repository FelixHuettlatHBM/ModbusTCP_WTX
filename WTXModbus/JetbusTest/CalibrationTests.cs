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
    [TestFixture]
    public class CalibrationTests
    {
        private TestJetbusConnection _jetTestConnection;
        private WtxJet _wtxObj;
        private int testGrossValue;


        // Test case source for writing values to the WTX120 device: Taring 
        public static IEnumerable CalibrationTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.CalibrationFail).Returns(false);
                yield return new TestCaseData(Behavior.CalibrationSuccess).Returns(true);
            }
        }

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0;
        }


        [Test, TestCaseSource(typeof(CalibrationTests), "CalibrationTestCases")]
        public bool CalibrationbTest(Behavior behavior)
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

        private void OnConnect(bool obj)
        {
            //Callback, do something ... 
        }
    }
}
