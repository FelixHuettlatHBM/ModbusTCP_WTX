
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
                yield return new TestCaseData(Behavior.DisconnectionSuccess).Returns(true);
                yield return new TestCaseData(Behavior.DisconnectionFail).Returns(false);
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ReadTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ReadFail).ExpectedResult = _dataReadFail;
                yield return new TestCaseData(Behavior.ReadSuccess).ExpectedResult = _dataReadSuccess;
            }
        }

        // Test case source for writing values to the WTX120 device. 
        public static IEnumerable WriteTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteFail).ExpectedResult = _dataWriteFail;
                yield return new TestCaseData(Behavior.WriteSuccess).ExpectedResult = _dataWriteSuccess;
            }
        }

        public static IEnumerable WriteArrayTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteFail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.WriteSuccess).ExpectedResult = 1;

                //yield return new TestCaseData(Behavior.WriteArrayFail).ExpectedResult = (new ushort[2] { 0, 0 });
                //yield return new TestCaseData(Behavior.WriteArraySuccess).ExpectedResult = (new ushort[2] { 0x0000ffff, 0x0000ffff });

            }
        }

        // Test case source for writing values to the WTX120 device. 
        public static IEnumerable WriteSyncTestModbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteSyncFail).ExpectedResult = _dataWriteFail;
                yield return new TestCaseData(Behavior.WriteSyncSuccess).ExpectedResult = _dataWriteSuccess;
            }
        }

        public static IEnumerable MeasureZeroTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.MeasureZeroFail).Returns(true);
                yield return new TestCaseData(Behavior.MeasureZeroSuccess).Returns(false);
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


        /*
        // Test case for the Backgroundworker enabling asynchronous data transfer between host-pc and WTX120 device. 
        public static IEnumerable AsyncBackgroundworkerTestCases
        {
            get
            {
                //yield return new TestCaseData()...;
                //yield return new TestCaseData()...;
            }
        }
        */


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

            _dataWriteFail = _dataReadSuccess;

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

            WTXModbusObj.Connect(this.OnConnect, 100);

            //Mit Callback-Funktion:
            Assert.AreEqual(this.connectCallbackCalled, true);

            return this.connectCompleted;
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

            //Mit Callback-Funktion:
            Assert.AreEqual(this.connectCallbackCalled, true);

            return this.connectCompleted;
        }

        private void OnDisconnect(bool disonnectCompleted)
        {
            this.disconnectCallbackCalled = true;
            this.disconnectCompleted = disonnectCompleted;
        }



        // Test for reading: 
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ReadTestCases")]
        public void ReadRegisterPublishingTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            //WTXModbusObj.Async_Call(0x0, callbackMethod);

            testConnection.ReadRegisterPublishing(new DataEvent(_dataReadSuccess));

            WTXModbusObj.UpdateEvent(new object(),new DataEvent(_dataReadSuccess));

            Assert.AreEqual(_dataReadSuccess[0], WTXModbusObj.GetDataUshort[0]);
        }

        // Test for writing : Tare 
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "TareTestCases")]
        public void TareAsyncTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.Async_Call(0x1, callbackMethod);

            testConnection.Write(0, 0x1);
            Assert.AreEqual(0x1, testConnection.getCommand);
                      
            //bool messageCheck = testConnection.getMessages.Contains(0x1);
            //Assert.IsTrue(messageCheck);          
            
        }

        // Test for writing : Tare 
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteSyncTestModbus")]
        public void WriteSyncTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
          
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.SyncCall_Write_Command(0, 0x100, callbackMethod);

            Assert.AreEqual(0x100, testConnection.getCommand);

        }


        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteTestCases")]
        public void WriteTestCasesModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            // Write : Gross/Net 

            WTXModbusObj.Async_Call(0x2, OnWriteData);

            testConnection.ReadRegisterPublishing(new DataEvent(_dataWriteSuccess));

            Assert.AreEqual(_dataWriteSuccess, WTXModbusObj.GetDataUshort);
        }

        /*
        [Test, TestCaseSource(typeof(xyz),"AsyncBackgroundworkerCases")]
        public void AsyncBackgroundWorkerTest(Behavior behavior)
        {           
            bool running = true;
            string result = null;

            Action<string> cb = name =>
            {
                result = name;
                running = false;
            };

            var d = new MyClass();
            d.Get("test", cb);

            while (running)
            {
                Thread.Sleep(100);
            }

            Assert.IsNotNull(result);
        
        //Though you probably want to add something in there to stop the test from running forever if it fails...
        }
        */

        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ReadTestCases")]
        public void AsyncReadBackgroundworkerTest(Behavior behavior)
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
                WTXModbusObj.Async_Call(0x00, callbackMethod);       // Read data from register 

                res = done.WaitOne(0);
            }
            
            Assert.IsTrue(res, "The RunWorkerCompleted method have not been executed within 10 seconds");
        }

        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ReadTestCases")]
        public void AsyncWriteBackgroundworkerTest(Behavior behavior)
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

            Assert.IsTrue(res, "The RunWorkerCompleted method have not been executed within 10 seconds");
        }


        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteSyncTestCases")]
        public void SyncBackgroundworkerTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            // Write : Gross/Net 

            //WTXModbusObj.Async_Call(0x2, callbackMethod);

            WTXModbusObj.SyncCall_Write_Command(0, 0x2, callbackMethod);
        
            testConnection.ReadRegisterPublishing(new DataEvent(_dataWriteSuccess));

            Assert.AreEqual(_dataWriteSuccess, WTXModbusObj.GetDataUshort);

        }

        /*
         [Test, TestCaseSource(typeof(ConnectTestsModbus), "ReadTestCases")]
         public void TestUpdateEvent(Behavior behavior)
         {
             TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
             WtxModbus WTXModbusObj = new WtxModbus((ModbusTcpConnection)testConnection, 200);

             WTXModbusObj.Connect(this.OnConnect, 100);


         }
         */

        private void callbackMethod(IDeviceData obj)
        {
            throw new NotImplementedException();
        }


        // Callback method for writing on the WTX120 device: 
        private void OnWriteData(IDeviceData obj)
        {
            throw new NotImplementedException();
        }


        /*
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "MeasureZeroTestCases")]
        public int MeasureZeroTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus((ModbusTcpConnection)testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.MeasureZero();             //WTXModbusObj.Async_Call(0x2, callbackMethod);      

            testConnection.ReadRegisterPublishing(new DataEvent(_dataWriteSuccess));

            Assert.AreEqual(0, WTXModbusObj.GetDataUshort[0]);  // If the measureZero method have been successful, the actually measured value is zero.

            return 0;
        }
        */


        /*
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteTestCases")]
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
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteTestCases")]
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
        
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteArrayTestCases")]
        public void WriteArrayTestCasesModbus(Behavior behavior)
        {
            bool parameterEqualArrayWritten = true; 

            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.WriteOutputWordS32(0x7FFFFFFF, 50, Write_DataReceived);
            
                if( (WTXModbusObj.getArrElement1 == (0x7FFFFFFF & 0xffff0000) >> 16) &&   //if( (testConnection.arr1 == (0x7FFFFFFF & 0xffff0000) >> 16) && (testConnection.arr2 == (0x7FFFFFFF & 0x0000ffff)) )
                    (WTXModbusObj.getArrElement2 == (0x7FFFFFFF & 0x0000ffff)) )
                {
                    parameterEqualArrayWritten = true;
                }
                else
                {
                    parameterEqualArrayWritten = false;
                }    

                Assert.IsTrue(parameterEqualArrayWritten);                
        }

        private void Write_DataReceived(IDeviceData obj)
        {
            throw new NotImplementedException();
        }



        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ReadTestCases")]
        public void testTimer(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);
            // In the constructor the timer is already started once an object of WtxModbus is created: 

            Thread.Sleep(200);

            Assert.AreEqual(WTXModbusObj.getCommand, 0);
        }

        // Alternative with mocking: 
        /*
        [Test]
        public void Start_WithValidParameters_TriggersTimeReached()
        {
            var subscriberMock = new Mock<INetConnection>();
            var timer = new TimeOutTimer(subscriberMock.Object);

            timer.Start();
            Thread.Sleep(1000);

            subscriberMock.Verify(subscriber => subscriber.());
        }
        */


        


    }
}
