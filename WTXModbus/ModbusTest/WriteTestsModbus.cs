using HBM.WT.API;
using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Modbus;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HBM.WT.API.WTX.Modbus
{
    [TestFixture]
    public class WriteTestsModbus
    {

        private TestModbusTCPConnection testConnection;
        private WtxModbus WTXModbusObj;

        private bool connectCallbackCalled;
        private bool connectCompleted;

        private bool disconnectCallbackCalled;
        private bool disconnectCompleted;

        private static ushort[] _dataReadSuccess;
        private static ushort[] _dataReadFail;

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

        public static IEnumerable WriteHandshakeTestModbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WriteHandshakeTestSuccess).Returns(0x1);
                yield return new TestCaseData(Behavior.WriteHandshakeTestFail).Returns(0x0);
            }
        }




        public static IEnumerable GrosMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.GrosMethodTestSuccess).Returns(0x2);
                yield return new TestCaseData(Behavior.GrosMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable TareMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.TareMethodTestSuccess).Returns(0x1);
                yield return new TestCaseData(Behavior.TareMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable ZeroMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ZeroMethodTestSuccess).Returns(0x40);
                yield return new TestCaseData(Behavior.ZeroMethodTestFail).Returns(0x0);
            }
        }
   
        public static IEnumerable AdjustingZeroMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.AdjustingZeroMethodSuccess).Returns(0x80);
                yield return new TestCaseData(Behavior.AdjustingZeroMethodFail).Returns(0x0);
            }
        }

        public static IEnumerable AdjustNominalMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.AdjustNominalMethodTestSuccess).Returns(0x100);
                yield return new TestCaseData(Behavior.AdjustNominalMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable ActivateDataMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ActivateDataMethodTestSuccess).Returns(0x800);
                yield return new TestCaseData(Behavior.ActivateDataMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable ManualTaringMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ManualTaringMethodTestSuccess).Returns(0x1000);
                yield return new TestCaseData(Behavior.ManualTaringMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable ClearDosingResultsMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ClearDosingResultsMethodTestSuccess).Returns(0x4);
                yield return new TestCaseData(Behavior.ClearDosingResultsMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable AbortDosingMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.AbortDosingMethodTestSuccess).Returns(0x8);
                yield return new TestCaseData(Behavior.AbortDosingMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable StartDosingMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.StartDosingMethodTestSuccess).Returns(0x10);
                yield return new TestCaseData(Behavior.StartDosingMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable RecordWeightMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.RecordWeightMethodTestSuccess).Returns(0x4000);
                yield return new TestCaseData(Behavior.RecordWeightMethodTestFail).Returns(0x0);
            }
        }
        public static IEnumerable ManualRedosingMethodTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.ManualRedosingMethodTestSuccess).Returns(0x8000);
                yield return new TestCaseData(Behavior.ManualRedosingMethodTestFail).Returns(0x0);
            }
        }



        [SetUp]
        public void Setup()
        {
            this.connectCallbackCalled = true;
            this.connectCompleted = true;

            //Array size for standard mode of the WTX120 device: 
            _dataReadFail = new ushort[59];
            _dataReadSuccess = new ushort[59];

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

        // Test for handshake:
        [Test, TestCaseSource(typeof(WriteTestsModbus), "WriteHandshakeTestModbus")]
        public int WriteHandshakeTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.Async_Call(0x1, callbackMethod);

            Thread.Sleep(300);

            return testConnection.getCommand;
            // Alternative : Assert.AreEqual(0x100, testConnection.getCommand);
        }


        // Test for writing : Tare 
        [Test, TestCaseSource(typeof(WriteTestsModbus), "WriteSyncTestModbus")]
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

        [Test, TestCaseSource(typeof(WriteTestsModbus), "WriteTestCases")]
        public int WriteTestCasesModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            // Write : Gross/Net 

            WTXModbusObj.Async_Call(0x2, OnWriteData);

            Thread.Sleep(300);        // Include a short sleep time for the former asynchronous call (Async_Call). 

            return testConnection.getCommand;
            // Alternative Assert.AreEqual(0x2, testConnection.getCommand);
        }

        [Test, TestCaseSource(typeof(WriteTestsModbus), "AsyncWriteBackgroundworkerCase")]
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

        [Test, TestCaseSource(typeof(WriteTestsModbus), "WriteArrayTestCases")]
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

        // Test for writing : Tare 
        [Test, TestCaseSource(typeof(WriteTestsModbus), "TareTestCases")]
        public void TareAsyncTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.Async_Call(0x1, callbackMethod);

            Assert.AreEqual(0x1, WTXModbusObj.getCommand);

        }

        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }

        private void Write_DataReceived(IDeviceData obj)
        {
            throw new NotImplementedException();
        }





        // Test for method : Switch to gross value or net value
        [Test, TestCaseSource(typeof(WriteTestsModbus), "GrosMethodTestCases")]
        public int GrosMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.gross(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x2, WTXModbusObj.getCommand);

        }

        // Test for method : Taring
        [Test, TestCaseSource(typeof(WriteTestsModbus), "TareMethodTestCases")]
        public int TareMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.taring(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x1, WTXModbusObj.getCommand);

        }

        // Test for method : Zeroing
        [Test, TestCaseSource(typeof(WriteTestsModbus), "ZeroMethodTestCases")]
        public int ZeroMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.taring(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x40, WTXModbusObj.getCommand);

        }

        // Test for method : Adjusting zero
        [Test, TestCaseSource(typeof(WriteTestsModbus), "AdjustingZeroMethodTestCases")]
        public int AdjustingZeroMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.taring(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x80, WTXModbusObj.getCommand);
        }

        // Test for method : Adjusting nominal
        [Test, TestCaseSource(typeof(WriteTestsModbus), "AdjustNominalMethodTestCases")]
        public int AdjustingNominalMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.adjustNominal(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x100, WTXModbusObj.getCommand);
        }

        // Test for method : Adjusting nominal
        [Test, TestCaseSource(typeof(WriteTestsModbus), "ActivateDataMethodTestCases")]
        public int ActivateDataMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.adjustNominal(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x800, WTXModbusObj.getCommand);
        }

        // Test for method : Adjusting nominal
        [Test, TestCaseSource(typeof(WriteTestsModbus), "ManualTaringMethodTestCases")]
        public int ManualTaringTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.manualTaring(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x1000, WTXModbusObj.getCommand);
        }


        // Test for method : Adjusting nominal
        [Test, TestCaseSource(typeof(WriteTestsModbus), "ClearDosingResultsMethodTestCases")]
        public int ClearDosingResultsMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.clearDosingResults(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x4, WTXModbusObj.getCommand);
        }

        // Test for method : Adjusting nominal
        [Test, TestCaseSource(typeof(WriteTestsModbus), "AbortDosingMethodTestCases")]
        public int AbortDosingMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.abortDosing(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x8, WTXModbusObj.getCommand);
        }

        // Test for method : Adjusting nominal
        [Test, TestCaseSource(typeof(WriteTestsModbus), "StartDosingMethodTestCases")]
        public int StartDosingMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.startDosing(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x10, WTXModbusObj.getCommand);
        }

        // Test for method : Record weight
        [Test, TestCaseSource(typeof(WriteTestsModbus), "RecordWeightMethodTestCases")]
        public int RecordweightMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.recordWeight(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x4000, WTXModbusObj.getCommand);
        }

        // Test for method : manualReDosing
        [Test, TestCaseSource(typeof(WriteTestsModbus), "ManualRedosingMethodTestCases")]
        public int ManualRedosingMethodTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            WTXModbusObj.manualReDosing(callbackMethod);

            return WTXModbusObj.getCommand;
            //Assert.AreEqual(0x8000, WTXModbusObj.getCommand);
        }




    }
}