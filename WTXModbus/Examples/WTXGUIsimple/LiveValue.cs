using HBM.WT.API;
using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Jet;
using HBM.WT.API.WTX.Modbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WTXModbusGUIsimple;

/*
 * This application example enables communication and a connection to the WTX120 device via 
 * Modbus and Jetbus.
 */
namespace WTXGUIsimple
{
    public partial class LiveValue : Form
    {
        private bool isJetbus;
        private bool isModbus;
        
        private string _ipAddress;
        private const string DEFAULT_IP_ADDRESS = "192.168.100.88";
        private string _uri;

        private int _timerInterval;

        private static ModbusTcpConnection _modbusObj;
        private static JetBusConnection _sConnection;

        private static BaseWtDevice _wtxObj;

        private CalcCalibration _calcCalObj;
        private WeightCalibration _weightCalObj;

        public LiveValue()
        {
            InitializeComponent();
        
            pictureBox1.Image = WTXGUIsimple.Properties.Resources.jet_symbol;
            
            isJetbus = true;
            isModbus = false;

            _timerInterval = 200;           

            this.Connect();
           
        }
        
        public LiveValue(string[] args)
        {
            InitializeComponent();
            
            this.Show();

            textBox2.Text = "Trying to connect...";
            pictureBox2.Image = WTXGUIsimple.Properties.Resources.NE107_OutOfSpecification;

            if (args.Length>0)
            {
                if (args[0] == "modbus" || args[0] == "Modbus")
                {
                    isJetbus = false;
                    isModbus = true;

                    button5.Text = "To Jetbus";
                    pictureBox1.Image = WTXGUIsimple.Properties.Resources.modbus_symbol;

                }
                if (args[0] == "jet" || args[0] == "Jet")
                {
                    isJetbus = true;
                    isModbus = false;

                    button5.Text = "To Modbus";
                    pictureBox1.Image = WTXGUIsimple.Properties.Resources.jet_symbol;
                }
            }

            if (args.Length > 1)
                _ipAddress = args[1];
            else
                _ipAddress = DEFAULT_IP_ADDRESS;

            if (args.Length > 2)
                this._timerInterval = Convert.ToInt32(args[2]);

            else
                this._timerInterval = 200; // Default value for the timer interval. 


            textBox1.Text = _ipAddress;

            this.Connect();
        }

        // This method is called in the constructor of class LiveValue and establishs a connection. 
        private void Connect()
        {

            if (this.isModbus == true && this.isJetbus == false)
            {
                _modbusObj = new ModbusTcpConnection(this._ipAddress);

                _wtxObj = new WtxModbus(_modbusObj, this._timerInterval);

                _wtxObj.getConnection.NumofPoints = 6;

                _wtxObj.getConnection.Connect();

                if (_wtxObj.isConnected == true)
                {
                    pictureBox1.Image = WTXGUIsimple.Properties.Resources.modbus_symbol;
                    pictureBox2.Image = WTXGUIsimple.Properties.Resources.NE107_DiagnosisActive;
                    _wtxObj.DataUpdateEvent += ValuesOnConsole;
                }
                else
                {
                    pictureBox2.Image = WTXGUIsimple.Properties.Resources.NE107_DiagnosisPassive;
                    textBox2.Text = "Connection establishment failed, please retry...";
                }


            }
            else
                if(this.isModbus == false && this.isJetbus == true)
                {
                    _uri = "wss://" + _ipAddress;
                    _uri = _uri + ":443/jet/canopen";     // For : -jet 172.19.103.8:443/jet/canopen ; Initialize Jet-Peer to address
                                                          // Initializing an object of JetBusConnection and WtxJet to establish a connection to the WTX device, to read and write values. 

                    _sConnection = new JetBusConnection(_uri, "Administrator", "wtx");

                    _wtxObj = new WtxJet(_sConnection);              
                
                    try
                    {
                        _sConnection.Connect();
                    }
                    catch (Exception exc)
                    {
                        textBox2.Text = "Connection failed, enter an other IP address please.";
                    }

                //pictureBox1.Image = WTXJetGUISimple.Properties.Resources.NE107_DiagnosisActive;  // Check, ob der Verbindungsaufbau erfolgreich war? 
                
                if (_wtxObj.isConnected == true)
                {
                    _wtxObj.DataUpdateEvent += ValuesOnConsole;
                    pictureBox1.Image = WTXGUIsimple.Properties.Resources.jet_symbol;
                    pictureBox2.Image = WTXGUIsimple.Properties.Resources.NE107_DiagnosisActive;

                }
                else
                {
                    pictureBox2.Image = WTXGUIsimple.Properties.Resources.NE107_DiagnosisPassive;
                    textBox2.Text = "Connection establishment failed, please retry...";
                }
            }
        }
      
        // Method executed after read from WTX by eventbased call from WTXModbus, UpdateEvent(..) 
        // Updates displayed values and states
        private void ValuesOnConsole(object sender, DataEvent e)
        {
            int taraValue = 0;

            textBox2.Invoke(new Action(() =>
            {
                textBox2.Text = "Net:" + _wtxObj.NetGrossValueStringComment(_wtxObj.NetValue, _wtxObj.Decimals) + _wtxObj.UnitStringComment() + Environment.NewLine
                + "Gross:" + _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals) + _wtxObj.UnitStringComment() + Environment.NewLine
                + "Tara:" + _wtxObj.NetGrossValueStringComment(taraValue, _wtxObj.Decimals) + _wtxObj.UnitStringComment();
                textBox2.TextAlign = HorizontalAlignment.Right;
            }));

        }

        // button click event for switching to gross or net value. 
        private void button4_Click(object sender, EventArgs e)
        {
                _wtxObj.gross(WriteDataCompleted);
        }

        // button click event for zeroing
        private void button3_Click(object sender, EventArgs e)
        {
                _wtxObj.zeroing(WriteDataCompleted);
        }

        // button click event for taring 
        private void button2_Click(object sender, EventArgs e)
        {

                _wtxObj.taring(WriteDataCompleted);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox1_Click(this,null);
        }

        // Change from Modbus to Jetbus and from Jetbus to Modbus: 
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            _wtxObj.DataUpdateEvent -= ValuesOnConsole;
            _wtxObj.getConnection.Disconnect();

            if (this.isJetbus == true && this.isModbus == false)
            {
                this.isModbus = true;
                this.isJetbus = false;

                textBox2.Text = "Disconnect, change to Modbus and re-connect...";

                button5.Text = "To Jetbus";
            }

            else
             if (this.isModbus == true && this.isJetbus == false)
             {
                this.isModbus = false;
                this.isJetbus = true;
           
                textBox2.Text = "Disconnect, change to Jetbus and re-connect...";

                button5.Text = "To Modbus";
             }

            this.Connect();
        }

        //Method for calculate calibration with dead load and span: 
        private void calibrationWithWeightToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (this.isJetbus == true && this.isModbus == false)
                _calcCalObj = new CalcCalibration(_wtxObj, _sConnection.IsConnected);

            if (this.isJetbus == false && this.isModbus == true)
                _calcCalObj = new CalcCalibration(_wtxObj, _modbusObj.IsConnected);

            DialogResult res = _calcCalObj.ShowDialog();
        }
        //Method for calibrating with weight: 
        private void calibrationToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (this.isJetbus == true && this.isModbus == false)
                _weightCalObj = new WeightCalibration(_wtxObj, _sConnection.IsConnected);

            if (this.isJetbus == false && this.isModbus == true)
                _weightCalObj = new WeightCalibration(_wtxObj, _modbusObj.IsConnected);

            DialogResult res = _weightCalObj.ShowDialog();
        }
        private void WriteDataCompleted(IDeviceData obj)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
        }

        private void calculateCalibrationToolStripMenuItem_Click_2(object sender, EventArgs e)
        {

        }

        private void calibrationWithWeightToolStripMenuItem_Click_2(object sender, EventArgs e)
        {

        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

    }
}
