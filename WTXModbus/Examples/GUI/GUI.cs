/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
 * 
 * Author : Felix Huettl 
 * 
 *  */


using System;
using System.Windows.Forms;
using System.IO;
using Hbm.Devices.WTXModbus;
using WTXModbusGUIsimple;
using WTXModbus;
using System.Threading;
using System.ComponentModel;

namespace WTXModbusExamples
{
    /// <summary>
    /// First, objects of class 'ModbusConnection' and 'WTX120' are created to establish a connection and data transfer to the device (WTX120). 
    /// Class 'ModbusConnection' has the purpose to establish a connection, to read from the device (its register)
    /// and to write to the device (its register). Class 'WTX120' creates timer to read and update periodically the values of the WTX in a certain timer
    /// interval given in the constructor of class 'WTX120' while generating an object of it. Class 'WTX120' has all the values, 
    /// which will be interpreted from the read bytes and class 'WTX120' manages the asynchronous data transfer to GUI and the eventbased data transfer #
    /// to class ModbusConnection. 
    ///  
    /// This class 'GUI' represents a window or a dialog box that makes up the application's user interface for the values and their description of the device.
    /// It uses a datagrid to put the description and the periodically updated values together. The description shown in the form and initialized in
    /// the datagrid is based on the manual (see page manual PCLC link on page 154-161). 
    /// Futhermore the data is only displayed, if the values have changed to save reconstruction time on the GUI Form. 
    /// 
    /// Beside a form the GUI could also be a console application by applying that in program.cs instead of a windows form (see on Git).
    /// Therefore the design of the classes and its relations are seperated in 
    /// connection specific classes and interfaces (class ModbusConnection, interface "IModbusConnection")
    /// and in a device specific class and in a device specific interface (class "WTX120", interface "IDevice_Values").
    ///  
    /// In the Windows form, there are several buttons to activate events for the output words, a menu bar on the top to start/stop the application, to save the values,
    /// to show help (like the manual) and to change the settings.
    /// The latter is implemented by an additional form and changes the IP address, number of inputs read out by the register, sending/timer interval. 
    /// A status bar on the bottom shows the actually updated status of the connection ("Connected", "IP adress", "Mode", "TCP/Modbus", "NumInputs").
    /// </summary>
    partial class GUI : Form
    {
        private ModbusConnection ModbusObj;
        private WTX120 WTXModbusObj;

        private SettingsForm Set_obj;

        private CalcCalibration CalcCalObj;
        private WeightCalibration WeightCalObj;

        private string[] data_str_arr;

        private bool is_standard;

        private string ipAddress;
        private int timerInterval;

        private int startIndex ;   // Default setting for standard mode. 
        private int i;
        private int arrayLength;

        // Constructor of class GUI for the initialisation: 
        public GUI(string[] args)
        {
            //Get IPAddress and the timer interval from the command line of the VS project arguments (at "Debug").
            if (args.Length > 0)
            {
                this.ipAddress = args[0];
            }
            if (args.Length > 1)
            {
                this.timerInterval = Convert.ToInt32(args[1]);
            }
            else
                this.timerInterval = 200; // Default value for the timer interval.

            ModbusObj = new ModbusConnection(ipAddress);
            WTXModbusObj = new WTX120(ModbusObj, this.timerInterval);
            
            is_standard =true;      // change between standard and application mode in the GUI. 
            ModbusObj.IP_Adress = ipAddress;

            InitializeComponent();   // Call of this method to initialize the form.

            this.data_str_arr = new string[59];

            for (int i = 0; i < 59; i++)
            {
                this.data_str_arr[i] = "0";
            }

            startToolStripMenuItem_Click(this, new EventArgs());                   

            startIndex = 8;   // Default setting for standard mode. 
            i = 0;
            arrayLength = 0;

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
            dataGridView1.Columns.Add("Input:Bit_header", "Input:Bit");                           // column 4
            dataGridView1.Columns.Add("Input:Interface call routine_header", "Input:Interface call routine"); // column 5
            dataGridView1.Columns.Add("Input:List call routine_header", "Input:List call routine");           // column 6
            dataGridView1.Columns.Add("Input:Description_header", "Input:Description");           // column 7
            dataGridView1.Columns.Add("Input:Value_header", "Input:Value");                       // column 8

            dataGridView1.Columns.Add("Output:Word_header", "Output:Word");                       // column 9
            dataGridView1.Columns.Add("Output:Name_header", "Output:Name");                       // column 10
            dataGridView1.Columns.Add("Output:Type_header", "Output:Type");                       // column 11
            dataGridView1.Columns.Add("Output:Bit_header", "Output:Bit");                         // column 12
            dataGridView1.Columns.Add("Input:Content Content_header", "Input:Content Content");   // column 13
            dataGridView1.Columns.Add("Output:Value_header", "Output:Value");                     // column 16

            if (this.is_standard==true) // case 1) Standard application. Initializing the description and a placeholder for the values in the data grid.
            { 
                dataGridView1.Rows.Add("0", "Measured Value", "Int32", "32Bit", "IDevice_Values.NetValue", "data_str[1]", "Net measured", "0", "0", "Control word", "Bit", ".0", "Taring", "Button taring");                                           // row 1 ; data_str_arr[1]      
                dataGridView1.Rows.Add("2", "Measured Value", "Int32", "32Bit", "IDevice_Values.GrossValue", "data_str[2]", "Gross measured", "0", "0",  "Control word", "Bit", ".1", "Gross/Net",  "Button Gross/Net");                               // row 2 ; data_str_arr[2]      
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".0", "IDevice_Values.general_weight_error", "data_str[3]", "General weight error", "0", "0", "Control word", "Bit", ".6", "Zeroing",  "Button Zeroing");                    // row 3 ; data_str_arr[3]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".1", "IDevice_Values.scale_alarm_triggered", "data_str[4]", "Scale alarm(s) triggered", "0", "0", "Control word", "Bit", ".7", "Adjust zero", "Button Adjust zero");         // row 4 ; data_str_arr[4]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".2-3", "IDevice_Values.limit_status", "data_str[5]", "Limit status", "0", "0", "Control word", "Bit", ".8",  "Adjust nominal", "Button Adjust nominal");                    // row 5 ; data_str_arr[5]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".4", "IDevice_Values.weight_moving", "data_str[6]", "Weight moving", "0", "0",  "Control word", "Bit", ".11",  "Activate data", "Button Activate data");                     // row 6 ; data_str_arr[6]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".5", "IDevice_Values.scale_seal_is_open", "data_str[7]", "Scale seal is open", "0", "0", "Control word", "Bit", ".12",  "Manual taring", "Button Manual taring");            // row 7 ; data_str_arr[7]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".6", "IDevice_Values.manual_tare", "data_str[8]", "Manual tare", "0", "0", "Control word", "Bit", ".14",  "Record weight", "Button Record weight");                          // row 8 ; data_str_arr[8]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".7", "IDevice_Values.weight_type", "data_str[9]", "Weight type", "0", "2", "Manual tare value", "S32", ".0-31",  "Manual tare value", "0");                               // row 9  ; data_str_arr[9]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".8-9", "IDevice_Values.scale_range", "data_str[10]", "Scale range", "0", "4", "Limit value 1", "U08", ".0-7",  "Source 1", "0");                                         // row 10 ; data_str_arr[10]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".10", "IDevice_Values.zero_required", "data_str[11]", "Zero required", "0", "5", "Limit value 1", "U08", ".0-7",  "Mode 1", "0");                                         // row 11 ; data_str_arr[11]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".11", "IDevice_Values.weight_within_the_center_of_zero", "data_str[12]", "Weight within the center of zero", "0", "6", "Limit value 1", "S32", ".0-31",  "Activation level/Lower band limit 1", "0");   // row 12 ; data_str_arr[12]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".12", "IDevice_Values.weight_in_zero_range", "data_str[13]", "Weight in zero range", "0", "8", "Limit value 1", "S32", ".0-31",  "Hysteresis/Band height 1", "0");        // row 13 ; data_str_arr[13]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".0-1", "IDevice_Values.application_mode", "data_str[14]", "Application mode", "0", "10", "Limit value 2", "U08", ".0-7",  "Source 2", "0");                            // row 14 ; data_str_arr[14]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".4-6", "IDevice_Values.decimals", "data_str[15]", "Decimals", "0", "11", "Limit value 2", "U08", ".0-7",  "Mode 2", "0");                                              // row 15 ; data_str_arr[15]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".7-8", "IDevice_Values.unit", "data_str[16]", "Unit", "0", "12", "Limit value 2", "S32", ".0-31",  "Activation level/Lower band limit 2", "0");                        // row 16 ; data_str_arr[16]

                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".14", "IDevice_Values.handshake", "data_str[17]", "Handshake", "0", "14", "Limit value 2", "S32", ".0-31",  "Hysteresis/Band height 2", "0");            // row 17 ; data_str_arr[17] 
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".15", "IDevice_Values.status", "data_str[18]", "Status", "0", "16", "Limit value 3", "U08", ".0-7",  "Source 3", "0");                                   // row 18 ; data_str_arr[18]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".0", "IDevice_Values.input1", "data_str[19]", "Input 1", "0", "17", "Limit value 3", "U08", ".0-7",  "Mode 3", "0");                                            // row 19 ; data_str_arr[19]

                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".1", "IDevice_Values.input2", "data_str[20]", "Input 2", "0", "19", "Limit value 3", "S32", ".0-31",  "Activation level/Lower band limit 3", "0");      // row 20 ; data_str_arr[20]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".2", "IDevice_Values.input3", "data_str[21]", "Input 3", "0", "20", "Limit value 3", "S32", ".0-31",  "Hysteresis/Band height 3", "0");                 // row 21 ; data_str_arr[21]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".3", "IDevice_Values.input4", "data_str[22]", "Input 4", "0", "22", "Limit value 4", "U08", ".0-7",  "Source 4", "0");                                  // row 22 ; data_str_arr[22]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".0", "IDevice_Values.output1", "data_str[23]", "Output 1", "0", "23", "Limit value 4", "U08", ".0-7",  "Mode 4", "0");                                 // row 23 ; data_str_arr[23]

                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".1", "IDevice_Values.output2", "data_str[24]", "Output 2", "0", "24", "Limit value 4", "S32", ".0-31",  "Activation level/Lower band limit 4", "0");      // row 24 ; data_str_arr[24]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".2", "IDevice_Values.output3", "data_str[25]", "Output 3", "0", "26", "Limit value 4", "S32", ".0-31",  "Hysteresis/Band height 4", "0");                 // row 25 ; data_str_arr[25]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".3", "IDevice_Values.output4", "data_str[26]", "Output 4", "0", "46", "Calibration weight", "S32", ".0-31",  "Calibration weight", "Tools Calibration");  // row 26 ; data_str_arr[26]
                dataGridView1.Rows.Add("8", "Limit value 1",   "Bit", ".0", "IDevice_Values.limitValue1", "data_str[27]", "Limit value 1", "0", "48", "Zero load", "S32", ".0-31",  "Zero load", "Tools Calibration");           // row 27 ; data_str_arr[27]

                dataGridView1.Rows.Add("8", "Limit value 2", "Bit", ".1", "IDevice_Values.limitValue2", "data_str[28]", "Limit value 2", "0", "50", "Nominal load", "S32", ".0-31",  "Nominal load", "Tools Calibration");      // row 28 ; data_str_arr[28]              
                dataGridView1.Rows.Add("8", "Limit value 3", "Bit", ".2", "IDevice_Values.limitValue3", "data_str[29]", "Limit value 3", "0", "-", "-", "-", "-", "-", "-", "-", "-", "-");                                     // row 30 ; data_str_arr[29]
                dataGridView1.Rows.Add("8", "Limit value 4", "Bit", ".3", "IDevice_Values.limitValue4", "data_str[30]", "Limit value 4", "0", "-", "-", "-", "-", "-", "-", "-", "-");                                          // row 31 ; data_str_arr[30]
                dataGridView1.Rows.Add("9", "Weight memory, Day", "Int16", ".0-15", "IDevice_Values.weightMemDay", "data_str[31]", "Stored value for day", "0", "-", "-", "-", "-", "-", "-", "-", "-");                        // row 32 ; data_str_arr[31]

                dataGridView1.Rows.Add("10", "Weight memory, Month", "Int16", ".0-15", "IDevice_Values.weightMemMonth", "data_str[32]", "Stored value for month", "0", "-", "-", "-", "-", "-", "-", "-", "-");   // row 33 ; data_str_arr[32]
                dataGridView1.Rows.Add("11", "Weight memory, Year", "Int16", ".0-15", "IDevice_Values.weightMemYear", "data_str[33]", "Stored value for year", "0", "-", "-", "-", "-", "-", "-", "-", "-");      // row 34 ; data_str_arr[33]

                dataGridView1.Rows.Add("12", "Weight memory, Seq...number", "Int16", ".0-15", "IDevice_Values.weightMemSeqNumber", "data_str[34]", "Stored value for seq.number", "0", "-", "-", "-", "-", "-", "-", "-", "-");       // row 35 ; data_str_arr[34]
                dataGridView1.Rows.Add("13", "Weight memory, gross", "Int16", ".0-15", "IDevice_Values.weightMemGross", "data_str[35]", "Stored gross value", "0", "-", "-", "-", "-", "-", "-", "-", "-");                           // row 36 ; data_str_arr[35]
                dataGridView1.Rows.Add("14", "Weight memory, net", "Int16", ".0-15", "IDevice_Values.weightMemNet", "data_str[36]", "Stored net value", "0", "-", "-", "-", "-", "-", "-", "-", "-");                                 // row 37 ; data_str_arr[36]             
            }            
            if (this.is_standard==false) // case 2) Filler application. Initializing the description and a placeholder for the values in the data grid.
            {
                dataGridView1.Rows.Add("0", "Measured Value", "Int32", "32Bit", "IDevice_Values.NetValue", "data_str[1]", "Net measured", "0", "0", "Control word", "Bit", ".0", "Taring",  "Button Taring");                               // row 1 ; data_str_arr[1]      
                dataGridView1.Rows.Add("2", "Measured Value", "Int32", "32Bit", "IDevice_Values.GrossValue", "data_str[2]", "Gross measured", "0", "0", "Control word", "Bit", ".1", "Gross/Net", "Button Gross/Net");                      // row 2 ; data_str_arr[2]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".0", "IDevice_Values.general_weight_error", "data_str[3]", "General weight error", "0", "0", "Control word", "Bit", ".2", "Clear dosing results", "Button Clear dosing results");         // row 3 ; data_str_arr[3]        
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".1", "IDevice_Values.scale_alarm_triggered", "data_str[4]", "Scale alarm(s) triggered", "0", "0", "Control word", "Bit", ".3", "Abort dosing", "Button Abort dosing");                    // row 4 ; data_str_arr[4]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".2-3", "IDevice_Values.limit_status", "data_str[5]", "Limit status", "0", "0", "Control word", "Bit", ".4", "Start dosing", "Button Start dosing");              // row 5 ; data_str_arr[5]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".4", "IDevice_Values.weight_moving", "data_str[6]", "Weight moving", "0", "0", "Control word", "Bit", ".6", "Zeroing", "Button Zeroing");                         // row 6 ; data_str_arr[6]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".5", "IDevice_Values.scale_seal_is_open", "data_str[7]", "Scale seal is open", "0", "0", "Control word", "Bit", ".7", "Adjust zero", "Button Adjust zero");       // row 7 ; data_str_arr[7]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".6", "IDevice_Values.manual_tare", "data_str[8]", "Manual tare", "0", "0", "Control word", "Bit", ".8", "Adjust nominal", "Button Adjust nominal");               // row 8 ; data_str_arr[8]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".7", "IDevice_Values.weight_type", "data_str[9]", "Weight type", "0", "0", "Control word", "Bit", ".11", "Activate data", "Button Activate data");                         // row 9 ;  data_str_arr[9]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".8-9", "IDevice_Values.scale_range", "data_str[10]", "Scale range", "0", "0", "Control word", "Bit", ".14", "Record weight", "Button Record weight");                     // row 10 ; data_str_arr[10]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".10", "IDevice_Values.zero_required", "data_str[11]", "Zero required", "0", "1", "Control word", "Bit", ".15", "Manual re-dosing", "Button Manual re-dosing");             // row 11 ; data_str_arr[11]           
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".11", "IDevice_Values.weight_within_the_center_of_zero", "data_str[12]", "Weight within the center of zero", "0", "2", "Residual flow time", "U16", ".0-15",  "", "0");    // row 12 ; data_str_arr[12]
                
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".12", "IDevice_Values.weight_in_zero_range", "data_str[13]", "Weight in zero range", "0", "4", "Filling weight", "S32", ".0-31",  "", "0");           // row 13 ; data_str_arr[13]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".0-1", "IDevice_Values.application_mode", "data_str[14]", "Application mode", "0", "6", "Coarse flow cut-off point", "S32", ".0-31",  "", "0");    // row 14 ; data_str_arr[14]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".4-6", "IDevice_Values.decimals", "data_str[15]", "Decimals", "0", "8", "Fine flow cut-off point", "S32", ".0-31",  "", "0");                      // row 15 ; data_str_arr[15]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".7-8", "IDevice_Values.unit", "data_str[16]", "Unit", "0", "10", "Minimum fine flow", "S32", ".0-31",  "", "0");                                   // row 16 ; data_str_arr[16]
                
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".14", "IDevice_Values.handshake", "data_str[17]", "Handshake", "0", "11", "Optimization of cut-off points", "U08", ".0-7",  "", "0");  // row 17 ; data_str_arr[17]
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".15", "IDevice_Values.status", "data_str[18]", "Status", "0", "12", "Maximum dosing time", "U16", ".0-15",  "", "0");                  // row 18 ; data_str_arr[18]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".0", "IDevice_Values.input1", "data_str[19]", "Input 1", "0", "13", "Start with fine flow", "U16", ".0-15",  "", "0");                        // row 19 ; data_str_arr[19]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".1", "IDevice_Values.input2", "data_str[20]", "Input 2", "0", "14", "Coarse lockout time", "U16", ".0-15",  "", "0");                         // row 20 ; data_str_arr[20] 
                
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".2", "IDevice_Values.input3", "data_str[21]", "Input 3", "0", "15", "Fine lockout time", "U16", ".0-35",  "", "0");                           // row 21 ; data_str_arr[21]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".3", "IDevice_Values.input4", "data_str[22]", "Input 4", "0", "16", "Tare mode", "U08", ".0-7",  "", "0");                                    // row 22 ; data_str_arr[22]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".0", "IDevice_Values.output1", "data_str[23]", "Output 1", "0", "18", "Tolerance limit +", "S32", ".0-31",  "", "0");                        // row 23 ; data_str_arr[23]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".1", "IDevice_Values.output2", "data_str[24]", "Output 2", "0", "20", "Tolerance limit -", "S32", ".0-31",  "", "0");                        // row 24 ; data_str_arr[24]
                
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".2", "IDevice_Values.output3", "data_str[25]", "Output 3", "0", "22", "Minimum start weight", "S32", ".0-31",  "", "0");                   // row 25 ; data_str_arr[25]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".3", "IDevice_Values.output4", "data_str[26]", "Output 4", "0", "24", "Empty weight", "S32", ".0-31",  "", "0");                           // row 26 ; data_str_arr[26] 
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".0", "IDevice_Values.coarseFlow", "data_str[27]", "Coarse flow", "0", "26", "Tare", "U16", ".0-35",  "", "0");                               // row 27 ; data_str_arr[27]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".1", "IDevice_Values.fineFlow", "data_str[28]", "Fine flow", "0", "28", "Coarse flow monitoring time", "U16", ".0-15",  "", "0");            // row 28 ; data_str_arr[28]

                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".2", "IDevice_Values.ready", "data_str[29]", "Ready", "0", "30", "Coarse flow monitoring", "U32", ".0-31",  "", "0");                            // row 29 ; data_str_arr[29]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".3", "IDevice_Values.reDosing", "data_str[30]", "Re-dosing", "0", "31", "Fine flow monitoring", "U32", ".0-31",  "", "0");                       // row 30 ; data_str_arr[30]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".4", "IDevice_Values.emptying", "data_str[31]", "Emptying", "0", "32", "Fine flow monitoring time", "U16", ".0-15",  "", "0");                   // row 31 ; data_str_arr[31]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".5", "IDevice_Values.flowError", "data_str[32]", "Flow error", "0", "34", "Delay time after fine flow", "U08", ".0-7",  "", "0");                // row 32 ; data_str_arr[32]
               
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".6", "IDevice_Values.alarm", "data_str[33]", "Alarm", "0", "36", "Activation time after fine flow", "U08", ".0-7",  "", "0");                                     // row 33 ; data_str_arr[33]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".7", "IDevice_Values.ADC_OverUnderload", "data_str[34]", "ADC-Overload/Underload", "0", "38", "Systematic difference", "U32", ".0-31",  "", "0");                 // row 34 ; data_str_arr[34]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".8", "IDevice_Values.maxDosingTime", "data_str[35]", "Max. Dosing time", "0", "40", "Downwards dosing", "U08", ".0-7",  "", "0");                                 // row 35 ; data_str_arr[35]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".9", "IDevice_Values.legalTradeOP", "data_str[36]", "Legal-for-trade operation", "0", "46", "Valve control", "U08", ".0-7",  "", "0");                            // row 36 ; data_str_arr[36]
                
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".10", "IDevice_Values.toleranceErrorPlus", "data_str[37]", "Tolerance error +", "0", "48", "Emptying mode", "U08", ".0-7",  "", "0");                             // row 37 ; data_str_arr[37]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".11", "IDevice_Values.toleranceErrorMinus", "data_str[38]", "Tolerance error -", "0", "50", "Calibration weight", "S32", ".0-31",  "", "Tools calibration");      // row 38 ; data_str_arr[38]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".14", "IDevice_Values.statusInput1", "data_str[39]", "Status digital input 1", "0", "Zero load", "S32", ".0-31", "", "", "Tools calibration");                    // row 39 ; data_str_arr[39]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".15", "IDevice_Values.generalScaleError", "data_str[40]", "General scale error", "0", "Nominal load", "S32", ".0-31", "", "", "Tools calibration");               // row 40 ; data_str_arr[40]
               
                dataGridView1.Rows.Add("9", "Dosing process status", "U16", ".0-15", "IDevice_Values.fillingProcessStatus", "data_str[41]", "Initializing,Pre-dosing to Analysis", "0", "-", "-", "-", "-", "-", "-", "-", "-");     // row 41 ; data_str_arr[41]
                dataGridView1.Rows.Add("11", "Dosing count", "U16", ".0-15", "IDevice_Values.numberDosingResults", "data_str[42]", " ", "0", "-");                                                                    // row 43 ; data_str_arr[42]
                dataGridView1.Rows.Add("12", "Dosing result", "S32", ".0-31", "IDevice_Values.dosingResult", "data_str[43]", " ", "0", "-", "-", "-", "-", "-", "-", "-", "-");                                       // row 44 ; data_str_arr[43]

                dataGridView1.Rows.Add("14", "Mean value of dosing results", "S32", ".0-31", "IDevice_Values.meanValueDosingResults", "data_str[44]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                   // row 45 ; data_str_arr[44]
                dataGridView1.Rows.Add("16", "Standard deviation", "S32", ".0-31", "IDevice_Values.standardDeviation", "data_str[45]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                                  // row 46 ; data_str_arr[45]
                dataGridView1.Rows.Add("18", "Total weight", "S32", ".0-31", "IDevice_Values.totalWeight", "data_str[46]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                                              // row 47 ; data_str_arr[46]
                dataGridView1.Rows.Add("20", "Fine flow cut-off point", "S32", ".0-31", "IDevice_Values.fineFlowCutOffPoint", "data_str[47]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                           // row 48 ; data_str_arr[47]
               
                dataGridView1.Rows.Add("22", "Coarse flow cut-off point", "S32", ".0-31", "IDevice_Values.coarseFlowCutOffPoint", "data_str[48]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                       // row 49 ; data_str_arr[48]
                dataGridView1.Rows.Add("24", "Actual dosing time", "U16", ".0-15", "IDevice_Values.actualDosingTime", "data_str[49]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                                   // row 50 ; data_str_arr[49]
                dataGridView1.Rows.Add("25", "Actual coarse flow time", "U16", ".0-15", "IDevice_Values.actualCoarseFlowTime", "data_str[50]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                          // row 51 ; data_str_arr[50]
                dataGridView1.Rows.Add("26", "Actual fine flow time", "U16", ".0-15", "IDevice_Values.actualFineFlowTime", "data_str[51]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                              // row 52 ; data_str_arr[51]
                
                dataGridView1.Rows.Add("27", "Parameter set (product)", "U08", ".0-7", "IDevice_Values.parameterSetProduct", "data_str[52]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                            // row 53 ; data_str_arr[52]
                dataGridView1.Rows.Add("32", "Weight memory, Day", "Int16", ".0-15", "IDevice_Values.weightMemDay", "data_str[53]", "Stored value for day", "0", "-", "-", "-", "-", "-", "-", "-");                  // row 54 ; data_str_arr[53]
                dataGridView1.Rows.Add("33", "Weight memory, Month", "Int16", ".0-15", "IDevice_Values.weightMemMonth", "data_str[54]", "Stored value for month", "0", "-", "-", "-", "-", "-", "-", "-");            // row 55 ; data_str_arr[54]
                dataGridView1.Rows.Add("34", "Weight memory, Year", "Int16", ".0-15", "IDevice_Values.weightMemYear", "data_str[55]", "Stored value for year", "0", "-", "-", "-", "-", "-", "-", "-");               // row 56 ; data_str_arr[55]
               
                dataGridView1.Rows.Add("35", "Weight memory, Seq number", "Int16", ".0-15", "IDevice_Values.weightSeqNumber", "data_str[56]", "Stored value for seq.number", "0", "-", "-", "-", "-", "-", "-", "-"); // row 57 ; data_str_arr[56]
                dataGridView1.Rows.Add("36", "Weight memory, gross", "Int16", ".0-15", "IDevice_Values.weightMemGross", "data_str[57]", "Stored gross value", "0", "-", "-", "-", "-", "-", "-", "-");                // row 58 ; data_str_arr[57]
                dataGridView1.Rows.Add("37", "Weight memory, net", "Int16", ".0-15", "IDevice_Values.weightMemDayNet", "data_str[58]", "Stored net value", "0", "-", "-", "-", "-", "-", "-", "-");                   // row 59 ; data_str_arr[58]

            }

            label1.Text = "Only for Standard application:";       // label for information : Only output words for standard application
            label2.Text = "Only for Filler application:";         // label for information : Only output words for filler application 
            toolStripStatusLabel5.Text = "38";
            if (WTXModbusObj.getConnection.is_connected == true)
                toolStripStatusLabel1.Text = "Connected";
            else
                toolStripStatusLabel1.Text = "Disconnected";

            dataGridView1.Columns[4].Width = 250;                 // Width of the fourth column containing the periodically updated values.           
        }

        // This automatic property returns an instance of this class. It has usage in the class "Settings_Form".
        public WTX120 get_dataviewer
        {
            get
            {
                return this.WTXModbusObj;
            }
        }

        // This private method is called for initializing basic information for the tool menu bar on the bottom of the windows form: 
        // For the connection status, IP adress, application mode and number of inputs. 
        private void GUI_Load(object sender, EventArgs e)
        {
            if (WTXModbusObj.getConnection.is_connected == true)
                toolStripStatusLabel1.Text = "Connected";
            else
                toolStripStatusLabel1.Text = "Disconnected";

            toolStripStatusLabel2.Text = "IP adress: " + WTXModbusObj.getConnection.IP_Adress;
            toolStripStatusLabel3.Text = "Mode : " + this.data_str_arr[14]; // index 14 refers to application mode of the Device
            toolStripStatusLabel5.Text = "Number of Inputs : " + WTXModbusObj.getConnection.getNumOfPoints; 
        }

        // This method actualizes and resets the data grid with newly calculated values of the previous iteration. 
        // First it actualizes the tool bar menu regarding the status of the connection, afterwards it iterates the 
        // "data_str_arr" array to actualize every element of the data grid in the standard or filler application. 
        public void refresh_values()
        {     
            if (WTXModbusObj.getConnection.is_connected == true)
                toolStripStatusLabel1.Text = "Connected";
            if (WTXModbusObj.getConnection.is_connected == false)
                toolStripStatusLabel1.Text = "Disconnected";
            
            toolStripStatusLabel2.Text = "IP address: " + WTXModbusObj.getConnection.IP_Adress;
            toolStripStatusLabel3.Text = "Mode : " + this.data_str_arr[14];                 // index 14 refers to application mode of the Device
            toolStripStatusLabel2.Text = "IP adress: " + WTXModbusObj.getConnection.IP_Adress;

            //Changing the width of a column:
            /*foreach (DataGridViewTextBoxColumn c in dataGridView1.Columns)
                c.Width = 120;*/
            try
            {
                for (int index = 0; index <= 26; index++) // Up to index 26, the input words are equal in standard and filler application.                           
                    dataGridView1.Rows[index].Cells[7].Value = data_str_arr[index];
            }catch(Exception){ }

            if (WTXModbusObj.applicationMode == 0)             // In the standard application: 
            {
                try
                {
                    for (int index = 27; index <= 35; index++)
                    dataGridView1.Rows[(index+1)].Cells[7].Value = data_str_arr[index];  //ddd: Achtung Index auf die Schnelle hier verschben, es gibt kein Net and gross measured!?
                }
                catch (Exception) { }
            }
            else
            if (WTXModbusObj.applicationMode == 1 || WTXModbusObj.applicationMode == 2)   // In the filler application: 
                {
                    try
                    {
                        for (int index = 27; index <55; index++)
                        {
                             dataGridView1.Rows[(index + 1)].Cells[7].Value = data_str_arr[index];
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

        // This is the callback method for writing. First the values in "data_str_arr" are updated and 
        // the GUI is actualized.
        // A asynchronous call is used in the following button_Click methods. 
        // The callback method is Write_DataReceived, which is called once the command is written into the register of the device.      
        public void Write_DataReceived(IDeviceValues Device_Values)
        {
            //this.data_str_arr = Device_Values.get_data_str;
            //this.reset_values();
        }
        
        // This method sends a command to the device : Taring. Command : 0x1       
        // For standard and filler application.
        private void button4_Click(object sender, EventArgs e)
        {
            // Taring       
            WTXModbusObj.Async_Call(0x1, Write_DataReceived);
        }

        // This method sends a command to the device : Change between gross and net value. Command : 0x2 
        // For standard and filler application.
        private void button1_Click(object sender, EventArgs e)
        {
            // Gross/Net
            WTXModbusObj.Async_Call(0x2, Write_DataReceived);
        }

        
        // This method sends a command to the device : Zeroing. Command : 0x40
        // For standard and filler application.
        private void button5_Click(object sender, EventArgs e)
        {
            // Zeroing
            WTXModbusObj.Async_Call(0x40, Write_DataReceived);
        }

        // This method sends a command to the device : Adjust zero. Command : 0x80
        // For standard and filler application.
        private void button6_Click(object sender, EventArgs e)
        {
            // Adjust zero
            WTXModbusObj.Async_Call(0x80, Write_DataReceived);
        }

        // This method sends a command to the device : Adjust nominal. Command : 0x100
        // For standard and filler application.
        private void button7_Click(object sender, EventArgs e)
        {
            // Adjust nominal
            WTXModbusObj.Async_Call(0x100, Write_DataReceived);
        }

        // This method sends a command to the device : Activate data. Command : 0x800
        // For standard and filler application.
        // If the button 'Activate data' is clicked the output words entered into the datagrid in column ...
        // ... 'Output:value' from word 2 to 26 (standard mode) and from word 9 to 44 are written into the WTX device. 
        private void button8_Click(object sender, EventArgs e)
        {
            // Activate data
            
            int maximumIndex = 0;

            if (WTXModbusObj.applicationMode == 0)     // if in standard mode: 
            {
                startIndex = 8;
                arrayLength = 17;
                maximumIndex = 25;
            }
            else if (WTXModbusObj.applicationMode == 1 || WTXModbusObj.applicationMode == 2)  // if in filler mode: 
            {
                startIndex  = 11;
                arrayLength = 26;
                maximumIndex = 36;
            }

            ushort[] valueArr = new ushort[arrayLength];

            for (int index = startIndex; index < maximumIndex; index++)  // In Filler mode: From index 11 to the maximum row number.In Standard mode: From index 8 to the maximum row number.
            {
                i = index - startIndex;
                
                var input= dataGridView1.Rows[index].Cells[13].Value;
                valueArr[i] = (ushort)Convert.ToInt16(input);

                string inputStr = input.ToString();

                // Writing values to the WTX according to the data type : S32 or U08 or U16 (given in the GUI datagrid).
                if (inputStr != "0")
                {                   
                    if (dataGridView1.Rows[index].Cells[10].Value.ToString()=="S32")
                        WTXModbusObj.writeOutputWordS32(valueArr[i], (ushort)Convert.ToInt16(dataGridView1.Rows[index].Cells[8].Value), Write_DataReceived);

                    else
                        if(dataGridView1.Rows[index].Cells[10].Value.ToString() == "U08")
                            WTXModbusObj.writeOutputWordU08(valueArr[i], (ushort)Convert.ToInt16(dataGridView1.Rows[index].Cells[8].Value), Write_DataReceived);

                    else if (dataGridView1.Rows[index].Cells[10].Value.ToString() == "U16")
                              WTXModbusObj.writeOutputWordU16(valueArr[i], (ushort)Convert.ToInt16(dataGridView1.Rows[index].Cells[8].Value), Write_DataReceived);
                }


                WTXModbusObj.Async_Call(0x100, Write_DataReceived);
            }
        }

        // This method sends a command to the device : Manual taring. Command : 0x1000
        // Only for standard application.
        private void button9_Click(object sender, EventArgs e)
        {
            // Manual taring
            //if (this.is_standard == true)      // Activate this if-conditon only in case, if the should be a change between standard and filler application. 
            WTXModbusObj.Async_Call(0x1000, Write_DataReceived);             
        }

        // This method sends a command to the device : Record weight. Command : 0x4000
        // For standard and filler application.
        private void button10_Click(object sender, EventArgs e)
        {
            // Record weight
            WTXModbusObj.Async_Call(0x4000, Write_DataReceived);   // Bit .14
        }

        // This method sends a command to the device : Clear dosing results. Command : 0x4
        // Only for filler application.
        private void button11_Click(object sender, EventArgs e)
        {
            // Clear dosing results
            //if (this.is_standard == false)
            WTXModbusObj.Async_Call(0x4, Write_DataReceived);  // Bit .2
        }

        // This method sends a command to the device : Abort dosing. Command : 0x8
        // Only for filler application.
        private void button12_Click(object sender, EventArgs e)
        {
            // Abort dosing
            //if (this.is_standard == false)
            WTXModbusObj.Async_Call(0x8, Write_DataReceived);   // Bit .3
        }

        // This method sends a command to the device : Start dosing. Command : 0x10
        // Only for filler application.
        private void button13_Click(object sender, EventArgs e)
        {
            // Start dosing
            //if (this.is_standard == false)
            WTXModbusObj.Async_Call(0x10, Write_DataReceived);    // Bit .4
        }

        // Write Bit .14 to the register of the WTX device. Only in the filler mode: 
        private void button2_Click_1(object sender, EventArgs e)
        {
            // Manual re-dosing
            //if (this.is_standard == false)
            WTXModbusObj.Async_Call(0x4000, Write_DataReceived);        // Bit .14

        }

        // This method sends a command to the device : Manual re-dosing. Command : 0x8000
        // Only for filler application.
        private void button14_Click(object sender, EventArgs e)
        {
            // Manual re-dosing
            //if (this.is_standard == false)
            WTXModbusObj.Async_Call(0x8000, Write_DataReceived);        // Bit .15
        }

        // This event starts the timer and the periodical fetch of values from the device (here: WTX120).
        // The timer interval is set in the connection specific class "ModbusConnection".
        // For the application mode(standard or filler) and the printing on the GUI the WTX registers are read out first. 
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WTXModbusObj.getConnection.Connect();  // First the connection to the device should be established.          

            this.data_str_arr = WTXModbusObj.getDataStr;

            if (WTXModbusObj.applicationMode == 0 && this.is_standard == false)
                this.is_standard = true;

            else
                if (WTXModbusObj.applicationMode == 1 && this.is_standard == true)
                this.is_standard = false;
            else
                if (WTXModbusObj.applicationMode == 2 && this.is_standard == true)
                this.is_standard = false;

            // For the application mode(standard or filler) and the printing on the GUI the WTX registers are read out first.      
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            this.set_GUI_rows();
            WTXModbusObj.Refreshed = true;

            WTXModbusObj.DataUpdateEvent += ValuesOnConsole;

            // New eventhandler for a change in a data grid cell : 

            dataGridView1.CellValueChanged += new DataGridViewCellEventHandler(gridValueChangedMethod);

        }

        // This method is set if the output value in column 13 has changed - For writing some of the first output words of the standard application. 
        private void gridValueChangedMethod(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 13)
            {
                ushort value = 0;
                ushort index = 0;
                bool inputFormatIsRight = false;

                if (this.is_standard == true)
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

                if (this.is_standard == false)
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
                    if (this.is_standard == true)
                    {
                        if (e.RowIndex >= 8 && e.RowIndex <= 24)
                        {
                            //MessageBox.Show(value.ToString());  // for test purpose only.

                            // If the specific cell of the row 8,9,10 till row 24 has changed, write the value to the specific properties. 

                            switch (e.RowIndex)
                            {
                                case 8: WTXModbusObj.manualTareValue = value; break;
                                case 9: WTXModbusObj.limitValue1Input = value; break;
                                case 10: WTXModbusObj.limitValue1Mode = value; break;
                                case 11: WTXModbusObj.limitValue1ActivationLevelLowerBandLimit = value; break;
                                case 12: WTXModbusObj.limitValue1HysteresisBandHeight = value; break;
                                case 13: WTXModbusObj.limitValue2Source = value; break;
                                case 14: WTXModbusObj.limitValue2Mode = value; break;
                                case 15: WTXModbusObj.limitValue2ActivationLevelLowerBandLimit = value; break;
                                case 16: WTXModbusObj.limitValue2HysteresisBandHeight = value; break;

                                case 17: WTXModbusObj.limitValue3Source = value; break;
                                case 18: WTXModbusObj.limitValue3Mode = value; break;
                                case 19: WTXModbusObj.limitValue3ActivationLevelLowerBandLimit = value; break;
                                case 20: WTXModbusObj.limitValue3HysteresisBandHeight = value; break;
                                case 21: WTXModbusObj.limitValue4Source = value; break;
                                case 22: WTXModbusObj.limitValue4Mode = value; break;
                                case 23: WTXModbusObj.limitValue4ActivationLevelLowerBandLimit = value; break;
                                case 24: WTXModbusObj.limitValue4HysteresisBandHeight = value; break;

                                default: break;
                            }
                        }
                    }
                    else if (this.is_standard == false)  // for the filler application. 
                    {
                        if (e.RowIndex >= 11 && e.RowIndex <= 36)
                        {
                            MessageBox.Show(value.ToString());  // for test purpose only.
                            switch (e.RowIndex)
                            {
                                case 11: WTXModbusObj.ResidualFlowTime = value; break;
                                case 12: WTXModbusObj.targetFillingWeight = value; break;
                                case 13: WTXModbusObj.coarseFlowCutOffPointSet = value; break;
                                case 14: WTXModbusObj.fineFlowCutOffPointSet = value; break;
                                case 15: WTXModbusObj.minimumFineFlow = value; break;
                                case 16: WTXModbusObj.optimizationOfCutOffPoints = value; break;

                                case 17: WTXModbusObj.maximumDosingTime = value; break;
                                case 18: WTXModbusObj.startWithFineFlow = value; break;
                                case 19: WTXModbusObj.coarseLockoutTime = value; break;
                                case 20: WTXModbusObj.fineLockoutTime = value; break;
                                case 21: WTXModbusObj.tareMode = value; break;
                                case 22: WTXModbusObj.upperToleranceLimit = value; break;
                                case 23: WTXModbusObj.lowerToleranceLimit = value; break;
                                case 24: WTXModbusObj.minimumStartWeight = value; break;

                                case 25: WTXModbusObj.emptyWeight = value; break;
                                case 26: WTXModbusObj.tareDelay = value; break;
                                case 27: WTXModbusObj.coarseFlowMonitoringTime = value; break;
                                case 28: WTXModbusObj.coarseFlowMonitoring = value; break;
                                case 29: WTXModbusObj.fineFlowMonitoring = value; break;
                                case 30: WTXModbusObj.fineFlowMonitoringTime = value; break;

                                case 31: WTXModbusObj.delayTimeAfterFineFlow = value; break;
                                case 32: WTXModbusObj.activationTimeAfterFineFlow = value; break;
                                case 33: WTXModbusObj.systematicDifference = value; break;
                                case 34: WTXModbusObj.downardsDosing = value; break;
                                case 35: WTXModbusObj.valveControl = value; break;
                                case 36: WTXModbusObj.emptyingMode = value; break;

                            }
                        }
                    }

                    // For the filler application: 

                    // According to the data type (given in the data grid) the words are written as type 'S32', 'U08' or 'U16' to the WTX. 

                    // Only for testing : 
                    /*
                    if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "S32")
                        WTXModbusObj.writeOutputWordS32(value, index, Write_DataReceived);
                    else
                     if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "U08")
                        WTXModbusObj.writeOutputWordU08(value, index, Write_DataReceived);
                    else
                    if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "U16")
                        WTXModbusObj.writeOutputWordU16(value, index, Write_DataReceived);
                    */
                } // end - if (inputFormatIsRight == true)

                if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "S32")
                    WTXModbusObj.writeOutputWordS32(value, index, Write_DataReceived);

                if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "U08")
                    WTXModbusObj.writeOutputWordU08(value, index, Write_DataReceived);

                if (dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString() == "U16")
                    WTXModbusObj.writeOutputWordU16(value, index, Write_DataReceived);
            }

            // Test Activate Data after the write of an output word: 
            //WTXModbusObj.Async_Call(0x800, Write_DataReceived);        // Bit .11 - Activate Data


        }

        private void ValuesOnConsole(object sender, NetConnectionEventArgs<ushort[]> e)
        {
            this.data_str_arr = WTXModbusObj.getDataStr;

            refresh_values();
        }

        // This method stops the timer after the corresponding event has been triggered during the application.
        // Afterwards the timer and the application can be restarted.
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WTXModbusObj.DataUpdateEvent -= ValuesOnConsole;
            toolStripStatusLabel1.Text = "Disconnected";    
        }

        // This method stops the timer and exits the application after the corresponding event has been triggered during the application.
        // Afterwards the timer and the application can not be restarted.
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Disconnected";

            WTXModbusObj.DataUpdateEvent -= ValuesOnConsole;
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

                string full_path = Path.Combine(System.IO.Path.GetDirectoryName(saveFileDialog1.FileName), "\\", saveFileDialog1.FileName);

                System.IO.File.Delete(@full_path);

                System.IO.File.AppendAllText(@full_path, " PLC Interface , Input words WTX120 -> SPS , Application : " + this.data_str_arr[14] + "\n\n\n");// index 14 ("this.data_str_arr[14]") refers to application mode of the Device
                System.IO.File.AppendAllText(@full_path, "\nWord|\n" + "Name|\n" + "Type|\n" + "Bit|\n" + "Interface Call Routine|\n" + "List Call Routine|\n" + "Content|\n" + "Value\n\n");

                for (int x=0;x < (dataGridView1.RowCount-1); x++)       // Iterating through the whole data grid: 
                    for(int y = 0; y < (dataGridView1.ColumnCount); y++)
                    {
                        dataGridView1.CurrentCell = dataGridView1.Rows[x].Cells[y];    // Writes the descriptions and values into a word file.
                        System.IO.File.AppendAllText(@full_path, "\n" + dataGridView1.CurrentCell.Value.ToString());
                    }
                
            }
        }

        // This method is used to call another form ("Settings_Form") once the corresponding event is triggerred.
        // It is used to change the connection specific attributes, like IP adress, number of inputs and sending/timer interval.
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;     // Stop the timer (Restart is in Class "Settings_Form").
            timer1.Stop();
                  
            Set_obj = new SettingsForm(WTXModbusObj.getConnection.IP_Adress, this.timer1.Interval, WTXModbusObj.getConnection.getNumOfPoints, this);
            Set_obj.Show();
        }

        // This method updates the values of the connection(IP adress, timer/sending interval, number of inputs), set in class "Settings_Form".
        // See class "Setting_Form" in method button2_Click(sender,e).
        // After updating the values the tool bar labels on the bottom (f.e. "toolStripStatusLabel2") is rewritten with the new values. 
        public void setting()
        {
            WTXModbusObj.getConnection.IP_Adress = Set_obj.get_IP_address;
            toolStripStatusLabel2.Text = "IP adress: " + WTXModbusObj.getConnection.IP_Adress;

            WTXModbusObj.getConnection.Sending_interval = Set_obj.get_sending_interval;     
            this.timer1.Interval = Set_obj.get_sending_interval;

            WTXModbusObj.getConnection.getNumOfPoints = Set_obj.get_number_inputs;
            toolStripStatusLabel5.Text = "Number of Inputs : " + WTXModbusObj.getConnection.getNumOfPoints;
        }

        // This method changes the GUI concerning the application mode.
        // If the menu item "Standard" in the menu bar item "View appllication" is selected, the GUI shows the description
        // of the standard application. This is only for the purpose of testing. The values of the device is not changed, only
        // the GUI. 
        private void standardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.is_standard = true;
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
            this.is_standard = false;
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
            WTXModbusObj.stopTimer();

            CalcCalObj = new CalcCalibration(WTXModbusObj, WTXModbusObj.getConnection.is_connected);
            DialogResult res = CalcCalObj.ShowDialog();

            WTXModbusObj.restartTimer();
        }

        /*
         *  This method is called once the tool item "Calibration with weight" is clicked. It creates a windows form for
         *  the calibration with an individual weight put on the load cell or sensor. 
         */
        private void calibrationWithWeightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WTXModbusObj.stopTimer();

            WeightCalObj = new WeightCalibration(WTXModbusObj, WTXModbusObj.getConnection.is_connected);
            DialogResult res = WeightCalObj.ShowDialog();

            WTXModbusObj.restartTimer();
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
            WTXModbusObj.Calibrating = true;

            WTXModbusObj.stopTimer();

            WTXModbusObj.Calculate(0, 2);

            WTXModbusObj.Calibrating = false;

            //WTX_obj.restartTimer();   // The timer is restarted in the method 'Calculate(..)'.
        }

        // Refresh the GUI if the change between standard and filler have been made: 
        private void button10_Click_1(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            if (WTXModbusObj.applicationMode == 0 && this.is_standard == false)
                this.is_standard = true;

            else
            if (WTXModbusObj.applicationMode == 1 && this.is_standard == true)
                this.is_standard = false;
            else
            if (WTXModbusObj.applicationMode == 2 && this.is_standard == true)
                this.is_standard = false;

            // For the application mode(standard or filler) and the printing on the GUI the WTX registers are read out first.      
            this.set_GUI_rows();

            WTXModbusObj.Refreshed = true;
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

    }
}