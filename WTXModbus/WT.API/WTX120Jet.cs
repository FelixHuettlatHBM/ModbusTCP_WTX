using System;
using Hbm.Devices.WTXModbus;
using Hbm.Wt.CommonNetLib.Utils;
using Hbm.Wt.Connection;

namespace Hbm.Wt.WTXInterface.WTX120Jet
{
    public enum ParameterEnum : uint
    {
        MeasuredValue                = 0x601A/01,
        MeasuredValueStatus          = 0x602001,
        DecimalPoint                 = 0x211003,

        DeviceIdentification         = 0x252001,

    };

    public class ParameterProperty : DeviceAbstract
    {
        private string[] dataStr;
        private ushort[] data;
        
        public ParameterProperty(INetConnection S_Connection) : base(S_Connection)
        {
            dataStr = new string[59];

            data = new ushort[59];
            
            getConnection.RaiseDataEvent += this.UpdateEvent;   // Subscribe to the event.
        }

        public override void Calibration(ushort command) { }
        public override void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e) { }
        //public override void UpdateEvent(object sender, MessageEvent<ushort> e) { }

        public override string[] get_data_str { get { return new string[1]; } set { this.dataStr = value; } }
        public override ushort[] get_data_ushort { get { return new ushort[1]; } set { this.data = value; } }

        public override int NetValue { get { return 1; } }                   // data[1]
        public override int GrossValue { get { return 1; } }                 // data[2]
        public override int general_weight_error { get { return 1; } }       // data[3]
        public override int scale_alarm_triggered { get { return 1; } }      // data[4]
        public override int limit_status { get { return 1; } }               // data[5]
        public override int weight_moving { get { return 1; } }              // data[6]
        public override int scale_seal_is_open { get { return 1; } }         // data[7]
        public override int manual_tare { get { return 1; } }                // data[8]
        public override int weight_type { get { return 1; } }                // data[9]
        public override int scale_range { get { return 1; } }                // data[10]
        public override int zero_required { get { return 1; } }              // data[11]
        public override int weight_within_the_center_of_zero { get { return 1; } }   // data[12]
        public override int weight_in_zero_range { get { return 1; } }               // data[13]
        public override int application_mode { get { return 1; } }           // data[14]
        public override int decimals { get { return 1; } }                   // data[15]
        public override int unit { get { return 1; } }                       // data[16]
        public override int handshake { get { return 1; } }                  // data[17]
        public override int status { get { return 1; } }                     // data[18]

        public override int digital_input_1 { get { return 1; } }            // data[19]
        public override int digital_input_2 { get { return 1; } }            // data[20]
        public override int digital_input_3 { get { return 1; } }            // data[21]
        public override int digital_input_4 { get { return 1; } }            // data[22]
        public override int digital_output_1 { get { return 1; } }           // data[23]
        public override int digital_output_2 { get { return 1; } }           // data[24]
        public override int digital_output_3 { get { return 1; } }           // data[25]
        public override int digital_output_4 { get { return 1; } }           // data[26]

        public override int limit_value_status_1 { get { return 1; } }       // data[27]
        public override int limit_value_status_2 { get { return 1; } }       // data[28]
        public override int limit_value_status_3 { get { return 1; } }       // data[29]
        public override int limit_value_status_4 { get { return 1; } }       // data[30]

        public override int weight_memory_day { get { return 1; } }          // data[31]
        public override int weight_memory_month { get { return 1; } }        // data[32]
        public override int weight_memory_year { get { return 1; } }         // data[33]
        public override int weight_memory_seq_number { get { return 1; } }   // data[34]
        public override int weight_memory_gross { get { return 1; } }        // data[35]
        public override int weight_memory_net { get { return 1; } }          // data[36]

        public override int coarse_flow { get { return 1; } }                // data[37]
        public override int fine_flow { get { return 1; } }                  // data[38]
        public override int ready { get { return 1; } }                      // data[39]
        public override int re_dosing { get { return 1; } }                  // data[40]
        public override int emptying { get { return 1; } }                   // data[41]
        public override int flow_error { get { return 1; } }                 // data[42]
        public override int alarm { get { return 1; } }                      // data[43]
        public override int ADC_overload_underload { get { return 1; } }     // data[44]
        public override int max_dosing_time { get { return 1; } }            // data[45]
        public override int legal_for_trade_operation { get { return 1; } }  // data[46]
        public override int tolerance_error_plus { get { return 1; } }       // data[47]
        public override int tolerance_error_minus { get { return 1; } }      // data[48]
        public override int status_digital_input_1 { get { return 1; } }     // data[49]
        public override int general_scale_error { get { return 1; } }        // data[50]

        public override int dosing_process_status { get { return 1; } }             // data[51]
        public override int dosing_count { get { return 1; } }                      // data[52]
        public override int dosing_result { get { return 1; } }                     // data[53]
        public override int mean_value_of_dosing_results { get { return 1; } }      // data[54]
        public override int standard_deviation { get { return 1; } }                // data[55]
        public override int total_weight { get { return 1; } }                      // data[56]
        public override int fine_flow_cut_off_point { get { return 1; } }           // data[57]
        public override int coarse_flow_cut_off_point { get { return 1; } }         // data[58]
        public override int actual_dosing_time { get { return 1; } }                // data[59]
        public override int actual_coarse_flow_time { get { return 1; } }           // data[60]
        public override int actual_fine_flow_time { get { return 1; } }             // data[61]
        public override int parameter_set { get { return 1; } }                     // data[62]
    }

    /*
    public class ParameterProperty : DeviceAbstract
    {
        private static class ParameterKeys
        {
            public const string MesaureValue = "0x601A/01";

        }
                


        public ParameterProperty (INetConnection connection) : base (connection){ }

        
        public override int MeasureValue { get { return m_Connection.ReadINT(ParameterKeys.MesaureValue); } }
        public override int MeasureValueType { get { return m_Connection.ReadINT(ParameterEnum.MeasuredValueStatus.ToString()); } }

        public override string DeviceIdentification {
            get { return m_Connection.ReadASC(ParameterEnum.DeviceIdentification.ToString()); }
            set { m_Connection.WriteASC(ParameterEnum.DeviceIdentification.ToString(), value); }
        }

        public override int DecimalPonit {
            get { return m_Connection.ReadINT(ParameterEnum.DecimalPoint.ToString()); }
            set { m_Connection.WriteINT(ParameterEnum.DecimalPoint.ToString(), value); }
        }

        public int TestValue {
            get;
        }
    }
    */
}
