using Hbm.Devices.WTXModbus;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WTXModbus
{
    public abstract class DeviceAbstract : IDeviceValues
    {
        private Action<IDeviceValues> callback_obj;
    
        private ushort command;

        private string ipAddr;

        private ModbusConnection ModbusConnObj;
        //private JetBusConnection JetConnObj;

        private int timeoutMS;
        private bool inputModbusJet;

        //private INetCommunication<uint, JToken> commObj;

        public DeviceAbstract(ModbusConnection connection, int paramTimerInterval)
        {
            /*
            inputModbusJet = true;
            timeoutMS = 5000;

            this.ipAddr = "172.19.103.8";

            this.ModbusConnObj = new ModbusConnection(ipAddr);
            */

            /*
            if (inputModbusJet == true)
            { 
                this.ModbusConnObj = new ModbusConnection(ipAddr);
                }
            else
                if (inputModbusJet == false)
                {
                IJetConnection IJetObj = new WebSocketJetConnection(ipAddr, delegate { return true; });      // Unter Umständen die Certification Callback ausimplementieren. 
                JetPeer jetObj = new JetPeer(IJetObj);                                                       // Certification Callbackmethode in API verpackt? Oder Nutzer selbst implementieren? Machen wir! Erstmal als delegate -> true. 

                this.JetConnObj = new JetBusConnection(jetObj, timeoutMS);
            }
            */
        }

        public abstract ModbusConnection getConnection { get; }
        public abstract IDeviceValues DeviceValues { get; }

        public abstract event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;

        public abstract void initialize_timer(int timer_interval);
        
        public abstract void Calibration(ushort command);
        public abstract void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e);

        public abstract string[] get_data_str { get; set; }
        public abstract ushort[] get_data_ushort { get; set; }
      
        public abstract int NetValue { get; }                   // data[1]
        public abstract int GrossValue { get; }                 // data[2]
        public abstract int general_weight_error { get; }       // data[3]
        public abstract int scale_alarm_triggered { get; }      // data[4]
        public abstract int limit_status { get; }               // data[5]
        public abstract int weight_moving { get; }              // data[6]
        public abstract int scale_seal_is_open { get; }         // data[7]
        public abstract int manual_tare { get; }                // data[8]
        public abstract int weight_type { get; }                // data[9]
        public abstract int scale_range { get; }                // data[10]
        public abstract int zero_required { get; }              // data[11]
        public abstract int weight_within_the_center_of_zero { get; }   // data[12]
        public abstract int weight_in_zero_range { get; }               // data[13]
        public abstract int application_mode { get; }           // data[14]
        public abstract int decimals { get; }                   // data[15]
        public abstract int unit { get; }                       // data[16]
        public abstract int handshake { get; }                  // data[17]
        public abstract int status { get; }                     // data[18]
        
        public abstract int digital_input_1 { get; }            // data[19]
        public abstract int digital_input_2 { get; }            // data[20]
        public abstract int digital_input_3 { get; }            // data[21]
        public abstract int digital_input_4 { get; }            // data[22]
        public abstract int digital_output_1 { get; }           // data[23]
        public abstract int digital_output_2 { get; }           // data[24]
        public abstract int digital_output_3 { get; }           // data[25]
        public abstract int digital_output_4 { get; }           // data[26]

        public abstract int limit_value_status_1 { get; }       // data[27]
        public abstract int limit_value_status_2 { get; }       // data[28]
        public abstract int limit_value_status_3 { get; }       // data[29]
        public abstract int limit_value_status_4 { get; }       // data[30]

        public abstract int weight_memory_day { get; }          // data[31]
        public abstract int weight_memory_month { get; }        // data[32]
        public abstract int weight_memory_year { get; }         // data[33]
        public abstract int weight_memory_seq_number { get; }   // data[34]
        public abstract int weight_memory_gross { get; }        // data[35]
        public abstract int weight_memory_net { get; }          // data[36]

        public abstract int coarse_flow { get; }                // data[37]
        public abstract int fine_flow { get; }                  // data[38]
        public abstract int ready { get; }                      // data[39]
        public abstract int re_dosing { get; }                  // data[40]
        public abstract int emptying { get; }                   // data[41]
        public abstract int flow_error { get; }                 // data[42]
        public abstract int alarm { get; }                      // data[43]
        public abstract int ADC_overload_underload { get; }     // data[44]
        public abstract int max_dosing_time { get; }            // data[45]
        public abstract int legal_for_trade_operation { get; }  // data[46]
        public abstract int tolerance_error_plus { get; }       // data[47]
        public abstract int tolerance_error_minus { get; }      // data[48]
        public abstract int status_digital_input_1 { get; }     // data[49]
        public abstract int general_scale_error { get; }        // data[50]

        public abstract int dosing_process_status { get; }             // data[51]
        public abstract int dosing_count { get; }                      // data[52]
        public abstract int dosing_result { get; }                     // data[53]
        public abstract int mean_value_of_dosing_results { get; }      // data[54]
        public abstract int standard_deviation { get; }                // data[55]
        public abstract int total_weight { get; }                      // data[56]
        public abstract int fine_flow_cut_off_point { get; }           // data[57]
        public abstract int coarse_flow_cut_off_point { get; }         // data[58]
        public abstract int actual_dosing_time { get; }                // data[59]
        public abstract int actual_coarse_flow_time { get; }           // data[60]
        public abstract int actual_fine_flow_time { get; }             // data[61]
        public abstract int parameter_set { get; }                     // data[62]


    }
}

