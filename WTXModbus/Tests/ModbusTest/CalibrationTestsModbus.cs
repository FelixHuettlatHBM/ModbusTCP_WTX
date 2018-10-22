using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Modbus;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBM.WT.API.WTX.Modbus
{
    [TestFixture]
    public class CalibrationTestsModbus
    {

        // Test case source for writing values to the WTX120 device : Calibrating
        public static IEnumerable CalculateCalibrationTestCases
        {
            get
            {
                yield return new TestCaseData(Behavior.CalibrationFail).Returns(false);
                yield return new TestCaseData(Behavior.CalibrationSuccess).Returns(true);
            }
        }

        // The following 2 tests as a first draw : Implementation for the following 2 tests follows in the week from 27.08-31.08

        [Test, TestCaseSource(typeof(CalibrationTestsModbus), "CalculateCalibrationTestCases")]
        public bool CalculateCalibrationTest(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");
            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

            WTXModbusObj.Connect(this.OnConnect, 100);

            double preload = 1;
            double capacity = 2;

            double multiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)

            double dPreload = preload * multiplierMv2D;
            double dNominalLoad = dPreload + (capacity * multiplierMv2D);

            WTXModbusObj.Calculate(preload, capacity);

            // Testbedingung noch bearbeiten: 

            if (
                (testConnection.getArrElement1 == (Convert.ToInt32(dPreload) & 0xffff0000) >> 16) &&
                (testConnection.getArrElement2 == (Convert.ToInt32(dPreload) & 0x0000ffff)) &&

                (testConnection.getArrElement3 == (Convert.ToInt32(dNominalLoad) & 0xffff0000) >> 16) &&
                (testConnection.getArrElement4 == (Convert.ToInt32(dNominalLoad) & 0x0000ffff)) &&

                testConnection.getCommand == 0x100 && WTXModbusObj.getDPreload == dPreload && WTXModbusObj.getDNominalLoad == dNominalLoad
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



        [Test, TestCaseSource(typeof(CalibrationTestsModbus), "CalculateCalibrationTestCases")]
        public bool CalibrationTest(Behavior behavior)
        {
            TestModbusTCPConnection testConnection = new TestModbusTCPConnection(behavior, "172.19.103.8");

            WtxModbus WTXModbusObj = new WtxModbus(testConnection, 200);

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

        private void OnConnect(bool obj)
        {
            throw new NotImplementedException();
        }
    }
}
