
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

        private TestJetbusConnection testConnection;

        private bool connectCallbackCalled;
        private bool connectCompleted;

        private int testGrossValue;

        public static IEnumerable Connect_TestCases_Jetbus
        {
            get
            {
                yield return new TestCaseData(Behavior.ConnectionSuccess).Returns(true);
                yield return new TestCaseData(Behavior.ConnectionFail).Returns(false);
            }
        }

        public static IEnumerable Disconnect_Testcases_Jetbus
        {
            get
            {
                yield return new TestCaseData(Behavior.DisconnectionSuccess).Returns(false);
                yield return new TestCaseData(Behavior.DisconnectionFail).Returns(true);
            }
        }

        [SetUp]
        public void Setup()
        {
            testGrossValue = 0; 

            this.connectCallbackCalled = false;
            this.connectCompleted = true;
        }

        [Test, TestCaseSource(typeof(ConnectTestsJetbus), "Connect_TestCases_Jetbus")]
        public bool TestConnectJetbus(Behavior behaviour)
        {        
            testConnection = new TestJetbusConnection(behaviour, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            WtxJet WTXJetObj = new WtxJet(testConnection);

            this.connectCallbackCalled = false;

            WTXJetObj.Connect(this.OnConnect, 100);
            
            return WTXJetObj.isConnected;
        }

        [Test, TestCaseSource(typeof(ConnectTestsJetbus), "Disconnect_Testcases_Jetbus")]
        public bool TestDisconnectJetbus(Behavior behaviour)
        {
            testConnection = new TestJetbusConnection(behaviour, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            WtxJet WTXJetObj = new WtxJet(testConnection);

            this.connectCallbackCalled = false;
            
            WTXJetObj.Connect(this.OnConnect, 100);

            WTXJetObj.Disconnect(this.OnDisconnect);

            return WTXJetObj.isConnected;
        }


        private void OnConnect(bool completed)
        {
            this.connectCallbackCalled = true; 

            this.connectCompleted = completed;
        }


        private void OnDisconnect(bool completed)
        {
            this.connectCallbackCalled = false;

            this.connectCompleted = completed;
        }

    }
}
