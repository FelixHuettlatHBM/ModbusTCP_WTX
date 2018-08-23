
namespace HBM.WT.API.WTX.Jet
{
    using System;
    using System.Collections;
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;
    using HBM.WT.API.WTX;
    using HBM.WT.API.WTX.Modbus;
    
    using HBM.WT.API.WTX.Jet;
    using Hbm.Devices.Jet;

    [TestFixture]
    public class ConnectTestsJetbus
    {

        private TestJetbusConnection testConnection;

        private bool connectCallbackCalled;
        private bool connectCompleted;

        private int testGrossValue;

        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ConnectionSuccess).Returns(true);
                yield return new TestCaseData(Behavior.ConnectionFail).Returns(false);
            }
        }

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0; 

            this.connectCallbackCalled = false;
            this.connectCompleted = true;
        }

        [Test, TestCaseSource(typeof(ConnectTestsJetbus), "TestCases")]
        public bool ConnectTestJetbus(Behavior behaviour)
        {        
            testConnection = new TestJetbusConnection(behaviour, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            WtxJet WTXJetObj = new WtxJet(testConnection);

            this.connectCallbackCalled = false;

            WTXJetObj.Connect(this.OnConnect, 100);
            
            return WTXJetObj.isConnected;
            
            //Assert.AreEqual(this.connectCallbackCalled, WTXJetObj.isConnected);
        }



        private void OnResponse(bool arg1, JToken arg2)
        {
            throw new NotImplementedException();
        }

        private JToken OnSet(string arg1, JToken arg2)
        {
            throw new NotImplementedException();
        }

        private void OnConnect(bool completed)
        {
            this.connectCallbackCalled = true; 

            this.connectCompleted = completed;
        }

    }
}
