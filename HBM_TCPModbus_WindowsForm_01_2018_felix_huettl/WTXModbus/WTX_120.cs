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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTXModbus
{
    /// <summary>
    /// This class is used to receive the data from the class "Modbus_TCP" after it has started an asynchronous call 
    /// by the use of a Backgroundworker. To start an asynchronous call for writing and reading the register of the
    /// device, the timer in the GUI has to be started by an event. If so, the timer demands periodic new data to read,
    /// which will be done asynchronously. 
    /// Furthermore this class processes and interprets data. The array from the device(here: WTX120) will be transformed into
    /// single values, like specified in the interface IDevice_Values. (The methods for this task are declared as private
    /// and named "comment_...".)
    /// 
    /// Furthermore you can access individual values if an object/instance of the class is known by for example
    /// > WTX_120_object.NetandGrossValue <  or > WTX_120_object.get_data_str[0] > WTX_120_object.get_data_ushort[0] <. 
    /// </summary>
    class WTX120 : ICallAsyncEvent, IDeviceValues
    {
        private ushort[] data;
        private ushort[] previous_data;
        private bool is_net;   
        private ushort command;
        private string[] data_str;    
        Action<IDeviceValues> callback_obj;          

        private ModbusTCP pub;

        public WTX120(string WTX, ModbusTCP pub_param)
        {
            this.pub = pub_param;
            
            this.data_str = new string[59]; // 59 is the maximum number of input words for the filler application, 
            this.data = new ushort[59];     // 37 is the maximum number for the standard application, here 59 is initialized for the length of the data arrays. 
            this.command = 0x00;

            pub.RaiseDataEvent += HandleDataEvent;  // Subscribe to the event.

            this.previous_data = new ushort[59];

            for (int i = 0; i < 59; i++)
            {
                this.previous_data[i] = 0x00;
            }
        }

        // This method is called once the register has been read in method ReadRegisterPublishing(MessageEvent e) from class "Modbus_TCP",
        // via "handler(this,e)" this Method HandleDataEvent(sender,e) is called up.
        // In e.Message the data from the register is given and commited to "this.data" (type: ushort[]) .
        // The data will be processed and interpreted by several internal methods, like method "measurement_with_comma" to set the comma into the
        // integer value, or like method "comment_application_mode" to set a 0 to "standard application" and a 2 to "filler application" .
        public void HandleDataEvent(object sender, MessageEvent e)
        {
            if (this.data[0] == 999)
                for (int index = 0; index < this.data.Length; index++)
                {
                    this.data_str[index] = "Booting";
                }

            this.data = e.Message;
            
            this.data_str[0] = this.measurement_with_comma(0);  // 0 equal to "Net and gross measured" as a parameter 
            this.data_str[1] = this.measurement_with_comma(1);  // 1 equal to "Net measured" as a parameter
            this.data_str[2] = this.measurement_with_comma(2);  // 2 equal to "Gross measured" as a parameter

            this.data_str[3] = this.general_weight_error.ToString();
            this.data_str[4] = this.scale_alarm_triggered.ToString();
            this.data_str[5] = this.comment_limit_status();
            this.data_str[6] = this.comment_weight_moving();

            this.data_str[7] = this.scale_seal_is_open.ToString();
            this.data_str[8] = this.manual_tare.ToString();
            this.data_str[9] = this.comment_weight_type();
            this.data_str[10] = this.comment_scale_range();

            this.data_str[11] = this.zero_required.ToString();
            this.data_str[12] = this.weight_within_the_center_of_zero.ToString();
            this.data_str[13] = this.weight_in_zero_range.ToString();
            this.data_str[14] = this.comment_application_mode();

            this.data_str[15] = this.decimals.ToString();
            this.data_str[16] = this.comment_unit();
            this.data_str[17] = this.handshake.ToString();
            this.data_str[18] = this.comment_status();

            this.data_str[19] = this.digital_input_1.ToString();
            this.data_str[20] = this.digital_input_2.ToString();
            this.data_str[21] = this.digital_input_3.ToString();
            this.data_str[22] = this.digital_input_4.ToString();

            this.data_str[23] = this.digital_output_1.ToString();
            this.data_str[24] = this.digital_output_2.ToString();
            this.data_str[25] = this.digital_output_3.ToString();
            this.data_str[26] = this.digital_output_4.ToString();

            if (this.application_mode==0)
            {
                this.data_str[27] = this.limit_value_status_1.ToString();
                this.data_str[28] = this.limit_value_status_2.ToString();
                this.data_str[29] = this.limit_value_status_3.ToString();
                this.data_str[30] = this.limit_value_status_4.ToString();

                this.data_str[31] = this.weight_memory_day.ToString();
                this.data_str[32] = this.weight_memory_month.ToString();
                this.data_str[33] = this.weight_memory_year.ToString();
                this.data_str[34] = this.weight_memory_seq_number.ToString();
                this.data_str[35] = this.weight_memory_gross.ToString();
                this.data_str[36] = this.weight_memory_net.ToString();
            }
            else
                if (this.application_mode==2 || this.application_mode==0) // in filler mode 
                {
                this.data_str[27] = this.coarse_flow.ToString();
                this.data_str[28] = this.fine_flow.ToString();
                this.data_str[29] = this.ready.ToString();
                this.data_str[30] = this.re_dosing.ToString();

                this.data_str[31] = this.emptying.ToString();
                this.data_str[32] = this.flow_error.ToString();
                this.data_str[33] = this.alarm.ToString();
                this.data_str[34] = this.ADC_overload_underload.ToString();

                this.data_str[35] = this.max_dosing_time.ToString();
                this.data_str[36] = this.legal_for_trade_operation.ToString();
                this.data_str[37] = this.tolerance_error_plus.ToString();
                this.data_str[38] = this.tolerance_error_minus.ToString();

                this.data_str[39] = this.status_digital_input_1.ToString();
                this.data_str[40] = this.general_scale_error.ToString();
                this.data_str[41] = this.dosing_process_status.ToString();
                this.data_str[42] = this.dosing_count.ToString();

                this.data_str[43] = this.dosing_result.ToString();
                this.data_str[44] = this.mean_value_of_dosing_results.ToString();
                this.data_str[45] = this.standard_deviation.ToString();
                this.data_str[46] = this.total_weight.ToString();

                this.data_str[47] = this.fine_flow_cut_off_point.ToString();
                this.data_str[48] = this.coarse_flow_cut_off_point.ToString();
                this.data_str[49] = this.actual_dosing_time.ToString();
                this.data_str[50] = this.actual_coarse_flow_time.ToString();

                this.data_str[51] = this.actual_fine_flow_time.ToString();
                this.data_str[52] = this.parameter_set.ToString();

                this.data_str[53] = this.filler_weight_memory_day.ToString();
                this.data_str[54] = this.filler_weight_memory_month.ToString();
                this.data_str[55] = this.filler_weight_memory_year.ToString();
                this.data_str[56] = this.filler_weight_memory_seq_number.ToString();
                this.data_str[57] = this.filler_weight_memory_gross.ToString();
                this.data_str[58] = this.filler_weight_memory_net.ToString();
            }
        }

        // This method establishs an asynchronous call to read and to write a register on the device. Therefore a 
        // Backgroundworker is implemented.        
        // The BackgroundWorker class allows to run an operation on a separate thread for reading and writing. That can be 
        // time-consuming operations and cause the user inferface to seem as though it has stopped responding while it runs.
        // So reading and writing is executed in the background by the use of a Backgroundworker. 
        //
        // To set up for a background operation, an event handler for the DoWork event ("DoWorkEventHandler") is added, afterwards 
        // the reading or writing method is called in this event handler (here: "bgWorker.DoWork+=...")
        // To receive notifications of progress updates, the ProgressChanged event can be used (not used here).
        // To receive a notification when the operation is completed, the RunWorkerCompleted event is used(here: "bgWorker.RunWorkerCompleted+=...").

        public void Async_Call(ushort command_param, Action<IDeviceValues> callback_param)
        {
            this.command = command_param;
            this.callback_obj = callback_param;

            BackgroundWorker bgWorker = new BackgroundWorker();   // At the class level, create an instance of the BackgroundWorker class.
            
            bgWorker.WorkerSupportsCancellation = true;  // Specify whether you want the background operation to allow cancellation and to report progress.
            bgWorker.WorkerReportsProgress = true;

            if (this.command == 0x00)       // command=0x00 , read data from register 
            {
                bgWorker.DoWork += new DoWorkEventHandler(this.Read_DoWork);  // To set up for a background operation, an event handler, "DoWorkEventHandler" is added.
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.Read_Completed);  // Create an event handler for the RunWorkerCompleted event (method "Read_Completed"). 
            }
            else  // else , write command into register 
            {
                bgWorker.DoWork += new DoWorkEventHandler(this.Write_DoWork);
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.Write_Completed);
            }

            bgWorker.WorkerReportsProgress = true;
            bgWorker.RunWorkerAsync();
        }
           
        // This method is executed asynchronously in the background for reading the register by a Backgroundworker. 
        // @param : sender - the object of this class. dowork_asynchronous - the argument of the event. 
        public void Read_DoWork(object sender, DoWorkEventArgs dowork_asynchronous)
        {
            dowork_asynchronous.Result = (IDeviceValues)this.read_data((BackgroundWorker)sender); // the private method "this.read_data" in called to read the register in class Modbus_TCP
            // dowork_asynchronous.Result contains all values defined in Interface IDevice_Values.
        }
 
        // This method read the register of the Device(here: WTX120), therefore it calls the method in class Modbus_TCP to read the register. 
        // @return: IDevice_Values - Interface, that contains all values for the device. 
        private IDeviceValues read_data(BackgroundWorker worker)
        {
            pub.ReadRegister();
         
            return this;      
        }

        // Get-Method to return an object of class Modbus_TCP
        public ModbusTCP get_Modbus
        {
            get
            {
                return this.pub;
            }
        }

        // This method is called by the Backgroundworker once the RunWorkerCompletedEventHandler triggers its event:
        // The callback-method is called to return the values of the device (here: WTX120) to the class "GUI".
        // In class GUI the method "Write_DataReceived(IDevice_Values)" for writing and the method "Read_DataReceived(IDevice_Values)"
        // for reading is called and written into the GUI form. 
        public void Read_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            this.callback_obj((IDeviceValues)e.Result);         // Neu : 21.11.2017         Interface übergeben. 
        }

        // This method is executed asynchronously in the background for writing the register by a Backgroundworker. 
        // @param : sender - the object of this class. dowork_asynchronous - The argument of the Event. 
        public void Write_DoWork(object sender, DoWorkEventArgs e)
        {
            // (1) Sending of a command:        
            pub.WriteRegister(this.command);

            while (this.handshake == 0) ;

            // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
            if (this.handshake == 1)
            {
                pub.WriteRegister(0x00);
            }
            while (this.handshake == 1) ;
        }

        // This method is called by the Backgroundworker once the RunWorkerCompletedEventHandler triggers its event:
        // The callback-method is called to return the values of the device (here: WTX120) to the class "GUI".
        // The interface IDevice_Values is returned to the class GUI by committing an instance of this class, which is allowed because this class
        // inherits from IDevice_Values.
        // In class GUI the method "Write_DataReceived(IDevice_Values)" for writing and the method "Read_DataReceived(IDevice_Values Device_Values)"
        // for reading is called and written into the GUI form.   
        public void Write_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            this.callback_obj(this);         // Neu : 21.11.2017         Interface übergeben. 
        }


// The following methods set the specific, single values from the whole array "data".

        // This method sets the net and gross value. Therefore the ushort array
        // data[] is masked and shifted. It has "Integer32"(look on the manual, page xxx) as a Type for data[0] and data[1], 
        // so data[0] has to left-shifted << 16 to add the upper 16bit to the lower 16 bit of data[1]. 
        public int NetandGrossValue
        {
            get
            {
                try
                {   
                    if (this.pub.NumOfPoints > 1)     
                        return (data[1] + (data[0] << 16));
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        
        public int NetValue
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 0)
                        return (this.data[0] << 16);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int GrossValue
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 1)
                        return (data[1] << 32);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int general_weight_error
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return (data[4] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int scale_alarm_triggered
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int limit_status
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0xC) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_moving
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x10) >> 4);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int scale_seal_is_open
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x20) >> 5);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int manual_tare
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x40) >> 6);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_type
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x80) >> 7);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int scale_range
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x300) >> 8);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int zero_required
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x400) >> 10);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_within_the_center_of_zero
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x800) >> 11);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_in_zero_range
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 4)
                        return ((data[4] & 0x1000) >> 12);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int application_mode
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 5)
                        return ((data[5] & 0x3) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int decimals
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 5)
                        return ((data[5] & 0x70) >> 4);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int unit
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 5)
                        return ((data[5] & 0x180) >> 7);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int handshake
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 5)
                        return ((data[5] & 0x4000) >> 14);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0; 
                }
            }
        }
        public int status
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 5)
                        return ((data[5] & 0x8000) >> 15);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
}

        public string[] get_data_str
        {   
            get
            {
                    return this.data_str;
            }
            set
            {
                this.data_str = value;
            }
        }

        public int digital_input_1
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 6)
                        return (data[6] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }


        }
        public int digital_input_2
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 6)
                        return ((data[6] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int digital_input_3
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 6)
                        return ((data[6] & 0x4) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int digital_input_4
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 6)
                        return ((data[6] & 0x8) >> 3);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int digital_output_1
        {
            get
            { 
                try
                {
                    if (this.pub.NumOfPoints > 7)
                    return (data[7] & 0x1);
                else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int digital_output_2
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 7)
                        return ((data[7] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int digital_output_3
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 7)
                        return ((data[7] & 0x4) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int digital_output_4
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 7)
                        return ((data[7] & 0x8) >> 3);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int limit_value_status_1
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return (data[8] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int limit_value_status_2
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int limit_value_status_3
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x4) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int limit_value_status_4
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x8) >> 3);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_memory_day
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 9)
                        return (data[9]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_memory_month
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 10)
                        return (data[10]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_memory_year
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 11)
                        return (data[11]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_memory_seq_number
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 12)
                        return (data[12]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_memory_gross
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 13)
                        return (data[13]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int weight_memory_net
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 14)
                        return (data[14]);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int coarse_flow
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return (data[8] & 0x1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }   
        public int fine_flow
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x2) >> 1);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int ready
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x4) >> 2);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int re_dosing
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x8) >> 3);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int emptying
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x10) >> 4);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int flow_error
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x20) >> 5);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int alarm
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x40) >> 6);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int ADC_overload_underload
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x80) >> 7);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int max_dosing_time
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x100) >> 8);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int legal_for_trade_operation
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x200) >> 9);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int tolerance_error_plus
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x400) >> 10);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int tolerance_error_minus
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x800) >> 11);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int status_digital_input_1
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x4000) >> 14);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int general_scale_error
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 8)
                        return ((data[8] & 0x8000) >> 15);
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int dosing_process_status
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 9)
                        return data[9];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int dosing_count
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 11)
                        return data[11];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int dosing_result
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 12)
                        return data[12];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int mean_value_of_dosing_results
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 14)
                        return data[14];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int standard_deviation
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 16)
                        return data[16];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int total_weight
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 18)
                        return data[18];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int fine_flow_cut_off_point
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 20)
                        return data[20];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int coarse_flow_cut_off_point
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 22)
                        return data[22];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int actual_dosing_time
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 24)
                        return data[24];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int actual_coarse_flow_time
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 25)
                        return data[25];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int actual_fine_flow_time
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 26)
                        return data[26];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int parameter_set
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 27)
                        return data[27];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int filler_weight_memory_day
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 28)
                        return data[28];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int filler_weight_memory_month
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 29)
                        return data[29];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int filler_weight_memory_year
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 30)
                        return data[30];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int filler_weight_memory_seq_number
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 31)
                        return data[31];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int filler_weight_memory_gross
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 32)
                        return data[32];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public int filler_weight_memory_net
        {
            get
            {
                try
                {
                    if (this.pub.NumOfPoints > 33)
                        return data[33];
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }

        public ushort[] get_data_ushort
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }

        public bool get_is_net
        {
            get
            {
                return this.is_net;
            }
         }

        /* In den folgenden Comment-Methoden werden jeweils verschiedene Auswahloptionen mit Fallunterscheidungen
        * betrachtet und je nach Fall eine unterschiedliche Option ausgewählt.
        */
        
        
        // In the following methods the different options for the single integer values are used to define and
        // interpret the value. Finally a string should be returned from the methods to write it onto the GUI Form. 
        
        private string measurement_with_comma(int index)
        {
            int value=1;
            if (index == 0)
                value = this.NetandGrossValue;
            if (index == 1)
                value = this.NetValue;
            if (index == 2)
                value = this.GrossValue;


            if (value.ToString().Length == 5 && value > 0)
               data_str[index] = ("0." + value.ToString());
            else
            if (value.ToString().Length == 4 && value > 0)
                data_str[index] = ("0.0" + value.ToString());
            else
            if (value.ToString().Length == 3 && value > 0)
                data_str[index] = ("0.00" + value.ToString());
            else
            if (value.ToString().Length == 2 && value > 0)
                data_str[index] = ("0.000" + value.ToString());
            else
            if (value.ToString().Length == 1 && value > 0)
                data_str[index] = ("0.0000" + value.ToString());

            else
                switch (this.decimals)
                {
                    case 1:
                        if (value <= -100000) data_str[index] = value.ToString().Insert(6, ".");
                        else if (value > -100000) data_str[index] = value.ToString().Insert(5, ".");
                        break;
                    case 2:
                        if (value <= -100000) data_str[index] = value.ToString().Insert(5, ".");
                        else if (value > -100000) data_str[index] = value.ToString().Insert(4, ".");
                        break;
                    case 3:
                        if (value <= -100000) data_str[index] = value.ToString().Insert(4, ".");
                        else if (value > -100000) data_str[index] = value.ToString().Insert(3, ".");
                        break;
                    case 4:
                        if (value <= -100000) data_str[index] = value.ToString().Insert(3, ".");
                        else if (value > -100000) data_str[index] = value.ToString().Insert(2, ".");
                        break;
                    case 5:
                        if (value <=  -100000) data_str[index] = value.ToString().Insert(2, ".");
                        else if (value > -100000) data_str[index] = value.ToString().Insert(1, ".");
                        break;
                    case 6:
                        if (value <= -100000) data_str[index] = value.ToString().Insert(1, ".");
                        else if (value > -100000) data_str[index] = value.ToString().Insert(0, ".");
                        break;
                    default:
                        Console.WriteLine("error, wrong decimal/comma number.");
                        return data_str[index];
                }

            if (value < 0)
            {
                // If the measurement is negative, less than 0...

                if (value.ToString().Length == 6)
                {
                    data_str[index] = data_str[index].Insert(1, "0");
                }

                if (value.ToString().Length == 5)
                {
                    data_str[index] = data_str[index].Insert(1, "0");
                    data_str[index] = data_str[index].Insert(3, "0");
                }
                if (value.ToString().Length == 4)
                {
                    data_str[index] = data_str[index].Insert(1, "0");
                    data_str[index] = data_str[index].Insert(3, "00");
                }
                if (value.ToString().Length == 3)
                {
                    data_str[index] = data_str[index].Insert(3, "0");
                    data_str[index] = data_str[index].Insert(3, "000");
                }
                if (value.ToString().Length == 2)
                {
                    data_str[index] = data_str[index].Insert(1, "0");
                    data_str[index] = data_str[index].Insert(3, "0000");
                }
                if (value.ToString().Length == 1)
                {
                    data_str[index] = data_str[index].Insert(3, "0");
                    data_str[index] = data_str[index].Insert(3, "00000");
                }
            }

            if (value == 0)
                data_str[index] = "0.0";

            return data_str[index];
        }
        private string comment_weight_moving()
        {
            if (this.weight_moving == 0)
                return "0=Weight is not moving.";
            else
                if (this.weight_moving == 1)
                return "1=Weight is moving";
            else
                return "Error";
        }
        private string comment_limit_status()
        {
            switch (this.limit_status)
            {
                case 0:
                    return "Weight within limits";
                case 1:
                    return "Lower than minimum";
                case 2:
                    return "Higher than maximum capacity";
                case 3:
                    return "Higher than safe load limit";
                default:
                    return "Error.";
            }
        }
        private string comment_weight_type()
        {
            if (this.weight_type == 0)
            {
                this.is_net = false;
                return "gross";
            }
            else 
            if (this.weight_type == 1)
            {
                this.is_net = true;
                return "net";
            }
            else

                return "error";
        }
        private string comment_scale_range()
        {
            switch (this.scale_range)
            {
                case 0:
                    return "Range 1";
                case 1:
                    return "Range 2";
                case 2:
                    return "Range 3";
                default:
                    return "error";
            }
        }
        private string comment_application_mode()
        {
            if (this.application_mode == 0)
                return "Standard";
            else

                if (this.application_mode == 2 || this.application_mode == 1)  // Will be changed to '2', so far '1'. 
                return "Filler";
            else

                return "error";
        }
        private string comment_unit()
        {
            switch (this.unit)
            {
                case 0:
                    return "kg";
                case 1:
                    return "g";
                case 2:
                    return "t";
                case 3:
                    return "lb";
                default:
                    return "error";
            }
        }
        private string comment_status()
        {
            if (this.status == 1)
                return "Execution OK!";
            else
                if (this.status != 1)
                return "Execution not OK!";
            else
                return "error.";

        }
    }
}
