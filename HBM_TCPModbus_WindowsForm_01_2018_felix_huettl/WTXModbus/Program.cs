/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
 * 
 * Author : Felix Huettl
 * 
 *  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WTXModbus
{   
    /// <summary>
    /// This is the class, which contains the static main method as an entry point into the application. 
    /// </summary>
 
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles(); //This method enables visual styles for the application. Visual styles are the colors, fonts, and other visual elements that form an operating system theme.
            Application.SetCompatibleTextRenderingDefault(false); // This is the standard setting for text rendering. 
            
            // Initialize an instance of GUI: 
            GUI GUI_form_obj = new GUI();

            // To get a single value from the device in this class: (The values are shown as soon as the GUI is closed after a run of the application) 

            //int single_value_1 = GUI_form_obj.get_dataviewer.NetandGrossValue;
            //Console.WriteLine("\nGUI_form_obj.get_dataviewer.GrossValue: "+single_value_1);

            //string str_single_value_1 = GUI_form_obj.get_dataviewer.get_data_str[0];    // get_data_str[0] is the net and gross value, with comma. 
            //Console.WriteLine("\nDevice_obj_1.GrossValue: " + str_single_value_1);

            // For the other integer values and - // the index of data array in interface for the interpreted values (with comments) :

            /*
            int single_value_2 = GUI_form_obj.get_dataviewer.NetValue;                                    // data[1]
            int single_value_3 = GUI_form_obj.get_dataviewer.GrossValue;                                  // data[2]
            int single_value_4 = GUI_form_obj.get_dataviewer.general_weight_error;                        // data[3]
            int single_value_5 = GUI_form_obj.get_dataviewer.scale_alarm_triggered;                       // data[4]
            int single_value_6 = GUI_form_obj.get_dataviewer.limit_status;                                // data[5]
            int single_value_7 = GUI_form_obj.get_dataviewer.weight_moving;                               // data[6]
            int single_value_8 = GUI_form_obj.get_dataviewer.scale_seal_is_open;                          // data[7]
            int single_value_9 = GUI_form_obj.get_dataviewer.manual_tare;                                 // data[8]
            int single_value_10 = GUI_form_obj.get_dataviewer.weight_type;                                // data[9]
            int single_value_11 = GUI_form_obj.get_dataviewer.scale_range;                                // data[10]
            int single_value_12 = GUI_form_obj.get_dataviewer.zero_required;                              // data[11]
            int single_value_13 = GUI_form_obj.get_dataviewer.weight_within_the_center_of_zero;           // data[12]
            int single_value_14 = GUI_form_obj.get_dataviewer.weight_in_zero_range;                       // data[13]
            int single_value_15 = GUI_form_obj.get_dataviewer.application_mode;                           // data[14]
            int single_value_16 = GUI_form_obj.get_dataviewer.decimals;                                   // data[15]
            int single_value_17 = GUI_form_obj.get_dataviewer.unit;                                       // data[16]
            int single_value_18 = GUI_form_obj.get_dataviewer.handshake;                                  // data[17]
            int single_value_19 = GUI_form_obj.get_dataviewer.status;                                     // data[18]
            */
            // ... and so on... 

            // For the interpreted string values (with comma, or comment) : 

            /*
            string str_single_value_2 = GUI_form_obj.get_dataviewer.get_data_str[1];
            string str_single_value_3 = GUI_form_obj.get_dataviewer.get_data_str[2];
            string str_single_value_4 = GUI_form_obj.get_dataviewer.get_data_str[3];
            string str_single_value_5 = GUI_form_obj.get_dataviewer.get_data_str[4];
            string str_single_value_6 = GUI_form_obj.get_dataviewer.get_data_str[5];
            string str_single_value_7 = GUI_form_obj.get_dataviewer.get_data_str[6];
            string str_single_value_8 = GUI_form_obj.get_dataviewer.get_data_str[7];
            string str_single_value_9 = GUI_form_obj.get_dataviewer.get_data_str[8];
            string str_single_value_10 = GUI_form_obj.get_dataviewer.get_data_str[9];
            string str_single_value_11 = GUI_form_obj.get_dataviewer.get_data_str[10];
            string str_single_value_12 = GUI_form_obj.get_dataviewer.get_data_str[11];
            string str_single_value_13 = GUI_form_obj.get_dataviewer.get_data_str[12];
            string str_single_value_14 = GUI_form_obj.get_dataviewer.get_data_str[13];
            string str_single_value_15 = GUI_form_obj.get_dataviewer.get_data_str[14];
            string str_single_value_16 = GUI_form_obj.get_dataviewer.get_data_str[15];
            string str_single_value_17 = GUI_form_obj.get_dataviewer.get_data_str[16];
            string str_single_value_18 = GUI_form_obj.get_dataviewer.get_data_str[17];
            string str_single_value_19 = GUI_form_obj.get_dataviewer.get_data_str[18];
            string str_single_value_20 = GUI_form_obj.get_dataviewer.get_data_str[19];
            */

            // ... and so on ... 

            // There are much more values available from the device. Please look in the interface IDevice_Values for the other values.
            // The way to call these values in class "Program" is the same as above. 
            // In class GUI you can call the values by the interface IDevice_Values, if there is an instance of the interface IDevice_Values. For example: 
            // 
            // >>   single_value=IDevice_values_instance.application_mode;    <<
            // or
            // >>   single_value=IDevice_values_instance.get_data_str[14];    <<

            // This class is convenient for the a console application (Console application is in work). Instead of using and initializing a Windows form you
            // just use a console application. 
        }
    }
}
