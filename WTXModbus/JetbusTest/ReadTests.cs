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


        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testInput1(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Input1;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("IM1"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testInput2(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Input2;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("IM2"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testInput3(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Input3;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("IM3"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testInput4(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Input4;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("IM4"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testOutput1(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Output1;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OM1"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testOutput2(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Output2;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OM2"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testOutput3(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Output3;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OM3"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testOutput4(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Output4;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OM4"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testLimitStatus1(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus1;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OS1"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testLimitStatus2(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus2;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OS2"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testLimitStatus3(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus3;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OS3"));
        }


        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testLimitStatus4(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus4;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OS4"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testMaxDosingTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MaxDosingTime;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("MDT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testMeanValueDosingResults(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MeanValueDosingResults;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("SDM"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testStandardDeviation(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.StandardDeviation;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("SDS"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testFineFlowCutOffPoint(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FineFlowCutOffPoint;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("FFD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testCoarseFlowCutOffPoint(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.CoarseFlowCutOffPoint;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("CFD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testResidualFlowTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ResidualFlowTime;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("RFT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testMinimumFineFlow(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MinimumFineFlow;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("FFM"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testOptimizationOfCutOffPoints(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.OptimizationOfCutOffPoints;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("OSN"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testMaximumDosingTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MaximumDosingTime;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("MDT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testCoarseLockoutTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.CoarseLockoutTime;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("CFT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testFineLockoutTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FineLockoutTime;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("FFT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testTareMode(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.TareMode;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("TMD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testUpperToleranceLimit(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.UpperToleranceLimit;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("UTL"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testLowerToleranceLimit(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LowerToleranceLimit;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("LTL"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testMinimumStartWeight(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MinimumStartWeight;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("MSW"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testEmptyWeight(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.EmptyWeight;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("EWT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testTareDelay(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.TareDelay;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("TAD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testCoarseFlowMonitoringTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.CoarseFlowMonitoringTime;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("CBT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testCoarseFlowMonitoring(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.CoarseFlowMonitoring;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("CBK"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testFineFlowMonitoring(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FineFlowMonitoring;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("FBK"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testFineFlowMonitoringTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FineFlowMonitoringTime;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("FBT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testSystematicDifference(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.SystematicDifference;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("SYD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testValveControl(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ValveControl;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("VCT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testEmptyingMode(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.EmptyingMode;

            Assert.IsTrue(_jetTestConnection.getTokenBuffer.ContainsKey("EMD"));
        }
    }
}
