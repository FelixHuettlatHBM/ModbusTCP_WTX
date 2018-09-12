using HBM.WT.API;
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
        public static IEnumerable ReadGrossValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ReadGrossValueFail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadGrossValueSuccess).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ReadNetValueTestCases
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadNetValueFail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadNetValueSuccess).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadFail_WEIGHING_DEVICE_1_WEIGHT_STATUS).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadSuccess_WEIGHING_DEVICE_1_WEIGHT_STATUS).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device : Decimals 
        public static IEnumerable ReadTestCases_Decimals
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadFail_Decimals).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadSuccess_Decimals).ExpectedResult = 1;
            }
        }


        // Test case source for reading values from the WTX120 device : Filling process status 
        public static IEnumerable ReadTestCases_FillingProcessSatus
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadFail_FillingProcessSatus).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadSuccess_FillingProcessSatus).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device : Dosing result 
        public static IEnumerable ReadTestCases_DosingResult
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadFail_DosingResult).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadSuccess_DosingResult).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device : Number of dosing results 
        public static IEnumerable ReadTestCases_NumberDosingResults
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadFail_NumberDosingResults).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadSuccess_NumberDosingResults).ExpectedResult = 1;
            }
        }


        // Test case source for reading values from the WTX120 device : Unit or prefix or fixed parameters 
        public static IEnumerable ReadTestCases_Unit
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadFail_Unit).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadSuccess_Unit).ExpectedResult = 1;
            }
        }

        

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0;            
        }
      
        [Test, TestCaseSource(typeof(ReadTests), "ReadGrossValueTestCases")]
        public void testReadGrossValue(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);
            
            _wtxObj.Connect(this.OnConnect, 100);        

            testGrossValue = _wtxObj.GrossValue;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6144/00"));

            /*
            if (_jetTestConnection.getTokenBuffer.ContainsKey("6144/00"))
                return true;
            else
                if (_jetTestConnection.getTokenBuffer.ContainsKey(""))
                return false;
            return false;
           */

        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadNetValueTestCases")]
        public void testReadNetValue(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.NetValue;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("601A/01"));
        }


        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testWeightMovingValue(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.WeightMoving;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testGeneralWeightError(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.GeneralWeightError;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testScaleAlarmTriggered(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ScaleAlarmTriggered;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testLimitStatus(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testScaleSealIsOpen(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ScaleSealIsOpen;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testManualTare(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ManualTare;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testWeightType(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.WeightType;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testScaleRange(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ScaleRange;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testZeroRequired(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ZeroRequired;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testWeightWithinTheCenterOfZero(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.WeightWithinTheCenterOfZero;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testWeightInZeroRange(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.WeightInZeroRange;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6012/01"));
        }



        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Decimals")]
        public void testDecimals(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Decimals;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6013/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_FillingProcessSatus")]
        public void testFillingProcessStatus(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FillingProcessStatus;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("SDO"));
        }
        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_DosingResult")]
        public void testDosingResult(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.DosingResult;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("FRS1"));
        }


        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_NumberDosingResults")]
        public void testNumberDosingResults(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.NumberDosingResults;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("NDS"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testUnit(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Unit;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("6014/01"));
        }


    }
}
