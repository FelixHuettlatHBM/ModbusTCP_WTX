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

        
        // Test case source for writing values to the WTX120 device: Taring 
        public static IEnumerable CalibrationPreloadCapacityTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.CalibratePreloadCapacityFail).Returns(false);
                yield return new TestCaseData(Behavior.CalibratePreloadCapacitySuccess).Returns(true);
            }
        }

        // Test case source for writing values to the WTX120 device: Taring 
        public static IEnumerable MeasureZeroTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.MeasureZeroFail).Returns(false);
                yield return new TestCaseData(Behavior.MeasureZeroSuccess).Returns(true);
            }
        }

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0;
        }


        [Test, TestCaseSource(typeof(CalibrationTests), "CalibrationTestCases")]
        public bool CalibrationTest(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);
           
            _wtxObj.Calibrate(15000, "15000");

            if (_jetTestConnection.getDataBuffer.ContainsKey("6152/00") && _jetTestConnection.getDataBuffer.ContainsValue(15000) &&       // LFT_SCALE_CALIBRATION_WEIGHT = "6152/00" 
                _jetTestConnection.getDataBuffer.ContainsKey("6002/01") && _jetTestConnection.getDataBuffer.ContainsValue(1852596579) &&  // CALIBRATE_NOMINAL_WEIGHT = 1852596579 // SCALE_COMMAND = "6002/01"
                _jetTestConnection.getDataBuffer.ContainsKey("6002/02") && _jetTestConnection.getDataBuffer.ContainsValue(1801543519)
                )

                return true;

            else
                return false;

        }

        [Test, TestCaseSource(typeof(CalibrationTests), "MeasureZeroTestCases")]
        public bool MeasureZeroTest(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            _wtxObj.MeasureZero();

            if (
                _jetTestConnection.getDataBuffer.ContainsKey("6002/01") && _jetTestConnection.getDataBuffer.ContainsValue(2053923171) &&
                _jetTestConnection.getDataBuffer.ContainsKey("6002/02") && _jetTestConnection.getDataBuffer.ContainsValue(1801543519)
                )
                return true;

            else
                return false;
        }

        [Test, TestCaseSource(typeof(CalibrationTests), "CalibrationPreloadCapacityTestCases")]
        public bool CalibrationPreloadCapacityTest(Behavior behavior)
        {
            double preload = 1;
            double capacity = 2;

            double testdPreload = 0;
            double testdNominalLoad = 0;
            int testIntPreload = 0;
            int testIntNominalLoad = 0;

            double multiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)

            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            _wtxObj.Calculate(preload, capacity);

            testdPreload = preload * multiplierMv2D;
            testdNominalLoad = testdPreload + (capacity * multiplierMv2D);

            testIntPreload = Convert.ToInt32(testdPreload);
            testIntNominalLoad = Convert.ToInt32(testdPreload);

            if (
                _jetTestConnection.getDataBuffer.ContainsKey("2110/06") && _jetTestConnection.getDataBuffer.ContainsValue(testIntPreload) &&
                _jetTestConnection.getDataBuffer.ContainsKey("2110/07") && _jetTestConnection.getDataBuffer.ContainsValue(testIntNominalLoad) 
                )

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
