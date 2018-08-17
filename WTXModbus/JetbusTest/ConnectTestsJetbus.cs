
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
            this.connectCallbackCalled = true;
            this.connectCompleted = true;
        }

        [Test, TestCaseSource(typeof(ConnectTestsJetbus), "TestCases")]
        public bool ConnectTestJetbus(Behavior behaviour)
        {        
            object testConnection = new TestJetbusConnection(behaviour, "wss://172.19.103.8:443/jet/canopen", "Administrator", "wtx", delegate { return true; });

            WtxJet WTXJetObj = new WtxJet((JetBusConnection) testConnection);

            WTXJetObj.Connect(this.OnConnect, 5000);
            
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

        [Test]
        public void RemoveStatesWhenDisconnect()
        {
            const string state = "theState";
           
            var connection = new TestJetbusConnection(Behavior.ConnectionSuccess, "172.19.103.8", "Administrator", "wtx", delegate { return true; });
            
            var peer = new JetPeer((IJetConnection)connection);

            peer.Connect(this.OnConnect, 100);

            JValue stateValue = new JValue(12);

            peer.AddState(state, stateValue, this.OnSet, this.OnResponse, 3000);

            peer.Disconnect();

            /*
            Assert.AreEqual(0, peer.numberOfRegisteredStateCallback());
       
            string removeJson = connection.SendMessage[1];

            JToken json = JToken.Parse(removeJson);
            JToken method = json["method"];
            Assert.AreEqual("remove", method.ToString());
            JToken parameters = json["params"];
            JToken path = parameters["path"];
            Assert.AreEqual(state, path.ToString());       
        
            */
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
