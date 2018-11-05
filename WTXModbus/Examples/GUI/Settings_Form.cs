/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120_Modbus | 01/2018
 * 
 * Author : Felix Huettl 
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

namespace WTXModbusExamples
{
    // This class implements a windows form to change the specific values of the connection, like
    // IP Adress, number of inputs read out in the register and the sending interval, which
    // is the interval of the timer. 

    partial class SettingsForm : Form
    {
        private string _ipAddressBefore;
        private string _ipAddress;

        private int _sendingInterval;
        private ushort _numberInputs;

        private Gui _guiInfo;

        // Constructor of class 'SettingForm': 
        public SettingsForm(string ipAddressParam, int sendingIntervalParam, ushort numberInputsParam, Gui guiObjParam)
        {
            InitializeComponent();

            this._guiInfo = guiObjParam;
           
            this._ipAddressBefore = ipAddressParam;    // IP_address_before is used to change the IP adress. 
            this._ipAddress = ipAddressParam;
            this._sendingInterval = sendingIntervalParam;
            this._numberInputs = numberInputsParam;
                      
            textBox1.Text = this._ipAddress;
            textBox2.Text = this._sendingInterval.ToString();
            textBox3.Text = this._numberInputs.ToString();
            
            label2.Text = "IP address";
            label3.Text = "Timer/Sending interval";
            label4.Text = "Number of inputs";
        }

        public string GetIpAddress
        {
            get
            {
                return this._ipAddress;
            }
        }
        public int GetSendingInterval
        {
            get
            {
                return this._sendingInterval;
            }
        }
        public ushort GetNumberInputs
        {
            get
            {
                return this._numberInputs;
            }
        }

        // This method sets and actualize the attributes of the connection
        // (IP adress, sending/timer interval, number of inputs), if they have changed. 
        private void button2_Click(object sender, EventArgs e)
        {
            this._ipAddress = textBox1.Text;

            this._sendingInterval = Convert.ToInt32(textBox2.Text);

            this._numberInputs = Convert.ToUInt16(textBox3.Text);

            _guiInfo.Setting();

            if (this._ipAddress != this._ipAddressBefore)
            {
                _guiInfo.GetDataviewer.getConnection.Connect();
            }
            
            //GUI_info.timer1_start();
            
            _guiInfo.setTimerInterval(this._sendingInterval);

            //Store IPAddress in Settings .settings
            WTXModbus.Properties.Settings.Default.IPAddress = this._ipAddress;
            WTXModbus.Properties.Settings.Default.Save();

            this.Close();
        }

        private void Settings_Form_Load(object sender, EventArgs e)
        {

        }
    }
}
