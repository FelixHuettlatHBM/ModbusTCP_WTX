/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120_Modbus | 03/2018
 * 
 * Author : Felix Retsch 
 * 
 *  */


using HBM.WT.API;
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


namespace WTXModbusGUIsimple
{
    // This class provides a window to calibrate the WTX with a calibration weight.
    // First ´the dead load is measured and after that the calibration weight is measured.
    // You can step back with button2 (Back).

    public partial class WeightCalibration : Form
    {
        private BaseWtDevice _wtxObj;
        private int _state;
        private double _calibrationWeight;
        //private IFormatProvider Provider;

        private double _powCalibrationWeight; 
        private double _potenz;

        private string _strCommaDot;

        // Constructor of class WeightCalibration: 
        public WeightCalibration(BaseWtDevice wtxObj, bool connected)
        {
            this._powCalibrationWeight = 0.0;
            this._potenz = 0.0;
            
            this._wtxObj = wtxObj;
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

            switch (wtxObj.Unit)
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

            textBox2.Text = "Enter a calibration weight";
        }

        // Calls the correct method depending on the current calibration state "State" and 
        // adapts respectively the text on the button "Start".
        // The states are start, measure zero, measure calibration weight, close.
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            //Switch depending on the current calibration step described by State
            switch (_state)
            {
                case 0: //start

                    try
                    {
                        //CalibrationWeight = Convert.ToDouble(textBox1.Text, Provider);

                        _strCommaDot = textBox1.Text.Replace(".", ",");                   // Neu : 12.3 - Für die Umwandlung in eine Gleitkommazahl. 
                        _calibrationWeight = double.Parse(_strCommaDot);                   // Damit können Kommata und Punkte eingegeben werden. 

                        textBox1.Enabled = false;
                        textBox2.Text = _calibrationWeight.ToString();
                    }
                    catch (FormatException)
                    {
                        textBox2.Text = "Wrong format!" + Environment.NewLine
                            + "Only numbers in the form of 123,456,789.0123 allowed!";
                        break;
                    }
                    catch (OverflowException)
                    {
                        textBox2.Text = "Overflow! Number to big.";
                        break;
                    }

                    textBox2.Text = "Unload Scale!";
                    button1.Text = "Measure Zero";
                    button2.Text = "<Back";
                    _state = 1;
                    break;

                case 1: // measure zero

                    button1.Enabled = false;

                    textBox2.Text = "Measure zero in progess.";
                    Application.DoEvents();

                    _wtxObj.MeasureZero();

                    textBox2.Text = "Dead load measured." + Environment.NewLine + "Put weight on scale.";
                    button1.Text = "Calibrate";
                    _state = 2;

                    break;

                case 2: // start calibration   

                    button1.Enabled = false;

                    textBox2.Text = "Calibration in progress.";
                    Application.DoEvents();
                    
                    _wtxObj.Calibrate(this.PotencyCalibrationWeight(), _calibrationWeight.ToString());

                    if (_wtxObj.Status == 1 && _wtxObj.Handshake == 0)
                        textBox2.Text = "Calibration successful and finished.";
                    else
                        textBox2.Text = "Calibration  failed.";

                    button1.Text = "Close";
                    _state = 3;
                    break;

                default: //close window

                    button1.Enabled = false;
                    _state = 0;
                    
                    Close();
                    break;
            }
            button1.Enabled = true;
            this.Cursor = Cursors.Default;
        }
        

        private int PotencyCalibrationWeight()
        {
            //this.DoubleCalibrationWeight = Convert.ToDouble(textBox1.Text, Provider); 

            this._potenz = Math.Pow(10, _wtxObj.Decimals);

            this._powCalibrationWeight = _calibrationWeight * _potenz;

            return (int)this._powCalibrationWeight;
        }

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
    }
}
