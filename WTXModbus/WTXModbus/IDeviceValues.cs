/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 03/2018
 * 
 * Author : Felix Huettl
 * 
 *  */


using System;
using WTXModbus;

namespace Hbm.Devices.WTXModbus
{
    /// <summary>
    /// This is the interface for the values of the device. For example for the device WTX120. 
    /// The values are given in realtime by the device via the method ReadHoldingRegister() of the ModbusIpMaster. 
    /// The values have to be declared in this interface and initalized in the derived class "WTX120". 
    /// 
    /// For data transfer the entire interface is submitted from the derived class of IDevice_Values to the GUI or console application:
    /// From method Read_Completed(...) in class "WTX120" to method Read_DataReceived(IDevice_Values Device_Values) in class "GUI". 
    /// Furthermore you can access individual values if the interface is known and its derived class is completely implemented by 
    /// for example > IDevice_Values.NetandGrossValue <  or > IDevice_Values.get_data_str[0] > IDevice_Values.get_data_ushort[0] <. 
    /// >
    /// There are 2 more arrays: string[] get_data_str and ushort[] get_data_ushort to sum up all values in an array to simplify 
    /// further operations, like output or conditions.
    /// The Eventhandler "DataUpdateEvent" is triggered once the data read in class ModbusConnection from the WTX device and committed 
    /// to the GUI or consoleapplication "Programm.cs". 
    /// 
    /// Behind the variables the index of the arrays is given. 
    /// </summary>
    public interface IDeviceValues
    {
        event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;   

        string[] get_data_str { get; set; }
        ushort[] get_data_ushort { get; set; }
                
        int NetValue { get; }                   // data[1]
        int GrossValue { get; }                 // data[2]
        int general_weight_error { get; }       // data[3]
        int scale_alarm_triggered { get; }      // data[4]
        int limit_status { get; }               // data[5]
        int weight_moving { get; }              // data[6]
        int scale_seal_is_open { get; }         // data[7]
        int manual_tare { get; }                // data[8]
        int weight_type { get; }                // data[9]
        int scale_range { get; }                // data[10]
        int zero_required { get; }              // data[11]
        int weight_within_the_center_of_zero { get; }   // data[12]
        int weight_in_zero_range { get; }               // data[13]
        int application_mode { get; }           // data[14]
        int decimals { get; }                   // data[15]
        int unit { get; }                       // data[16]
        int handshake { get; }                  // data[17]
        int status { get; }                     // data[18]

        int digital_input_1 { get; }            // data[19]
        int digital_input_2 { get; }            // data[20]
        int digital_input_3 { get; }            // data[21]
        int digital_input_4 { get; }            // data[22]
        int digital_output_1 { get; }           // data[23]
        int digital_output_2 { get; }           // data[24]
        int digital_output_3 { get; }           // data[25]
        int digital_output_4 { get; }           // data[26]

        int limit_value_status_1 { get; }       // data[27]
        int limit_value_status_2 { get; }       // data[28]
        int limit_value_status_3 { get; }       // data[29]
        int limit_value_status_4 { get; }       // data[30]

        int weight_memory_day { get; }          // data[31]
        int weight_memory_month { get; }        // data[32]
        int weight_memory_year { get; }         // data[33]
        int weight_memory_seq_number { get; }   // data[34]
        int weight_memory_gross { get; }        // data[35]
        int weight_memory_net { get; }          // data[36]

        int coarse_flow { get; }                // data[37]
        int fine_flow { get; }                  // data[38]
        int ready { get; }                      // data[39]
        int re_dosing { get; }                  // data[40]
        int emptying { get; }                   // data[41]
        int flow_error { get; }                 // data[42]
        int alarm { get; }                      // data[43]
        int ADC_overload_underload { get; }     // data[44]
        int max_dosing_time { get; }            // data[45]
        int legal_for_trade_operation { get; }  // data[46]
        int tolerance_error_plus { get; }       // data[47]
        int tolerance_error_minus { get; }      // data[48]
        int status_digital_input_1 { get; }     // data[49]
        int general_scale_error { get; }        // data[50]

        int dosing_process_status { get; }             // data[51]
        int dosing_count { get; }                      // data[52]
        int dosing_result { get; }                     // data[53]
        int mean_value_of_dosing_results { get; }      // data[54]
        int standard_deviation { get; }                // data[55]
        int total_weight { get; }                      // data[56]
        int fine_flow_cut_off_point { get; }           // data[57]
        int coarse_flow_cut_off_point { get; }         // data[58]
        int actual_dosing_time { get; }                // data[59]
        int actual_coarse_flow_time { get; }           // data[60]
        int actual_fine_flow_time { get; }             // data[61]
        int parameter_set { get; }                     // data[62]
         
        // Set-properties to set the output words from 9 to 44 
             
        /*
        int Residual_flow_time { set; }             
        int target_filling_weight { set; }
        int coarse_flow_cut_off_pointSet { set; }
        int fine_flow_cut_off_pointSet { set; }
        int minimum_fine_flow { set; }
        int optimization_of_cut_off_points { set; }
        int maximum_dosing_tome { set; }
        int start_with_fine_flow { set; }
        int coarse_lockout_time { set; }
        int fine_lockout_time { set; }
        int tare_mode { set; }
        int upper_tolerance_limit { set; }
        int lower_tolerance_limit { set; }
        int minimum_start_weight { set; }
        int empty_weight { set; }
        int tare_delay { set; }
        int coarse_flow_monitoring_time { set; }
        int coarse_flow_monitoring { set; }
        int fine_flow_monitoring { set; }
        int fine_flow_monitoring_time { set; }
        int delay_time_after_fine_flow { set; }
        int activation_time_after_fine_flow { set; }
        int systematic_difference { set; }
        int downards_dosing { set; }
        int valve_control { set; }
        int emptying_mode { set; }
        */

        // Set-properties to set the output words 46, 48, 50 for the calibration:

        int calibration_weight { set; }
        int zero_load { set; }
        int nominal_load { set; }
    }
}
