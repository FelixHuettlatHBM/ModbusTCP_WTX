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
using System.Timers;
using System.Windows.Forms;


/*
 * This application example enables communication and a connection to the WTX120 device via 
 * Modbus and Jetbus.
 */
namespace WTXGUIsimple
{
    public partial class Form1 : Form
    {
        private bool isJetbus;
        private bool isModbus;

        private static System.Timers.Timer _aTimer;

        private string _ipAddress;
        private const string DEFAULT_IP_ADDRESS = "192.168.100.88";
        private string _uri;

        private int _timerInterval;

        private static ModbusTcpConnection _modbusObj;
        private static JetBusConnection _sConnection;

        private static WtxModbus _wtxModbusObj;
        private static WtxJet _wtxJetObj;
        

        public Form1()
        {
            InitializeComponent();
        
            pictureBox1.Image = WTXGUIsimple.Properties.Resources.jet_symbol;
            
            isJetbus = true;
            isModbus = false;

            _timerInterval = 200;           

            this.Connect();
           
        }
        

        public Form1(string[] args)
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

                _wtxModbusObj = new WtxModbus(_modbusObj, this._timerInterval);

                _wtxModbusObj.getConnection.NumofPoints = 6;

                _wtxModbusObj.getConnection.Connect();

                if (_wtxModbusObj.isConnected == true)
                {
                    pictureBox1.Image = WTXGUIsimple.Properties.Resources.modbus_symbol;
                    pictureBox2.Image = WTXGUIsimple.Properties.Resources.NE107_DiagnosisActive;
                    _wtxModbusObj.DataUpdateEvent += ValuesOnConsole;
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

                    _sConnection = new JetBusConnection(_uri, "Administrator", "wtx", delegate { return true; });

                    _wtxJetObj = new WtxJet(_sConnection);              
                
                    try
                    {
                        _sConnection.Connect();
                    }
                    catch (Exception exc)
                    {
                        _wtxJetObj.isConnected = false;
                        textBox2.Text = "Connection failed, enter an other IP address please.";
                    }
                
                //pictureBox1.Image = WTXJetGUISimple.Properties.Resources.NE107_DiagnosisActive;  // Check, ob der Verbindungsaufbau erfolgreich war? 

                if (_wtxJetObj.isConnected == true)
                {
                        InitializeTimerJetbus(_timerInterval);
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

        // This method initializes the with the timer interval as a parameter: 
        private void InitializeTimerJetbus(int timerInterval)
        {
            // Create a timer with an interval of 500ms. 
            _aTimer = new System.Timers.Timer(timerInterval);

            // Connect the elapsed event for the timer. 
            _aTimer.Elapsed += JetbusOnTimedEvent;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
        }

        // Event method, which will be triggered after a interval of the timer is elapsed- 
        // After triggering (after 500ms) the register is read. 
        private void JetbusOnTimedEvent(Object source, ElapsedEventArgs e)
        {
            int taraValue = 0;

            try
            {
                taraValue = _wtxJetObj.GrossValue - _wtxJetObj.NetValue;
            }
            catch (Exception exc)
            {
                _aTimer.Stop();
                _aTimer.Enabled = false;
                Console.WriteLine(exc.ToString());
            }

            textBox2.Invoke(new Action(() =>
            {
                try
                {
                    textBox2.Text = "Net:" + _wtxJetObj.NetGrossValueStringComment(_wtxJetObj.NetValue, _wtxJetObj.Decimals) + _wtxJetObj.UnitStringComment(_wtxJetObj.Unit) + Environment.NewLine
                    + "Gross:" + _wtxJetObj.NetGrossValueStringComment(_wtxJetObj.GrossValue, _wtxJetObj.Decimals) + _wtxJetObj.UnitStringComment(_wtxJetObj.Unit) + Environment.NewLine
                    + "Tara:" + _wtxJetObj.NetGrossValueStringComment(taraValue, _wtxJetObj.Decimals) + _wtxJetObj.UnitStringComment(_wtxJetObj.Unit);
                    textBox2.TextAlign = HorizontalAlignment.Right;

                }
                catch (Exception exx)
                {
                    Console.WriteLine(exx.ToString());
                }
                
            }));
        }

        // Method executed after read from WTX by eventbased call from WTX120Modbus, UpdateEvent(..) 
        // Updates displayed values and states
        //public void ReadDataReceived(IDeviceValues deviceValues)
        private void ValuesOnConsole(object sender, DataEvent e)
        {
            int taraValue = 0;

            textBox2.Invoke(new Action(() =>
            {
                textBox2.Text = "Net:" + _wtxModbusObj.NetGrossValueStringComment(_wtxModbusObj.NetValue, _wtxModbusObj.Decimals) + _wtxModbusObj.UnitStringComment() + Environment.NewLine
                + "Gross:" + _wtxModbusObj.NetGrossValueStringComment(_wtxModbusObj.GrossValue, _wtxModbusObj.Decimals) + _wtxModbusObj.UnitStringComment() + Environment.NewLine
                + "Tara:" + _wtxModbusObj.NetGrossValueStringComment(taraValue, _wtxModbusObj.Decimals) + _wtxModbusObj.UnitStringComment();
                textBox2.TextAlign = HorizontalAlignment.Right;
                //pictureBox1.Image = Properties.Resources.NE107_DiagnosisActive;
            }));

        }

        // button click event for switching to gross or net value. 
        private void button4_Click(object sender, EventArgs e)
        {
            if (this.isJetbus == true && this.isModbus == false)
            {
                _wtxJetObj.gross(WriteDataCompleted);
            }

            else
                if(this.isModbus==true && this.isJetbus==false)
                {
                _wtxModbusObj.gross(WriteDataCompleted);
                }
        }

        // button click event for zeroing
        private void button3_Click(object sender, EventArgs e)
        {
            if (this.isJetbus == true && this.isModbus == false)
            {
                _wtxJetObj.zeroing(WriteDataCompleted);
            }

            else
              if (this.isModbus == true && this.isJetbus == false)
              {
                _wtxModbusObj.zeroing(WriteDataCompleted);
              }

        }

        // button click event for taring 
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.isJetbus == true && this.isModbus == false)
            {
                _wtxJetObj.taring(WriteDataCompleted);
            }

            else
              if (this.isModbus == true && this.isJetbus == false)
              {
                _wtxModbusObj.taring(WriteDataCompleted);
              }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox1_Click(this,null);
        }

        // Change from Modbus to Jetbus and from Jetbus to Modbus: 
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (this.isJetbus == true && this.isModbus == false)
            {
                _aTimer.Stop();
                _wtxJetObj.getConnection.Disconnect();
                this.isModbus = true;
                this.isJetbus = false;

                textBox2.Text = "Disconnect, change to Modbus and re-connect...";

                button5.Text = "To Jetbus";

                this.Connect();
            }

            else
             if (this.isModbus == true && this.isJetbus == false)
             {
                _wtxModbusObj.getConnection.Disconnect();
                this.isModbus = false;
                this.isJetbus = true;
           
                textBox2.Text = "Disconnect, change to Jetbus and re-connect...";

                button5.Text = "To Modbus";

                this.Connect();
             }
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

    }
}
