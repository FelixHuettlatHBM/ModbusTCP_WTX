
using HBM.Weighing.API;
using HBM.Weighing.API.WTX;
using HBM.Weighing.API.WTX.Jet;

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetbusTest
{
    // Class for testing the set properties the class WTXJet:
    [TestFixture]
    public class SetTests
    {
        private TestJetbusConnection _jetTestConnection;
        private WtxJet _wtxObj;

        private int testValue;

        // Test case source for writing values to the WTX120 device : Zeroing
        public static IEnumerable setTests
        {
            get
            {
                yield return new TestCaseData(Behavior.setTestsFail).Returns(false);
                yield return new TestCaseData(Behavior.setTestsSuccess).Returns(true);
            }
        }

        [SetUp]
        public void Setup()
        {
            testValue = 1234;
        }


        [Test, TestCaseSource(typeof(SetTests), "setTests")]
        public bool SetActivationTimeTest(Behavior behavior)
        {
            _jetTestConnection = new TestJetbusConnection(behavior, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            _wtxObj = new WtxJet(_jetTestConnection);

            _wtxObj.Connect(this.OnConnect, 100);

            _wtxObj.ActivationTimeAfterFineFlow = 1;
            _wtxObj.CoarseFlowMonitoring = 10;
            _wtxObj.CoarseFlowMonitoringTime = 101;
            _wtxObj.CoarseLockoutTime = 110;
            _wtxObj.DelayTimeAfterFineFlow = 111;
            _wtxObj.EmptyingMode = 1000;
            _wtxObj.EmptyWeight = 1001;
            _wtxObj.FineFlowMonitoring = 1011;
            _wtxObj.FineFlowMonitoringTime = 1100;
            _wtxObj.FineLockoutTime = 1111;
            _wtxObj.LimitValue1Input = 10000;
            _wtxObj.LimitValue2Source = 10001;
            _wtxObj.LimitValue3Source = 10010;
            _wtxObj.LimitValue4Source = 10100;
            _wtxObj.LowerToleranceLimit = 11000;
            _wtxObj.MaximumDosingTime = 11001;
            _wtxObj.MinimumFineFlow = 11001;
            _wtxObj.MinimumStartWeight = 11010;
            _wtxObj.OptimizationOfCutOffPoints = 11100;
            _wtxObj.ResidualFlowTime = 11101;
            _wtxObj.SystematicDifference = 11110;
            _wtxObj.TareDelay = 11111;
            _wtxObj.TareMode = 100000;
            _wtxObj.UpperToleranceLimit = 100001;
            _wtxObj.ValveControl = 100010;           

            if (_jetTestConnection.getDataBuffer.ContainsKey("VCT") && _jetTestConnection.getDataBuffer.ContainsValue(100010))           
                return true;
            else
                if (_jetTestConnection.getDataBuffer.ContainsKey("6002/02") && _jetTestConnection.getDataBuffer.ContainsValue(1230))
                    return false;

            return false;
        }

        private void WriteDataCompleted(IDeviceData obj)
        {
            throw new NotImplementedException();
        }

        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }
    }
}
