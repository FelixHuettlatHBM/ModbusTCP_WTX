
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

        private static ushort[] _dataReadSuccess;
        private static ushort[] _dataReadFail;


        public static IEnumerable ConnectTestCases 
        { 
        get 
        { 
            yield return new TestCaseData(Behavior.ConnectionSuccess).Returns(true); 
            yield return new TestCaseData(Behavior.ConnectionFail).Returns(false); 
        } 
        }

        public static IEnumerable ReadTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ReadFail).ExpectedResult = _dataReadFail;
                yield return new TestCaseData(Behavior.ReadSuccess).ExpectedResult = _dataReadSuccess;
            }
        }

        [SetUp]
        public void Setup()
        {
            this.connectCallbackCalled = true;
            this.connectCompleted = true;

            //Array size for standard mode of the WTX120 device: 
            _dataReadFail = new ushort[38];
            _dataReadSuccess = new ushort[38];

            for (int i = 0; i < _dataReadFail.Length; i++)
            {
                _dataReadSuccess[i] = 0;
                _dataReadFail[i] = 0;
            }

            _dataReadSuccess[0] = 10000;       // Net value
            _dataReadSuccess[1] = 10000;       // Gross value
            _dataReadSuccess[2] = 0;           // General weight error
            _dataReadSuccess[3] = 0;           // Scale alarm triggered
            _dataReadSuccess[4] = 0;           // Limit status
            _dataReadSuccess[5] = 0;           // Weight moving
            _dataReadSuccess[6] = 1;           // Scale seal is open
            _dataReadSuccess[7] = 0;           // Manual tare
            _dataReadSuccess[8] = 0;           // Weight type
            _dataReadSuccess[9] = 0;           // Scale range
            _dataReadSuccess[10] = 0;          // Zero required/True zero
            _dataReadSuccess[11] = 0;          // Weight within center of zero 
            _dataReadSuccess[12] = 0;          // weight in zero range
            _dataReadSuccess[13] = 0;          // Application mode = 0
            _dataReadSuccess[14] = 4;          // Decimal Places
            _dataReadSuccess[15] = 2;          // Unit
            _dataReadSuccess[16] = 0;          // Handshake
            _dataReadSuccess[17] = 0;          // Status

         }

        [Test, TestCaseSource(typeof(ConnectTestsModbus),"ReadTestCases")]
        public void ReadTestCasesModbus(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus WTXModbusObj = new WtxModbus((ModbusTcpConnection)testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            
            testConnection.ReadRegisterPublishing(new DataEvent(_dataReadSuccess));

            Assert.AreEqual(_dataReadSuccess, WTXModbusObj.GetDataUshort);
        
        }
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ConnectTestCases")] 
         public bool ConnectTestModbus(Behavior behavior)
         {

            object testConnection  = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus WTXModbusObj = new WtxModbus((ModbusTcpConnection)testConnection, 200);
            
            WTXModbusObj.Connect(this.OnConnect, 100);

            //Mit Callback-Funktion:
            Assert.AreEqual(this.connectCallbackCalled, true);

            return this.connectCompleted;          
        }

         private void OnConnect(bool completed)
         { 
             this.connectCallbackCalled = true; 
             this.connectCompleted = completed; 
         }

}
}
