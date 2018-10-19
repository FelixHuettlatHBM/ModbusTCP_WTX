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
using HBM.WT.API;
using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Jet;

namespace WTXGUISimple
{
    // This class provides a window to perform a calibration without a calibration weight,
    // based on know values for dead load and nominal load in mV/V
    public partial class CalcCalibration : Form
    {
        private WtxJet _wtxObj;

        private bool _finished;
        private double _preload;
        private double _capacity;
        private IFormatProvider _provider;
        private const double MULTIPLIER_MV2_D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)
        private string _strCommaDot;       

        // Constructor of class 'CalcCalibration' : 
        public CalcCalibration(BaseWtDevice BaseWTObjParam, bool connected)
        {
            this._wtxObj = (WtxJet)BaseWTObjParam;
            
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

                _wtxObj.Calculate(_preload, _capacity);

                label5.Text = "Calibration done via Jetbus";
                _finished = true;
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

        private void CalcCalibration_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}