using HBM.WT.API;
using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Modbus;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HBM.WT.API.WTX.Modbus
{
    [TestFixture]
    public class ReadTestsModbus
    {
        private TestModbusTCPConnection testConnection;
        private WtxModbus WTXModbusObj;


        private bool connectCallbackCalled;
        private bool connectCompleted;

        private bool disconnectCallbackCalled;
        private bool disconnectCompleted;

        private static ushort[] _dataReadSuccess;
        private static ushort[] _dataReadFail;

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


        public static IEnumerable MeasureZeroTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.MeasureZeroFail).Returns(false);
                yield return new TestCaseData(Behavior.MeasureZeroSuccess).Returns(true);
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

        // Test case source for checking the values of the application mode: 

        public static IEnumerable LogEventTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.LogEvent_Fail).Returns(false);
                yield return new TestCaseData(Behavior.LogEvent_Success).Returns(true);
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

        // Test for reading: 
        [Test, TestCaseSource(typeof(ReadTestsModbus), "ReadTestCases")]
        public ushort ReadTestModbus(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            testConnection.IsConnected = true;

            WTXModbusObj.Async_Call(0x00, callbackMethod);

            Thread.Sleep(100);

            return WTXModbusObj.GetDataUshort[1];

            //return (ushort)WTXModbusObj.NetValue;  
            // Alternative :Assert.AreEqual(_dataReadSuccess[0], WTXModbusObj.GetDataUshort[0]);
        }

        // Test for checking the handshake bit 
        [Test, TestCaseSource(typeof(ReadTestsModbus), "HandshakeTestCases")]
        public int testHandshake(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            WTXModbusObj.SyncCall(0, 0x1, OnWriteData);

            WTXModbusObj.SyncCall(0, 0x00, OnReadData);

            return WTXModbusObj.Handshake;
        }

        private void OnWriteData(IDeviceData obj)
        {
            throw new NotImplementedException();
        }

        [Test, TestCaseSource(typeof(ReadTestsModbus), "MeasureZeroTestCases")]
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
                WTXModbusObj.NetValue == 0 && WTXModbusObj.GrossValue == 0)
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


        [Test, TestCaseSource(typeof(ReadTestsModbus), "ApplicationModeTestCases")]
        public int ApplicationModeTest(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            testConnection.Write(0, 0);

            testConnection.Read(0);

            return testConnection.getData[5] & 0x3 >> 1;

            //return WTXModbusObj.ApplicationMode;
        }

        [Test, TestCaseSource(typeof(ReadTestsModbus), "LogEventTestCases")]
        public bool LogEventGetTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            testConnection.IsConnected = true;

            WTXModbusObj.Async_Call(0x00, callbackMethod);

            Thread.Sleep(100);

            if (testConnection._logObj.Args.Equals("Read successful: Registers have been read"))
                return true;

            else
                if (testConnection._logObj.Args.Equals("Read failed : Registers have not been read"))
                return false;

            else
                return false; 
            //return WTXModbusObj.ApplicationMode;
        }

        [Test, TestCaseSource(typeof(ReadTestsModbus), "LogEventTestCases")]
        public bool LogEventSetTest(Behavior behavior)
        {
            testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            testConnection.IsConnected = true;

            WTXModbusObj.Async_Call(0x00, callbackMethod);

            Thread.Sleep(100);

            if (testConnection._logObj.Args.Equals("Read successful: Registers have been read"))
                return true;

            else
                if (testConnection._logObj.Args.Equals("Read failed : Registers have not been read"))
                return false;

            else
                return false;
            //return WTXModbusObj.ApplicationMode;
        }


        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }

        private void callbackMethod(IDeviceData obj)
        {
            throw new NotImplementedException();
        }
    }
}
