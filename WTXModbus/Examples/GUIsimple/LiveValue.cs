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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Modbus;
using HBM.WT.API;

namespace WTXModbusGUIsimple
{
    /// <summary>
    /// This class creates a window, which shows live the measurement values from a via ModbusTCP connected WTX120_Modbus.
    /// 
    /// First you have to insert the IP-Address of the WTX120_Modbus, then press "Connect". If the connection could be established
    /// the Net, Gross and Tara values are displayed in nearly real-time. The buttons "Tare" and "Zero" do the same as if you would
    /// use the buttons on the WTX. "Gross/Net" switches the displayed value on the WTX' display.
    /// 
    /// In the menu "Tools" you find to different calibration methods, which open an own window. One is for calibration with a 
    /// calibration weight and one is for calculated calibration with mV/V values.
    /// </summary>
    public partial class LiveValue : Form
    {
        const string DEFAULT_IP_ADDRESS = "172.19.103.8";

        private static ModbusTcpConnection _modbusObj;
        private static WtxModbus _wtxObj;
        
        private String _ipAddress;
        private CalcCalibration _calcCalObj;
        private WeightCalibration _weightCalObj;

        private int _timerInterval;

        // toolStripLabel1: Label connectionStatus
        // toolstripLabel2: Label movingStatuDefaultTimerIntervals
        // toolstripLabel3: Gross/net status
        // textBox1: Textbox IP-Address
        // textBox2: big Textbox for display values etc.
        // pictureBox1.Image: status pictures

        // Basic Constructor without arguments
        public LiveValue()
        {
            InitializeComponent();

            _ipAddress = WTXModbusGUIsimple.Properties.Settings.Default.IPAddress;
            textBox1.Text = _ipAddress;
        }

        // Advanced Constructor with args
        // arg[0] is the default IP-Address.
        // An attempt to connect to default IP is executed.
        public LiveValue(string[] args): this()
        {
            this.Show();

            this._ipAddress = "172.19.103.8"; ; // Default Setting
            this._timerInterval = 500;          // Default setting

            if (args.Length > 0)
            {
                if (args[0] == "modbus" || args[0] == "Modbus")
                    toolStripLabel4.Text = "Modbus";

                if (args[0] == "jet" || args[0] == "Jet")
                    toolStripLabel4.Text = "Jetbus";
            }
            if (args.Length > 1)
            {
                this._ipAddress = args[1];                
                textBox1.Text  = args[1];
            }

            if (args.Length > 2)
            {
                this._timerInterval = Convert.ToInt32(args[2]);
            }
            else
                this._timerInterval = 200; // Default value for the timer interval. 

            this.Connect();
        }


        private void LiveValue_Shown(object sender, EventArgs e)
        {
            this.Connect();
        }

        // This method is called in the constructor of class LiveValue and establishs a connection. 
        private void Connect()
        {
            _modbusObj = new ModbusTcpConnection(this._ipAddress);

            _wtxObj = new WtxModbus(_modbusObj, this._timerInterval);
            
            _wtxObj.getModbusConnection.NumofPoints = 6;
            
            _wtxObj.DataUpdateEvent += ValuesOnConsole;

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

                string tempIpAddress = textBox1.Text;
                _wtxObj.getModbusConnection.IpAddress = tempIpAddress; // Equal to : ModbusObj.IP_Adress = tempIpAddress;

                // The connection to the device should be established.   
                _wtxObj.getModbusConnection.Connect();     // Alternative : _wtxObj.Connect();    

                if (_wtxObj.getModbusConnection.IsConnected)
                {
                    _wtxObj.RestartTimer();
                    _ipAddress = tempIpAddress;
                    this.toolStripLabel1.Text = "connected";
                    RenameButtonGrossNet();
                    _wtxObj.getModbusConnection.SendingInterval = this._timerInterval;

                }
                else
                {
                    _wtxObj.getModbusConnection.IpAddress = this._ipAddress;

                    _wtxObj.StopTimer();
                    
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

        // Method executed after read from WTX by eventbased call from WTX120Modbus, UpdateEvent(..) 
        // Updates displayed values and states
        //public void ReadDataReceived(IDeviceValues deviceValues)
        private void ValuesOnConsole(object sender, DataEvent e)
        {
            if (_wtxObj.LimitStatus == 0)  //Check for Errors
            {
                int taraValue = _wtxObj.GrossValue - _wtxObj.NetValue;

                textBox2.Invoke(new Action(() =>
                {
                    textBox2.Text = "Net:" + _wtxObj.NetGrossValueStringComment(_wtxObj.NetValue,_wtxObj.Decimals) + _wtxObj.UnitStringComment() + Environment.NewLine
                    + "Gross:" + _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals) + _wtxObj.UnitStringComment() + Environment.NewLine
                    + "Tara:"  + _wtxObj.NetGrossValueStringComment(taraValue        , _wtxObj.Decimals) + _wtxObj.UnitStringComment();
                    textBox2.TextAlign = HorizontalAlignment.Right;
                    pictureBox1.Image = Properties.Resources.NE107_DiagnosisActive;
                }));
            }
            else
            {
                textBox2.Invoke(new Action(() =>
                {
                    pictureBox1.Image = Properties.Resources.NE107_OutOfSpecification;
                    textBox2.Text = _wtxObj.LimitStatusStringComment(/*WTXObj.limitStatus*/);
                    textBox2.TextAlign = HorizontalAlignment.Left;

                }));
            }

            if (_wtxObj.WeightMoving != 0)
            {
                toolStripLabel2.Text = "Moving";
            }
            else  //Not moving
            {
                toolStripLabel2.Text = "";
            }

            if (_wtxObj.WeightType == 0)
            {
                toolStripLabel3.Text = "Gross";
            }
            else
            {
                toolStripLabel3.Text = "Net";
            }

            RenameButtonGrossNet();

        }

        // Button Tare
        private void button2_Click(object sender, EventArgs e)
        {
            if (_wtxObj.getModbusConnection.IsConnected)
            {
                RenameButtonGrossNet();
                _wtxObj.Async_Call(0x1, WriteDataReceived);
            }
            else
            {
                textBox2.Invoke(new Action(() =>
                {
                    textBox2.Text = "No WTX connected!";
                }));
            }

        }

        // Button Zero
        private void button3_Click(object sender, EventArgs e)
        {
            if (_wtxObj.getModbusConnection.IsConnected)
            { 
                _wtxObj.Async_Call(0x40, WriteDataReceived);
            }
            else
            {
                textBox2.Invoke(new Action(() =>
                {
                    textBox2.Text = "No WTX connected!";
                }));
            }
        }       

        // Button Gross/Net
        private void button4_Click(object sender, EventArgs e)
        {
            if (_wtxObj.getModbusConnection.IsConnected)
            {
                _wtxObj.Async_Call(0x2, WriteDataReceived);
                RenameButtonGrossNet();
            }
            else
            {
                textBox2.Invoke(new Action(() =>
                {
                    textBox2.Text = "No WTX connected!";
                }));
            }
        }

        // CallbackMethod executed after write to WTX
        public void WriteDataReceived(IDeviceData deviceValues)
        {
            textBox2.Invoke(new Action(() =>
            {
                textBox2.Text = "write executed";
            }));

            RenameButtonGrossNet();
        }

        // Adapts button Gross/Net text
        private void RenameButtonGrossNet()
        {
            if (_wtxObj.WeightType == 0) //is gross?
            {
                textBox2.Invoke(new Action(() =>
                {
                    button4.Text = "Net";
                }));
            }
            else // is net
            {
                textBox2.Invoke(new Action(() =>
                {
                    button4.Text = "Gross";
                }));

                textBox2.Invoke(new Action(() =>
                {
                    button4.Text = "Gross";
                }));
            }
        }

        //Opens a menu window for calculated calibration
        private void CalculateCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _wtxObj.StopTimer();

            _calcCalObj = new CalcCalibration(_wtxObj, _wtxObj.getModbusConnection.IsConnected);
            DialogResult res = _calcCalObj.ShowDialog();

            _wtxObj.RestartTimer();

        }

        //Opens a menu window for calibration with a calibration weight
        private void CalibrationWithWeightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _wtxObj.StopTimer();

            _weightCalObj = new WeightCalibration(_wtxObj, _wtxObj.getModbusConnection.IsConnected);
            DialogResult res = _weightCalObj.ShowDialog();

            _wtxObj.RestartTimer();
        }


        private void LiveValue_FormClosing(object sender, FormClosingEventArgs e)
        {
            WTXModbusGUIsimple.Properties.Settings.Default.IPAddress = this._ipAddress;
            WTXModbusGUIsimple.Properties.Settings.Default.Save();
        }

        private void LiveValue_Load(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripLabel4_Click(object sender, EventArgs e)
        {

        }
    }
}
