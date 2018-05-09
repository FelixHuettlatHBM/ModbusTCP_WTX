using Hbm.Devices.WTXModbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace WTXModbus
{
    public class WTX120Modbus : DeviceAbstract      // PLC 
    {
        private string[] dataStr;
        private ushort[] data;

        private System.Timers.Timer aTimer;
        private bool isNet;

        private ModbusConnection ModbusConnObj;
        private IDeviceValues thisValues;
        
        private ushort command;

        private Action<IDeviceValues> callback_obj;

        public override event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;


        public WTX120Modbus(ModbusConnection connection, int paramTimerInterval) : base(connection,paramTimerInterval)
        {
            ModbusConnObj = connection;
            
            data = new ushort[59];
            dataStr = new string[59];

            for (int i = 0; i < 59; i++)
            {
                this.dataStr[i] = "0";
                data[i] = 0;
            }

            getConnection.RaiseDataEvent += this.UpdateEvent;   // Subscribe to the event.

            this.initialize_timer(paramTimerInterval);
        }


        public override ModbusConnection getConnection
        {
            get
            {
                return this.ModbusConnObj;
            }

        }

        public void Async_Call(/*ushort wordNumberParam, */ushort commandParam, Action<IDeviceValues> callbackParam)
        {
            BackgroundWorker bgWorker = new BackgroundWorker();   // At the class level, create an instance of the BackgroundWorker class.

            //this.wordNumber = wordNumberParam;
            this.command = commandParam;
            this.callback_obj = callbackParam;

            bgWorker.WorkerSupportsCancellation = true;  // Specify whether you want the background operation to allow cancellation and to report progress.
            bgWorker.WorkerReportsProgress = true;

            if (this.command == 0x00)       // command=0x00 , read data from register 
            {
                bgWorker.DoWork += new DoWorkEventHandler(this.ReadDoWork);  // To set up for a background operation, an event handler, "DoWorkEventHandler" is added.
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.ReadCompleted);  // Create an event handler for the RunWorkerCompleted event (method "Read_Completed"). 
            }
            else  // else , write command into register 
            {
                bgWorker.DoWork += new DoWorkEventHandler(this.WriteDoWork);
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.WriteCompleted);
            }

            bgWorker.WorkerReportsProgress = true;
            bgWorker.RunWorkerAsync();
        }


        // Neu - 8.3.2018 - Ohne Backgroundworker - Ohne Asynchronität
        public void SyncCall_Write_Command(ushort wordNumber, ushort commandParam, Action<IDeviceValues> callbackParam)      // Callback-Methode nicht benötigt. 
        {
            this.command = commandParam;
            this.callback_obj = callbackParam;

            if (this.command == 0x00)
                getConnection.Read();

            else
            {
                // (1) Sending of a command:        
                getConnection.Write(wordNumber, this.command);  // Alternativ : 1.Parameter = wordNumber

                while (this.handshake == 0)
                {
                    Thread.Sleep(100);
                    getConnection.Read();
                    //this.JetConnObj.Read();
                }

                // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
                if (this.handshake == 1)
                {
                    getConnection.Write(wordNumber, 0x00);      // Alternativ : 1.Parameter = wordNumber
                    //this.JetConnObj.Write(0, 1);        // Parameter: uint index, uint data. 
                }
                while (/*this.status == 1 &&*/ this.handshake == 1)
                {
                    Thread.Sleep(100);
                    getConnection.Read();
                    //this.JetConnObj.Read();
                }
            }
        }

        // This method is executed asynchronously in the background for reading the register by a Backgroundworker. 
        // @param : sender - the object of this class. dowork_asynchronous - the argument of the event. 
        public void ReadDoWork(object sender, DoWorkEventArgs dowork_asynchronous)
        {
            dowork_asynchronous.Result = (IDeviceValues)this.asyncReadData((BackgroundWorker)sender); // the private method "this.read_data" in called to read the register in class Modbus_TCP
            // dowork_asynchronous.Result contains all values defined in Interface IDevice_Values.
        }

        // This method read the register of the Device(here: WTX120), therefore it calls the method in class Modbus_TCP to read the register. 
        // @return: IDevice_Values - Interface, that contains all values for the device. 
        private IDeviceValues asyncReadData(BackgroundWorker worker)
        {
            getConnection.Read();

            return this;
        }

        // Neu : 8.3.2018
        public IDeviceValues syncReadData()
        {
            getConnection.Read();
            //this.JetConnObj.Read();

            return this;
        }

        public DeviceAbstract getDeviceAbstract
        {
            get
            {
                return this;
            }
        }

        public void ReadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.callback_obj((IDeviceValues)e.Result);         // Neu : 21.11.2017         Interface übergeben. 
        }

        public void WriteDoWork(object sender, DoWorkEventArgs e)
        {
            // (1) Sending of a command:        

            getConnection.Write(0, this.command);
            //this.JetConnObj.Write(0,1);

            while (this.handshake == 0) ;

            // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
            if (this.handshake == 1)
            {
                getConnection.Write(0, 0x00);
                //this.JetConnObj.Write(0,1);
            }
            while (/*this.status == 1 && */this.handshake == 1) ;
        }

        public void WriteCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.callback_obj(this);         // Neu : 21.11.2017         Interface übergeben. 
        }

        public void write_Zero_Calibration_Nominal_Load(char choice, int load_written, Action<IDeviceValues> callbackParam)
        {
            this.callback_obj = callbackParam;

            ushort[] data_written = new ushort[2];

            data_written[0] = (ushort)((load_written & 0xffff0000) >> 16);
            data_written[1] = (ushort)(load_written & 0x0000ffff);

            if (choice == 'c')
            {
                getConnection.Write(46, data_written);
                //this.JetConnObj.Write(46, data_written);
            }
            if (choice == 'z')
            {
                getConnection.Write(48, data_written);
                //this.JetConnObj.Write(48, data_written);
            }
            if (choice == 'n')
            {
                getConnection.Write(50, data_written);
                //this.JetConnObj.Write(50, data_written);
            }

        }


        // 13.04.2018 : Neu - Timer function

        // This method initializes the with the timer interval as a parameter: 
        public override void initialize_timer(int timer_interval)
        {
            // Create a timer with an interval of 500ms. 
            aTimer = new System.Timers.Timer(timer_interval);

            // Connect the elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        // Event method, which will be triggered after a interval of the timer is elapsed- 
        // After triggering (after 500ms) the register is read. 
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Async_Call(0x00, DataReceivedTimer);

            getConnection.RaiseDataEvent += UpdateEvent;   // Subscribe to the event.
        }

        private void DataReceivedTimer(IDeviceValues Device_Values)
        {
            thisValues = Device_Values;

            int previousNetValue = Device_Values.NetValue;

        }

        public override void Calibration(ushort command)
        {
            // Set zero, set nominal, set calibration weight... siehe anderen Code. 
        }

        //public override void UpdateEvent(object sender, MessageEvent<ushort> e)
        public override void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e)
        {
            this.data = e.Args;
            //this.data = e.Message;        // Mit MessageEvent 

            this.dataStr[0] = this.measurement_with_comma(this.NetValue, this.decimals);  // 1 equal to "Net measured" as a parameter
            this.dataStr[1] = this.measurement_with_comma(this.GrossValue, this.decimals);  // 2 equal to "Gross measured" as a parameter

            this.dataStr[2] = this.general_weight_error.ToString();
            this.dataStr[3] = this.scale_alarm_triggered.ToString();
            this.dataStr[4] = this.comment_limit_status();
            this.dataStr[5] = this.comment_weight_moving();

            this.dataStr[6] = this.scale_seal_is_open.ToString();
            this.dataStr[7] = this.manual_tare.ToString();
            this.dataStr[8] = this.comment_weight_type();
            this.dataStr[9] = this.comment_scale_range();

            this.dataStr[10] = this.zero_required.ToString();
            this.dataStr[11] = this.weight_within_the_center_of_zero.ToString();
            this.dataStr[12] = this.weight_in_zero_range.ToString();
            this.dataStr[13] = this.comment_application_mode();

            this.dataStr[14] = this.decimals.ToString();
            this.dataStr[15] = this.comment_unit();
            this.dataStr[16] = this.handshake.ToString();
            this.dataStr[17] = this.comment_status();

            this.dataStr[18] = this.digital_input_1.ToString();
            this.dataStr[19] = this.digital_input_2.ToString();
            this.dataStr[20] = this.digital_input_3.ToString();
            this.dataStr[21] = this.digital_input_4.ToString();

            this.dataStr[22] = this.digital_output_1.ToString();
            this.dataStr[23] = this.digital_output_2.ToString();
            this.dataStr[24] = this.digital_output_3.ToString();
            this.dataStr[25] = this.digital_output_4.ToString();

            if (this.application_mode == 0)
            {
                this.dataStr[26] = this.limit_value_status_1.ToString();
                this.dataStr[27] = this.limit_value_status_2.ToString();
                this.dataStr[28] = this.limit_value_status_3.ToString();
                this.dataStr[29] = this.limit_value_status_4.ToString();

                this.dataStr[30] = this.weight_memory_day.ToString();
                this.dataStr[31] = this.weight_memory_month.ToString();
                this.dataStr[32] = this.weight_memory_year.ToString();
                this.dataStr[33] = this.weight_memory_seq_number.ToString();
                this.dataStr[34] = this.weight_memory_gross.ToString();
                this.dataStr[35] = this.weight_memory_net.ToString();
            }
            else
                if (this.application_mode == 2 || this.application_mode == 0) // in filler mode 
            {
                this.dataStr[26] = this.coarse_flow.ToString();
                this.dataStr[27] = this.fine_flow.ToString();
                this.dataStr[28] = this.ready.ToString();
                this.dataStr[29] = this.re_dosing.ToString();

                this.dataStr[30] = this.emptying.ToString();
                this.dataStr[31] = this.flow_error.ToString();
                this.dataStr[32] = this.alarm.ToString();
                this.dataStr[33] = this.ADC_overload_underload.ToString();

                this.dataStr[34] = this.max_dosing_time.ToString();
                this.dataStr[35] = this.legal_for_trade_operation.ToString();
                this.dataStr[36] = this.tolerance_error_plus.ToString();
                this.dataStr[37] = this.tolerance_error_minus.ToString();

                this.dataStr[38] = this.status_digital_input_1.ToString();
                this.dataStr[39] = this.general_scale_error.ToString();
                this.dataStr[40] = this.dosing_process_status.ToString();
                this.dataStr[41] = this.dosing_count.ToString();

                this.dataStr[42] = this.dosing_result.ToString();
                this.dataStr[43] = this.mean_value_of_dosing_results.ToString();
                this.dataStr[44] = this.standard_deviation.ToString();
                this.dataStr[45] = this.total_weight.ToString();

                this.dataStr[46] = this.fine_flow_cut_off_point.ToString();
                this.dataStr[47] = this.coarse_flow_cut_off_point.ToString();
                this.dataStr[48] = this.actual_dosing_time.ToString();
                this.dataStr[49] = this.actual_coarse_flow_time.ToString();

                this.dataStr[50] = this.actual_fine_flow_time.ToString();
                this.dataStr[51] = this.parameter_set.ToString();

                this.dataStr[52] = this.filler_weight_memory_day.ToString();
                this.dataStr[53] = this.filler_weight_memory_month.ToString();
                this.dataStr[54] = this.filler_weight_memory_year.ToString();
                this.dataStr[55] = this.filler_weight_memory_seq_number.ToString();
                this.dataStr[56] = this.filler_weight_memory_gross.ToString();
                this.dataStr[57] = this.filler_weight_memory_net.ToString();
            }

            e.Args = this.data;

            EventHandler<NetConnectionEventArgs<ushort[]>> handler2 = DataUpdateEvent;        // Neu : 4.5.18

            if (handler2 != null)
                handler2(this, e);
        }

        public override IDeviceValues DeviceValues
        {
            get
            {
                return thisValues;
            }
        }


        // The following methods set the specific, single values from the whole array "data".

        public override int NetValue
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 1)
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

        public override int GrossValue
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 3)
                        return (data[3] + (data[2] << 16));
                    else
                        return 0;
                }
                catch (System.IndexOutOfRangeException)
                {
                    return 0;
                }
            }
        }
        public override int general_weight_error
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override int scale_alarm_triggered
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int limit_status
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int weight_moving
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int scale_seal_is_open
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int manual_tare
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int weight_type
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int scale_range
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int zero_required
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int weight_within_the_center_of_zero
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int weight_in_zero_range
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 4)
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
        public override  int application_mode
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 5)
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
        public override  int decimals
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 5)
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
        public override  int unit
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 5)
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
        public override  int handshake
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 5)
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
        public override  int status
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 5)
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

        public override  string[] get_data_str
        {
            get
            {
                return this.dataStr;
            }
            set
            {
                this.dataStr = value;
            }
        }

        public override  int digital_input_1
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 6)
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
        public override  int digital_input_2
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 6)
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
        public override  int digital_input_3
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 6)
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
        public override  int digital_input_4
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 6)
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
        public override  int digital_output_1
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 7)
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
        public override  int digital_output_2
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 7)
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
        public override  int digital_output_3
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 7)
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
        public override  int digital_output_4
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 7)
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
        public override  int limit_value_status_1
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int limit_value_status_2
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int limit_value_status_3
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int limit_value_status_4
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int weight_memory_day
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 9)
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
        public override  int weight_memory_month
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 10)
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
        public override  int weight_memory_year
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 11)
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
        public override  int weight_memory_seq_number
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 12)
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
        public override  int weight_memory_gross
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 13)
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
        public override  int weight_memory_net
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 14)
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
        public override  int coarse_flow
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int fine_flow
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int ready
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int re_dosing
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int emptying
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int flow_error
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int alarm
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int ADC_overload_underload
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int max_dosing_time
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int legal_for_trade_operation
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int tolerance_error_plus
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int tolerance_error_minus
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int status_digital_input_1
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int general_scale_error
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 8)
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
        public override  int dosing_process_status
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 9)
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
        public override  int dosing_count
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 11)
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
        public override  int dosing_result
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 12)
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
        public override  int mean_value_of_dosing_results
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 14)
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
        public override  int standard_deviation
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 16)
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
        public override  int total_weight
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 18)
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
        public override int fine_flow_cut_off_point
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 20)
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
        public override int coarse_flow_cut_off_point
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 22)
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
        public override  int actual_dosing_time
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 24)
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
        public override  int actual_coarse_flow_time
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 25)
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
        public override  int actual_fine_flow_time
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 26)
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
        public override int parameter_set
        {
            get
            {
                try
                {
                    if (getConnection.getNumOfPoints > 27)
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
                    if (getConnection.getNumOfPoints > 28)
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
                    if (getConnection.getNumOfPoints > 29)
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
                    if (getConnection.getNumOfPoints > 30)
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
                    if (getConnection.getNumOfPoints > 31)
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
                    if (getConnection.getNumOfPoints > 32)
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
                    if (getConnection.getNumOfPoints > 33)
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

        public override ushort[] get_data_ushort
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
                return this.isNet;
            }
        }

        /* In den folgenden Comment-Methoden werden jeweils verschiedene Auswahloptionen mit Fallunterscheidungen
        * betrachtet und je nach Fall eine unterschiedliche Option ausgewählt.
        */

        // In the following methods the different options for the single integer values are used to define and
        // interpret the value. Finally a string should be returned from the methods to write it onto the GUI Form. 

        private string measurement_with_comma(int value, int decimals)
        {
            double dvalue = value / Math.Pow(10, decimals);
            string returnvalue = "";

            switch (decimals)
            {
                case 0: returnvalue = dvalue.ToString(); break;
                case 1: returnvalue = dvalue.ToString("0.0"); break;
                case 2: returnvalue = dvalue.ToString("0.00"); break;
                case 3: returnvalue = dvalue.ToString("0.000"); break;
                case 4: returnvalue = dvalue.ToString("0.0000"); break;
                case 5: returnvalue = dvalue.ToString("0.00000"); break;
                case 6: returnvalue = dvalue.ToString("0.000000"); break;
                default: returnvalue = dvalue.ToString(); break;

            }
            return returnvalue;
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
                this.isNet = false;
                return "gross";
            }
            else
                if (this.weight_type == 1)
            {
                this.isNet = true;
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
