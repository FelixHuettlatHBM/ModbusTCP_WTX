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
using System.Threading;
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

        // Test case source for reading values from the WTX120 device : Unit or prefix or fixed parameters 
        public static IEnumerable ReadTestCases_Attributes
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadFail_Attributes).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.ReadSuccess_Attributes).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device : Have data been reveiceid? (Property 'isDataReceived') 
        public static IEnumerable ReadTestCases_DataReceived
        {
            get
            {

                yield return new TestCaseData(Behavior.ReadFail_DataReceived).Returns(false);
                yield return new TestCaseData(Behavior.ReadSuccess_DataReceived).Returns(true);
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

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6144/00"));

            /*
            if (_jetTestConnection.getDataBuffer.ContainsKey("6144/00"))
                return true;
            else
                if (_jetTestConnection.getDataBuffer.ContainsKey(""))
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

            Assert.IsTrue(_jetTestConnection.getData().ContainsKey("601A/01"));

            //Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("601A/01"));
        }


        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testWeightMovingValue(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.WeightMoving;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
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

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testScaleAlarmTriggered(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ScaleAlarmTriggered;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testLimitStatus(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testScaleSealIsOpen(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ScaleSealIsOpen;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testManualTare(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ManualTare;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testWeightType(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.WeightType;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testScaleRange(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ScaleRange;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testZeroRequired(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ZeroRequired;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testWeightWithinTheCenterOfZero(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.WeightWithinTheCenterOfZero;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_WEIGHING_DEVICE_1_WEIGHT_STATUS")]
        public void testWeightInZeroRange(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.WeightInZeroRange;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6012/01"));
        }



        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Decimals")]
        public void testDecimals(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Decimals;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6013/01"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_FillingProcessSatus")]
        public void testFillingProcessStatus(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FillingProcessStatus;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("SDO"));
        }
        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_DosingResult")]
        public void testDosingResult(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.DosingResult;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("FRS1"));
        }


        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_NumberDosingResults")]
        public void testNumberDosingResults(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.NumberDosingResults;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("NDS"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Unit")]
        public void testUnit(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Unit;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("6014/01"));
        }


        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testInput1(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Input1;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("IM1"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testInput2(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Input2;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("IM2"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testInput3(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Input3;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("IM3"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testInput4(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Input4;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("IM4"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testOutput1(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Output1;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OM1"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testOutput2(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Output2;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OM2"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testOutput3(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Output3;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OM3"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testOutput4(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.Output4;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OM4"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLimitStatus1(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus1;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OS1"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLimitStatus2(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus2;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OS2"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLimitStatus3(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus3;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OS3"));
        }


        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLimitStatus4(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LimitStatus4;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OS4"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testMaxDosingTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MaxDosingTime;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("MDT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testMeanValueDosingResults(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MeanValueDosingResults;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("SDM"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testStandardDeviation(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.StandardDeviation;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("SDS"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testFineFlowCutOffPoint(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FineFlowCutOffPoint;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("FFD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testCoarseFlowCutOffPoint(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.CoarseFlowCutOffPoint;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("CFD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testResidualFlowTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ResidualFlowTime;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("RFT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testMinimumFineFlow(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MinimumFineFlow;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("FFM"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testOptimizationOfCutOffPoints(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.OptimizationOfCutOffPoints;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("OSN"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testMaximumDosingTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MaximumDosingTime;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("MDT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testCoarseLockoutTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.CoarseLockoutTime;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("CFT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testFineLockoutTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FineLockoutTime;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("FFT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testTareMode(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.TareMode;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("TMD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testUpperToleranceLimit(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.UpperToleranceLimit;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("UTL"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLowerToleranceLimit(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.LowerToleranceLimit;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("LTL"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testMinimumStartWeight(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.MinimumStartWeight;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("MSW"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testEmptyWeight(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.EmptyWeight;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("EWT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testTareDelay(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.TareDelay;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("TAD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testCoarseFlowMonitoringTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.CoarseFlowMonitoringTime;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("CBT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testCoarseFlowMonitoring(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.CoarseFlowMonitoring;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("CBK"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testFineFlowMonitoring(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FineFlowMonitoring;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("FBK"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testFineFlowMonitoringTime(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.FineFlowMonitoringTime;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("FBT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testSystematicDifference(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.SystematicDifference;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("SYD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testValveControl(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ValveControl;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("VCT"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testEmptyingMode(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.EmptyingMode;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("EMD"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testDelayTimeAfterFineFlow(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.DelayTimeAfterFineFlow;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("DL1"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testActivationTimeAfterFineFlow(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.ActivationTimeAfterFineFlow;

            Assert.IsTrue(_jetTestConnection.getDataBuffer.ContainsKey("FFL"));
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLimitValueStatusLVS1(Behavior behavior)
        {
            bool testVar = false;

            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            int limitvalue1 = _wtxObj.LimitValue1Input;

            if (_jetTestConnection.getDataBuffer.ContainsKey("2020/25") == true && limitvalue1 == 0)
                testVar = true;
            else
                testVar = false;

            Assert.IsTrue(testVar);
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLimitValueStatusLVS2(Behavior behavior)
        {
            bool testVar = false;

            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            int limitvalue2 = _wtxObj.LimitValue2Source;

            if (_jetTestConnection.getDataBuffer.ContainsKey("2020/25") == true && limitvalue2 == 1)
                testVar = true;
            else
                testVar = false;

            Assert.IsTrue(testVar);
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLimitValueStatusLVS3(Behavior behavior)
        {
            bool testVar = false;

            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            int limitvalue3 = _wtxObj.LimitValue3Source;

            if (_jetTestConnection.getDataBuffer.ContainsKey("2020/25") == true && limitvalue3 == 0)
                testVar = true;
            else
                testVar = false;

            Assert.IsTrue(testVar);
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testLimitValueStatusLVS4(Behavior behavior)
        {
            bool testVar = false;

            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            int limitvalue4 = _wtxObj.LimitValue4Source;

            if (_jetTestConnection.getDataBuffer.ContainsKey("2020/25") == true && limitvalue4 == 1)
                testVar = true;
            else
                testVar = false;

            Assert.IsTrue(testVar);
        }

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testGetDataStr(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            testGrossValue = _wtxObj.NetValue;

            Assert.IsNotNull(_wtxObj.GetDataStr);
        }
   
        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_DataReceived")]
        public bool testIsDataReceived(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            _wtxObj.IsDataReceived = false;

            _wtxObj.getConnection.Read("601A/01");
            
            return _wtxObj.IsDataReceived;
        }
        

        [Test, TestCaseSource(typeof(ReadTests), "ReadTestCases_Attributes")]
        public void testGetDataUshort(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            ushort[] testArray = new ushort[185];

            for (int index = 0; index < testArray.Length; index++)
                testArray[index] = 0;

            Assert.AreEqual(testArray, _wtxObj.GetDataUshort);

        }
    }
}
