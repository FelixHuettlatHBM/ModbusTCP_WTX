
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
                yield return new TestCaseData(Behavior.ReadSuccess).Returns(16448);

                //Alternatives: 

                //yield return new TestCaseData(Behavior.ReadFail).ExpectedResult=(_dataReadFail);
                //yield return new TestCaseData(Behavior.ReadSuccess).ExpectedResult=(_dataReadSuccess);

            }
        }

        // Test case source for checking the transition of the handshake bit. 
        public static IEnumerable HandshakeTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.HandshakeFail).Returns(0);
                yield return new TestCaseData(Behavior.HandshakeSuccess).Returns(1);
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

        // Test case source for writing values to the WTX120 device. 
        public static IEnumerable CalculateCalibrationTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.CalibrationFail).Returns(false);
                yield return new TestCaseData(Behavior.CalibrationSuccess).Returns(true);
            }
        }

        // Test case source for checking the values of the application mode: 
       
        public static IEnumerable ApplicationModeTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.InStandardMode).Returns(0);
                yield return new TestCaseData(Behavior.InFillerMode).Returns(1);
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
        
        // Test for reading: Have to be re-engineered till 27.8-31.8
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ReadTestCases")]
        public ushort ReadTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj   = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            testConnection.IsConnected = true;

            WTXModbusObj.Async_Call(0x00, callbackMethod);

            Thread.Sleep(100);

            return WTXModbusObj.GetDataUshort[1];

            //return (ushort)WTXModbusObj.NetValue;  
            // Alternative :Assert.AreEqual(_dataReadSuccess[0], WTXModbusObj.GetDataUshort[0]);
        }
        

        // Test for writing : Tare 
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "TareTestCases")]
        public void TareAsyncTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.Async_Call(0x1, callbackMethod);

            Assert.AreEqual(0x1, WTXModbusObj.getCommand);
        
        }

        // Test for checking the handshake bit 
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "HandshakeTestCases")]
        public int testHandshake(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.SyncCall(0, 0x1, OnWriteData);

            WTXModbusObj.SyncCall(0, 0x00, OnReadData);
            
                
            return WTXModbusObj.Handshake;
        }


        // Test for writing : Tare 
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteSyncTestModbus")]
        public int WriteSyncTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
          
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.SyncCall(0, 0x100, callbackMethod);
            
            return testConnection.getCommand;
                 // Alternative : Assert.AreEqual(0x100, testConnection.getCommand);
        }

        [Test, TestCaseSource(typeof(ConnectTestsModbus), "WriteTestCases")]
        public int WriteTestCasesModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            // Write : Gross/Net 

            WTXModbusObj.Async_Call(0x2, OnWriteData);

            Thread.Sleep(200);        // Include a short sleep time for the former asynchronous call (Async_Call). 

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
            WTXModbusObj.isConnected = true;

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
            WTXModbusObj.isConnected = true;

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
            WTXModbusObj.isConnected = true;

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
        
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "CalculateCalibrationTestCases")]
        public bool CalculateCalibrationTest(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            double preload  = 1;
            double capacity = 2; 

            double multiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)

            double dPreload = preload * multiplierMv2D;
            double dNominalLoad = dPreload + (capacity * multiplierMv2D);

            WTXModbusObj.Calculate(preload,capacity); 

            // Testbedingung noch bearbeiten: 

            if (
                (testConnection.getArrElement1 == (Convert.ToInt32(dPreload) & 0xffff0000) >> 16) &&
                (testConnection.getArrElement2 == (Convert.ToInt32(dPreload) & 0x0000ffff)) &&

                (testConnection.getArrElement3 == (Convert.ToInt32(dNominalLoad) & 0xffff0000) >> 16) &&
                (testConnection.getArrElement4 == (Convert.ToInt32(dNominalLoad) & 0x0000ffff)) &&

                testConnection.getCommand==0x100 && WTXModbusObj.getDPreload==dPreload && WTXModbusObj.getDNominalLoad==dNominalLoad
               )
            {
                return true;
            }
            else
            {
                return false;
            }   

            //_dataWritten[0] = (ushort)((valueParam & 0xffff0000) >> 16);
            //_dataWritten[1] = (ushort)(valueParam & 0x0000ffff);
        }
        

        
        [Test, TestCaseSource(typeof(ConnectTestsModbus), "CalculateCalibrationTestCases")]
        public bool CalibrationTest(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            
            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.isConnected = true; 
            WTXModbusObj.Connect(this.OnConnect, 100);

            int testCalibrationValue = 111; 

            WTXModbusObj.Calibrate(testCalibrationValue, "111");

            // Check if: write reg 46, CalibrationWeight and write reg 50, 0x7FFFFFFF

            if (
                (testConnection.getArrElement1 == (testCalibrationValue & 0xffff0000) >> 16) &&
                (testConnection.getArrElement2 == (testCalibrationValue & 0x0000ffff)) &&

                (testConnection.getArrElement3 == (0x7FFFFFFF & 0xffff0000) >> 16) &&
                (testConnection.getArrElement4 == (0x7FFFFFFF & 0x0000ffff))             
            )
            {
                    return true;
            }
            else
            {
                    return false;
            }

        }

        [Test, TestCaseSource(typeof(ConnectTestsModbus), "ApplicationModeTestCases")]
        public int ApplicationModeTest(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.isConnected = true;
            WTXModbusObj.Connect(this.OnConnect, 100);
            
            testConnection.Write(0, 0);

            testConnection.Read(0);

            return testConnection.getData[5] & 0x3 >> 1; 

            //return WTXModbusObj.ApplicationMode;
        }


    }
}
