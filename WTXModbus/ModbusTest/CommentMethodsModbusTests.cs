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

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "T_UnitValueTestCases")]
        public int testUnit_t(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);
           
            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58],new string[58]));

            return WTXModbusObj.Unit;
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "KG_UnitValueTestCases")]
        public int testUnit_kg(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return WTXModbusObj.Unit;
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "G_UnitValueTestCases")]
        public int testUnit_g(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return WTXModbusObj.Unit;
        }

        [Test, TestCaseSource(typeof(CommentMethodsModbusTests), "LB_UnitValueTestCases")]
        public int testUnit_lb(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);
            WTXModbusObj.isConnected = true;

            testConnection.ReadRegisterPublishing(new DataEvent(new ushort[58], new string[58]));

            return WTXModbusObj.Unit;
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
