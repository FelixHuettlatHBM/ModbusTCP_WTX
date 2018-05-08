﻿/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 03/2018
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
    // This class provides a window to calibrate the WTX with a calibration weight.
    //
    // First dead load is measured and after that the calibration weight is measured.
    // You can step back with button2 (Back).

    public partial class WeightCalibration : Form
    {

        //private System.Windows.Forms.Timer myTimer;      // Neu : 8.3.2018 - Idee : Timer für zyklische Abfrage der Werte nutzen um Asynchronität zu nutzen, hier momentan noch nicht angewendet. 

        private WTX120Modbus WTXObj;
        private int State;
        private double CalibrationWeight;
        //private IFormatProvider Provider;

        private int handshake_compare;      // Neu : 8.3.2018
        private int status_compare;         // Neu : 8.3.2018

        private double PowCalibrationWeight; // Neu : 9.3.2018
        private double potenz;

        private string str_comma_dot;


        public WeightCalibration(WTX120Modbus WTXObj, bool connected)
        {
            //myTimer = new System.Windows.Forms.Timer();
            //myTimer.Tick += new EventHandler(timerWeightCalibrationTick);

            this.PowCalibrationWeight = 0.0;
            this.potenz = 0.0;

            this.handshake_compare = 0;
            this.status_compare = 0;

            this.WTXObj = WTXObj;
            State = 0;

            str_comma_dot = "";
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

            switch (WTXObj.unit)
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
            this.Cursor=Cursors.WaitCursor;

            //Switch depending on the current calibration step described by State
            switch (State)
            {          
                case 0: //start

                    try
                    {
                        //CalibrationWeight = Convert.ToDouble(textBox1.Text, Provider);

                        str_comma_dot = textBox1.Text.Replace(".", ",");                   // Neu : 12.3 - Für die Umwandlung in eine Gleitkommazahl. 
                        CalibrationWeight = double.Parse(str_comma_dot);                   // Damit können Kommata und Punkte eingegeben werden. 

                        textBox1.Enabled = false;
                        textBox2.Text = CalibrationWeight.ToString();
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
                    State = 1;
                    break;

                case 1: // measure zero

                    //myTimer.Interval = 1;     // Neu : 8.3.2018 - Timer für einen asnychrones Lesen und Schreiben, was aber beim Kalibrieren wenig Sinn machen würde. 
                    //myTimer.Start();         
                    
                    button1.Enabled = false;

                    textBox2.Text = "Measure zero in progess.";
                    Application.DoEvents();

                    this.MeasureZero();

                    textBox2.Text = "Dead load measured." + Environment.NewLine + "Put weight on scale.";
                    button1.Text = "Calibrate";
                    State = 2;

                    break;

                case 2: // start calibration   

                    button1.Enabled = false;

                    textBox2.Text = "Calibration in progress.";
                    Application.DoEvents();

                    this.Calibrate(this.potencyCalibrationWeight());

                    if(WTXObj.status==1 && WTXObj.handshake==0)
                        textBox2.Text = "Calibration finished successfully!";
                    else
                        textBox2.Text = "Calibration failed.";

                    button1.Text = "Close";
                    State = 3;
                    break;

                default: //close window

                    button1.Enabled = false;
                    State = 0;

                    //myTimer.Enabled = false;        // Neu : 8.3.2018 - Timer für einen asnychrones Lesen und Schreiben, was aber beim Kalibrieren wenig Sinn machen würde. 
                    //myTimer.Stop();

                    Close();
                    break;
            }
            button1.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        // Neu : 9.3.2018
        private int potencyCalibrationWeight()
        {
            //this.DoubleCalibrationWeight = Convert.ToDouble(textBox1.Text, Provider); 

            this.potenz = Math.Pow(10, WTXObj.decimals);

            this.PowCalibrationWeight = CalibrationWeight * potenz;

            return (int)this.PowCalibrationWeight;
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

        // Sets the value for dead load in the WTX
        private void MeasureZero()
        {
            //todo: write reg 48, 0x7FFFFFFF

            WTXObj.write_Zero_Calibration_Nominal_Load('z', 0x7FFFFFFF, WriteDataReceived);           // 'z' steht für das Setzen der zero load.

            WTXObj.SyncCall_Write_Command(0, 0x80, WriteDataReceived);          
        }

        // Sets the value for the nominal weight in the WTX
        private void Calibrate(int calibrationValue)
        {
            this.handshake_compare = 0;
            this.status_compare = 0;
            
            //write reg 46, CalibrationWeight

            WTXObj.write_Zero_Calibration_Nominal_Load('c', calibrationValue, WriteDataReceived);          // 'c' steht für das Setzen der Calibration Weight.

            //write reg 50, 0x7FFFFFFF

            WTXObj.write_Zero_Calibration_Nominal_Load('n', 0x7FFFFFFF, WriteDataReceived);       // 'n' steht für das Setzen der Nominal Load 

            WTXObj.SyncCall_Write_Command(0, 0x100, WriteDataReceived);

        }


        // New(8.3.2018) : This is the method to run when the timer is raised.
        /*private void timerWeightCalibrationTick(Object myObject, EventArgs myEventArgs)
        {
            this.handshake_compare = WTXObj.handshake;
            this.status_compare = WTXObj.status;

            label3.Text = this.handshake_compare.ToString();
            label6.Text = this.status_compare.ToString();

            WTXObj.Async_Call(0x00, ReadDataReceived);
        }*/


        // Callback method executed after read out of the WTX
        private void ReadDataReceived(IDeviceValues obj)
        {
            this.handshake_compare = obj.handshake;
            this.status_compare = obj.status;
        }

        // Callback method executed after write values in the WTX
        private void WriteDataReceived(IDeviceValues obj)
        {
            this.handshake_compare = obj.handshake;
            this.status_compare = obj.status;

            textBox2.Text += "Write executed";
        }

        // Choose the action of the cancel/back button, depending on the current
        // calibration state
        private void button2_Click(object sender, EventArgs e)
        {
            switch (State)
            {
                case 0:
                    Close();
                    break;
                case 1:
                    State = 0;
                    button2.Text = "Cancel";
                    textBox1.Enabled = true;
                    button1.Text = "Start";
                    textBox2.Text = "";
                    break;
                case 2:
                    State = 1;
                    textBox2.Text = "Unload Scale!";
                    button1.Text = "Measure Zero";
                    break;
                default:
                    State = 2;
                    textBox2.Text = "Dead load measured." + Environment.NewLine + "Put weight on scale.";
                    button1.Text = "Calibrate";
                    break;
            }
        }

        private void WeightCalibration_Load(object sender, EventArgs e)
        {

        }
    }
}


/*
 *  // Mit Übertragungsprotokoll: 
 *  
 *             //WTXObj.get_Modbus.WriteRegister(0, 0x80);

            //WTXObj.SyncCall(0, 0x80, WriteDataReceived);

            while (this.handshake_compare == 0)
            {
                WTXObj.Async_Call(0x00, ReadDataReceived);
            }

            if(this.handshake_compare==1)
                WTXObj.Async_Call(0x00, WriteDataReceived);
                //WTXObj.get_Modbus.WriteRegister(0, 0x00);

            while (this.handshake_compare == 1)
            {
                WTXObj.Async_Call(0x00, ReadDataReceived);
            }

    */