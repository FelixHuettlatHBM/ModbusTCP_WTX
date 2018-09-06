/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120_Modbus | 03/2018
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

using HBM.WT.API.WTX.Modbus;
using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Jet;

namespace WTXGUISimple
{
    // This class provides a window to calibrate the WTX with a calibration weight.
    // First ´the dead load is measured and after that the calibration weight is measured.
    // You can step back with button2 (Back).

    public partial class WeightCalibration : Form
    {
        private JetBusConnection _jetObj;
        private int _state;
        private double _calibrationWeight;
        //private IFormatProvider Provider;

        private double _powCalibrationWeight;
        private double _potenz;

        private string _strCommaDot;

        // Constructor of class WeightCalibration: 
        public WeightCalibration(JetBusConnection jetObjParam, bool connected)
        {
            this._powCalibrationWeight = 0.0;
            this._potenz = 0.0;

            this._jetObj = jetObjParam;
            _state = 0;

            _strCommaDot = "";
            //Provider for english number format
            //Provider = CultureInfo.InvariantCulture;

            InitializeComponent();

            if (!connected)
            {
                textBox1.Enabled = false;
                button1.Enabled = false;
                button2.Text = "Close";
                textBox2.Text = "No WTX connected!";
            }

            /*
            switch (_jetObj.Unit)
            {
                case 0:
                    label2.Text = "kg";
                    break;
                case 1:
                    label2.Text = "g";
                    break;
                case 2:
                    label2.Text = "t";
                    break;
                case 3:
                    label2.Text = "lb";
                    break;
                default:
                    label2.Text = "unit";
                    break;
            }
            */

            textBox2.Text = "Enter a calibration weight";
        }

    /*
        private int PotencyCalibrationWeight()
        {
            //this.DoubleCalibrationWeight = Convert.ToDouble(textBox1.Text, Provider); 

            this._potenz = Math.Pow(10, _jetObj.Decimals);

            this._powCalibrationWeight = _calibrationWeight * _potenz;

            return (int)this._powCalibrationWeight;
        }
    */

        // Limits the input of the textbox to digits, ',' and '.'
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
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

        // Choose the action of the cancel/back button, depending on the current
        // calibration state
        private void button2_Click(object sender, EventArgs e)
        {
            switch (_state)
            {
                case 0:
                    Close();
                    break;
                case 1:
                    _state = 0;
                    button2.Text = "Cancel";
                    textBox1.Enabled = true;
                    button1.Text = "Start";
                    textBox2.Text = "";
                    break;
                case 2:
                    _state = 1;
                    textBox2.Text = "Unload Scale!";
                    button1.Text = "Measure Zero";
                    break;
                default:
                    _state = 2;
                    textBox2.Text = "Dead load measured." + Environment.NewLine + "Put weight on scale.";
                    button1.Text = "Calibrate";
                    break;
            }
        }

        private void WeightCalibration_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        // Calls the correct method depending on the current calibration state "State" and 
        // adapts respectively the text on the button "Start".
        // The states are start, measure zero, measure calibration weight, close.
        private void button1_Click(object sender, EventArgs e)
        {
            string[] argument = new string[3];

            switch(_state)
            {
                case 0: // start

                    textBox2.Text = "Unload Scale!";
                    button1.Text = "Measure Zero";
                    button2.Text = "<Back";

                    _state = 1;

                    break;

                case 1:  // measure zero

                    textBox2.Text = "Measure zero in progess.";

                    argument[0] = "6002/01";
                    argument[1] = "6002/01";
                    argument[2] = "2053923171";

                    WriteParameter(argument);

                    Thread.Sleep(5000);

                    textBox2.Text = "Dead load measured." + Environment.NewLine + "Put weight on scale.";
                    button1.Text = "Calibrate";
                    _state = 2;

                    break;

                case 2: // start calibration 

                    textBox2.Text = "Calibration in progress.";

                    argument[0] = "6002/01";
                    argument[1] = "6002/01";
                    argument[2] = "1852596579";

                    WriteParameter(argument);

                    Thread.Sleep(3000);

                    _jetObj.WriteInt("6002/01", 15000);

                    textBox2.Text = "Calibration successful and finished.";
                    button1.Text = "Close";

                    _state = 3; 

                    break;

                default:

                    this.Close();
                    _state = 0;

                    break; 
            }

            /*
            // state 0 : 
            textBox2.Text = "Unload Scale!";
            button1.Text = "Measure Zero";
            button2.Text = "<Back";

            // state 1 : 
            argument[0] = "6002/01";
            argument[1] = "6002/01";
            argument[2] = "2053923171";

            WriteParameter(argument);
            Thread.Sleep(5000);

            // state 2: 
            argument[0] = "6002/01";
            argument[1] = "6002/01";
            argument[2] = "1852596579";

            WriteParameter(argument);
            */    
        }

        private int WriteParameter(string[] args)
        {
            if (args.Length < 3)
                return -1;

            int value = Convert.ToInt32(args[2]);

            _jetObj.Write(args[1], value);

            return 0;
        }

    }
}