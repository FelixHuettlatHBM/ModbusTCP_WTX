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
    public class CommentMethodsModbusTests
    {

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable T_UnitValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.t_UnitValue_Fail).Returns(0);
                yield return new TestCaseData(Behavior.t_UnitValue_Success).Returns(2);
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable KG_UnitValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.kg_UnitValue_Fail).Returns(3);
                yield return new TestCaseData(Behavior.kg_UnitValue_Success).Returns(0);
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable G_UnitValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.g_UnitValue_Fail).Returns(0);
                yield return new TestCaseData(Behavior.g_UnitValue_Success).Returns(1);
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable LB_UnitValueTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.lb_UnitValue_Fail).Returns(0);
                yield return new TestCaseData(Behavior.lb_UnitValue_Success).Returns(3);
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable NetGrossValueStringComment_0D_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_0D_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_0D_Success).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable NetGrossValueStringComment_1D_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_1D_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_1D_Success).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable NetGrossValueStringComment_2D_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_2D_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_2D_Success).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable NetGrossValueStringComment_3D_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_3D_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_3D_Success).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable NetGrossValueStringComment_4D_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_4D_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_4D_Success).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable NetGrossValueStringComment_5D_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_5D_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_5D_Success).ExpectedResult = 1;
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable NetGrossValueStringComment_6D_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_6D_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.NetGrossValueStringComment_6D_Success).ExpectedResult = 1;
            }
        }


        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "T_UnitValueTestCases")]
        public int testUnit_t(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
           
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58],new string[58]));

            return _wtxObj.Unit;
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "KG_UnitValueTestCases")]
        public int testUnit_kg(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);

            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.Unit;
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "G_UnitValueTestCases")]
        public int testUnit_g(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);

            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.Unit;
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "LB_UnitValueTestCases")]
        public int testUnit_lb(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.Unit;
        }

        
        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "NetGrossValueStringComment_0D_TestCase_Modbus")]
        public void testModbus_NetGrossValueStringComment_0D(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            string strValue = _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals);
            double dValue = _wtxObj.GrossValue / Math.Pow(10, _wtxObj.Decimals);

            Assert.AreEqual(dValue.ToString(), strValue);
        }
        

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "NetGrossValueStringComment_1D_TestCase_Modbus")]
        public void testModbus_NetGrossValueStringComment_1D(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));
            string strValue = _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals);

            double dValue = _wtxObj.GrossValue / Math.Pow(10, _wtxObj.Decimals);

            Assert.AreEqual(dValue.ToString(), strValue);
        }
        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "NetGrossValueStringComment_2D_TestCase_Modbus")]
        public void testModbus_NetGrossValueStringComment_2D(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            string strValue = _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals);
            double dValue = _wtxObj.GrossValue / Math.Pow(10, _wtxObj.Decimals);

            Assert.AreEqual(dValue.ToString(), strValue);
        }
        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "NetGrossValueStringComment_3D_TestCase_Modbus")]
        public void testModbus_NetGrossValueStringComment_3D(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            string strValue = _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals);
            double dValue = _wtxObj.GrossValue / Math.Pow(10, _wtxObj.Decimals);

            Assert.AreEqual(dValue.ToString(), strValue);
        }
        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "NetGrossValueStringComment_4D_TestCase_Modbus")]
        public void testModbus_NetGrossValueStringComment_4D(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            string strValue = _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals);
            double dValue = _wtxObj.GrossValue / Math.Pow(10, _wtxObj.Decimals);

            Assert.AreEqual(dValue.ToString(), strValue);
        }
        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "NetGrossValueStringComment_5D_TestCase_Modbus")]
        public void testModbus_NetGrossValueStringComment_5D(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            string strValue = _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals);
            double dValue = _wtxObj.GrossValue / Math.Pow(10, _wtxObj.Decimals);

            Assert.AreEqual(dValue.ToString(), strValue);
        }
        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "NetGrossValueStringComment_6D_TestCase_Modbus")]
        public void testModbus_NetGrossValueStringComment_6D(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            string strValue = _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals);
            double dValue = _wtxObj.GrossValue / Math.Pow(10, _wtxObj.Decimals);

            Assert.AreEqual(dValue.ToString(), strValue);
        }




        /*
        public string NetGrossValueStringComment(int value, int decimals)
        {
            double dvalue = value / Math.Pow(10, decimals);
            string returnvalue = "";

            switch (decimals)
            {
                case 0: returnvalue = dvalue.ToString(); break;
                case 1: returnvalue = dvalue.ToString("0.0"); break;
                case 2: returnvalue = dvalue.ToString("0.00"); break;
                case 3: returnvalue = dvalue.ToString("0.000"); break;
                case 4: returnvalue = dvalue.ToString("0.0000"); break;
                case 5: returnvalue = dvalue.ToString("0.00000"); break;
                case 6: returnvalue = dvalue.ToString("0.000000"); break;
                default: returnvalue = dvalue.ToString(); break;

            }
            return returnvalue;
        }

        public string WeightMovingStringComment()
        {
            if (this.WeightMoving == 0)
                return "0=Weight is not moving.";
            else
                if (this.WeightMoving == 1)
                return "1=Weight is moving";
            else
                return "Error";
        }
        public string LimitStatusStringComment()
        {
            switch (this.LimitStatus)
            {
                case 0:
                    return "Weight within limits";
                case 1:
                    return "Lower than minimum";
                case 2:
                    return "Higher than maximum capacity";
                case 3:
                    return "Higher than safe load limit";
                default:
                    return "Error.";
            }
        }
        public string WeightTypeStringComment()
        {
            if (this.WeightType == 0)
            {
                this._isNet = false;
                return "gross";
            }
            else
                if (this.WeightType == 1)
            {
                this._isNet = true;
                return "net";
            }
            else

                return "error";
        }
        public string ScaleRangeStringComment()
        {
            switch (this.ScaleRange)
            {
                case 0:
                    return "Range 1";
                case 1:
                    return "Range 2";
                case 2:
                    return "Range 3";
                default:
                    return "error";
            }
        }
        */

        private void ReadDataReceived(IDeviceData obj)
        {
            throw new NotImplementedException();
        }

        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }
    }
}
