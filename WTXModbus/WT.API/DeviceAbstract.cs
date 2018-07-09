using Hbm.Devices.WTXModbus;
using Hbm.Wt.Connection;
using System;
using System.ComponentModel;
using System.Threading;
using Hbm.Wt.WTXInterface;
using Hbm.Wt.CommonNetLib.Utils;

namespace Hbm.Wt.WTXInterface
{
    public abstract class DeviceAbstract : IDeviceValues
    {
        /*
        protected INetConnection<ushort,ushort[]> m_Connection;

        public DeviceAbstract(INetConnection<ushort, ushort[]> connection) {
            m_Connection = connection;
        }
        */

        private Action<IDeviceValues> callback_obj;

        private ushort command;

        private string ipAddr;

        private ModbusConnection ModbusConnObj;
        private JetBusConnection JetConnObj;

        private int timeoutMS;
        private bool inputModbusJet;
        private INetConnection s_Connection;

        //private INetCommunication<uint, JToken> commObj;

        public DeviceAbstract(INetConnection S_Connection)
        {

            this.s_Connection = s_Connection;

            inputModbusJet = true;
            timeoutMS = 5000;

            this.ipAddr = "172.19.103.8";

            if (inputModbusJet == true)
            {
                this.ModbusConnObj = new ModbusConnection(ipAddr);
            }

            /*
            else
                if (inputModbusJet == false)
            {
                IJetConnection IJetObj = new WebSocketJetConnection(ipAddr, delegate { return true; });      // Unter Umständen die Certification Callback ausimplementieren. 
                JetPeer jetObj = new JetPeer(IJetObj);                                                       // Certification Callbackmethode in API verpackt? Oder Nutzer selbst implementieren? Machen wir! Erstmal als delegate -> true. 

                this.JetConnObj = new JetBusConnection(jetObj, timeoutMS);
            }
            */

        }


        public ModbusConnection getConnection
        {
            get
            {
                return this.ModbusConnObj;
            }

        }

        //public abstract void connect(ushort command);
        public abstract void Calibration(ushort command);
        public abstract void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e);
        //public abstract void UpdateEvent(object sender, MessageEvent<ushort> e);

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
                this.ModbusConnObj.Read<ushort>("0");

            else
            {
                // (1) Sending of a command:        
                getConnection.Write(wordNumber.ToString(), this.command);  // Alternativ : 1.Parameter = wordNumber

                while (this.handshake == 0)
                {
                    Thread.Sleep(100);
                    this.ModbusConnObj.Read<ushort>("0");
                    //this.JetConnObj.Read(0);
                }

                // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
                if (this.handshake == 1)
                {
                    this.ModbusConnObj.Write(wordNumber.ToString(), 0x00);      // Alternativ : 1.Parameter = wordNumber
                    //this.JetConnObj.Write(0, 1);        // Parameter: uint index, uint data. 
                }
                while (/*this.status == 1 &&*/ this.handshake == 1)
                {
                    Thread.Sleep(100);
                    this.ModbusConnObj.Read<ushort>("0");
                    //this.JetConnObj.Read(0);
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
            this.ModbusConnObj.Read<ushort>("0");

            return this;
        }

        // Neu : 8.3.2018
        public IDeviceValues syncReadData()
        {
            this.ModbusConnObj.Read<ushort>("0");
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

            this.ModbusConnObj.Write(0.ToString(), this.command);
            //this.JetConnObj.Write(0,1);

            while (this.handshake == 0) ;

            // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
            if (this.handshake == 1)
            {
                this.ModbusConnObj.Write(0.ToString(), 0x00);
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
                this.ModbusConnObj.Write(46.ToString(), data_written);
                //this.JetConnObj.Write(46, data_written);

                /// Am besten so lösen: this.m_Connection.Write
            }
            if (choice == 'z')
            {
                this.ModbusConnObj.Write(48.ToString(), data_written);
                //this.JetConnObj.Write(48, data_written);
            }
            if (choice == 'n')
            {
                this.ModbusConnObj.Write(50.ToString(), data_written);
                //this.JetConnObj.Write(50, data_written);
            }

        }


    }
}

