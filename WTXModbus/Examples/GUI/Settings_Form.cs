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
        private string IP_address_before;
        private string IP_address;

        private int sending_interval;
        private ushort number_inputs;

        private GUI GUI_info;

        // Constructor of class 'SettingForm': 
        public SettingsForm(string IP_address_param, int sending_interval_param, ushort number_inputs_param, GUI GUI_obj_param)
        {
            InitializeComponent();

            this.GUI_info = GUI_obj_param;
           
            this.IP_address_before = IP_address_param;    // IP_address_before is used to change the IP adress. 
            this.IP_address = IP_address_param;
            this.sending_interval = sending_interval_param;
            this.number_inputs = number_inputs_param;
                      
            textBox1.Text = this.IP_address;
            textBox2.Text = this.sending_interval.ToString();
            textBox3.Text = this.number_inputs.ToString();
            
            label2.Text = "IP address";
            label3.Text = "Timer/Sending interval";
            label4.Text = "Number of inputs";
        }

        public string get_IP_address
        {
            get
            {
                return this.IP_address;
            }
        }
        public int get_sending_interval
        {
            get
            {
                return this.sending_interval;
            }
        }
        public ushort get_number_inputs
        {
            get
            {
                return this.number_inputs;
            }
        }

        // This method sets and actualize the attributes of the connection
        // (IP adress, sending/timer interval, number of inputs), if they have changed. 
        private void button2_Click(object sender, EventArgs e)
        {
            this.IP_address = textBox1.Text;

            this.sending_interval = Convert.ToInt32(textBox2.Text);

            this.number_inputs = Convert.ToUInt16(textBox3.Text);

            GUI_info.setting();

            if (this.IP_address != this.IP_address_before)
            {
                GUI_info.get_dataviewer.getConnection.Connect();
            }
            
            this.Close();

            //GUI_info.timer1_start();

            //Store IPAddress in Settings .settings
            WTXModbus.Properties.Settings.Default.IPAddress = this.IP_address;
            WTXModbus.Properties.Settings.Default.Save();
        }

        private void Settings_Form_Load(object sender, EventArgs e)
        {

        }
    }
}
