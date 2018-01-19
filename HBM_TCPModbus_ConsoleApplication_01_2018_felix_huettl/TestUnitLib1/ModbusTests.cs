using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTXModbus;

using NUnit.Framework;   // NUnit framework for testing


namespace TestUnitLib1
{
    [TestFixture]
    public class ModbusTests
    {

        private Modbus Test_Modbus;




        // A test for Modbus Constructor:
        [Test]
        public void Modbus_ConstructorTest()
        {
            Test_Modbus = new Modbus();

            Assert.IsNotNull(Test_Modbus);

            Assert.AreEqual(0, Test_Modbus.get_before());
            Assert.AreEqual(0, Test_Modbus.get_time());
            Assert.AreEqual(true, Test_Modbus.get_is_standard());
            Assert.AreEqual(false, Test_Modbus.get_is_connected());

            Assert.AreEqual("", Test_Modbus.get_value());
            Assert.AreEqual("172.19.103.8", Test_Modbus.get_IP());
            Assert.AreEqual(0, Test_Modbus.get_startAddress());
            Assert.AreEqual(38, Test_Modbus.get_numInputs());
            Assert.AreEqual(1, Test_Modbus.get_timer1_interval());
            Assert.AreEqual(1, Test_Modbus.get_sending_interval());
            Assert.AreEqual(0, Test_Modbus.get_handshake_bit());
            Assert.AreEqual(0, Test_Modbus.get_status_bit());
        }

        // First test method to check whether the change between gross and net is right:
        [Test]
        public void Gross_Test()
        {
            // Arrange all necessary preconditions and inputs:
            Test_Modbus = new Modbus();

            Test_Modbus.data_transfer(0x2);

            if (Test_Modbus.get_weight_type() == false)
            {
                // weight_type = false -> Gross measurement 
                Assert.AreEqual(Test_Modbus.get_complete_measure(), Test_Modbus.get_gross_measure());
                Assert.AreEqual("gross", Test_Modbus.get_weight_type());
                Assert.AreEqual(0, Test_Modbus.get_manual_tare());
            }
        }

        [Test]
        public void Net_Test()
        {
            // Arrange all necessary preconditions and inputs:
            Test_Modbus = new Modbus();

            Test_Modbus.data_transfer(0x2);

            if (Test_Modbus.get_weight_type() == true)
            {
                // weight_type = true -> Net measurement 
                Assert.AreEqual(Test_Modbus.get_complete_measure(), Test_Modbus.get_net_measure());
                Assert.AreEqual("net", Test_Modbus.get_weight_type());
                Assert.AreEqual(1, Test_Modbus.get_manual_tare());
            }
        }

        [Test]
        public void Handshake_Test()
        {
            Test_Modbus = new Modbus();

            Test_Modbus.data_transfer(0x2);         // Beliebiges Kommando ausführen -> Umstellung Gross/Net

            Assert.AreEqual(0, Test_Modbus.get_handshake_bit());        // Wenn die Funktion richtig ausgeführt wurde, werden diese Bits gesetzt oder eben nicht. 
            Assert.AreEqual(1, Test_Modbus.get_status_bit());  
        }

        [Test]
        public void Weight_Moving_Test()
        {
            Test_Modbus = new Modbus();

            // Abfrage ob ich zur Laufzeit des Programm der Messwert ändert

            Assert.AreEqual(1, Test_Modbus.get_weight_moving());        
        }

        [Test]
        public void Weight_too_high_Test()
        {
            Test_Modbus = new Modbus();

            if (Convert.ToDouble(Test_Modbus.get_complete_measure()) >= 2.00000)
            {
                Assert.AreEqual(1, Test_Modbus.get_general_weight_error());
                Assert.AreEqual("Higher than maximum capacity", Test_Modbus.get_limit_status());
            }
        }

        [Test]
        public void Weight_too_low_Test()
        {
            Test_Modbus = new Modbus();

            if (Convert.ToDouble(Test_Modbus.get_complete_measure()) <= 0.00000)
            {
                Assert.AreEqual(1, Test_Modbus.get_general_weight_error());
                Assert.AreEqual("Lower than minimum", Test_Modbus.get_limit_status());
            }
        }

        [Test]
        public void Weight_within_limits_Test()
        {
            Test_Modbus = new Modbus();

            if (Convert.ToInt32(Test_Modbus.get_complete_measure()) < 2 && Convert.ToDouble(Test_Modbus.get_complete_measure()) > 0.00000)
            {
                Assert.AreEqual(0, Test_Modbus.get_general_weight_error());
                Assert.AreEqual("Weight within limits", Test_Modbus.get_limit_status());
            }
        }

        [Test]
        public void Weight_within_the_center_of_zero_Test()
        {
            Test_Modbus = new Modbus();
            
            if (Test_Modbus.get_complete_measure() == "0.00000" | Convert.ToDouble(Test_Modbus.get_complete_measure()) == 0.00000)
                Assert.AreEqual(1, Test_Modbus.get_weight_within_the_center_of_zero());
        }

        [Test]
        public void Weight_in_zero_range_Test()
        {
            Test_Modbus = new Modbus();

            if (Convert.ToDouble(Test_Modbus.get_complete_measure()) <= 0.04000)
                Assert.AreEqual(1, Test_Modbus.get_weight_in_zero_range());

            else
                if(Convert.ToDouble(Test_Modbus.get_complete_measure()) > 0.04000)
                  Assert.AreEqual(0, Test_Modbus.get_weight_in_zero_range());
        }

        [Test]
        public void Application_mode_Test()
        {
            Test_Modbus = new Modbus();

            if (Test_Modbus.get_is_standard() == true)
                Assert.AreEqual("Standard", Test_Modbus.get_application_mode());
            else
                if (Test_Modbus.get_is_standard() == false)
                Assert.AreEqual("Filler", Test_Modbus.get_application_mode());
        }



        // *Missing Test : Higher than safe load limit???*

    }
}


/* 
Arrange all necessary preconditions and inputs:
Act on the object or method under test: standardApplicationToolStripMenuItem_Click_1
Assert, that the expected results have occured:
*/          

        
