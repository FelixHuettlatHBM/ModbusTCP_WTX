/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WTXModbus
{
    /// <summary>
    /// First, an object of this class is created in Program.cs and in the constructor of GUI objects of class ModbusTCP and WTX120 are created to establish
    /// a connection and data transfer to the device (WTX120), class ModbusTcp. Class ModbusTCP has the purpose to establish a connection, to read from the device (its register)
    /// and to write to the device (its register). Class WTX120 has all the values, which will be interpreted from the read bytes and class WTX120 manages 
    /// the asynchronous data transfer to GUI and the eventbased data transfer to class ModbusTCP. 
    /// By first initializing the class  GUI, we have the graphical interface seperated from the other class. 
    /// 
    /// This class represents a window or a dialog box that makes up the application's user interface for the values and their description of the device.
    /// It uses a datagrid to put the description and the periodically updated values together. The description shown in the form and initialized in
    /// the datagrid is based on the manual (see page 154-161). 
    /// 
    /// This class contains a timer to read the data periodically from the device in a user-defined interval (timer1.Interval, sending_interval). 
    /// Alternative: The timer could also be implemented in class "Modbus_TCP" (Example in the Console Application, see Git).
    /// Futhermore the data is only displayed, if the values have changed to save reconstruction time on the GUI Form. 
    /// 
    /// Beside a form the GUI could also be a console application by applying that in program.cs instead of a windows form (see on Git).
    /// Therefore the design of the classes and its relations are seperated in 
    /// connection specific classes and interfaces (class "Modbus_TCP", interface "Communication_Device_Interface", "Communication_Interface")
    /// and in a device specific class and interface (class "WTX_120", interface "IDevice_Values").
    ///  
    /// In the form, there are several buttons to activate events for the output words, a menu bar on the top to start/stop the application, to save the values,
    /// to show help (like the manual) and to change the settings.
    /// The latter is implemented by an additional form and changes the IP address, number of inputs read out by the register, sending/timer interval. 
    /// A status bar on the bottom shows the actually updated status of the connection ("Connected", "IP adress", "Mode", "TCP/Modbus", "NumInputs").
    /// </summary>
     partial class GUI : Form
    {
        private ModbusTCP Modbus_TCP_obj;
        private WTX120 WTX_obj;
        private SettingsForm Set_obj;

        private string[] data_str_arr;
        private ushort[] previous_data_ushort_arr;

        private bool is_standard;
        int t;
   
        bool compare_test;

        // Constructor: 
        public GUI()
        {
            // Implementation of the publisher (Modbus_TCP_obj) and 
            // the subscribter (Device_WTX_obj)

            this.Modbus_TCP_obj = new ModbusTCP();
            this.WTX_obj = new WTX120("WTX120_1", Modbus_TCP_obj);

            // In the following the opportunity is shown to attach more than
            // one Subsriber (More than one Device), based on the same publisher (Modbus_TCP): 

            //WTX_120 Device_obj_2 = new WTX_120("WTX120_2", Modbus_TCP_obj);
            //WTX_120 Device_obj_3 = new WTX_120("WTX120_3", Modbus_TCP_obj);
            //WTX_120 Device_obj_4 = new WTX_120("WTX120_4", Modbus_TCP_obj);
            // ... 

            t = 0;
            compare_test=true;
            is_standard =false;

            InitializeComponent();   // Call of this method to initialize the form.

            this.data_str_arr = new string[59];
            this.previous_data_ushort_arr = new ushort[59];

            for (int i = 0; i < 59; i++)
            {
                this.data_str_arr[i] = "0";
                this.previous_data_ushort_arr[i] = 0;
            }
            this.set_GUI_rows();
        }

        // This method is called from the constructor and sets the columns and the rows of the data grid and shows it as a form.  
        // There are 2 cases:
        // 1) Standard application : Input words "0+2" till "14". Output words "0" till "50". 
        // 2) Filler   application : Input words "0+2" till "37". Output words "0" till "50". 
        public void set_GUI_rows()
        {
            if (WTX_obj.application_mode == 0)
                this.is_standard = true;

            if (WTX_obj.application_mode == 1 || WTX_obj.application_mode == 2)
                this.is_standard = false;

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
            dataGridView1.Columns.Add("Input:Interface call routine_header", "Input:Interface call routine");  // column 13
            dataGridView1.Columns.Add("Input:List call routine_header", "Input:List call routine");            // column 14
            dataGridView1.Columns.Add("Output:Description_header", "Output:Description");         // column 15
            dataGridView1.Columns.Add("Output:Value_header", "Output:Value");                     // column 16

            if (this.is_standard==true) // case 1) Standard application. Initializing the description and a placeholder for the values in the data grid.
            {
                dataGridView1.Rows.Add("0+2", "Measured Value", "Int32", "32Bit", "IDevice_Values.NetandGrossValue", "data_str[0]", "Net and gross measured", "0", "0", "Control word", "Bit", ".0", "", "", "Taring", "0");              // row 1 ; data_str_arr[0]
                dataGridView1.Rows.Add("0", "Measured Value", "Int32", "32Bit", "IDevice_Values.NetValue", "data_str[1]", "Net measured", "0", "0", "Control word", "Bit", ".1", "", "", "Gross/Net", "0");                               // row 2 ; data_str_arr[1]      
                dataGridView1.Rows.Add("2", "Measured Value", "Int32", "32Bit", "IDevice_Values.GrossValue", "data_str[2]", "Gross measured", "0", "0", "Control word", "Bit", ".2", "", "", "Zeroing", "0");                             // row 3 ; data_str_arr[2]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".0", "IDevice_Values.general_weight_error", "data_str[3]", "General weight error", "0", "0", "Control word", "Bit", ".3", "", "", "Adjust zero", "0");         // row 4 ; data_str_arr[3]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".1", "IDevice_Values.scale_alarm_triggered", "data_str[4]", "Scale alarm(s) triggered", "0", "0", "Control word", "Bit", ".4", "", "", "Adjust nominal", "0"); // row 5 ; data_str_arr[4]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".2-3", "IDevice_Values.limit_status", "data_str[5]", "Limit status", "0", "0", "Control word", "Bit", ".5", "", "", "Activate data", "0");                    // row 6 ; data_str_arr[5]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".4", "IDevice_Values.weight_moving", "data_str[6]", "Weight moving", "0", "0", "Control word", "Bit", ".6", "", "", "Manual taring", "0");                     // row 7 ; data_str_arr[6]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".5", "IDevice_Values.scale_seal_is_open", "data_str[7]", "Scale seal is open", "0", "0", "Control word", "Bit", ".7", "", "", "Weight storage", "0");          // row 8 ; data_str_arr[7]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".6", "IDevice_Values.manual_tare", "data_str[8]", "Manual tare", "0", "2", "Manual tare value", "S32", ".0-31", "", "", "Manual tare value", "0");             // row 9  ; data_str_arr[8]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".7", "IDevice_Values.weight_type", "data_str[9]", "Weight type", "0", "4", "Limit value 1", "U08", ".0-7", "", "", "Source 1", "0");                           // row 10 ; data_str_arr[9]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".8-9", "IDevice_Values.scale_range", "data_str[10]", "Scale range", "0", "5", "Limit value 1", "U08", ".0-7", "", "", "Mode 1", "0");                         // row 11 ; data_str_arr[10]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".10", "IDevice_Values.zero_required", "data_str[11]", "Zero required", "0", "6", "Limit value 1", "S32", ".0-31", "", "", "Activation level/Lower band limit 1", "0");                             // row 12 ; data_str_arr[11]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".11", "IDevice_Values.weight_within_the_center_of_zero", "data_str[12]", "Weight within the center of zero", "0", "8", "Limit value 1", "S32", ".0-31", "", "", "Hysteresis/Band height 1", "0");  // row 13 ; data_str_arr[12]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".12", "IDevice_Values.weight_in_zero_range", "data_str[13]", "Weight in zero range", "0", "10", "Limit value 2", "U08", ".0-7", "", "", "Source 2", "0");               // row 14 ; data_str_arr[13]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".0-1", "IDevice_Values.application_mode", "data_str[14]", "Application mode", "0", "11", "Limit value 2", "U08", ".0-7", "", "", "Mode 2", "0");                     // row 15 ; data_str_arr[14]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".4-6", "IDevice_Values.decimals", "data_str[15]", "Decimals", "0", "12", "Limit value 2", "S32", ".0-31", "", "", "Activation level/Lower band limit 2", "0");       // row 16 ; data_str_arr[15]

                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".7-8", "IDevice_Values.unit", "data_str[16]", "Unit", "0", "14", "Limit value 2", "S32", ".0-31", "", "", "Hysteresis/Band height 2", "0");            // row 17 ; data_str_arr[16] 
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".14", "IDevice_Values.handshake", "data_str[17]", "Handshake", "0", "16", "Limit value 3", "U08", ".0-7", "", "", "Source 3", "0");                     // row 18 ; data_str_arr[17]
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".15", "IDevice_Values.status", "data_str[18]", "Status", "0", "17", "Limit value 3", "U08", ".0-7", "", "", "Mode 3", "0");                             // row 19 ; data_str_arr[18]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".0", "IDevice_Values.status", "data_str[19]", "Input 1", "0", "18", "Limit value 3", "S32", ".0-31", "", "", "Activation level/Lower band limit 3", "0");      // row 20 ; data_str_arr[19]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".1", "IDevice_Values.status", "data_str[20]", "Input 2", "0", "20", "Limit value 3", "S32", ".0-31", "", "", "Hysteresis/Band height 3", "0");                 // row 21 ; data_str_arr[20]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".2", "IDevice_Values.status", "data_str[21]", "Input 3", "0", "22", "Limit value 4", "U08", ".0-7", "", "", "Source 4", "0");                                  // row 22 ; data_str_arr[21]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".3", "IDevice_Values.status", "data_str[22]", "Input 4", "0", "23", "Limit value 4", "U08", ".0-7", "", "", "Mode 4", "0");                                    // row 23 ; data_str_arr[22]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".0", "IDevice_Values.status", "data_str[23]", "Output 1", "0", "24", "Limit value 4", "S32", ".0-31", "", "", "Activation level/Lower band limit 4", "0");    // row 24 ; data_str_arr[23]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".1", "IDevice_Values.status", "data_str[24]", "Output 2", "0", "26", "Limit value 4", "S32", ".0-31", "", "", "Hysteresis/Band height", "0");                 // row 25 ; data_str_arr[24]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".2", "IDevice_Values.status", "data_str[25]", "Output 3", "0", "46", "Calibration weight", "S32", ".0-31", "", "", "Calibration weight", "0");                // row 26 ; data_str_arr[25]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".3", "IDevice_Values.status", "data_str[26]", "Output 4", "0", "48", "Zero load", "S32", ".0-31", "", "", "Zero load", "0");                                  // row 27 ; data_str_arr[26]
                dataGridView1.Rows.Add("", "", "", "", "", "", "", "", "50", "Nominal load", "S32", ".0-31", "", "", "Nominal load", "0");                                                                                           // row 28 ; data_str_arr[27]    

                dataGridView1.Rows.Add("8", "Limit value 1", "Bit", ".0", "IDevice_Values.status", "data_str[27]", "Limit value 1", "0", "-", "-", "-", "-", "-", "-", "-", "-");           // row 29 ; data_str_arr[28]
                dataGridView1.Rows.Add("8", "Limit value 2", "Bit", ".1", "IDevice_Values.status", "data_str[28]", "Limit value 2", "0", "-", "-", "-", "-", "-", "-", "-", "-");           // row 30 ; data_str_arr[29]
                dataGridView1.Rows.Add("8", "Limit value 3", "Bit", ".2", "IDevice_Values.status", "data_str[29]", "Limit value 3", "0", "-", "-", "-", "-", "-", "-", "-", "-");           // row 31 ; data_str_arr[30]
                dataGridView1.Rows.Add("8", "Limit value 4", "Bit", ".3", "IDevice_Values.status", "data_str[30]", "Limit value 4", "0", "-", "-", "-", "-", "-", "-", "-", "-");           // row 32 ; data_str_arr[31]
                dataGridView1.Rows.Add("9", "Weight memory, Day", "Int16", ".0-15", "IDevice_Values.status", "data_str[31]", "Stored value for day", "0", "-", "-", "-", "-", "-", "-", "-", "-");                      // row 33 ; data_str_arr[32]
                dataGridView1.Rows.Add("10", "Weight memory, Month", "Int16", ".0-15", "IDevice_Values.status", "data_str[32]", "Stored value for month", "0", "-", "-", "-", "-", "-", "-", "-", "-");                 // row 34 ; data_str_arr[33]
                dataGridView1.Rows.Add("11", "Weight memory, Year", "Int16", ".0-15", "IDevice_Values.status", "data_str[33]", "Stored value for year", "0", "-", "-", "-", "-", "-", "-", "-", "-");                   // row 35 ; data_str_arr[34]
                dataGridView1.Rows.Add("12", "Weight memory, Seq...number", "Int16", ".0-15", "IDevice_Values.status", "data_str[34]", "Stored value for seq.number", "0", "-", "-", "-", "-", "-", "-", "-", "-");     // row 36 ; data_str_arr[35]
                dataGridView1.Rows.Add("13", "Weight memory, gross", "Int16", ".0-15", "IDevice_Values.status", "data_str[35]", "Stored gross value", "0", "-", "-", "-", "-", "-", "-", "-", "-");                     // row 37 ; data_str_arr[36]
                dataGridView1.Rows.Add("14", "Weight memory, net", "Int16", ".0-15", "IDevice_Values.status", "data_str[36]", "Stored net value", "0", "-", "-", "-", "-", "-", "-", "-", "-");                         // row 38 ; data_str_arr[37]
            }            
            if (this.is_standard==false/*WTX_obj.application_mode == 2 || WTX_obj.application_mode==1*/) // case 2) Filler application. Initializing the description and a placeholder for the values in the data grid.
            {
                dataGridView1.Rows.Add("0+2", "Measured Value", "Int32", "32Bit", "IDevice_Values.NetandGrossValue", "data_str[0]", "Net and gross measured", "0", "0", "Control word", "Bit", ".0", "Taring", "0");             // row 1 ; data_str_arr[0]
                dataGridView1.Rows.Add("0", "Measured Value", "Int32", "32Bit", "IDevice_Values.NetValue", "data_str[1]", "Net measured", "0", "0", "Control word", "Bit", ".1", "Gross/Net", "0");                              // row 2 ; data_str_arr[1]
                dataGridView1.Rows.Add("2", "Measured Value", "Int32", "32Bit", "IDevice_Values.GrossValue", "data_str[2]", "Gross measured", "0", "0", "Control word", "Bit", ".2", "Clear dosing results0", "0");              // row 3 ; data_str_arr[2]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".0", "IDevice_Values.general_weight_error", "data_str[3]", "General weight error", "0", "0", "Control word", "Bit", ".3", "Abort dosing", "0");       // row 4 ; data_str_arr[3]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".1", "IDevice_Values.scale_alarm_triggered", "data_str[4]", "Scale alarm(s) triggered", "0", "0", "Control word", "Bit", ".4", "Start dosing", "0");  // row 5 ; data_str_arr[4]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".2-3", "IDevice_Values.limit_status", "data_str[5]", "Limit status", "0", "0", "Control word", "Bit", ".6", "Zeroing", "0");                         // row 6 ; data_str_arr[5]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".4", "IDevice_Values.weight_moving", "data_str[6]", "Weight moving", "0", "0", "Control word", "Bit", ".7", "Adjust zero", "0");                      // row 7 ; data_str_arr[6]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".5", "IDevice_Values.scale_seal_is_open", "data_str[7]", "Scale seal is open", "0", "0", "Control word", "Bit", ".8", "Adjust nominal", "0");         // row 8 ; data_str_arr[7]

                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".6", "IDevice_Values.manual_tare", "data_str[8]", "Manual tare", "0", "0", "Control word", "Bit", ".11", "Activate data", "0");                       // row 9 ;  data_str_arr[8]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".7", "IDevice_Values.weight_type", "data_str[9]", "Weight type", "0", "0", "Control word", "Bit", ".14", "Weight storage", "0");                      // row 10 ; data_str_arr[9]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bits", ".8-9", "IDevice_Values.scale_range", "data_str[10]", "Scale range", "0", "0", "Control word", "Bit", ".15", "Manual re-dosing", "0");                // row 11 ; data_str_arr[10]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".10", "IDevice_Values.zero_required", "data_str[11]", "Zero required", "0", "1", "Residual flow time", "U16", ".0-15", "", "0");                      // row 12 ; data_str_arr[11]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".11", "IDevice_Values.weight_within_the_center_of_zero", "data_str[12]", "Weight within the center of zero", "0", "2", "Filling weight", "S32", ".0-31", "", "0");   // row 13 ; data_str_arr[12]
                dataGridView1.Rows.Add("4", "DS461-Weight status", "Bit", ".12", "IDevice_Values.weight_in_zero_range", "data_str[13]", "Weight in zero range", "0", "4", "Coarse flow cut-off point", "S32", ".0-31", "", "0");                // row 14 ; data_str_arr[13]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".0-1", "IDevice_Values.application_mode", "data_str[14]", "Application mode", "0", "6", "Fine flow cut-off point", "S32", ".0-31", "", "0");                      // row 15 ; data_str_arr[14]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".4-6", "IDevice_Values.decimals", "data_str[15]", "Decimals", "0", "8", "Minimum fine flow", "S32", ".0-31", "", "0");                                            // row 16 ; data_str_arr[15]
                dataGridView1.Rows.Add("5", "Measured value status", "Bits", ".7-8", "IDevice_Values.unit", "data_str[16]", "Unit", "0", "10", "Optimization of cut-off points", "U08", ".0-7", "", "0");       // row 17 ; data_str_arr[16]
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".14", "IDevice_Values.handshake", "data_str[17]", "Handshake", "0", "11", "Maximum dosing time", "U16", ".0-15", "", "0");         // row 18 ; data_str_arr[17]
                dataGridView1.Rows.Add("5", "Measured value status", "Bit", ".15", "IDevice_Values.status", "data_str[18]", "Status", "0", "12", "Start with fine flow", "U16", ".0-15", "", "0");              // row 19 ; data_str_arr[18]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".0", "IDevice_Values.status", "data_str[19]", "Input 1", "0", "13", "Coarse lockout time", "U16", ".0-15", "", "0");                      // row 20 ; data_str_arr[19] 
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".1", "IDevice_Values.status", "data_str[20]", "Input 2", "0", "14", "Fine lockout time", "U16", ".0-35", "", "0");                        // row 21 ; data_str_arr[20]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".2", "IDevice_Values.status", "data_str[21]", "Input 3", "0", "15", "Tare mode", "U08", ".0-7", "", "0");                                 // row 22 ; data_str_arr[21]
                dataGridView1.Rows.Add("6", "Digital inputs", "Bit", ".3", "IDevice_Values.status", "data_str[22]", "Input 4", "0", "16", "Tolerance limit +", "S32", ".0-31", "", "0");                        // row 23 ; data_str_arr[22]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".0", "IDevice_Values.status", "data_str[23]", "Output 1", "0", "18", "Tolerance limit -", "S32", ".0-31", "", "0");                      // row 24 ; data_str_arr[23]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".1", "IDevice_Values.status", "data_str[24]", "Output 2", "0", "20", "Minimum start weight", "S32", ".0-31", "", "0");                   // row 25 ; data_str_arr[24]
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".2", "IDevice_Values.status", "data_str[25]", "Output 3", "0", "22", "Empty weight", "S32", ".0-31", "", "0");                           // row 26 ; data_str_arr[25] 
                dataGridView1.Rows.Add("7", "Digital outputs", "Bit", ".3", "IDevice_Values.status", "data_str[26]", "Output 4", "0", "24", "Tare", "U16", ".0-35", "", "0");                                   // row 27 ; data_str_arr[26]
                dataGridView1.Rows.Add("", "", "", "", "", "", "", "", "25", "Coarse flow monitoring time", "U16", ".0-15", "", "0");                                                                           // row 28 ; data_str_arr[27]

                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".0", "IDevice_Values.status", "data_str[27]", "Coarse flow", "0", "26", "Coarse flow monitoring", "U32", ".0-31", "", "0");                // row 29 ; data_str_arr[28]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".1", "IDevice_Values.status", "data_str[28]", "Fine flow", "0", "28", "Fine flow monitoring", "U32", ".0-31", "", "0");                    // row 30 ; data_str_arr[29]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".2", "IDevice_Values.status", "data_str[29]", "Ready", "0", "30", "Fine flow monitoring time", "U16", ".0-15", "", "0");                   // row 31 ; data_str_arr[30]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".3", "IDevice_Values.status", "data_str[30]", "Re-dosing", "0", "31", "Delay time after fine flow", "U08", ".0-7", "", "0");               // row 32 ; data_str_arr[31]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".4", "IDevice_Values.status", "data_str[31]", "Emptying", "0", "32", "Activation time after fine flow", "U08", ".0-7", "", "0");           // row 33 ; data_str_arr[32]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".5", "IDevice_Values.status", "data_str[32]", "Flow error", "0", "34", "Systematic difference", "U32", ".0-31", "", "0");                  // row 34 ; data_str_arr[33]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".6", "IDevice_Values.status", "data_str[33]", "Alarm", "0", "36", "Downwards dosing", "U08", ".0-7", "", "0");                             // row 35 ; data_str_arr[34]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".7", "IDevice_Values.status", "data_str[34]", "ADC-Overload/Underload", "0", "38", "Valve control", "U08", ".0-7", "", "0");               // row 36 ; data_str_arr[35]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".8", "IDevice_Values.status", "data_str[35]", "Max. Dosing time", "0", "40", "Emptying mode", "U08", ".0-7", "", "0");                     // row 37 ; data_str_arr[36]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".9", "IDevice_Values.status", "data_str[36]", "Legal-for-trade operation", "0", "46", "Calibration weight", "S32", ".0-31", "", "0");      // row 38 ; data_str_arr[37]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".10", "IDevice_Values.status", "data_str[37]", "Tolerance error +", "0", "48", "Zero load", "S32", ".0-31", "", "0");                      // row 39 ; data_str_arr[38]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".11", "IDevice_Values.status", "data_str[38]", "Tolerance error -", "0", "50", "Nominal load", "S32", ".0-31", "", "0");                   // row 40 ; data_str_arr[39]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".14", "IDevice_Values.status", "data_str[39]", "Status digital input 1", "0", "-", "-", "-", "-", "-", "-", "-", "-");                     // row 41 ; data_str_arr[40]
                dataGridView1.Rows.Add("8", "Dosing status", "Bit", ".15", "IDevice_Values.status", "data_str[40]", "General scale error", "0", "-", "-", "-", "-", "-", "-", "-", "-");                        // row 42 ; data_str_arr[41]
                dataGridView1.Rows.Add("9", "Dosing process status", "U16", ".0-15", "IDevice_Values.status", "data_str[41]", "Initializing,Pre-dosing to Analysis", "0");                                      // row 43 ; data_str_arr[42]
                dataGridView1.Rows.Add("11", "Dosing count", "U16", ".0-15", "IDevice_Values.status", "data_str[42]", " ", "0", "-", "-", "-", "-", "-", "-", "-", "-");                                        // row 44 ; data_str_arr[43]

                dataGridView1.Rows.Add("12", "Dosing result", "S32", ".0-31", "IDevice_Values.status", "data_str[43]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                          // row 45 ; data_str_arr[44]
                dataGridView1.Rows.Add("14", "Mean value of dosing results", "S32", ".0-31", "IDevice_Values.status", "data_str[44]", " ", "0", "-", "-", "-", "-", "-", "-", "-");           // row 46 ; data_str_arr[45]
                dataGridView1.Rows.Add("16", "Standard deviation", "S32", ".0-31", "IDevice_Values.status", "data_str[45]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                     // row 47 ; data_str_arr[46]
                dataGridView1.Rows.Add("18", "Total weight", "S32", ".0-31", "IDevice_Values.status", "data_str[46]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                           // row 48 ; data_str_arr[47]
                dataGridView1.Rows.Add("20", "Fine flow cut-off point", "S32", ".0-31", "IDevice_Values.status", "data_str[47]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                // row 49 ; data_str_arr[48]
                dataGridView1.Rows.Add("22", "Coarse flow cut-off point", "S32", ".0-31", "IDevice_Values.status", "data_str[48]", " ", "0", "-", "-", "-", "-", "-", "-", "-");              // row 50 ; data_str_arr[49]
                dataGridView1.Rows.Add("24", "Actual dosing time", "U16", ".0-15", "IDevice_Values.status", "data_str[49]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                     // row 51 ; data_str_arr[50]
                dataGridView1.Rows.Add("25", "Actual coarse flow time", "U16", ".0-15", "IDevice_Values.status", "data_str[50]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                // row 52 ; data_str_arr[51]
                dataGridView1.Rows.Add("26", "Actual fine flow time", "U16", ".0-15", "IDevice_Values.status", "data_str[51]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                  // row 53 ; data_str_arr[52]
                dataGridView1.Rows.Add("27", "Parameter set (product)", "U08", ".0-7", "IDevice_Values.status", "data_str[52]", " ", "0", "-", "-", "-", "-", "-", "-", "-");                 // row 54 ; data_str_arr[53]

                dataGridView1.Rows.Add("32", "Weight memory, Day", "Int16", ".0-15", "IDevice_Values.status", "data_str[53]", "Stored value for day", "0", "-", "-", "-", "-", "-", "-", "-");                   // row 55 ; data_str_arr[54]
                dataGridView1.Rows.Add("33", "Weight memory, Month", "Int16", ".0-15", "IDevice_Values.status", "data_str[54]", "Stored value for month", "0", "-", "-", "-", "-", "-", "-", "-");               // row 56 ; data_str_arr[55]
                dataGridView1.Rows.Add("34", "Weight memory, Year", "Int16", ".0-15", "IDevice_Values.status", "data_str[55]", "Stored value for year", "0", "-", "-", "-", "-", "-", "-", "-");                 // row 57 ; data_str_arr[56]
                dataGridView1.Rows.Add("35", "Weight memory, Seq number", "Int16", ".0-15", "IDevice_Values.status", "data_str[56]", "Stored value for seq.number", "0", "-", "-", "-", "-", "-", "-", "-");     // row 58 ; data_str_arr[57]
                dataGridView1.Rows.Add("36", "Weight memory, gross", "Int16", ".0-15", "IDevice_Values.status", "data_str[57]", "Stored gross value", "0", "-", "-", "-", "-", "-", "-", "-");                   // row 59 ; data_str_arr[58]
                dataGridView1.Rows.Add("37", "Weight memory, net", "Int16", ".0-15", "IDevice_Values.status", "data_str[58]", "Stored net value", "0", "-", "-", "-", "-", "-", "-", "-");                       // row 60 ; data_str_arr[59]

            }
            label1.Text = "Only for Standard application:";       // label for information : Only output words for standard application
            label2.Text = "Only for Filler application:";         // label for information : Only output words for filler application 

            dataGridView1.Columns[4].Width = 250;                 // Width of the fourth column containing the periodically updated values.           
            
            try
            {
                this.ShowDialog(); // Shows the form as a modal dialog box.
            }
            catch (Exception y)
            {
                if (y is System.ArgumentException ||
                    y is System.ArgumentOutOfRangeException ||
                    y is System.NullReferenceException)

                    Console.WriteLine("System.NullReferenceException, System.ArgumentOutOfRangeException or System.ArgumentException has been thrown in timer1_tick");
            }
        }

        // This automatic property returns an instance of this class. It has usage in the class "Settings_Form".
        public WTX120 get_dataviewer
        {
            get
            {
                return this.WTX_obj;
            }
        }

        // This private method is called for initializing basic information for the tool menu bar on the bottom of the windows form: 
        // For the connection status, IP adress, application mode and number of inputs. 
        private void GUI_Load(object sender, EventArgs e)
        {
            if (WTX_obj.get_Modbus.Is_connected() == true)
                toolStripStatusLabel1.Text = "Connected";
            else
                toolStripStatusLabel1.Text = "Disconnected";

            toolStripStatusLabel2.Text = "IP adress: " + WTX_obj.get_Modbus.IP_Adress;
            toolStripStatusLabel3.Text = "Mode : " + this.data_str_arr[14]; // index 14 refers to application mode of the Device
            toolStripStatusLabel5.Text = "Number of Inputs : " + this.WTX_obj.get_Modbus.NumOfPoints; 
        }

        // This method implements a timer triggering an event in user-defined intervals (here: timer1.Inteval,sending_interval).
        // This timer is only useable in C# Windows forms applications (System.Windows.Forms.Timer). There is another on for 
        // for non-Windows forms application in class System.Timers.Timer (Not for Windows Forms, but for the console application) .
        // This method checks first, whether a connection to the device exists. If so, the data array for the previous timer event 
        // is written and afterwards the new data will be updated by a call of the asynchronous method "WTX_obj.Async_Call(command,callback_method)". 
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (WTX_obj.get_Modbus.Is_connected() == true)      // Checks whether a connections exists.
            {
                for (int i = 0; i < WTX_obj.get_data_ushort.Length; i++)
                {
                    this.previous_data_ushort_arr[i] = WTX_obj.get_data_ushort[i]; //the data array for the previous timer1_Tick event is written. (as soon as one timer interval is elapsed) 
                }
            }

            WTX_obj.Async_Call(0x00, Read_DataReceived);    // The new data will be updated by a call of the asynchronous method. "Read_DataReceived" is the 
                                                            // callback method being called once the register of the device have been read. 
        }

        // If the data should be read, this is the callback method for the asynchronous call in method timer1_Tick.
        // This method will be called once the register is read and will write the updated values on the GUI (Windows Form). 
        // It checks whether the values has been changed. Only if the value changes the GUI will be updated.
        public void Read_DataReceived(IDeviceValues Device_Values)
        {
            compare_test = true ;       // for every iteration of this method, compare_test has to be "true" in the beginning. 

            for (int i = 0; i < WTX_obj.get_data_ushort.Length; i++)
            {
                // If one value of the data changes, the boolean value "compare_test" will be set to
                // false and the data array "data_str_arr" will be updated in the following, as well as the GUI form.
                // ("compare_test" is for the purpose of comparision.)
                if(WTX_obj.get_data_ushort[i] != this.previous_data_ushort_arr[i])
                {
                    compare_test = false;
                }
            }

            // If the data is unequal to the previous one, the array "data_str_arr" will be updated in the following, as well as the GUI form. 
            if (this.compare_test==false)
            {
                this.data_str_arr = Device_Values.get_data_str;
                reset_values();
            }

            // In form "Settings_Form" you are able to change also the "NumInputs"(Number of the inputs). Having less "NumInputs" it is possible,
            // that the number of byte read out is not high enough to check the application mode.  
            // If the number of bytes is high enough for the following if-condition(Application mode requires byte 14),
            // the application mode can be checked. 

            if (this.WTX_obj.get_Modbus.NumOfPoints > 14 && this.previous_data_ushort_arr.Length > 14) 
            // Second if-condition: Only if the length of the previous data array is larger than 14, it will be possible to access values otherwise you get an "ArrayoutofBounds"-Exception 
            {

                if (this.previous_data_ushort_arr[14] != Device_Values.get_data_ushort[14]) // Checks whether the application mode has been changed. If so, the output of the GUI has to be reset. 
                {
                    if (Device_Values.application_mode == 0)
                       // Checks whether the WTX device is in standard application mode (application=0) or in filler application mode (application=2). 
                        this.is_standard = true;
                    else
                        if(Device_Values.application_mode == 2 || Device_Values.application_mode == 1)
                            this.is_standard = false;



                    // If the upper if-conditions are complied, the data grid will actualized, reset and written onto the GUI, because
                    // the decriptions and values differ between standard and filler application (see page 154-161 in manual for the different values
                    // and description).

                    dataGridView1.Columns.Clear();
                    dataGridView1.Rows.Clear();
                    this.set_GUI_rows();

                }
            }
        }

        // This method actualizes and resets the data grid with newly calculated values of the previous iteration. 
        // First it actualizes the tool bar menu regarding the status of the connection, afterwards it iterates the 
        // "data_str_arr"-Array to actualize every element of the data grid in the standard or filler application. 
        public void reset_values()
        {
            if (WTX_obj.get_Modbus.Is_connected() == true)
                toolStripStatusLabel1.Text = "Connected";
            else
                toolStripStatusLabel1.Text = "Disconnected";

            toolStripStatusLabel2.Text = "IP adress: " + WTX_obj.get_Modbus.IP_Adress;
            toolStripStatusLabel3.Text = "Mode : " + this.data_str_arr[14];                 // index 14 refers to application mode of the Device
            toolStripStatusLabel2.Text = "IP adress: " + WTX_obj.get_Modbus.IP_Adress;

            //Changing the width of a column:
            /*foreach (DataGridViewTextBoxColumn c in dataGridView1.Columns)
                c.Width = 120;*/

            for (int index = 0; index <= 26; index++) // Up to index 26, the input words are equal in standard and filler application.                           
                dataGridView1.Rows[index].Cells[7].Value = data_str_arr[index];

            if (WTX_obj.application_mode == 0)             // In the standard application: 
            {
                this.is_standard = true;
                for (int index = 27; index <= 35; index++)
                    dataGridView1.Rows[(index + 1)].Cells[7].Value = data_str_arr[index];
            }
            else
            if (WTX_obj.application_mode == 1 || WTX_obj.application_mode == 2)   // In the filler application: 
            {
                this.is_standard = false;
                for (int index = 27; index < 59; index++)
                {
                        dataGridView1.Rows[(index + 1)].Cells[7].Value = data_str_arr[index];
                }
            }

            // Changing the width of a row in every iteration -> That would also mean a  loss of performance.
            // for (int index = 0; index < dataGridView1.Rows.Count; index++)
            //     dataGridView1.Rows[index].Height = 15;

        }

        // Button-Click event to close the application: 
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Stop();
            this.Close();
        }

        // This is the callback method for writing. First the values in "data_str_arr" are updated and 
        // the GUI is actualized.
        // A asynchronous call is used in the following button_Click methods. 
        // The callback method is Write_DataReceived, which is called once the command is written into the register of the device. 
        public void Write_DataReceived(IDeviceValues Device_Values)
        {
            this.data_str_arr = Device_Values.get_data_str;
            this.reset_values();
        }

        // This method sends a command to the device : Taring. Command : 0x1       
        // For standard and filler application.
        private void button4_Click(object sender, EventArgs e)
        {
            // Taring
            WTX_obj.Async_Call(0x1, Write_DataReceived);
        }

        // This method sends a command to the device : Change between gross and net value. Command : 0x2 
        // For standard and filler application.
        private void button1_Click(object sender, EventArgs e)
        {
            // Gross/Net
            WTX_obj.Async_Call(0x2, Write_DataReceived);  
        }

        // This method sends a command to the device : Zeroing. Command : 0x40
        // For standard and filler application.
        private void button5_Click(object sender, EventArgs e)
        {
            // Zeroing
            WTX_obj.Async_Call(0x40, Write_DataReceived);
        }

        // This method sends a command to the device : Adjust zero. Command : 0x80
        // For standard and filler application.
        private void button6_Click(object sender, EventArgs e)
        {
            // Adjust zero
            WTX_obj.Async_Call(0x80, Write_DataReceived);
        }

        // This method sends a command to the device : Adjust nominal. Command : 0x100
        // For standard and filler application.
        private void button7_Click(object sender, EventArgs e)
        {
            // Adjust nominal
            WTX_obj.Async_Call(0x100, Write_DataReceived);
        }

        // This method sends a command to the device : Activate data. Command : 0x800
        // For standard and filler application.
        private void button8_Click(object sender, EventArgs e)
        {
            // Activate data
            WTX_obj.Async_Call(0x800, Write_DataReceived);
        }

        // This method sends a command to the device : Manual taring. Command : 0x1000
        // Only for standard application.
        private void button9_Click(object sender, EventArgs e)
        {
            // Manual taring
            //if (this.is_standard == true)      // Activate this if-conditon only in case, if the should be a change between standard and filler application. 
            WTX_obj.Async_Call(0x1000, Write_DataReceived);             
        }

        // This method sends a command to the device : Weight storage. Command : 0x4000
        // For standard and filler application.
        private void button10_Click(object sender, EventArgs e)
        {
            // Weight storage
            WTX_obj.Async_Call(0x4000, Write_DataReceived);
        }

        // This method sends a command to the device : Clear dosing results. Command : 0x4
        // Only for filler application.
        private void button11_Click(object sender, EventArgs e)
        {
            // Clear dosing results
            //if (this.is_standard == false)
            WTX_obj.Async_Call(0x4, Write_DataReceived);
        }

        // This method sends a command to the device : Abort dosing. Command : 0x8
        // Only for filler application.
        private void button12_Click(object sender, EventArgs e)
        {
            // Abort dosing
            //if (this.is_standard == false)
            WTX_obj.Async_Call(0x8, Write_DataReceived);
        }

        // This method sends a command to the device : Start dosing. Command : 0x10
        // Only for filler application.
        private void button13_Click(object sender, EventArgs e)
        {
            // Start dosing
            //if (this.is_standard == false)
            WTX_obj.Async_Call(0x10, Write_DataReceived);
        }

        // This method sends a command to the device : Manual re-dosing. Command : 0x8000
        // Only for filler application.
        private void button14_Click(object sender, EventArgs e)
        {
            // Manual re-dosing
            //if (this.is_standard == false)
            WTX_obj.Async_Call(0x8000, Write_DataReceived);
        }

        // This event starts the timer and the periodical fetch of values from the device (here: WTX120).
        // The timer interval is set in the connection specific class "Modbus_TCP".
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WTX_obj.get_Modbus.Connect();   // First the connection to the device should be established.             

            timer1.Enabled = true;
            timer1.Interval = WTX_obj.get_Modbus.Sending_interval; // the timer interval(Sending_interval) is set in class "Modbus_TCP".
            timer1.Start();                                            // The timer is started. 
        }

        // This method stops the timer after the corresponding event has been triggered during the application.
        // Afterwards the timer and the application can be restarted.
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Stop();
        }

        // This method stops the timer and exits the application after the corresponding event has been triggered during the application.
        // Afterwards the timer and the application can not be restarted.
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Stop();
            this.Close();
            Application.Exit();
        }

        // This method saves the values from the GUI in the actual iteration in an extra word file: 
        private void saveInputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;    // Stop the timer. 
            timer1.Stop();           

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
            // Restart the timer.
            timer1.Enabled = true;
            timer1.Interval = WTX_obj.get_Modbus.Sending_interval;    
            timer1.Start();

        }

        // This method starts the timer and it is used by class Settings_Form to restart the timer once it is stopped. 
        // See class "Settings_Form" in method "button2_Click(sender,e)".
        public void timer1_start()
        {
            timer1.Enabled = true;
            timer1.Interval = WTX_obj.get_Modbus.Sending_interval;     
            timer1.Start();
        }

        // This method is used to call another form ("Settings_Form") once the corresponding event is triggerred.
        // It is used to change the connection specific attributes, like IP adress, number of inputs and sending/timer interval.
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;     // Stop the timer (Restart is in Class "Settings_Form").
            timer1.Stop();
                  
            Set_obj = new SettingsForm(WTX_obj.get_Modbus.IP_Adress, this.timer1.Interval, WTX_obj.get_Modbus.NumOfPoints, this);
            Set_obj.Show();
        }

        // This method updates the values of the connection(IP adress, timer/sending interval, number of inputs), set in class "Settings_Form".
        // See class "Setting_Form" in method button2_Click(sender,e).
        // After updating the values the tool bar labels on the bottom (f.e. "toolStripStatusLabel2") is rewritten with the new values. 
        public void setting()
        {
            WTX_obj.get_Modbus.IP_Adress = Set_obj.get_IP_address;
            toolStripStatusLabel2.Text = "IP adress: " + WTX_obj.get_Modbus.IP_Adress;

            WTX_obj.get_Modbus.Sending_interval = Set_obj.get_sending_interval;     
            this.timer1.Interval = Set_obj.get_sending_interval;          

            WTX_obj.get_Modbus.NumOfPoints = Set_obj.get_number_inputs;
            toolStripStatusLabel5.Text = "Number of Inputs : " + this.WTX_obj.get_Modbus.NumOfPoints;
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
        
        // This method returns the data array with type ushort from the previous iteration. 
        public ushort[] previous_data
        {
            get
            {
                return this.previous_data_ushort_arr;
            }
        }

    }
}