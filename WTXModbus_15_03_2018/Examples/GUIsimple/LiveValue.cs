﻿/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
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
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using WTXModbusGUIsimple;

using Hbm.Devices.WTXModbus;
using WTXModbus;

namespace WTXModbusGUIsimple
{
    /// <summary>
    /// This class creates a window, which shows live the measurement values from a via ModbusTCP connected WTX120.
    /// 
    /// First you have to insert the IP-Address of the WTX120, then press "Connect". If the connection could be established
    /// the Net, Gross and Tara values are displayed in nearly real-time. The buttons "Tare" and "Zero" do the same as if you would
    /// use the buttons on the WTX. "Gross/Net" switches the displayed value on the WTX' display.
    /// 
    /// In the menu "Tools" you find to different calibration methods, which open an own window. One is for calibration with a 
    /// calibration weight and one is for calculated calibration with mV/V values.
    /// </summary>
    public partial class LiveValue : Form
    {
        const string DEFAULT_IP_ADDRESS = "172.19.103.8";

        private static ModbusConnection ModbusObj;
        private static WTX120Modbus WTXObj;
        private static int DefaultTimerInterval = 500;
        private String IPAddress;
        private CalcCalibration CalcCalObj;
        private WeightCalibration WeightCalObj;

        // toolStripLabel1: Label connectionStatus
        // toolstripLabel2: Label movingStatus
        // toolstripLabel3: Gross/net status
        // textBox1: Textbox IP-Address
        // textBox2: big Textbox for display values etc.
        // pictureBox1.Image: status pictures

        // Basic Constructor without arguments
        public LiveValue()
        {
            // Implementation of the publisher (Modbus_TCP_obj) and 
            // the subscribter (Device_WTX_obj)

            InitializeComponent();

            IPAddress = WTXModbusGUIsimple.Properties.Settings.Default.IPAddress;
            textBox1.Text = IPAddress;
        }

        // Advanced Constructor with args
        // arg[0] is the default IP-Address.
        // An attempt to connect to default IP is executed.
        public LiveValue(string[] args): this()
        {
            if (args.Length > 0)
            {
                IPAddress = args[0];                
                textBox1.Text = IPAddress;
            }

            this.Connect();
        }


        private void LiveValue_Shown(object sender, EventArgs e)
        {
            this.Connect();
        }

        private void Connect()
        {
            ModbusObj = new ModbusConnection(IPAddress);

            WTXObj = new WTX120Modbus(ModbusObj, 1000);
            
            WTXObj.getConnection.getNumOfPoints = 6;

            WTXObj.DataUpdateEvent += ValuesOnConsole;

            this.toolStripLabel1.Text = "disconnected";
            button1_Click(this, null);
        }

        // Button Connect
        // Uses the new IP-address from textbox1 and tries to set up a connection. 
        // If it was successful, the timer is initialized and started.
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            try
            {
                textBox2.Text = "Trying to connect...";
                toolStripLabel1.Text = "connecting";
                textBox2.TextAlign = HorizontalAlignment.Left;
                pictureBox1.Image = Properties.Resources.NE107_DiagnosisPassive;
                Update();

                String tempIpAddress = textBox1.Text;
                WTXObj.getConnection.IP_Adress = tempIpAddress;
                WTXObj.getConnection.Connect();
                if (WTXObj.getConnection.is_connected)
                {
                    IPAddress = tempIpAddress;
                    this.toolStripLabel1.Text = "connected";
                    //WriteDataReceived(null);
                    RenameButtonGrossNet();
                    WTXObj.getConnection.Sending_interval = DefaultTimerInterval;
                    InitializeTimer(DefaultTimerInterval);
                }
                else
                {
                    WTXObj.getConnection.IP_Adress = IPAddress;
                    timer1.Enabled = false;
                    timer1.Stop();
                    textBox2.Text = "Connection could not be established!" + Environment.NewLine
                        + "Please check connection or IP-Address.";
                    toolStripLabel1.Text = "disconnected";
                    toolStripLabel3.Text = "Gross/Net";
                }
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        // Initializes the timer with the timer_interval as a parameter
        // If new_timer_interval <1, previous respectively default value of 200 is used 
        private void InitializeTimer(int newTimerInterval)
        {
            timer1.Enabled = true;
            if (newTimerInterval  > 0)
            {
                timer1.Interval = newTimerInterval;
            }
            else
            {
                timer1.Interval = DefaultTimerInterval;
            }         
            timer1.Start();
            textBox2.Text = "Connection established.";
        }

        // Method executed after read from WTX by eventbased call from WTX120Modbus, UpdateEvent(..) 
        // Updates displayed values and states
        //public void ReadDataReceived(IDeviceValues deviceValues)

        private void ValuesOnConsole(object sender, NetConnectionEventArgs<ushort[]> e)
        {
            if (WTXObj.limit_status == 0)  //Check for Errors
            {

                textBox2.Invoke(new Action(() =>
                {
                    textBox2.Text = "Net:" + ConvertValue2String(WTXObj.NetValue) + Environment.NewLine
                    + "Gross:" + ConvertValue2String(WTXObj.GrossValue) + Environment.NewLine
                    + "Tara:" + ConvertValue2String(WTXObj.GrossValue - WTXObj.NetValue);
                    textBox2.TextAlign = HorizontalAlignment.Right;
                    pictureBox1.Image = Properties.Resources.NE107_DiagnosisActive;

                }));
            }
            else
            {

                textBox2.Invoke(new Action(() =>
                {
                    pictureBox1.Image = Properties.Resources.NE107_OutOfSpecification;
                    textBox2.Text = ConvertLimitStatus(WTXObj.limit_status);
                    textBox2.TextAlign = HorizontalAlignment.Left;

                }));
            }

            if (WTXObj.weight_moving != 0)
            {
                toolStripLabel2.Text = "Moving";
            }
            else  //Not moving
            {
                toolStripLabel2.Text = "";
            }

            if (WTXObj.weight_type == 0)
            {
                toolStripLabel3.Text = "Gross";
            }
            else
            {
                toolStripLabel3.Text = "Net";
            }

            //RenameButtonGrossNet();

        }

        // Returns a String containing the parameter value transformed with its correct number of decimals
        // and its unit
        private String ConvertValue2String(int value)
        {
            double temp = (double)value;
            String format = "F" + WTXObj.decimals;
            if (WTXObj.decimals > 0)
            {
                temp /= Math.Pow(10, WTXObj.decimals);// (double)(WTXObj.decimals * 10);

            }
            String unit;
            switch (WTXObj.unit)
            {
                case 0:
                    unit = "kg";
                    break;
                case 1:
                    unit = "g";
                    break;
                case 2:
                    unit = "t";
                    break;
                case 3:
                    unit = "lb";
                    break;
                default:
                    unit = "unknown unit";
                    break;
            }

            String ret = temp.ToString(format, CultureInfo.InvariantCulture);
            while (ret.Length < 9)
            {
                ret = " " + ret;
            }

            return ret + unit;
        }

        // Returns the limit status code as readable text string
        private String ConvertLimitStatus(int limitStatus)
        {
            switch (limitStatus)
            {
                case 0:
                    return "Weight within limits";
                case 1:
                    return "W1  U n d e r l o a d";
                case 2:
                    return "W1  O v e r l o a d";
                case 3:
                    return "Higher than safe load limit";
                default:
                    return "Error.";
            }
        }

        // Button Tare
        private void button2_Click(object sender, EventArgs e)
        {
            if (WTXObj.getConnection.is_connected)
            {
                //RenameButtonGrossNet();
                WTXObj.Async_Call(0x1, WriteDataReceived);
            }
            else
            {
                textBox2.Text = "No WTX connected!";
            }

        }

        // Button Zero
        private void button3_Click(object sender, EventArgs e)
        {
            if (WTXObj.getConnection.is_connected)
            { 
                WTXObj.Async_Call(0x40, WriteDataReceived);
            }
            else
            {
                textBox2.Text = "No WTX connected!";
            }
        }       

        // Button Gross/Net
        private void button4_Click(object sender, EventArgs e)
        {
            if (WTXObj.getConnection.is_connected)
            {
                WTXObj.Async_Call(0x2, WriteDataReceived);
                //RenameButtonGrossNet();
            }
            else
            {
                textBox2.Text = "No WTX connected!";
            }
        }

        // CallbackMethod executed after write to WTX
        public void WriteDataReceived(IDeviceValues deviceValues)
        {
            textBox2.Text = "write executed";
            //RenameButtonGrossNet();
        }

        // Adapts button Gross/Net text
        private void RenameButtonGrossNet()
        {
            if (WTXObj.weight_type == 0) //is gross?
            {
                button4.Text = "Net";
            }
            else // is net
            {
                button4.Text = "Gross";
            }
        }

        //Opens a menu window for calculated calibration
        private void CalculateCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool restart = false;
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
                timer1.Stop();
                restart = true;
            }
            CalcCalObj = new CalcCalibration(WTXObj, WTXObj.getConnection.is_connected);
            DialogResult res = CalcCalObj.ShowDialog();
            if (restart)
            {
                timer1.Enabled = true;
                timer1.Start();
            }
        }

        //Opens a menu window for calibration with a calibration weight
        private void CalibrationWithWeightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool restart = false;
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
                timer1.Stop();
                restart = true;
            }
            WeightCalObj = new WeightCalibration(WTXObj, WTXObj.getConnection.is_connected);
            DialogResult res = WeightCalObj.ShowDialog();
            if (restart)
            {
                timer1.Enabled = true;
                timer1.Start();
            }
        }


        private void LiveValue_FormClosing(object sender, FormClosingEventArgs e)
        {
            WTXModbusGUIsimple.Properties.Settings.Default.IPAddress = this.IPAddress;
            WTXModbusGUIsimple.Properties.Settings.Default.Save();
        }

        private void LiveValue_Load(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}