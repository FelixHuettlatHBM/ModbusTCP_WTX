
namespace HBM.WT.API.WTX.Modbus
{
    using System.Collections;
    using NUnit.Framework;
    using HBM.WT.API.WTX;
    using System.Threading;

    [TestFixture]
    public class ConnectTestsModbus 
    {

        private bool connectCallbackCalled;
        private bool connectCompleted;

        private bool disconnectCallbackCalled;
        private bool disconnectCompleted;

        private static ushort[] _dataReadSuccess;
        private static ushort[] _dataReadFail;
        
        private TestModbusTCPConnection testConnection;
        private WtxModbus WTXModbusObj;

        // Test case source for the connection establishment. 
        public static IEnumerable ConnectTestCases 
        { 
        get 
        { 
            yield return new TestCaseData(Behavior.ConnectionSuccess).Returns(true);                
            yield return new TestCaseData(Behavior.ConnectionFail).Returns(false); 
        } 
        }

        // Test case source for the connection establishment. 
        public static IEnumerable DisconnectTestCases
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
            this.connectCallbackCalled = true;
            this.connectCompleted = true;

            //Array size for standard mode of the WTX120 device: 
            _dataReadFail     = new ushort[59];
            _dataReadSuccess  = new ushort[59];

            for (int i = 0; i < _dataReadSuccess.Length; i++)
            {
                _dataReadSuccess[i] = 0;
                _dataReadFail[i] = 0;
            }

            _dataReadSuccess[0] = 16448;       // Net value
            _dataReadSuccess[1] = 16448;       // Gross value
            _dataReadSuccess[2] = 0;           // General weight error
            _dataReadSuccess[3] = 0;           // Scale alarm triggered
            _dataReadSuccess[4] = 0;           // Limit status
            _dataReadSuccess[5] = 0;           // Weight moving
            _dataReadSuccess[6] = 0;//1;       // Scale seal is open
            _dataReadSuccess[7] = 0;           // Manual tare
            _dataReadSuccess[8] = 0;           // Weight type
            _dataReadSuccess[9] = 0;           // Scale range
            _dataReadSuccess[10] = 0;          // Zero required/True zero
            _dataReadSuccess[11] = 0;          // Weight within center of zero 
            _dataReadSuccess[12] = 0;          // weight in zero range
            _dataReadSuccess[13] = 0;          // Application mode = 0
            _dataReadSuccess[14] = 0; //4;     // Decimal Places
            _dataReadSuccess[15] = 0; //2;     // Unit
            _dataReadSuccess[16] = 0;          // Handshake
            _dataReadSuccess[17] = 0;          // Status

        }

        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ConnectTestCases")]
        public bool ConnectTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            this.connectCallbackCalled = false;

            WTXModbusObj.Connect(this.OnConnect, 100);

            Thread.Sleep(300);
            
            return WTXModbusObj.isConnected;
                 // Alternative : Assert.AreEqual(this.connectCallbackCalled, true); 
        }

        private void OnConnect(bool connectCompleted)
        {
            this.connectCallbackCalled = true;
            this.connectCompleted = connectCompleted;
        }
       
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "DisconnectTestCases")]
        public bool DisconnectTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            Thread.Sleep(1000); // Do something.... and disconnect.

            WTXModbusObj.Disconnect(this.OnDisconnect);

            return WTXModbusObj.isConnected;
                // Alternative : Assert.AreEqual(WTXModbusObj.isConnected, true);           
        }

        private void OnDisconnect(bool disonnectCompleted)
        {
            this.disconnectCallbackCalled = true;
            this.disconnectCompleted = disonnectCompleted;
        }
    }
}
