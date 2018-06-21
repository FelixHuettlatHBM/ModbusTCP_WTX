/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 02/2018
 * 
 * Author : Felix Retsch 
 * 
 *  */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hbm.Devices.WTXModbus;
using WTXModbus;

namespace WTXModbusGUIsimple
{
    // This class provides a window to perform a calibration without a calibration weight,
    // based on know values for dead load and nominal load in mV/V
    public partial class CalcCalibration : Form
    {
        private WTX120 WTXObj;

        private bool Finished;
        private double Preload;
        private double Capacity;
        private IFormatProvider Provider;
        private const double MultiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)
        private string str_comma_dot;
              
        // Constructor of class 'CalcCalibration' : 
        public CalcCalibration(WTX120 WTXObj, bool connected)
        {
            this.WTXObj = WTXObj;
            
            Finished = false;
            //Provider for english number format
            Provider = CultureInfo.InvariantCulture;

            str_comma_dot = "";

            InitializeComponent();

            if (!connected)
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                buttonCalculate.Text = "Close";
                Finished = true;
                label5.Visible = true;
                label5.Text = "No WTX connected!";
            }
        }

        // Checks the input values of the textboxes and start the calibration calculation.
        // If the caluclation is finished, the window can be closed with the button.
        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            if (!Finished)
            {
                label5.Visible = true;
                bool abort = false;
                try
                {
                    //Preload = Convert.ToDouble(textBox1.Text, Provider);
                    str_comma_dot = textBox1.Text.Replace(".", ",");          
                    Preload = double.Parse(str_comma_dot);
                    textBox1.Enabled = false;
                }
                catch (FormatException)
                {
                    label5.Text = "wrong number format";
                    abort = true;
                }
                catch (OverflowException)
                {
                    label5.Text = "Overflow! Number to big.";
                    abort = true;
                }

                try
                {
                    //Capacity = Convert.ToDouble(textBox2.Text, Provider);
                    str_comma_dot = textBox2.Text.Replace(".", ",");                   
                    Capacity = double.Parse(str_comma_dot);
                    textBox2.Enabled = false;
                }
                catch (FormatException)
                {
                    label5.Text = "wrong number format";
                    abort = true;
                }
                catch (OverflowException)
                {
                    label5.Text = "Overflow! Number to big.";
                    abort = true;
                }
                if (abort) return;
                
                WTXObj.Calculate(Preload,Capacity);

                label5.Text = "Calibration Successful!";
                Finished = true;
                buttonCalculate.Text = "Close";
            }
            else
            {

                Close();
            }
        }

        // Limits the input of the textbox 1/2 to digits, ',' and '.'
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == ',')
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        // This is a callback method for the synchronous command, a write instruction to the WTX registers. 
        // Once the writing is finished, this method is called. So the handshake and status bits are updated if
        // the user is interested in the data transfer between application and WTX device. 
        // Updating the handshake and status bit here is not necessary, because the data transfer is done
        // in class 'WTX120' and 'ModbusConnection'. 
        // By this optional example it is also shown how data can be simply called in another way:
        // By 'obj.NetValue', 'obj.GrossValue' or 'obj.handshake'.

/*
        private void WriteDataReceived(IDeviceValues obj)
        {
            this.handshake_compare_optional = obj.handshake;
            this.status_compare_optional = obj.status;
        }
*/
        private void CalcCalibration_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}