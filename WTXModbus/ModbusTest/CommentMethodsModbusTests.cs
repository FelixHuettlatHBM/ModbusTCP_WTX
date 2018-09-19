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

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ScaleRangeStringComment_Range1_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.ScaleRangeStringComment_Range1_Fail).Returns("Range 3");
                yield return new TestCaseData(Behavior.ScaleRangeStringComment_Range1_Success).Returns("Range 1");
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ScaleRangeStringComment_Range2_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.ScaleRangeStringComment_Range2_Fail).Returns("Range 1");
                yield return new TestCaseData(Behavior.ScaleRangeStringComment_Range2_Success).Returns("Range 2");
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable ScaleRangeStringComment_Range3_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.ScaleRangeStringComment_Range3_Fail).Returns("Range 2");
                yield return new TestCaseData(Behavior.ScaleRangeStringComment_Range3_Success).Returns("Range 3");
            }
        }




        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable LimitStatusStringComment_Case0_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.LimitStatusStringComment_Case0_Fail).Returns("Higher than safe load limit");
                yield return new TestCaseData(Behavior.LimitStatusStringComment_Case0_Success).Returns("Weight within limits");
            }
        }
        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable LimitStatusStringComment_Case1_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.LimitStatusStringComment_Case1_Fail).Returns("Higher than maximum capacity");
                yield return new TestCaseData(Behavior.LimitStatusStringComment_Case1_Success).Returns("Lower than minimum");
            }
        }
        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable LimitStatusStringComment_Case2_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.LimitStatusStringComment_Case2_Fail).Returns("Weight within limits");
                yield return new TestCaseData(Behavior.LimitStatusStringComment_Case2_Success).Returns("Higher than maximum capacity");
            }
        }
        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable LimitStatusStringComment_Case3_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.LimitStatusStringComment_Case3_Fail).Returns("Lower than minimum");
                yield return new TestCaseData(Behavior.LimitStatusStringComment_Case3_Success).Returns("Higher than safe load limit");
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable WeightMovingStringComment_Case0_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WeightMovingStringComment_Case0_Fail).Returns("1=Weight is moving");
                yield return new TestCaseData(Behavior.WeightMovingStringComment_Case0_Success).Returns("0=Weight is not moving.");
            }
        }

        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable WeightMovingStringComment_Case1_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WeightMovingStringComment_Case1_Fail).Returns("0=Weight is not moving.");
                yield return new TestCaseData(Behavior.WeightMovingStringComment_Case1_Success).Returns("1=Weight is moving");
            }
        }


        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable WeightTypeStringComment_Case0_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WeightTypeStringComment_Case0_Fail).Returns("net");
                yield return new TestCaseData(Behavior.WeightTypeStringComment_Case0_Success).Returns("gross");
            }
        }


        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable WeightTypeStringComment_Case1_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WeightTypeStringComment_Case1_Fail).Returns("gross");
                yield return new TestCaseData(Behavior.WeightTypeStringComment_Case1_Success).Returns("net");
            }
        }


        /*
        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable WeightTypeStringComment_Case0_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WeightTypeStringComment_Case0_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.WeightTypeStringComment_Case0_Success).ExpectedResult = 1;
            }
        }


        // Test case source for reading values from the WTX120 device. 
        public static IEnumerable WeightTypeStringComment_Case1_TestCase_Modbus
        {
            get
            {
                yield return new TestCaseData(Behavior.WeightTypeStringComment_Case1_Fail).ExpectedResult = 0;
                yield return new TestCaseData(Behavior.WeightTypeStringComment_Case1_Success).ExpectedResult = 1;
            }
        }
        */


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


        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "ScaleRangeStringComment_Range1_TestCase_Modbus")]
        public string testModbus_ScaleRangeStringComment_Range1(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.ScaleRangeStringComment();
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "ScaleRangeStringComment_Range2_TestCase_Modbus")]
        public string testModbus_ScaleRangeStringComment_Range2(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.ScaleRangeStringComment();
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "ScaleRangeStringComment_Range3_TestCase_Modbus")]
        public string testModbus_ScaleRangeStringComment_Range3(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.ScaleRangeStringComment();       
        }



        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "LimitStatusStringComment_Case0_TestCase_Modbus")]
        public string testModbus_LimitStatusStringComment_Case0(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.LimitStatusStringComment();
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "LimitStatusStringComment_Case1_TestCase_Modbus")]
        public string testModbus_LimitStatusStringComment_Case1(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.LimitStatusStringComment();
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "LimitStatusStringComment_Case2_TestCase_Modbus")]
        public string testModbus_LimitStatusStringComment_Case2(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.LimitStatusStringComment();
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "LimitStatusStringComment_Case3_TestCase_Modbus")]
        public string testModbus_LimitStatusStringComment_Case3(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.LimitStatusStringComment();
        }





        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "WeightMovingStringComment_Case0_TestCase_Modbus")]
        public string testModbus_WeightMovingStringComment_Case0(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

             return _wtxObj.WeightMovingStringComment();
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "WeightMovingStringComment_Case1_TestCase_Modbus")]
        public string testModbus_WeightMovingStringComment_Case1(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.WeightMovingStringComment();        
        }




        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "WeightTypeStringComment_Case0_TestCase_Modbus")]
        public string testModbus_WeightTypeStringComment_Case0(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.WeightTypeStringComment();
            //Assert.AreEqual("gross", strValue);
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "WeightTypeStringComment_Case1_TestCase_Modbus")]
        public string testModbus_WeightTypeStringComment_Case1(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus _wtxObj = new WtxModbus(testConnection, 200);
            _wtxObj.Connect(this.OnConnect, 100);
            _wtxObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return _wtxObj.WeightTypeStringComment();
            //Assert.AreEqual("net", strValue);
        }

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
