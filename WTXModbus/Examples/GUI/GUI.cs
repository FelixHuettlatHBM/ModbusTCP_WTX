/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120_Modbus | 01/2018
 * 
 * Author : Felix Huettl 
 * 
 *  */
using System;
using System.Windows.Forms;
using System.IO;
using WTXModbusGUIsimple;
using WTXModbus;
using System.Threading;
using System.ComponentModel;

using HBM.WT.API.WTX;
using HBM.WT.API;
using HBM.WT.API.WTX.Modbus;

namespace WTXModbusExamples
{
    /// <summary>
    /// First, objects of class 'ModbusTCPConnection' and 'WTX120_Modbus' are created to establish a connection and data transfer to the device (WTX120_Modbus). 
    /// Class 'ModbusTCPConnection' has the purpose to establish a connection, to read from the device (its register)
    /// and to write to the device (its register). Class 'WTX120_Modbus' creates timer to read and update periodically the values of the WTX in a certain timer
    /// interval given in the constructor of class 'WTX120_Modbus' while generating an object of it. Class 'WTX120_Modbus' has all the values, 
    /// which will be interpreted from the read bytes and class 'WTX120_Modbus' manages the asynchronous data transfer to GUI and the eventbased data transfer #
    /// to class ModbusTCPConnection. 
    ///  
    /// This class 'GUI' represents a window or a dialog box that makes up the application's user interface for the values and their description of the device.
    /// It uses a datagrid to put the description and the periodically updated values together. The description shown in the form and initialized in
    /// the datagrid is based on the manual (see page manual PCLC link on page 154-161). 
    /// Futhermore the data is only displayed, if the values have changed to save reconstruction time on the GUI Form. 
    /// 
    /// Beside a form the GUI could also be a console application by applying that in program.cs instead of a windows form (see on Git).
    /// Therefore the design of the classes and its relations are seperated in 
    /// connection specific classes and interfaces (class ModbusTCPConnection, interface "IModbusConnection")
    /// and in a device specific class and in a device specific interface (class "WTX120_Modbus", interface "IDevice_Values").
    ///  
    /// In the Windows form, there are several buttons to activate events for the output words, a menu bar on the top to start/stop the application, to save the values,
    /// to show help (like the manual) and to change the settings.
    /// The latter is implemented by an additional form and changes the IP address, number of inputs read out by the register, sending/timer interval. 
    /// A status bar on the bottom shows the actually updated status of the connection ("Connected", "IP address", "Mode", "TCP/Modbus", "NumInputs").
    /// </summary>
    partial class Gui : Form
    {
        private static ModbusTcpConnection _modbusObj;

        private static HBM.WT.API.WTX.WtxModbus _wtxObj;

        //private static WTXModbus WTXObj;

        private SettingsForm _setObj;

        private CalcCalibration _calcCalObj;
        private WeightCalibration _weightCalObj;

        private string[] _dataStr;

        private bool _isStandard;

        private string _ipAddress;
        private int _timerInterval;

        private int _startIndex ;   // Default setting for standard mode. 
        private int _i;
        private int _arrayLength;

        string[] _args;

        // Constructor of class GUI for the initialisation: 
        public Gui(string[] argsParam)
        {
            // Initialize the GUI Form: 
            InitializeComponent();           // Call of this method to initialize the form.

            this._args = argsParam;

            if (_args.Length > 0)
            {
                if (_args[0] == "modbus" || _args[0] == "Modbus")
                    toolStripStatusLabel6.Text = "Modbus";

                if (_args[0] == "jet" || _args[0] == "Jet")
                    toolStripStatusLabel6.Text = "Jetbus";
            }

            //Get IPAddress and the timer interval from the command line of the VS project arguments (at "Debug").
            if (this._args.Length > 1)
            {
                this._ipAddress = this._args[1];
            }
            else
            {
                MessageBox.Show("Bitte geben sie unter 'Edit->Settings' die IP-Adresse ein und auf 'File->Start' für einen Verbindungsaufbau. Es wird mit der 'Default IP-Adresse fortgefahren.");

                WTXModbus.Properties.Settings.Default.Reload();
                _ipAddress = WTXModbus.Properties.Settings.Default.IPAddress;
            }
            if (this._args.Length > 2)
            {
                this._timerInterval = Convert.ToInt32(_args[2]);
            }
            else
            {
                MessageBox.Show("Kein Timer-Intervall in der Kommandozeile gegeben, bitte unter Edit->Settings einfügen. Es wird einem Timer-Intervall von 100 msec. fortgefahren.");
                this._timerInterval = 100; // Default value for the timer interval.
            }
            _modbusObj = new ModbusTcpConnection(_ipAddress);
            _wtxObj = new HBM.WT.API.WTX.WtxModbus(_modbusObj, this._timerInterval);

            _isStandard = true;      // change between standard and application mode in the GUI. 
            _modbusObj.IpAddress = _ipAddress;

            this._dataStr = new string[59];

            for (int i = 0; i < 59; i++)
            {
                this._dataStr[i] = "0";
            }

            startToolStripMenuItem_Click(this, new EventArgs());

            _startIndex = 8;   // Default setting for standard mode. 
            _i = 0;
            _arrayLength = 0;
        }

        public void setTimerInterval(int timerIntervalParam)
        {
            _wtxObj.ResetTimer(timerIntervalParam);
        }

        // This method could also load the datagrid at the beginning of the application: For printing the datagrid on the beginning.
        private void GUI_Load_1(object sender, EventArgs e)
        {

        }
        
        // This method is called from the constructor and sets the columns and the rows of the data grid.
        // There are 2 cases:
        // 1) Standard application : Input words "0+2" till "14". Output words "0" till "50". 
        // 2) Filler   application : Input words "0+2" till "37". Output words "0" till "50". 
        public void set_GUI_rows()
        {
            dataGridView1.Columns.Add("Input:Word_header", "Input:Word");                         // column 1
            dataGridView1.Columns.Add("Input:Name_header", "Input:Name");                         // column 2
            dataGridView1.Columns.Add("Input:Type_header", "Input:Type");                         // column 3
            dataGridView1.Columns.Add("Input:Bit_header" , "Input:Bit");                           // column 4
            dataGridView1.Columns.Add("Input:Interface call routine_header", "Input:Interface call routine"); // column 5
            dataGridView1.Columns.Add("Input:List call routine_header", "Input:List call routine");           // column 6
            dataGridView1.Columns.Add("Input:Description_header", "Input:Description");           // column 7
            dataGridView1.Columns.Add("Input:Value_header", "Input:Value");                       // column 8

            dataGridView1.Columns.Add("Output:Word_header", "Output:Word");                       // column 9
            dataGridView1.Columns.Add("Output:Name_header", "Output:Name");                       // column 10
            dataGridView1.Columns.Add("Output:Type_header", "Output:Type");                       // column 11
            dataGridView1.Columns.Add("Output:Bit_header" , "Output:Bit");                         // column 12
            dataGridView1.Columns.Add("Input:Content Content_header", "Input:Content Content");   // column 13
            dataGridView1.Columns.Add("Output:Value_header", "Output:Value");                     // column 16

            if (this._isStandard == true) // case 1) Standard application. Initializing the description and a placeholder for the values in the data grid.
            {
                dataGridView1.Rows.Add("0", "Measured Value", "Int32", "32Bit", "IDevice_Values.NetValue", "dataStr[1]", "Net measured", "0", "0", "Control word", "Bit", ".0", "Taring", "Button taring");                                           // row 1 ; dataStr[1]      
                dataGridView1.Rows.Add("2", "Measured Value", "Int32", "32Bit", "IDevice_Values.GrossValue", "dataStr[2]", "Gross measured", "0", "0", "Control word", "Bit", ".1", "Gross/Net", "Button Gross/Net");                               // row 2 ; dataStr[2]      
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".0", "IDevice_Values.general_weight_error", "dataStr[3]", "General weight error", "0", "0", "Control word", "Bit", ".6", "Zeroing", "Button Zeroing");                    // row 3 ; dataStr[3]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".1", "IDevice_Values.scale_alarm_triggered", "dataStr[4]", "Scale alarm(s) triggered", "0", "0", "Control word", "Bit", ".7", "Adjust zero", "Button Adjust zero");         // row 4 ; dataStr[4]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".2-3", "IDevice_Values.limit_status", "dataStr[5]", "Limit status", "0", "0", "Control word", "Bit", ".8", "Adjust nominal", "Button Adjust nominal");                    // row 5 ; dataStr[5]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".4", "IDevice_Values.weight_moving", "dataStr[6]", "Weight moving", "0", "0", "Control word", "Bit", ".11", "Activate data", "Button Activate data");                     // row 6 ; dataStr[6]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".5", "IDevice_Values.scale_seal_is_open", "dataStr[7]", "Scale seal is open", "0", "0", "Control word", "Bit", ".12", "Manual taring", "Button Manual taring");            // row 7 ; dataStr[7]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".6", "IDevice_Values.manual_tare", "dataStr[8]", "Manual tare", "0", "0", "Control word", "Bit", ".14", "Record weight", "Button Record weight");                          // row 8 ; dataStr[8]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".7", "IDevice_Values.weight_type", "dataStr[9]", "Weight type", "0", "2", "Manual tare value", "S32", ".0-31", "Manual tare value", "0");                               // row 9  ; dataStr[9]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".8-9", "IDevice_Values.scale_range", "dataStr[10]", "Scale range", "0", "4", "Limit value 1", "U08", ".0-7", "Source 1", "0");                                         // row 10 ; dataStr[10]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".10", "IDevice_Values.zero_required", "dataStr[11]", "Zero required", "0", "5", "Limit value 1", "U08", ".0-7", "Mode 1", "0");                                         // row 11 ; dataStr[11]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".11", "IDevice_Values.weight_within_the_center_of_zero", "dataStr[12]", "Weight within the center of zero", "0", "6", "Limit value 1", "S32", ".0-31", "Activation level/Lower band limit 1", "0");   // row 12 ; dataStr[12]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".12", "IDevice_Values.weight_in_zero_range", "dataStr[13]", "Weight in zero range", "0", "8", "Limit value 1", "S32", ".0-31", "Hysteresis/Band height 1", "0");        // row 13 ; dataStr[13]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".0-1", "IDevice_Values.application_mode", "dataStr[14]", "Application mode", "0", "10", "Limit value 2", "U08", ".0-7", "Source 2", "0");                            // row 14 ; dataStr[14]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".4-6", "IDevice_Values.decimals", "dataStr[15]", "Decimals", "0", "11", "Limit value 2", "U08", ".0-7", "Mode 2", "0");                                              // row 15 ; dataStr[15]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".7-8", "IDevice_Values.unit", "dataStr[16]", "Unit", "0", "12", "Limit value 2", "S32", ".0-31", "Activation level/Lower band limit 2", "0");                        // row 16 ; dataStr[16]

                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".14", "IDevice_Values.handshake", "dataStr[17]", "Handshake", "0", "14", "Limit value 2", "S32", ".0-31", "Hysteresis/Band height 2", "0");            // row 17 ; dataStr[17] 
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".15", "IDevice_Values.status", "dataStr[18]", "Status", "0", "16", "Limit value 3", "U08", ".0-7", "Source 3", "0");                                   // row 18 ; dataStr[18]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".0", "IDevice_Values.input1", "dataStr[19]", "Input 1", "0", "17", "Limit value 3", "U08", ".0-7", "Mode 3", "0");                                            // row 19 ; dataStr[19]

                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".1", "IDevice_Values.input2", "dataStr[20]", "Input 2", "0", "19", "Limit value 3", "S32", ".0-31", "Activation level/Lower band limit 3", "0");      // row 20 ; dataStr[20]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".2", "IDevice_Values.input3", "dataStr[21]", "Input 3", "0", "20", "Limit value 3", "S32", ".0-31", "Hysteresis/Band height 3", "0");                 // row 21 ; dataStr[21]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".3", "IDevice_Values.input4", "dataStr[22]", "Input 4", "0", "22", "Limit value 4", "U08", ".0-7", "Source 4", "0");                                  // row 22 ; dataStr[22]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".0", "IDevice_Values.output1", "dataStr[23]", "Output 1", "0", "23", "Limit value 4", "U08", ".0-7", "Mode 4", "0");                                 // row 23 ; dataStr[23]

                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".1", "IDevice_Values.output2", "dataStr[24]", "Output 2", "0", "24", "Limit value 4", "S32", ".0-31", "Activation level/Lower band limit 4", "0");      // row 24 ; dataStr[24]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".2", "IDevice_Values.output3", "dataStr[25]", "Output 3", "0", "26", "Limit value 4", "S32", ".0-31", "Hysteresis/Band height 4", "0");                 // row 25 ; dataStr[25]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".3", "IDevice_Values.output4", "dataStr[26]", "Output 4", "0", "46", "Calibration weight", "S32", ".0-31", "Calibration weight", "Tools Calibration");  // row 26 ; dataStr[26]
                dataGridView1.Rows.Add("8", "Limit value 1", "Bit", ".0", "IDevice_Values.limitValue1", "dataStr[27]", "Limit value 1", "0", "48", "Zero load", "S32", ".0-31", "Zero load", "Tools Calibration");           // row 27 ; dataStr[27]

                dataGridView1.Rows.Add("8", "Limit value 2", "Bit", ".1", "IDevice_Values.limitValue2", "dataStr[28]", "Limit value 2", "0", "50", "Nominal load", "S32", ".0-31", "Nominal load", "Tools Calibration");      // row 28 ; dataStr[28]              
                dataGridView1.Rows.Add("8", "Limit value 3", "Bit", ".2", "IDevice_Values.limitValue3", "dataStr[29]", "Limit value 3", "0", "-", "-", "-", "-", "-", "-", "-", "-", "-");                                     // row 30 ; dataStr[29]
                dataGridView1.Rows.Add("8", "Limit value 4", "Bit", ".3", "IDevice_Values.limitValue4", "dataStr[30]", "Limit value 4", "0", "-", "-", "-", "-", "-", "-", "-", "-");                                          // row 31 ; dataStr[30]
                dataGridView1.Rows.Add("9", "Weight memory, Day", "Int16", ".0-15", "IDevice_Values.weightMemDay", "dataStr[31]", "Stored value for day", "0", "-", "-", "-", "-", "-", "-", "-", "-");                        // row 32 ; dataStr[31]

                dataGridView1.Rows.Add("10", "Weight memory, Month", "Int16", ".0-15", "IDevice_Values.weightMemMonth", "dataStr[32]", "Stored value for month", "0", "-", "-", "-", "-", "-", "-", "-", "-");   // row 33 ; dataStr[32]
                dataGridView1.Rows.Add("11", "Weight memory, Year", "Int16", ".0-15", "IDevice_Values.weightMemYear", "dataStr[33]", "Stored value for year", "0", "-", "-", "-", "-", "-", "-", "-", "-");      // row 34 ; dataStr[33]

                dataGridView1.Rows.Add("12", "Weight memory, Seq...number", "Int16", ".0-15", "IDevice_Values.weightMemSeqNumber", "dataStr[34]", "Stored value for seq.number", "0", "-", "-", "-", "-", "-", "-", "-", "-");       // row 35 ; dataStr[34]
                dataGridView1.Rows.Add("13", "Weight memory, gross", "Int16", ".0-15", "IDevice_Values.weightMemGross", "dataStr[35]", "Stored gross value", "0", "-", "-", "-", "-", "-", "-", "-", "-");                           // row 36 ; dataStr[35]
                dataGridView1.Rows.Add("14", "Weight memory, net", "Int16", ".0-15", "IDevice_Values.weightMemNet", "dataStr[36]", "Stored net value", "0", "-", "-", "-", "-", "-", "-", "-", "-");                                 // row 37 ; dataStr[36]             
            }
            if (this._isStandard==false) // case 2) Filler application. Initializing the description and a placeholder for the values in the data grid.
            {
                dataGridView1.Rows.Add("0", "Measured Value", "Int32", "32Bit", "IDevice_Values.NetValue", "dataStr[1]", "Net measured", "0", "0", "Control word", "Bit", ".0", "Taring",  "Button Taring");                               // row 1 ; dataStr[1]      
                dataGridView1.Rows.Add("2", "Measured Value", "Int32", "32Bit", "IDevice_Values.GrossValue", "dataStr[2]", "Gross measured", "0", "0", "Control word", "Bit", ".1", "Gross/Net", "Button Gross/Net");                      // row 2 ; dataStr[2]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".0", "IDevice_Values.general_weight_error", "dataStr[3]", "General weight error", "0", "0", "Control word", "Bit", ".2", "Clear dosing results", "Button Clear dosing results");         // row 3 ; dataStr[3]        
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".1", "IDevice_Values.scale_alarm_triggered", "dataStr[4]", "Scale alarm(s) triggered", "0", "0", "Control word", "Bit", ".3", "Abort dosing", "Button Abort dosing");                    // row 4 ; dataStr[4]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".2-3", "IDevice_Values.limit_status", "dataStr[5]", "Limit status", "0", "0", "Control word", "Bit", ".4", "Start dosing", "Button Start dosing");              // row 5 ; dataStr[5]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".4"   , "IDevice_Values.weight_moving", "dataStr[6]", "Weight moving", "0", "0", "Control word", "Bit", ".6", "Zeroing", "Button Zeroing");                         // row 6 ; dataStr[6]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".5"   , "IDevice_Values.scale_seal_is_open", "dataStr[7]", "Scale seal is open", "0", "0", "Control word", "Bit", ".7", "Adjust zero", "Button Adjust zero");       // row 7 ; dataStr[7]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".6"   , "IDevice_Values.manual_tare", "dataStr[8]", "Manual tare", "0", "0", "Control word", "Bit", ".8", "Adjust nominal", "Button Adjust nominal");               // row 8 ; dataStr[8]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".7",  "IDevice_Values.weight_type", "dataStr[9]", "Weight type", "0", "0", "Control word", "Bit", ".11", "Activate data", "Button Activate data");                         // row 9 ;  dataStr[9]
                dataGridView1.Rows.Add("4", "DS461-Weight status","Bits", ".8-9","IDevice_Values.scale_range", "dataStr[10]", "Scale range", "0", "0", "Control word", "Bit", ".14", "Record weight", "Button Record weight");                     // row 10 ; dataStr[10]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".10", "IDevice_Values.zero_required", "dataStr[11]", "Zero required", "0", "1", "Control word", "Bit", ".15", "Manual re-dosing", "Button Manual re-dosing");             // row 11 ; dataStr[11]           
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".11", "IDevice_Values.weight_within_the_center_of_zero", "dataStr[12]", "Weight within the center of zero", "0", "9", "Residual flow time", "U16", ".0-15",  "", "0");    // row 12 ; dataStr[12]
                
                dataGridView1.Rows.Add("4", "DS461-Weight status",  "Bit", ".12", "IDevice_Values.weight_in_zero_range", "dataStr[13]", "Weight in zero range", "0", "10", "Filling weight", "S32", ".0-31",  "", "0");           // row 13 ; dataStr[13]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".0-1", "IDevice_Values.application_mode", "dataStr[14]", "Application mode", "0", "12", "Coarse flow cut-off point", "S32", ".0-31",  "", "0");    // row 14 ; dataStr[14]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".4-6", "IDevice_Values.decimals", "dataStr[15]", "Decimals", "0", "14", "Fine flow cut-off point", "S32", ".0-31",  "", "0");                      // row 15 ; dataStr[15]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".7-8", "IDevice_Values.unit", "dataStr[16]", "Unit", "0", "16", "Minimum fine flow", "S32", ".0-31",  "", "0");                                   // row 16 ; dataStr[16]
                
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".14", "IDevice_Values.handshake", "dataStr[17]", "Handshake", "0", "18", "Optimization of cut-off points", "U08", ".0-7",  "", "0");  // row 17 ; dataStr[17]
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".15", "IDevice_Values.status"   , "dataStr[18]", "Status"   , "0", "19", "Maximum dosing time" , "U16" , ".0-15",  "" , "0");                  // row 18 ; dataStr[18]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit",  ".0", "IDevice_Values.input1"          , "dataStr[19]", "Input 1"  , "0", "20", "Start with fine flow", "U16" , ".0-15",  "" , "0");                        // row 19 ; dataStr[19]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit",  ".1", "IDevice_Values.input2"          , "dataStr[20]", "Input 2"  , "0", "21", "Coarse lockout time" , "U16" , ".0-15",  "" , "0");                         // row 20 ; dataStr[20] 
                
                dataGridView1.Rows.Add("6", "Digital inputs" , "Bit", ".2", "IDevice_Values.input3", "dataStr[21]", "Input 3", "0", "22", "Fine lockout time", "U16", ".0-35",  "", "0");                           // row 21 ; dataStr[21]
                dataGridView1.Rows.Add("6", "Digital inputs" , "Bit", ".3", "IDevice_Values.input4", "dataStr[22]", "Input 4", "0", "23", "Tare mode", "U08", ".0-7",  "", "0");                                    // row 22 ; dataStr[22]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".0", "IDevice_Values.output1", "dataStr[23]", "Output 1", "0", "24", "Tolerance limit +", "S32", ".0-31",  "", "0");                        // row 23 ; dataStr[23]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".1", "IDevice_Values.output2", "dataStr[24]", "Output 2", "0", "26", "Tolerance limit -", "S32", ".0-31",  "", "0");                        // row 24 ; dataStr[24]
                
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".2", "IDevice_Values.output3", "dataStr[25]", "Output 3", "0", "28", "Minimum start weight", "S32", ".0-31",  "", "0");                   // row 25 ; dataStr[25]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".3", "IDevice_Values.output4", "dataStr[26]", "Output 4", "0", "30", "Empty weight", "S32", ".0-31",  "", "0");                           // row 26 ; dataStr[26] 
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".0", "IDevice_Values.coarseFlow", "dataStr[27]", "Coarse flow", "0", "32", "Tare", "U16", ".0-35",  "", "0");                               // row 27 ; dataStr[27]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".1", "IDevice_Values.fineFlow", "dataStr[28]", "Fine flow", "0", "33", "Coarse flow monitoring time", "U16", ".0-15",  "", "0");            // row 28 ; dataStr[28]

                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".2", "IDevice_Values.ready", "dataStr[29]", "Ready", "0", "34", "Coarse flow monitoring", "U32", ".0-31",  "", "0");                            // row 29 ; dataStr[29]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".3", "IDevice_Values.reDosing", "dataStr[30]", "Re-dosing", "0", "36", "Fine flow monitoring", "U32", ".0-31",  "", "0");                       // row 30 ; dataStr[30]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".4", "IDevice_Values.emptying", "dataStr[31]", "Emptying", "0", "38", "Fine flow monitoring time", "U16", ".0-15",  "", "0");                   // row 31 ; dataStr[31]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".5", "IDevice_Values.flowError", "dataStr[32]", "Flow error", "0", "39", "Delay time after fine flow", "U08", ".0-7",  "", "0");                // row 32 ; dataStr[32]
               
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".6", "IDevice_Values.alarm", "dataStr[33]", "Alarm", "0", "40", "Activation time after fine flow", "U08", ".0-7",  "", "0");                                     // row 33 ; dataStr[33]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".7", "IDevice_Values.ADC_OverUnderload", "dataStr[34]", "ADC-Overload/Underload", "0", "41", "Systematic difference", "U32", ".0-31",  "", "0");                 // row 34 ; dataStr[34]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".8", "IDevice_Values.maxDosingTime", "dataStr[35]", "Max. Dosing time", "0", "42", "Downwards dosing", "U08", ".0-7",  "", "0");                                 // row 35 ; dataStr[35]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".9", "IDevice_Values.legalTradeOP", "dataStr[36]", "Legal-for-trade operation", "0", "43", "Valve control", "U08", ".0-7",  "", "0");                            // row 36 ; dataStr[36]
                
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".10", "IDevice_Values.toleranceErrorPlus", "dataStr[37]", "Tolerance error +", "0",  "44", "Emptying mode","U08", ".0-7" ,     "", "0");                             // row 37 ; dataStr[37]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".11", "IDevice_Values.toleranceErrorMinus", "dataStr[38]", "Tolerance error -", "0", "46", "Calibration weight" , "S32"  ,".0-31",  "", "Tools calibration");      // row 38 ; dataStr[38]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".14", "IDevice_Values.statusInput1", "dataStr[39]", "Status digital input 1"  , "0", "48", "Zero load"   , "S32", ".0-31", ""    , "Tools calibration");                    // row 39 ; dataStr[39]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".15", "IDevice_Values.generalScaleError", "dataStr[40]", "General scale error", "0", "50", "Nominal load", "S32", ".0-31", ""    , "Tools calibration");               // row 40 ; dataStr[40]
               
                dataGridView1.Rows.Add("9", "Dosing process status", "U16", ".0-15", "IDevice_Values.fillingProcessStatus", "dataStr[41]", "Initializing,Pre-dosing to Analysis", "0", "-", "-", "-", "-", "-", "-", "-", "-");     // row 41 ; dataStr[41]
                dataGridView1.Rows.Add("11", "Dosing count", "U16", ".0-15", "IDevice_Values.numberDosingResults", "dataStr[42]", " ", "0", "-");                                                                    // row 43 ; dataStr[42]
                dataGridView1.Rows.Add("12", "Dosing result", "S32", ".0-31", "IDevice_Values.dosingResult", "dataStr[43]", " ", "0", "-", "-", "-", "-", "-", "-", "-", "-");                                       // row 44 ; dataStr[43]

                dataGridView1.Rows.Add("14", "Mean value of dosing results", "S32"    , ".0-31", "IDevice_Values.meanValueDosingResults", "dataStr[44]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                   // row 45 ; dataStr[44]
                dataGridView1.Rows.Add("16", "Standard deviation" , "S32"  , ".0-31"  , "IDevice_Values.standardDeviation", "dataStr[45]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                                  // row 46 ; dataStr[45]
                dataGridView1.Rows.Add("18", "Total weight", "S32", ".0-31", "IDevice_Values.totalWeight", "dataStr[46]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                                              // row 47 ; dataStr[46]
                dataGridView1.Rows.Add("20", "Fine flow cut-off point", "S32", ".0-31", "IDevice_Values.fineFlowCutOffPoint", "dataStr[47]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                           // row 48 ; dataStr[47]
               
                dataGridView1.Rows.Add("22", "Coarse flow cut-off point", "S32", ".0-31", "IDevice_Values.coarseFlowCutOffPoint", "dataStr[48]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                       // row 49 ; dataStr[48]
                dataGridView1.Rows.Add("24", "Actual dosing time"     , "U16"  , ".0-15", "IDevice_Values.actualDosingTime", "dataStr[49]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                                   // row 50 ; dataStr[49]
                dataGridView1.Rows.Add("25", "Actual coarse flow time", "U16"  , ".0-15", "IDevice_Values.actualCoarseFlowTime", "dataStr[50]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                          // row 51 ; dataStr[50]
                dataGridView1.Rows.Add("26", "Actual fine flow time"  , "U16"  , ".0-15", "IDevice_Values.actualFineFlowTime", "dataStr[51]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                              // row 52 ; dataStr[51]
                
                dataGridView1.Rows.Add("27", "Parameter set (product)", "U08"  , ".0-7" , "IDevice_Values.parameterSetProduct", "dataStr[52]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                            // row 53 ; dataStr[52]
                dataGridView1.Rows.Add("32", "Weight memory, Day"     , "Int16", ".0-15", "IDevice_Values.weightMemDay", "dataStr[53]", "Stored value for day", "0", "-", "-", "-", "-", "-", "-", "-");                  // row 54 ; dataStr[53]
                dataGridView1.Rows.Add("33", "Weight memory, Month"   , "Int16", ".0-15", "IDevice_Values.weightMemMonth", "dataStr[54]", "Stored value for month", "0", "-", "-", "-", "-", "-", "-", "-");            // row 55 ; dataStr[54]
                dataGridView1.Rows.Add("34", "Weight memory, Year"    , "Int16", ".0-15", "IDevice_Values.weightMemYear", "dataStr[55]", "Stored value for year", "0", "-", "-", "-", "-", "-", "-", "-");               // row 56 ; dataStr[55]
               
                dataGridView1.Rows.Add("35", "Weight memory, Seq number", "Int16", ".0-15", "IDevice_Values.weightSeqNumber", "dataStr[56]", "Stored value for seq.number", "0", "-", "-", "-", "-", "-", "-", "-"); // row 57 ; dataStr[56]
                dataGridView1.Rows.Add("36", "Weight memory, gross"     , "Int16", ".0-15", "IDevice_Values.weightMemGross" , "dataStr[57]", "Stored gross value", "0", "-", "-", "-", "-", "-", "-", "-");                // row 58 ; dataStr[57]
                dataGridView1.Rows.Add("37", "Weight memory, net"       , "Int16", ".0-15", "IDevice_Values.weightMemDayNet", "dataStr[58]", "Stored net value"  , "0", "-", "-", "-", "-", "-", "-", "-");                   // row 59 ; dataStr[58]

            }

            label1.Text = "Only for Standard application:";       // label for information : Only output words for standard application
            label2.Text = "Only for Filler application:";         // label for information : Only output words for filler application 
            toolStripStatusLabel5.Text = "38";

            if (_wtxObj.getConnection.IsConnected == true)
                toolStripStatusLabel1.Text = "Connected";
            else
                toolStripStatusLabel1.Text = "Disconnected";

            dataGridView1.Columns[4].Width = 250;                 // Width of the fourth column containing the periodically updated values.           
        }

        // This automatic property returns an instance of this class. It has usage in the class "Settings_Form".
        public HBM.WT.API.WTX.WtxModbus GetDataviewer
        {
            get
            {
                return _wtxObj;
            }
        }

        // This private method is called for initializing basic information for the tool menu bar on the bottom of the windows form: 
        // For the connection status, IP address, application mode and number of inputs. 
        private void GUI_Load(object sender, EventArgs e)
        {
            if (_wtxObj.getConnection.IsConnected == true)
                toolStripStatusLabel1.Text = "Connected";
            else
                toolStripStatusLabel1.Text = "Disconnected";

            toolStripStatusLabel2.Text = "IP address: " + _wtxObj.getConnection.IpAddress;
            toolStripStatusLabel3.Text = "Mode : " + this._dataStr[14]; // index 14 refers to application mode of the Device
            toolStripStatusLabel5.Text = "Number of Inputs : " + _wtxObj.getConnection.NumofPoints; 
        }

        // This method actualizes and resets the data grid with newly calculated values of the previous iteration. 
        // First it actualizes the tool bar menu regarding the status of the connection, afterwards it iterates the 
        // "dataStr" array to actualize every element of the data grid in the standard or filler application. 
        public void refresh_values()
        {     
            if (_wtxObj.getConnection.IsConnected == true)
                toolStripStatusLabel1.Text = "Connected";
            if (_wtxObj.getConnection.IsConnected == false)
                toolStripStatusLabel1.Text = "Disconnected";
            
            toolStripStatusLabel2.Text = "IP address: " + _wtxObj.getConnection.IpAddress;
            toolStripStatusLabel3.Text = "Mode : " + this._dataStr[14];                 // index 14 refers to application mode of the Device
            toolStripStatusLabel2.Text = "IP address: " + _wtxObj.getConnection.IpAddress;

            //Changing the width of a column:
            /*foreach (DataGridViewTextBoxColumn c in dataGridView1.Columns)
                c.Width = 120;*/
            try
            {
                for (int index = 0; index <= 26; index++) // Up to index 26, the input words are equal in standard and filler application.                           
                    dataGridView1.Rows[index].Cells[7].Value = _dataStr[index];
            }catch(Exception){ }

            if (_wtxObj.ApplicationMode == 0)             // In the standard application: 
            {
                try
                {
                    for (int index = 27; index <= 35; index++)
                    dataGridView1.Rows[(index+1)].Cells[7].Value = _dataStr[index];  //ddd: Achtung Index auf die Schnelle hier verschben, es gibt kein Net and gross measured!?
                }
                catch (Exception) { }
            }
            else
            if (_wtxObj.ApplicationMode == 1 || _wtxObj.ApplicationMode == 2)   // In the filler application: 
                {
                    try
                    {
                        for (int index = 27; index <55; index++)
                        {
                             dataGridView1.Rows[(index + 1)].Cells[7].Value = _dataStr[index];
                        }
                    }   
                    catch (Exception) { }
            }
        }

        // Button-Click event to close the application: 
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // This is the callback method for writing. First the values in "dataStr" are updated and 
        // the GUI is actualized.
        // A asynchronous call is used in the following button_Click methods. 
        // The callback method is Write_DataReceived, which is called once the command is written into the register of the device.      
        public void Write_DataReceived(IDeviceData deviceValues)
        {
            //this.dataStr = Device_Values.getDataStr;
            //this.reset_values();
        }
        
        // This method sends a command to the device : Taring. Command : 0x1       
        // For standard and filler application.
        private void button4_Click(object sender, EventArgs e)
        {
            // Taring       
            _wtxObj.taring(Write_DataReceived);
        }

        // This method sends a command to the device : Change between gross and net value. Command : 0x2 
        // For standard and filler application.
        private void button1_Click(object sender, EventArgs e)
        {
            // Gross/Net
            _wtxObj.gross(Write_DataReceived);
        }

        
        // This method sends a command to the device : Zeroing. Command : 0x40
        // For standard and filler application.
        private void button5_Click(object sender, EventArgs e)
        {
            // Zeroing
            _wtxObj.zeroing(Write_DataReceived);
        }

        // This method sends a command to the device : Adjust zero. Command : 0x80
        // For standard and filler application.
        private void button6_Click(object sender, EventArgs e)
        {
            // Adjust zero
            _wtxObj.adjustZero(Write_DataReceived);
        }

        // This method sends a command to the device : Adjust nominal. Command : 0x100
        // For standard and filler application.
        private void button7_Click(object sender, EventArgs e)
        {
            // Adjust nominal
            _wtxObj.adjustNominal(Write_DataReceived);
        }

        // This method sends a command to the device : Activate data. Command : 0x800
        // For standard and filler application.
        // If the button 'Activate data' is clicked the output words entered into the datagrid in column ...
        // ... 'Output:value' from word 2 to 26 (standard mode) and from word 9 to 44 are written into the WTX device. 
        private void button8_Click(object sender, EventArgs e)
        {
            // Activate data
                       
            int maximumIndex = 0;

            if (_wtxObj.ApplicationMode == 0)     // if in standard mode: 
            {
                _startIndex = 8;
                _arrayLength = 17;
                maximumIndex = 25;
            }
            else if (_wtxObj.ApplicationMode == 1 || _wtxObj.ApplicationMode == 2)  // if in filler mode: 
            {
                _startIndex  = 11;
                _arrayLength = 26;
                maximumIndex = 36;
            }

            ushort[] valueArr = new ushort[_arrayLength];

            for (int index = _startIndex; index < maximumIndex; index++)  // In Filler mode: From index 11 to the maximum row number.In Standard mode: From index 8 to the maximum row number.
            {
                _i = index - _startIndex;
                
                var input= dataGridView1.Rows[index].Cells[13].Value;
                valueArr[_i] = (ushort)Convert.ToInt16(input);

                string inputStr = input.ToString();

                // Writing values to the WTX according to the data type : S32 or U08 or U16 (given in the GUI datagrid).
                if (inputStr != "0")
                {
                    if (dataGridView1.Rows[index].Cells[10].Value.ToString()=="S32")
                        _wtxObj.WriteOutputWordS32(valueArr[_i], (ushort)Convert.ToInt16(dataGridView1.Rows[index].Cells[8].Value), Write_DataReceived);
                    else
                        if(dataGridView1.Rows[index].Cells[10].Value.ToString() == "U08")
                            _wtxObj.WriteOutputWordU08(valueArr[_i], (ushort)Convert.ToInt16(dataGridView1.Rows[index].Cells[8].Value), Write_DataReceived);           
                    else if (dataGridView1.Rows[index].Cells[10].Value.ToString() == "U16")
                              _wtxObj.WriteOutputWordU16(valueArr[_i], (ushort)Convert.ToInt16(dataGridView1.Rows[index].Cells[8].Value), Write_DataReceived);
                }

                //WTXObj.Async_Call(0x100, Write_DataReceived);
            }

            _wtxObj.activateData(Write_DataReceived); 
        }       

        // This method sends a command to the device : Manual taring. Command : 0x1000
        // Only for standard application.
        private void button9_Click(object sender, EventArgs e)
        {
            // Manual taring
            //if (this.is_standard == true)      // Activate this if-conditon only in case, if the should be a change between standard and filler application. 
            _wtxObj.manualTaring(Write_DataReceived);
        }

        // This method sends a command to the device : Clear dosing results. Command : 0x4
        // Only for filler application.
        private void button11_Click_1(object sender, EventArgs e)
        {
            // Clear dosing results
            //if (this.is_standard == false)
            _wtxObj.clearDosingResults(Write_DataReceived);
        }

        // This method sends a command to the device : Abort dosing. Command : 0x8
        // Only for filler application.
        private void button12_Click_1(object sender, EventArgs e)
        {
            // Abort dosing
            //if (this.is_standard == false)
            _wtxObj.abortDosing(Write_DataReceived);
        }

        // This method sends a command to the device : Start dosing. Command : 0x10
        // Only for filler application.
        private void button13_Click_1(object sender, EventArgs e)
        {
            // Start dosing
            //if (this.is_standard == false)
            _wtxObj.startDosing(Write_DataReceived);
        }

        // This method sends a command to the device : Record weight. Command : 0x4000 , Bit .14
        // For standard and filler application.
        private void button2_Click_1(object sender, EventArgs e)
        {
            // Record weight: 
            //if (this.is_standard == false)
            _wtxObj.recordWeight(Write_DataReceived);

        }

        // This method sends a command to the device : Manual re-dosing. Command : 0x8000
        // Only for filler application.
        private void button14_Click_1(object sender, EventArgs e)
        {
            // Manual re-dosing
            //if (this.is_standard == false)
            _wtxObj.manualReDosing(Write_DataReceived);
        }

        // This event starts the timer and the periodical fetch of values from the device (here: WTX120_Modbus).
        // The timer interval is set in the connection specific class "ModbusTCPConnection".
        // For the application mode(standard or filler) and the printing on the GUI the WTX registers are read out first. 
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // First the connection to the device should be established.   
            _wtxObj.getConnection.Connect();     // Alternative : _wtxObj.Connect();    

            this._dataStr = _wtxObj.GetDataStr;

            if (_wtxObj.ApplicationMode == 0 && this._isStandard == false)
                this._isStandard = true;

            else
                if (_wtxObj.ApplicationMode == 1 && this._isStandard == true)
                this._isStandard = false;
            else
                if (_wtxObj.ApplicationMode == 2 && this._isStandard == true)
                this._isStandard = false;

            // For the application mode(standard or filler) and the printing on the GUI the WTX registers are read out first.      
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            this.set_GUI_rows();
            _wtxObj.Refreshed = true;

            _wtxObj.DataUpdateEvent += ValuesOnConsole;

            // New eventhandler for a change in a data grid cell : 

            dataGridView1.CellValueChanged += new DataGridViewCellEventHandler(GridValueChangedMethod);

        }

        // This method is set if the output value in column 13 has changed - For writing some of the first output words of the standard application. 
        private void GridValueChangedMethod(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 13)
            {
                ushort value = 0;
                ushort index = 0;
                bool inputFormatIsRight = false;

                if (this._isStandard == true)
                {
                    if (e.RowIndex >= 8 && e.RowIndex <= 24)
                    {
                        try
                        {
                            value = (ushort)Convert.ToInt16(dataGridView1[e.ColumnIndex, e.RowIndex].Value); // For the value which should be written to the WTX device 
                            inputFormatIsRight = true;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Die Eingabe hat das falsche Format. Bitte geben Sie eine Zahl ein.");
                            inputFormatIsRight = false;
                        }
                    }
                    else
                        MessageBox.Show("Bitte in den vorgegebenen Feldern eingeben für standard application.");
                }

                if (this._isStandard == false)
                {
                    if (e.RowIndex >= 11 && e.RowIndex <= 36)
                    {
                        try
                        {
                            value = (ushort)Convert.ToInt16(dataGridView1[e.ColumnIndex, e.RowIndex].Value); // For the value which should be written to the WTX device 
                            inputFormatIsRight = true;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Die Eingabe hat das falsche Format. Bitte geben Sie eine Zahl ein.");
                            inputFormatIsRight = false;
                        }
                    }
                    else
                        MessageBox.Show("Bitte in den vorgegebenen Feldern eingeben für filler application.");
                }

                if (inputFormatIsRight == true)
                {
                    index = (ushort)Convert.ToInt16(dataGridView1[8, e.RowIndex].Value); // For the index, the word number which should be written to the WTX device 

                    // For the standard application: 
                    if (this._isStandard == true)
                    {
                        if (e.RowIndex >= 8 && e.RowIndex <= 24)
                        {
                            //MessageBox.Show(value.ToString());  // for test purpose only.
                            // If the specific cell of the row 8,9,10 till row 24 has changed, write the value to the specific properties. 

                            switch (e.RowIndex)
                            {
                                case  8: _wtxObj.ManualTareValue = value; break;
                                case  9: _wtxObj.LimitValue1Input = value; break;
                                case 10: _wtxObj.LimitValue1Mode = value; break;
                                case 11: _wtxObj.LimitValue1ActivationLevelLowerBandLimit = value; break;
                                case 12: _wtxObj.LimitValue1HysteresisBandHeight = value; break;
                                case 13: _wtxObj.LimitValue2Source = value; break;
                                case 14: _wtxObj.LimitValue2Mode = value; break;
                                case 15: _wtxObj.LimitValue2ActivationLevelLowerBandLimit = value; break;
                                case 16: _wtxObj.LimitValue2HysteresisBandHeight = value; break;

                                case 17: _wtxObj.LimitValue3Source = value; break;
                                case 18: _wtxObj.LimitValue3Mode = value; break;
                                case 19: _wtxObj.LimitValue3ActivationLevelLowerBandLimit = value; break;
                                case 20: _wtxObj.LimitValue3HysteresisBandHeight = value; break;
                                case 21: _wtxObj.LimitValue4Source = value; break;
                                case 22: _wtxObj.LimitValue4Mode = value; break;
                                case 23: _wtxObj.LimitValue4ActivationLevelLowerBandLimit = value; break;
                                case 24: _wtxObj.LimitValue4HysteresisBandHeight = value; break;

                                default: break;
                            }
                        }
                    }
                    else if (this._isStandard == false)  // for the filler application. 
                    {
                        if (e.RowIndex >= 11 && e.RowIndex <= 36)
                        {
                            MessageBox.Show(value.ToString());  // for test purpose only.
                            switch (e.RowIndex)
                            {
                                case 11: _wtxObj.ResidualFlowTime = value; break;
                                case 12: _wtxObj.TargetFillingWeight = value; break;
                                case 13: _wtxObj.CoarseFlowCutOffPointSet = value; break;
                                case 14: _wtxObj.FineFlowCutOffPointSet = value; break;
                                case 15: _wtxObj.MinimumFineFlow = value; break;
                                case 16: _wtxObj.OptimizationOfCutOffPoints = value; break;

                                case 17: _wtxObj.MaximumDosingTime = value; break;
                                case 18: _wtxObj.StartWithFineFlow = value; break;
                                case 19: _wtxObj.CoarseLockoutTime = value; break;
                                case 20: _wtxObj.FineLockoutTime = value; break;
                                case 21: _wtxObj.TareMode = value; break;
                                case 22: _wtxObj.UpperToleranceLimit = value; break;
                                case 23: _wtxObj.LowerToleranceLimit = value; break;
                                case 24: _wtxObj.MinimumStartWeight = value; break;

                                case 25: _wtxObj.EmptyWeight = value; break;
                                case 26: _wtxObj.TareDelay = value; break;
                                case 27: _wtxObj.CoarseFlowMonitoringTime = value; break;
                                case 28: _wtxObj.CoarseFlowMonitoring = value; break;
                                case 29: _wtxObj.FineFlowMonitoring = value; break;
                                case 30: _wtxObj.FineFlowMonitoringTime = value; break;

                                case 31: _wtxObj.DelayTimeAfterFineFlow = value; break;
                                case 32: _wtxObj.ActivationTimeAfterFineFlow = value; break;
                                case 33: _wtxObj.SystematicDifference = value; break;
                                case 34: _wtxObj.DownwardsDosing = value; break;
                                case 35: _wtxObj.ValveControl = value; break;
                                case 36: _wtxObj.EmptyingMode = value; break;

                            }
                        }
                    }

                    // For the filler application: 

                    // According to the data type (given in the data grid) the words are written as type 'S32', 'U08' or 'U16' to the WTX. 

                    // Only for testing : 
                    /*
                    if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "S32")
                        WTXObj.writeOutputWordS32(value, index, Write_DataReceived);
                    else
                     if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "U08")
                        WTXObj.writeOutputWordU08(value, index, Write_DataReceived);
                    else
                    if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "U16")
                        WTXObj.writeOutputWordU16(value, index, Write_DataReceived);
                    */
                } // end - if (inputFormatIsRight == true)

                if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "S32")
                    _wtxObj.WriteOutputWordS32(value, index, Write_DataReceived);

                if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "U08")
                    _wtxObj.WriteOutputWordU08(value, index, Write_DataReceived);

                if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "U16")
                    _wtxObj.WriteOutputWordU16(value, index, Write_DataReceived);
            }

            // Test Activate Data after the write of an output word: 
            //WTXObj.Async_Call(0x800, Write_DataReceived);        // Bit .11 - Activate Data


        }

        private void ValuesOnConsole(object sender, DataEvent e)
        {
            this._dataStr = _wtxObj.GetDataStr;

            refresh_values();
        }

        // This method stops the timer after the corresponding event has been triggered during the application.
        // Afterwards the timer and the application can be restarted.
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _wtxObj.DataUpdateEvent -= ValuesOnConsole;
            toolStripStatusLabel1.Text = "Disconnected";    
        }

        // This method stops the timer and exits the application after the corresponding event has been triggered during the application.
        // Afterwards the timer and the application can not be restarted.
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Disconnected";

            _wtxObj.DataUpdateEvent -= ValuesOnConsole;
            this.Close();
            
            Application.Exit();
        }

        // This method saves the values from the GUI in the actual iteration in an extra word file: 
        private void saveInputToolStripMenuItem_Click(object sender, EventArgs e)
        {     
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Word Document|*.rtf";
            saveFileDialog1.Title = "Save input and output words";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {

                string fullPath = Path.Combine(System.IO.Path.GetDirectoryName(saveFileDialog1.FileName), "\\", saveFileDialog1.FileName);

                System.IO.File.Delete(fullPath);

                System.IO.File.AppendAllText(fullPath, " PLC Interface , Input words WTX120_Modbus -> SPS , Application : " + this._dataStr[14] + "\n\n\n");// index 14 ("this.dataStr[14]") refers to application mode of the Device
                System.IO.File.AppendAllText(fullPath, "\nWord|\n" + "Name|\n" + "Type|\n" + "Bit|\n" + "Interface Call Routine|\n" + "List Call Routine|\n" + "Content|\n" + "Value\n\n");

                for (int x=0;x < (dataGridView1.RowCount-1); x++)       // Iterating through the whole data grid: 
                    for(int y = 0; y < (dataGridView1.ColumnCount); y++)
                    {
                        dataGridView1.CurrentCell = dataGridView1.Rows[x].Cells[y];    // Writes the descriptions and values into a word file.
                        System.IO.File.AppendAllText(fullPath, "\n" + dataGridView1.CurrentCell.Value.ToString());
                    }
                
            }
        }

        // This method is used to call another form ("Settings_Form") once the corresponding event is triggerred.
        // It is used to change the connection specific attributes, like IP address, number of inputs and sending/timer interval.
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;     // Stop the timer (Restart is in Class "Settings_Form").
            timer1.Stop();
                  
            _setObj = new SettingsForm(_wtxObj.getConnection.IpAddress, this.timer1.Interval, (ushort)_wtxObj.getConnection.NumofPoints, this);
            _setObj.Show();
        }

        // This method updates the values of the connection(IP address, timer/sending interval, number of inputs), set in class "Settings_Form".
        // See class "Setting_Form" in method button2_Click(sender,e).
        // After updating the values the tool bar labels on the bottom (f.e. "toolStripStatusLabel2") is rewritten with the new values. 
        public void Setting()
        {
            _wtxObj.getConnection.IpAddress = _setObj.GetIpAddress;
            toolStripStatusLabel2.Text = "IP address: " + _wtxObj.getConnection.IpAddress;

            _wtxObj.getConnection.SendingInterval = _setObj.GetSendingInterval;     
            this.timer1.Interval = _setObj.GetSendingInterval;

            _wtxObj.getConnection.NumofPoints = _setObj.GetNumberInputs;
            toolStripStatusLabel5.Text = "Number of Inputs : " + _wtxObj.getConnection.NumofPoints;
        }

        // This method changes the GUI concerning the application mode.
        // If the menu item "Standard" in the menu bar item "View appllication" is selected, the GUI shows the description
        // of the standard application. This is only for the purpose of testing. The values of the device is not changed, only
        // the GUI. 
        private void standardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this._isStandard = true;
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            this.set_GUI_rows();
        }

        // This method changes the GUI concerning the application mode.
        // If the menu item "Filler" in the menu bar item "View appllication" is selected, the GUI shows the description
        // of the standard application. This is only for the purpose of testing. The values of the device is not changed, only
        // the GUI. 
        private void fillerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this._isStandard = false;
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            this.set_GUI_rows();
        }
        
        /*
         *  This method is called once the tool item "Calculate Calibration" is clicked. It creates a windows form for
         *  the calibration with a dead load and a nominal span. 
         */
        private void calculateCalibrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _wtxObj.StopTimer();

            _calcCalObj = new CalcCalibration(_wtxObj, _wtxObj.getConnection.IsConnected);
            DialogResult res = _calcCalObj.ShowDialog();

            _wtxObj.RestartTimer();
        }

        /*
         *  This method is called once the tool item "Calibration with weight" is clicked. It creates a windows form for
         *  the calibration with an individual weight put on the load cell or sensor. 
         */
        private void calibrationWithWeightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _wtxObj.StopTimer();

            _weightCalObj = new WeightCalibration(_wtxObj, _wtxObj.getConnection.IsConnected);
            DialogResult res = _weightCalObj.ShowDialog();

            _wtxObj.RestartTimer();
        }


        private void jetbusToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ModbusTCP_Click(object sender, EventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        // This button event resets the calibration to the following default setting : 
        // Dead load = 0 mv/V
        // Span (Nominal load) = 2 mV/V
        private void button3_Click(object sender, EventArgs e)
        {
            _wtxObj.Calibrating = true;

            _wtxObj.StopTimer();

            _wtxObj.Calculate(0, 2);

            _wtxObj.Calibrating = false;

            //WTX_obj.restartTimer();   // The timer is restarted in the method 'Calculate(..)'.
        }

        // Refresh the GUI if the change between standard and filler have been made: 
        private void button10_Click_1(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            if (_wtxObj.ApplicationMode == 0 && this._isStandard == false)
                this._isStandard = true;

            else
            if (_wtxObj.ApplicationMode == 1 && this._isStandard == true)
                this._isStandard = false;
            else
            if (_wtxObj.ApplicationMode == 2 && this._isStandard == true)
                this._isStandard = false;

            // For the application mode(standard or filler) and the printing on the GUI the WTX registers are read out first.      
            this.set_GUI_rows();

            _wtxObj.Refreshed = true;
        }

        private void Gui_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel6_Click(object sender, EventArgs e)
        {

        }
    }
}