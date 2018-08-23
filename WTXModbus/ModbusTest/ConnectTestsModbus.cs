
namespace HBM.WT.API.WTX.Modbus
{
    using System;
    using System.Collections;
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;
    using HBM.WT.API.WTX;
    using HBM.WT.API.WTX.Modbus;
    using System.ComponentModel;
    using System.Threading;
    using Moq;
    using Moq.Language;
    using System.Timers;

    [TestFixture]
    public class ConnectTestsModbus 
    {

        private bool connectCallbackCalled;
        private bool connectCompleted;

        private bool disconnectCallbackCalled;
        private bool disconnectCompleted;

        private static ushort[] _dataReadSuccess;
        private static ushort[] _dataReadFail;

        private static ushort[] _dataWriteSuccess;
        private static ushort[] _dataWriteFail;

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

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ReadTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ReadFail).Returns(0);
                yield return new TestCaseData(Behavior.ReadSuccess).Returns(17000);

                //Alternatives: 

                //yield return new TestCaseData(Behavior.ReadFail).Returns(false);
                //yield return new TestCaseData(Behavior.ReadSuccess).Returns(true);

                //yield return new TestCaseData(Behavior.ReadFail).ExpectedResult=(_dataReadFail);
                //yield return new TestCaseData(Behavior.ReadSuccess).ExpectedResult=(_dataReadSuccess);
            }
        }

        // Test case source for writing values to the WTX120 device. 
        public static IEnumerable WriteTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteFail).Returns(0);
                yield return new TestCaseData(Behavior.WriteSuccess).Returns(2);
            }
        }

        public static IEnumerable WriteArrayTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteArrayFail).Returns(false);
                yield return new TestCaseData(Behavior.WriteArraySuccess).Returns(true);              
            }
        }

        // Test case source for writing values to the WTX120 device. 
        public static IEnumerable WriteSyncTestModbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteSyncFail).Returns(0);
                yield return new TestCaseData(Behavior.WriteSyncSuccess).Returns(0x100);
            }
        }

        public static IEnumerable MeasureZeroTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.MeasureZeroFail).Returns(false);
                yield return new TestCaseData(Behavior.MeasureZeroSuccess).Returns(true);
            }
        }

        public static IEnumerable AsyncWriteBackgroundworkerCase
        {
            get
            {
                yield return new TestCaseData(Behavior.AsyncWriteBackgroundworkerSuccess).Returns(true);
                yield return new TestCaseData(Behavior.AsyncWriteBackgroundworkerFail).Returns(true);
            }
        }

        // Test case source for writing values to the WTX120 device. 
        public static IEnumerable TareTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.TareFail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.TareSuccess).ExpectedResult = 0x1;
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
            _dataWriteSuccess = new ushort[59];
            _dataWriteFail    = new ushort[59];

            for (int i = 0; i < _dataReadSuccess.Length; i++)
            {
                _dataReadSuccess[i] = 0;
                _dataReadFail[i] = 0;
                _dataWriteSuccess[i] = 0;
                _dataWriteFail[i] = 0; 
            }

            _dataReadSuccess[0] = 17000;       // Net value
            _dataReadSuccess[1] = 17000;       // Gross value
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

            _dataWriteSuccess[0] = 1995;        // Net value
            _dataWriteSuccess[1] = 17000;       // Gross value
            _dataWriteSuccess[2] = 0;           // General weight error
            _dataWriteSuccess[3] = 0;           // Scale alarm triggered
            _dataWriteSuccess[4] = 0;           // Limit status
            _dataWriteSuccess[5] = 0;           // Weight moving
            _dataWriteSuccess[6] = 1;           // Scale seal is open
            _dataWriteSuccess[7] = 1;           // Manual tare
            _dataWriteSuccess[8] = 1;           // Weight type
            _dataWriteSuccess[9] = 0;           // Scale range
            _dataWriteSuccess[10] = 0;          // Zero required/True zero
            _dataWriteSuccess[11] = 0;          // Weight within center of zero 
            _dataWriteSuccess[12] = 0;          // weight in zero range
            _dataWriteSuccess[13] = 0;          // Application mode = 0
            _dataWriteSuccess[14] = 4;          // Decimal Places
            _dataWriteSuccess[15] = 2;          // Unit
            _dataWriteSuccess[16] = 0;          // Handshake
            _dataWriteSuccess[17] = 1;          // Status

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
        
        // Test for reading: Have to be re-engineered till 27.8-31.8
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ReadTestCases")]
        public ushort ReadTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj   = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.Async_Call(0x00, callbackMethod);
            
            testConnection.ReadRegisterPublishing(new DataEvent(_dataReadSuccess));
            
            return WTXModbusObj.GetDataUshort[0];      
                // Alternative :Assert.AreEqual(_dataReadSuccess[0], WTXModbusObj.GetDataUshort[0]);
        }
        

        // Test for writing : Tare 
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "TareTestCases")]
        public void TareAsyncTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.Async_Call(0x1, callbackMethod);

            //testConnection.Write(0, 0x1);
            //Assert.AreEqual(0x1, testConnection.getCommand);

            Assert.AreEqual(0x1, WTXModbusObj.getCommand);

            //bool messageCheck = testConnection.getMessages.Contains(0x1);
            //Assert.IsTrue(messageCheck);          
            
        }

        // Test for writing : Tare 
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteSyncTestModbus")]
        public int WriteSyncTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
          
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.SyncCall_Write_Command(0, 0x100, callbackMethod);
      
            return testConnection.getCommand;
                 // Alternative : Assert.AreEqual(0x100, testConnection.getCommand);
        }

        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteTestCases")]
        public int WriteTestCasesModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            // Write : Gross/Net 

            WTXModbusObj.Async_Call(0x2, OnWriteData);

            Thread.Sleep(10);        // Include a short sleep time for the former asynchronous call (Async_Call). 

            return testConnection.getCommand;
                // Alternative Assert.AreEqual(0x2, testConnection.getCommand);
        }
      
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "AsyncWriteBackgroundworkerCase")]
        public bool AsyncWriteBackgroundworkerTest(Behavior behavior)
        {
            var runner = new BackgroundWorker();

            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            ManualResetEvent done = new ManualResetEvent(false);

            runner.RunWorkerCompleted += delegate { done.Set(); };

            runner.RunWorkerAsync();

            DateTime end = DateTime.Now.AddSeconds(20);
            bool res = false;

            while ((!res) && (DateTime.Now < end))
            {
                WTXModbusObj.Async_Call(0x2, callbackMethod);       // Read data from register 

                res = done.WaitOne(0);
            }

            return res;
            
        }
        

        private void callbackMethod(IDeviceData obj)
        {

        }


        // Callback method for writing on the WTX120 device: 
        private void OnWriteData(IDeviceData obj)
        {
            throw new NotImplementedException();
        }

        
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "MeasureZeroTestCases")]
        public bool MeasureZeroTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.MeasureZero();                 

            WTXModbusObj.Async_Call(0x00, OnReadData);

            //check if : write reg 48, 0x7FFFFFFF and if Net and gross value are zero. 

            if ((testConnection.getArrElement1 == (0x7FFFFFFF & 0xffff0000) >> 16) &&
                (testConnection.getArrElement2 == (0x7FFFFFFF & 0x0000ffff)) && 
                WTXModbusObj.NetValue==0 && WTXModbusObj.GrossValue==0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnReadData(IDeviceData obj)
        {
        }


        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteArrayTestCases")]
        public bool WriteArrayTestCasesModbus(Behavior behavior)
        {
            bool parameterEqualArrayWritten = false; 

            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.WriteOutputWordS32(0x7FFFFFFF, 50, Write_DataReceived);

                if ((testConnection.getArrElement1 == (0x7FFFFFFF & 0xffff0000) >> 16) &&
                    (testConnection.getArrElement2 == (0x7FFFFFFF & 0x0000ffff)))
                {
                    parameterEqualArrayWritten = true;
                }
                else
                {
                    parameterEqualArrayWritten = false;
                }    

                //Assert.AreEqual(true ,parameterEqualArrayWritten);

            return parameterEqualArrayWritten;
        }

        private void Write_DataReceived(IDeviceData obj)
        {
            throw new NotImplementedException();
        }

        // The following 2 tests as a first draw : Implementation for the following 2 tests follows in the week from 27.08-31.08
        /*
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "CalculateCalibrationTestCases")]
        public void CalculateCalibrationTest(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            //Alternative: 

            //ModbusTcpConnection testConnection = new ModbusTcpConnection("172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus((ModbusTcpConnection)testConnection, 200);
            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.Calculate(0.5,1.5);

            testConnection.ReadRegisterPublishing(new DataEvent(_dataWriteSuccess));

            // Testbedingung noch bearbeiten: 
            Assert.AreEqual(0, WTXModbusObj.GetDataUshort[0]);  
        }
        */

        /*
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "CalibrationTestCases")]
        public void CalibrationTest(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            //Alternative: 

            //ModbusTcpConnection testConnection = new ModbusTcpConnection("172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus((ModbusTcpConnection)testConnection, 200);
            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.Calibrate(111, "111");

            testConnection.ReadRegisterPublishing(new DataEvent(_dataWriteSuccess));

            // Testbedingung noch bearbeiten: 
            Assert.AreEqual(0, WTXModbusObj.GetDataUshort[0]);
        }
        */


    }
}
