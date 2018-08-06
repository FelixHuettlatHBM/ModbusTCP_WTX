
namespace HBM.WT.API.WTX.Jet
{
    using System;
    using System.Collections;
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;
    using HBM.WT.API.WTX;
    using HBM.WT.API.WTX.Modbus;
    
    using HBM.WT.API.WTX.Jet;
    [TestFixture]
    public class ConnectTestsJetbus
    {
        private bool connectCallbackCalled;
        private bool connectCompleted;

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
            this.connectCallbackCalled = false;
            this.connectCompleted = false;
        }

        [Test, TestCaseSource(typeof(ConnectTestsJetbus), "TestCases")]
        public bool ConnectTestJetbus(Behavior behaviour)
        {
            object testConnection = new TestJetbusConnection(behaviour);
            WtxJet WTXJetObj = new WtxJet((JetBusConnection) testConnection);

            WTXJetObj.Connect(this.OnConnect, 100);

            //Mit Callback-Funktion:
            Assert.AreEqual(this.connectCallbackCalled, true);

            return this.connectCompleted;

            // Ohne Callback:    

            /*
            object testConnection = new TestModbusTCPConnection(behaviour);

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);     
            
            WTXModbusObj.Connect();
            
            Assert.AreEqual(WTXModbusObj.isConnected(), true);

            return this.connectCompleted;
            
            */
        }

        private void OnConnect(bool completed)
        {
            this.connectCallbackCalled = true;
            this.connectCompleted = completed;
        }

    }
}
