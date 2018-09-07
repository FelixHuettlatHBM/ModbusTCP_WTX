﻿using HBM.WT.API.WTX;
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

            if (_jetTestConnection.getTokenBuffer.ContainsKey("6152/00") && _jetTestConnection.getTokenBuffer.ContainsValue(15000))
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

            if (_jetTestConnection.getTokenBuffer.ContainsKey("6002/01") && _jetTestConnection.getTokenBuffer.ContainsValue(2053923171))
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
                _jetTestConnection.getTokenBuffer.ContainsKey("6112/01") && _jetTestConnection.getTokenBuffer.ContainsValue(testIntPreload) &&
                _jetTestConnection.getTokenBuffer.ContainsKey("6113/01") && _jetTestConnection.getTokenBuffer.ContainsValue(testIntNominalLoad) 
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
