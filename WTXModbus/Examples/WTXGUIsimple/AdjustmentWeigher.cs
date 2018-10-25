// <copyright file="AdjustmentWeigher.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// WTXGUIsimple, a demo application for HBM Weighing-API  
//
// The MIT License (MIT)
//
// Copyright (C) Hottinger Baldwin Messtechnik GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>

using HBM.WT.API;
using System;
using System.Threading;
using System.Windows.Forms;


namespace WTXModbusGUIsimple
{
    // This class provides a window to calibrate the WTX with a calibration weight.
    // First ´the dead load is measured and after that the calibration weight is measured.
    // You can step back with button2 (Back).

    public partial class AdjustmentWeigher : Form
    {
        private BaseWtDevice _wtxDevice;
        private int _state = 0;
        
        private double _calibrationWeight = 0.0;
        private int _wtxDeviceDecimals = 1;

        private double potency, expCalibrationWeight;
        private string _calibrationWeightWithComma;

        // Constructor of class WeightCalibration: 
        public AdjustmentWeigher(BaseWtDevice wtxDevice)
        {
            this._wtxDevice = wtxDevice;

            InitializeComponent();

            if (!wtxDevice.isConnected)
            {
                txtCalibrationWeight.Enabled = false;
                cmdAdjust.Enabled = false;
                cmdCancel.Text = "Close";
                txtInfo.Text = "No WTX connected!";
            }

            // Get some settings from the connected Device
            _wtxDeviceDecimals = wtxDevice.Decimals;

            switch (wtxDevice.Unit)
            {
                case 0:
                    lblUnit.Text = "kg";
                    break;
                case 1:
                    lblUnit.Text = "g";
                    break;
                case 2:
                    lblUnit.Text = "t";
                    break;
                case 3:
                    lblUnit.Text = "lb";
                    break;
                default:
                    lblUnit.Text = "unit";
                    break;
            }

            txtInfo.Text = "Enter a calibration weight";
        }


        private void AdjustmentStateMachine()
        {

            //Switch depending on the current calibration step described by State
            switch (_state)
            {
                case 0: //start

                    try
                    {
                        _calibrationWeightWithComma = txtCalibrationWeight.Text.Replace(".", ",");  // Accept comma and dot
                        _calibrationWeight = double.Parse(_calibrationWeightWithComma);
                        txtCalibrationWeight.Enabled = false;
                        txtInfo.Text = _calibrationWeight.ToString();

                    }
                    catch (FormatException)
                    {
                        txtInfo.Text = "Wrong format!" + Environment.NewLine
                        + "Accepted format(comma): " + _wtxDevice.NetGrossValueStringComment(19876, _wtxDevice.Decimals)
                        + " ; or(dot): " + _wtxDevice.NetGrossValueStringComment(19876, _wtxDevice.Decimals).Replace(",", ".");
                        break;
                    }
                    catch (OverflowException)
                    {
                        txtInfo.Text = "Overflow!";
                        break;
                    }
                    
                    txtInfo.Text = "Unload Scale!";
                    cmdAdjust.Text = "Measure Zero";
                    cmdCancel.Text = "<Back";
                    _state = 1;
                    break;

                case 1: // measure zero

                    txtInfo.Text = "Measure zero in progess.";
                    Application.DoEvents(); //Change txtInfo

                    _wtxDevice.MeasureZero();

                    while (_wtxDevice.Handshake == 0) ;   // polling for a synchronous execution of the calibration. 

                    txtInfo.Text = "Zero load measured." + Environment.NewLine + "Put weight on scale.";
                    cmdAdjust.Text = "Calibrate";
                    _state = 2;

                    break;

                case 2: // start calibration   

                    txtInfo.Text = "Calibration in progress.";
                    Application.DoEvents();              //For changing the 'txtInfo' textbox. 

                    _wtxDevice.Calibrate(this.CalibrationWeightWithoutDecimals(), _calibrationWeight.ToString());

                    while (_wtxDevice.Handshake == 0) ;   // polling for a synchronous execution of the calibration. 
                    
                    cmdAdjust.Text = "Check";
                    _state = 3;

                    break;

                case 3:  // Check calibration:

                    if (
                         _wtxDevice.NetValue == (int)expCalibrationWeight ||
                        (_wtxDevice.NetValue > (int) expCalibrationWeight && _wtxDevice.NetValue < (int) expCalibrationWeight + 5) ||
                        (_wtxDevice.NetValue < (int) expCalibrationWeight && _wtxDevice.NetValue > (int) expCalibrationWeight - 5)
                        )
                        txtInfo.Text = "Calibration finished and successful";

                    else
                        txtInfo.Text = "Calibration failed.";
                    
                    cmdAdjust.Text = "Close";
                    _state = 4;
                    break;

                default: //close window

                    _state = 0;
                    Close();
                    break;
            }
        } 


        // Calls the correct method depending on the current calibration state "State" and 
        // adapts respectively the text on the button "Start".
        // The states are start, measure zero, measure calibration weight, close.
        private void cmdAdjust_Click(object sender, EventArgs e)
        {
            cmdAdjust.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            AdjustmentStateMachine();

            cmdAdjust.Enabled = true;
            this.Cursor = Cursors.Default;
        }
        

        private int CalibrationWeightWithoutDecimals()
        {
            potency = Math.Pow(10, _wtxDevice.Decimals);

            expCalibrationWeight = _calibrationWeight * potency;

            return (int) expCalibrationWeight;

            /*
            int _noDecimalFactor = (int) Math.Pow(10, _wtxDeviceDecimals);
            return _calibrationWeight * _noDecimalFactor;
            */    
        }

        // Limits the input of the textbox to digits, ',' and '.'
        private void txtCakibrationWeight_KeyPress(object sender, KeyPressEventArgs e)
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
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            switch (_state)
            {
                case 0:
                    Close();
                    break;
                case 1:
                    _state = 0;
                    cmdCancel.Text = "Cancel";
                    txtCalibrationWeight.Enabled = true;
                    cmdAdjust.Text = "Start";
                    txtInfo.Text = "";
                    break;
                case 2:
                    _state = 1;
                    txtInfo.Text = "Unload Scale!";
                    cmdAdjust.Text = "Measure Zero";
                    break;
                default:
                    _state = 2;
                    txtInfo.Text = "Zero load measured." + Environment.NewLine + "Put weight on scale.";
                    cmdAdjust.Text = "Calibrate";
                    break;
            }
        }

        private void AdjustmentWeigher_Load(object sender, EventArgs e)
        {

        }
    }
}
