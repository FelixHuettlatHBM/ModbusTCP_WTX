/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120_Modbus | 02/2018
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

using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Jet;

namespace WTXGUISimple
{
    // This class provides a window to perform a calibration without a calibration weight,
    // based on know values for dead load and nominal load in mV/V
    public partial class CalcCalibration : Form
    {
        private JetBusConnection _jetObj;

        private bool _finished;
        private double _preload;
        private double _capacity;
        private IFormatProvider _provider;
        private const double MULTIPLIER_MV2_D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)
        private string _strCommaDot;       

        // Constructor of class 'CalcCalibration' : 
        public CalcCalibration(JetBusConnection jetObjParam, bool connected)
        {
            this._jetObj = jetObjParam;
            
            _finished = false;
            //Provider for english number format
            _provider = CultureInfo.InvariantCulture;

            _strCommaDot = "";

            InitializeComponent();

            if (!connected)
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                buttonCalculate.Text = "Close";
                _finished = true;
                label5.Visible = true;
                label5.Text = "No WTX connected!";
            }
        }

        // Checks the input values of the textboxes and start the calibration calculation.
        // If the caluclation is finished, the window can be closed with the button.
        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            if (!_finished)
            {
                label5.Visible = true;
                bool abort = false;
                try
                {
                    //Preload = Convert.ToDouble(textBox1.Text, Provider);
                    _strCommaDot = textBox1.Text.Replace(".", ",");
                    _preload = double.Parse(_strCommaDot);
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
                    _strCommaDot = textBox2.Text.Replace(".", ",");
                    _capacity = double.Parse(_strCommaDot);
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

                string[] argument = new string[3];

                argument[0] = "6002/01";
                argument[1] = "6002/01";
                argument[2] = "1852596579";

                WriteParameter(argument);

                label5.Text = "Calibration Successful!";
                _finished = true;
                buttonCalculate.Text = "Close";
            }
            else
            {

                Close();
            }
        }


        private int WriteParameter(string[] args)
        {
            if (args.Length < 3)
                return -1;

            int value = Convert.ToInt32(args[2]);

            _jetObj.Write(args[1], value);

            return 0;
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
        // in class 'WTX120_Modbus' and 'ModbusConnection'. 
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