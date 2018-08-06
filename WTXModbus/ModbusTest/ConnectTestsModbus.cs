
namespace HBM.WT.API.WTX.Modbus
{
    using System;
    using System.Collections;
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;
    using HBM.WT.API.WTX;
    using HBM.WT.API.WTX.Modbus;

    [TestFixture]
    public class ConnectTestsModbus 
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


         [Test, TestCaseSource(typeof(ConnectTestsModbus), "TestCases")] 
         public bool ConnectTestModbus(Behavior behaviour)
         {

            object testConnection = new TestModbusTCPConnection(behaviour, "172.19.103.8");
            WtxModbus WTXModbusObj = new WtxModbus((ModbusTcpConnection)testConnection, 200);
            
            WTXModbusObj.Connect(this.OnConnect, 100);

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
