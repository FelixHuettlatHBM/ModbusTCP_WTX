
using HBM.WT.API;
using HBM.WT.API.WTX.Modbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace HBM.WT.API.WTX
{
    /*
    public enum ParameterEnum : uint
    {
        MeasuredValue = 0x200001,
    }
    */
    public class WTXModbus : BaseWTDevice     // ParameterProperty umändern 
    {
        //public ParameterProperty(INetConnection connection) : base(connection) { }
        //public override int MeasureValue { get { return m_Connection.Read<int>(ParameterEnum.MeasuredValue.ToString()); } }

        private string[] dataStr;
        private ushort[] previousData;
        private ushort[] data;
        private ushort[] outputData;
        private ushort[] data_written;

        private bool isNet;
        private bool isCalibrating;
        private bool isRefreshed;
        private bool compareDataChanged;

        private int timerInterval;

        private System.Timers.Timer aTimer;

        private Action<IDeviceData> callback_obj;

        private ushort command;

        private string ipAddr;

        private ModbusTCPConnection ModbusConnObj;

        private INetConnection m_Connection;

        INetConnection thisConnection;

        IDeviceData thisValues;

        private bool dataReceived;

        // Neu : 4.5.2018 - für asynchronen Aufruf - Eventbasiert

        public override event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;


        public override event Func<object, EventArgs, Task> Shutdown;

        /*
        public override async Task OnShutdown()
        {
            Func<object, EventArgs, Task> handler = Shutdown;

            if (handler == null)
                return;
            
            Delegate[] invocationList = handler.GetInvocationList();

            Task[] handlerTasks = new Task[invocationList.Length];

            for (int i = 0; i < invocationList.Length; i++)
                handlerTasks[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);

            await Task.WhenAll(handlerTasks);


        }
        */


        public override async Task OnShutdown()
        {
            Func<object, EventArgs, Task> handler = Shutdown;

            if (handler == null)
                return;

            Delegate[] invocationList = handler.GetInvocationList();

            Task[] handlerTasks = new Task[invocationList.Length];

            for (int i = 0; i < invocationList.Length; i++)
                handlerTasks[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);

            await Task.WhenAll(handlerTasks);

        }

        public WTXModbus(INetConnection connection,int paramTimerInterval) : base(connection)
         {
            m_Connection = connection;

            this.ipAddr = "172.19.103.8";

            this.ModbusConnObj = new ModbusTCPConnection(ipAddr);

            this.previousData = new ushort[59];
            this.dataStr = new string[59];
            this.data = new ushort[59];
            this.outputData = new ushort[43]; // Output data length for filler application, also used for the standard application.
            this.data_written = new ushort[2];

            for (int i = 0; i < 59; i++)
            {
                dataStr[i] = "0";
                data[i] = 0;
                this.previousData[i] = 0;
            }

            for (int i = 0; i < 43; i++)
            {
                this.outputData[i] = 0;
            }

            this.compareDataChanged = false;
            this.isCalibrating = false;
            this.isRefreshed = false;
            this.isNet = false;
            this.dataReceived = false;

            this.timerInterval = 0;


            // For the connection and initializing of the timer: 

            thisConnection = connection;

            getConnection.RaiseDataEvent += this.UpdateEvent;   // Subscribe to the event.

            this.initialize_timer(paramTimerInterval);
         }
        public override ModbusTCPConnection getConnection
        {
            get
            {
                return this.ModbusConnObj;
            }

        }

        // To establish a connection to the WTX device via class WTX120_Modbus.
        public override void Connect()
        {
            this.ModbusConnObj.Connect();
        }

        // To terminate,break, a connection to the WTX device via class WTX120_Modbus.
        public override void Disconnect()
        {
            this.ModbusConnObj.ResetDevice();
        }

        public override void Async_Call(/*ushort wordNumberParam, */ushort commandParam, Action<IDeviceData> callbackParam)
        {
            this.dataReceived = false;
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
        public override void SyncCall_Write_Command(ushort wordNumber, ushort commandParam, Action<IDeviceData> callbackParam)      // Callback-Methode nicht benötigt. 
        {
            this.dataReceived = false;
            this.command = commandParam;
            this.callback_obj = callbackParam;

            if (this.command == 0x00)
                this.ModbusConnObj.Read<ushort>(0);

            else
            {
                // (1) Sending of a command:        
                getConnection.Write(wordNumber, this.command);  // Alternativ : 1.Parameter = wordNumber

                while (this.handshake == 0)
                {
                    Thread.Sleep(100);
                    this.ModbusConnObj.Read<ushort>(0);
                    //this.JetConnObj.Read();
                }

                // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
                if (this.handshake == 1)
                {
                    this.ModbusConnObj.Write(wordNumber, 0x00);      // Alternativ : 1.Parameter = wordNumber
                    //this.JetConnObj.Write(0, 1);        // Parameter: uint index, uint data. 
                }
                while (/*this.status == 1 &&*/ this.handshake == 1)
                {
                    Thread.Sleep(100);
                    this.ModbusConnObj.Read<ushort>(0);
                    //this.JetConnObj.Read();
                }
            }
        }

        // This method is executed asynchronously in the background for reading the register by a Backgroundworker. 
        // @param : sender - the object of this class. dowork_asynchronous - the argument of the event. 
        public override void ReadDoWork(object sender, DoWorkEventArgs dowork_asynchronous)
        {
            this.dataReceived = false;
            dowork_asynchronous.Result = (IDeviceData)this.asyncReadData((BackgroundWorker)sender); // the private method "this.read_data" in called to read the register in class Modbus_TCP
            // dowork_asynchronous.Result contains all values defined in Interface IDevice_Values.
        }

        // This method read the register of the Device(here: WTX120), therefore it calls the method in class Modbus_TCP to read the register. 
        // @return: IDevice_Values - Interface, that contains all values for the device. 
        public override IDeviceData asyncReadData(BackgroundWorker worker)
        {
            this.ModbusConnObj.Read<ushort>(0);

            return this;
        }

        // Neu : 8.3.2018
        public override IDeviceData syncReadData()
        {
            this.ModbusConnObj.Read<ushort>(0);
            //this.JetConnObj.Read();

            return this;
        }

        public override IDeviceData DeviceValues
        {
            get
            {
                return thisValues;
            }
        }

        public override BaseWTDevice getDeviceAbstract
        {
            get
            {
                return this;
            }
        }

        public override void ReadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //EventHandler<RunWorkerCompletedEventArgs> handler = DataUpdateEvent;        // Neu : 4.5.18

            this.callback_obj((IDeviceData)e.Result);         // Interface commited via callback. 

            // For synchronous check that data is received:
            dataReceived = true;

            // For asynchronous check that data is received:
            //if (handler != null)
            //    handler(this, e);
        }

        public override bool isDataReceived
        {
            get
            {
                return this.dataReceived;
            }
            set
            {
                this.dataReceived = value;
            }
        }

        public override void WriteDoWork(object sender, DoWorkEventArgs e)
        {
            // (1) Sending of a command:        

            this.ModbusConnObj.Write(0, this.command);
            //this.JetConnObj.Write(0,1);

            while (this.handshake == 0) ;

            // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
            if (this.handshake == 1)
            {
                this.ModbusConnObj.Write(0, 0x00);
                //this.JetConnObj.Write(0,1);

                //this.NetObj.Write<ushort>(0, 0x00);
            }
            while (/*this.status == 1 && */this.handshake == 1) ;
        }

        public override void WriteCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.callback_obj(this);         // Neu : 21.11.2017         Interface übergeben. 
        }

        public void writeOutputWordS32(int valueParam, ushort wordNumber, Action<IDeviceData> callbackParam)
        {
            this.callback_obj = callbackParam;

            data_written[0] = (ushort)((valueParam & 0xffff0000) >> 16);
            data_written[1] = (ushort)(valueParam & 0x0000ffff);

            getConnection.Write(wordNumber, data_written);
        }


        public void writeOutputWordU08(int valueParam, ushort wordNumber, Action<IDeviceData> callbackParam)
        {
            this.callback_obj = callbackParam;

            /*
            data_written[0] = (ushort)((valueParam & 0x000000ff));
            getConnection.Write(wordNumber, data_written[0]);
            */

            getConnection.Write(wordNumber, (ushort)valueParam);
        }

        public void writeOutputWordU16(int valueParam, ushort wordNumber, Action<IDeviceData> callbackParam)
        {
            this.callback_obj = callbackParam;

            data_written[0] = (ushort)((valueParam & 0xffff0000) >> 16);

            getConnection.Write(wordNumber, data_written[0]);
        }

        
        // This method initializes the with the timer interval as a parameter: 
        public override void initialize_timer(int paramTimerInterval)
        {
            // Create a timer with an interval of the parameter value, if the argument paramTimerInterval is not valid,
            // an exception is catched and a default value for the timer interval is set, the timer tries to start again. 
            try
            {
                aTimer = new System.Timers.Timer(paramTimerInterval);
            }
            catch (ArgumentException)
            {
                this.timerInterval = 100;   // In case if the timer interval is not valid, an 'ArgumentException' is catched and a default value for
                                            // the timer interval is set. 
                aTimer = new System.Timers.Timer(this.timerInterval);
            }
            // Connect the elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            aTimer.Start();
        }

        /*
        * This method stops the timer, for example in case for the calibration.
        */
        public void stopTimer()
        {
            aTimer.Elapsed -= OnTimedEvent;
            aTimer.Enabled = false;
            aTimer.Stop();
        }

        /*
         * This method restarts the timer, for example in case for the calibration.
         */
        public void restartTimer()
        {
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
            aTimer.Start();
        }

        // Event method, which will be triggered after a interval of the timer is elapsed- 
        // After triggering (after 500ms) the register is read. 
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Async_Call(0x00, DataReceivedTimer);

            //thisConnection.RaiseDataEvent += UpdateEvent;   // Subscribe to the event.
        }

        private void DataReceivedTimer(IDeviceData Device_Values)
        {
            thisValues = Device_Values;

            int previousNetValue = Device_Values.NetValue;
            
        }

        public override void Calibration(ushort command)
        {
            // Set zero, set nominal, set calibration weight... siehe anderen Code. 
        }


        public override ushort[] getValuesAsync()
        {
            return data;
        }


        //public override void UpdateEvent(object sender, MessageEvent<ushort> e)
        public override void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e)
        {
            this.data = e.Args;

            //this.data = e.Message;        // Mit MessageEvent 

            this.getDataStr[0] = this.netGrossValueStringComment(this.NetValue, this.decimals);  // 1 equal to "Net measured" as a parameter
            this.getDataStr[1] = this.netGrossValueStringComment(this.GrossValue, this.decimals);  // 2 equal to "Gross measured" as a parameter

            this.getDataStr[2] = this.generalWeightError.ToString();
            this.getDataStr[3] = this.scaleAlarmTriggered.ToString();
            this.getDataStr[4] = this.limitStatusStringComment();
            this.getDataStr[5] = this.weightMovingStringComment();

            this.getDataStr[6] = this.scaleSealIsOpen.ToString();
            this.getDataStr[7] = this.manualTare.ToString();
            this.getDataStr[8] = this.weightTypeStringComment();
            this.getDataStr[9] = this.scaleRangeStringComment();

            this.getDataStr[10] = this.zeroRequired.ToString();
            this.getDataStr[11] = this.weightWithinTheCenterOfZero.ToString();
            this.getDataStr[12] = this.weightInZeroRange.ToString();
            this.getDataStr[13] = this.applicationModeStringComment();

            this.getDataStr[14] = this.decimals.ToString();
            this.getDataStr[15] = this.unitStringComment();
            this.getDataStr[16] = this.handshake.ToString();
            this.getDataStr[17] = this.statusStringComment();

            this.getDataStr[18] = this.input1.ToString();
            this.getDataStr[19] = this.input2.ToString();
            this.getDataStr[20] = this.input3.ToString();
            this.getDataStr[21] = this.input4.ToString();

            this.getDataStr[22] = this.output1.ToString();
            this.getDataStr[23] = this.output2.ToString();
            this.getDataStr[24] = this.output3.ToString();
            this.getDataStr[25] = this.output4.ToString();

            if (this.applicationMode == 0)
            {
                this.getDataStr[26] = this.limitStatus1.ToString();
                this.getDataStr[27] = this.limitStatus2.ToString();
                this.getDataStr[28] = this.limitStatus3.ToString();
                this.getDataStr[29] = this.limitStatus4.ToString();

                this.getDataStr[30] = this.weightMemDay.ToString();
                this.getDataStr[31] = this.weightMemMonth.ToString();
                this.getDataStr[32] = this.weightMemYear.ToString();
                this.getDataStr[33] = this.weightMemSeqNumber.ToString();
                this.getDataStr[34] = this.weightMemGross.ToString();
                this.getDataStr[35] = this.weightMemNet.ToString();
            }
            else
                if (this.applicationMode == 2 || this.applicationMode == 0) // in filler mode 
            {
                this.getDataStr[26] = this.coarseFlow.ToString();
                this.getDataStr[27] = this.fineFlow.ToString();
                this.getDataStr[28] = this.ready.ToString();
                this.getDataStr[29] = this.reDosing.ToString();

                this.getDataStr[30] = this.emptying.ToString();
                this.getDataStr[31] = this.flowError.ToString();
                this.getDataStr[32] = this.alarm.ToString();
                this.getDataStr[33] = this.ADC_overUnderload.ToString();

                this.getDataStr[34] = this.maxDosingTime.ToString();
                this.getDataStr[35] = this.legalTradeOp.ToString();
                this.getDataStr[36] = this.toleranceErrorPlus.ToString();
                this.getDataStr[37] = this.toleranceErrorMinus.ToString();

                this.getDataStr[38] = this.status.ToString();
                this.getDataStr[39] = this.generalScaleError.ToString();
                this.getDataStr[40] = this.fillingProcessStatus.ToString();
                this.getDataStr[41] = this.numberDosingResults.ToString();

                this.getDataStr[42] = this.dosingResult.ToString();
                this.getDataStr[43] = this.meanValueDosingResults.ToString();
                this.getDataStr[44] = this.standardDeviation.ToString();
                this.getDataStr[45] = this.totalWeight.ToString();

                this.getDataStr[46] = this.fineFlowCutOffPoint.ToString();
                this.getDataStr[47] = this.coarseFlowCutOffPoint.ToString();
                this.getDataStr[48] = this.currentDosingTime.ToString();
                this.getDataStr[49] = this.currentCoarseFlowTime.ToString();

                this.getDataStr[50] = this.currentFineFlowTime.ToString();
                this.getDataStr[51] = this.parameterSetProduct.ToString();

                this.getDataStr[52] = this.filler_weight_memory_day.ToString();
                this.getDataStr[53] = this.filler_weight_memory_month.ToString();
                this.getDataStr[54] = this.filler_weight_memory_year.ToString();
                this.getDataStr[55] = this.filler_weight_memory_seq_number.ToString();
                this.getDataStr[56] = this.filler_weight_memory_gross.ToString();
                this.getDataStr[57] = this.filler_weight_memory_net.ToString();
            }

            // Vorher: 
/*
            e.Args = this.data;

            EventHandler<NetConnectionEventArgs<ushort[]>> handler2 = DataUpdateEvent;        // Neu : 4.5.18

            if (handler2 != null)
                handler2(this, e);
 */
            // Oder : DataUpdateEvent?.Invoke(this, e);



            compareDataChanged = false;

            e.Args = this.data;

            for (int index = 0; index < 6; index++)
            {
                if (this.previousData[index] != this.data[index])
                    compareDataChanged = true;
            }
            // If one value of the data changes, the boolean value 'compareDataChanged' will be set to true and the data will be 
            // updated in the following, as well as the GUI form. ('compareDataChanged' is for the purpose of comparision.)

            // The data is only invoked by the event 'DataUpdateEvent' if the data has been changed. The comparision is made by...
            // ... the arrays 'previousData' and 'data' with the boolean 

            if ((this.compareDataChanged == true) || (this.isCalibrating == true) || this.isRefreshed == true)   // 'isCalibrating' indicates if a calibration is done just before ...
            {                                                                                                    // and the data should be send to the GUI/console and be printed out. 
                                                                                                                 // If the GUI has been refreshed, the values should also be send to the GUI/Console and be printed out. 

                DataUpdateEvent?.Invoke(this, e);

                this.isCalibrating = false;
                this.Refreshed = false;
            }

            this.previousData = this.data;

            // As an alternative to 'DataUpdateEvent?.Invoke(this, e);' : Both implementations do the same.  
            /*
            EventHandler<NetConnectionEventArgs<ushort[]>> handler2 = DataUpdateEvent;        

            if (handler2 != null)
                handler2(this, e);
            */
        }

        public bool Refreshed
        {
            get { return this.isRefreshed; }
            set { this.isRefreshed = value; }
        }

        public bool dataChanged
        {
            get { return this.compareDataChanged; }
            set { this.compareDataChanged = value; }
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
        public override int generalWeightError
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
        public override int scaleAlarmTriggered
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
        public override int limitStatus
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
        public override int weightMoving
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
        public override int scaleSealIsOpen
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
        public override int manualTare
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
        public override int weightType
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
        public override int scaleRange
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
        public override int zeroRequired
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
        public override int weightWithinTheCenterOfZero
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
        public override int weightInZeroRange
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
        public override int applicationMode
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
        public override int decimals
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
        public override int unit
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
        public override int handshake
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
        public override int status
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

        public override ushort[] getDataUshort
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

        public override string[] getDataStr
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

        public override int input1
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
        public override int input2
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
        public override int input3
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
        public override int input4
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
        public override int output1
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
        public override int output2
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
        public override int output3
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
        public override int output4
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
        public override int limitStatus1
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
        public override int limitStatus2
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
        public override int limitStatus3
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
        public override int limitStatus4
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
        public override int weightMemDay
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
        public override int weightMemMonth
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
        public override int weightMemYear
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
        public override int weightMemSeqNumber
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
        public override int weightMemGross
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
        public override int weightMemNet
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
        public override int coarseFlow
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
        public override int fineFlow
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
        public override int ready
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
        public override int reDosing
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
        public override int emptying
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
        public override int flowError
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
        public override int alarm
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
        public override int ADC_overUnderload
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


        public override int maxDosingTime
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
        public override int legalTradeOp
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
        public override int toleranceErrorPlus
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
        public override int toleranceErrorMinus
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
        public override int statusInput1
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
        public override int generalScaleError
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
        public override int fillingProcessStatus
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
        public override int numberDosingResults
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
        public override int dosingResult
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
        public override int meanValueDosingResults
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
        public override int standardDeviation
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
        public override int totalWeight
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
        public override int fineFlowCutOffPoint
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
        public override int coarseFlowCutOffPoint
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
        public override int currentDosingTime
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
        public override int currentCoarseFlowTime
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
        public override int currentFineFlowTime
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
        public override int parameterSetProduct
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
            set
            {
                this.data[62] = (ushort)value;
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
            set
            {
                this.data[63] = (ushort)value;
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
            set
            {
                this.data[64] = (ushort)value;
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
            set
            {
                this.data[65] = (ushort)value;
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
            set
            {
                this.data[66] = (ushort)value;
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
            set
            {
                this.data[67] = (ushort)value;
            }
        }

        // Get and Set-Properties of the output words, for the standard and filler application. (To be continued on 04-06-2018 for all the rest...)

        public override int manualTareValue
        {
            get
            {
                return this.outputData[0];
            }
            set
            {
                this.outputData[0] = (ushort)value;
            }
        }

        public override int limitValue1Input
        {
            get
            {
                return this.outputData[1];
            }
            set
            {
                this.outputData[1] = (ushort)value;
            }
        }

        public override int limitValue1Mode
        {
            get
            {
                return this.outputData[2];
            }
            set
            {
                this.outputData[2] = (ushort)value;
            }
        }

        public override int limitValue1ActivationLevelLowerBandLimit
        {
            get
            {
                return this.outputData[3];
            }
            set
            {
                this.outputData[3] = (ushort)value;
            }
        }

        public override int limitValue1HysteresisBandHeight
        {
            get
            {
                return this.outputData[4];
            }
            set
            {
                this.outputData[4] = (ushort)value;
            }
        }



        public override int limitValue2Source
        {
            get
            {
                return this.outputData[5];
            }
            set
            {
                this.outputData[5] = (ushort)value;
            }
        }

        public override int limitValue2Mode
        {
            get
            {
                return this.outputData[6];
            }
            set
            {
                this.outputData[6] = (ushort)value;
            }
        }

        public override int limitValue2ActivationLevelLowerBandLimit
        {
            get
            {
                return this.outputData[7];
            }
            set
            {
                this.outputData[7] = (ushort)value;
            }
        }

        public override int limitValue2HysteresisBandHeight
        {
            get
            {
                return this.outputData[8];
            }
            set
            {
                this.outputData[8] = (ushort)value;
            }
        }



        public override int limitValue3Source
        {
            get
            {
                return this.outputData[9];
            }
            set
            {
                this.outputData[9] = (ushort)value;
            }
        }

        public override int limitValue3Mode
        {
            get
            {
                return this.outputData[10];
            }
            set
            {
                this.outputData[10] = (ushort)value;
            }
        }

        public override int limitValue3ActivationLevelLowerBandLimit
        {
            get
            {
                return this.outputData[11];
            }
            set
            {
                this.outputData[11] = (ushort)value;
            }
        }

        public override int limitValue3HysteresisBandHeight
        {
            get
            {
                return this.outputData[12];
            }
            set
            {
                this.outputData[12] = (ushort)value;
            }
        }


        public override int limitValue4Source
        {
            get
            {
                return this.outputData[13];
            }
            set
            {
                this.outputData[13] = (ushort)value;
            }
        }

        public override int limitValue4Mode
        {
            get
            {
                return this.outputData[14];
            }
            set
            {
                this.outputData[14] = (ushort)value;
            }
        }

        public override int limitValue4ActivationLevelLowerBandLimit
        {
            get
            {
                return this.outputData[15];
            }
            set
            {
                this.outputData[15] = (ushort)value;
            }
        }

        public override int limitValue4HysteresisBandHeight
        {
            get
            {
                return this.outputData[16];
            }
            set
            {
                this.outputData[16] = (ushort)value;
            }
        }


        public override int ResidualFlowTime
        {
            get { return this.outputData[17]; }
            set { this.outputData[17] = (ushort)value; }
        }

        public override int targetFillingWeight
        {
            get { return this.outputData[18]; }
            set { this.outputData[18] = (ushort)value; }
        }

        public override int coarseFlowCutOffPointSet
        {
            get { return this.outputData[19]; }
            set { this.outputData[19] = (ushort)value; }
        }

        public override int fineFlowCutOffPointSet
        {
            get { return this.outputData[20]; }
            set { this.outputData[20] = (ushort)value; }
        }
        public override int minimumFineFlow
        {
            get { return this.outputData[21]; }
            set { this.outputData[21] = (ushort)value; }
        }

        public override int optimizationOfCutOffPoints
        {
            get { return this.outputData[22]; }
            set { this.outputData[22] = (ushort)value; }
        }
        public override int maximumDosingTime
        {
            get { return this.outputData[23]; }
            set { this.outputData[23] = (ushort)value; }
        }
        public override int startWithFineFlow
        {
            get { return this.outputData[24]; }
            set { this.outputData[24] = (ushort)value; }
        }
        public override int coarseLockoutTime
        {
            get { return this.outputData[25]; }
            set { this.outputData[25] = (ushort)value; }
        }
        public override int fineLockoutTime
        {
            get { return this.outputData[26]; }
            set { this.outputData[26] = (ushort)value; }
        }
        public override int tareMode
        {
            get { return this.outputData[27]; }
            set { this.outputData[27] = (ushort)value; }
        }
        public override int upperToleranceLimit
        {
            get { return this.outputData[28]; }
            set { this.outputData[28] = (ushort)value; }
        }
        public override int lowerToleranceLimit
        {
            get { return this.outputData[29]; }
            set { this.outputData[29] = (ushort)value; }
        }
        public override int minimumStartWeight
        {
            get { return this.outputData[30]; }
            set { this.outputData[30] = (ushort)value; }
        }
        public override int emptyWeight
        {
            get { return this.outputData[31]; }
            set { this.outputData[31] = (ushort)value; }
        }
        public override int tareDelay
        {
            get { return this.outputData[32]; }
            set { this.outputData[32] = (ushort)value; }
        }
        public override int coarseFlowMonitoringTime
        {
            get { return this.outputData[33]; }
            set { this.outputData[33] = (ushort)value; }
        }
        public override int coarseFlowMonitoring
        {
            get { return this.outputData[34]; }
            set { this.outputData[34] = (ushort)value; }
        }
        public override int fineFlowMonitoring
        {
            get { return this.outputData[35]; }
            set { this.outputData[35] = (ushort)value; }
        }
        public override int fineFlowMonitoringTime
        {
            get { return this.outputData[36]; }
            set { this.outputData[36] = (ushort)value; }
        }
        public override int delayTimeAfterFineFlow
        {
            get { return this.outputData[37]; }
            set { this.outputData[37] = (ushort)value; }
        }
        public override int activationTimeAfterFineFlow
        {
            get { return this.outputData[38]; }
            set { this.outputData[38] = (ushort)value; }
        }
        public override int systematicDifference
        {
            get { return this.outputData[39]; }
            set { this.outputData[39] = (ushort)value; }
        }
        public override int downardsDosing
        {
            get { return this.outputData[40]; }
            set { this.outputData[40] = (ushort)value; }
        }
        public override int valveControl
        {
            get { return this.outputData[41]; }
            set { this.outputData[41] = (ushort)value; }
        }
        public override int emptyingMode
        {
            get { return this.outputData[42]; }
            set { this.outputData[42] = (ushort)value; }
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

        public string netGrossValueStringComment(int value, int decimals)
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

        public string weightMovingStringComment()
        {
            if (this.weightMoving == 0)
                return "0=Weight is not moving.";
            else
                if (this.weightMoving == 1)
                return "1=Weight is moving";
            else
                return "Error";
        }
        public string limitStatusStringComment()
        {
            switch (this.limitStatus)
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
        public string weightTypeStringComment()
        {
            if (this.weightType == 0)
            {
                this.isNet = false;
                return "gross";
            }
            else
                if (this.weightType == 1)
            {
                this.isNet = true;
                return "net";
            }
            else

                return "error";
        }
        public string scaleRangeStringComment()
        {
            switch (this.scaleRange)
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
        public string applicationModeStringComment()
        {
            if (this.applicationMode == 0)
                return "Standard";
            else

                if (this.applicationMode == 2 || this.applicationMode == 1)  // Will be changed to '2', so far '1'. 
                return "Filler";
            else

                return "error";
        }
        public string unitStringComment()
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
        public string statusStringComment()
        {
            if (this.status == 1)
                return "Execution OK!";
            else
                if (this.status != 1)
                return "Execution not OK!";
            else
                return "error.";

        }



        // This method sets the value for the nominal weight in the WTX.
        public void Calibrate(int calibrationValue, string calibration_weight_Str)
        {
            //write reg 46, CalibrationWeight         

            this.writeOutputWordS32(calibrationValue, 46, Write_DataReceived);

            //write reg 50, 0x7FFFFFFF

            this.writeOutputWordS32(0x7FFFFFFF, 50, Write_DataReceived);

            Console.Write(".");

            this.SyncCall_Write_Command(0, 0x100, Write_DataReceived);

            this.restartTimer();

            this.isCalibrating = true;

            // Check if the values of the WTX device are equal to the calibration value. It is also checked within a certain interval if the measurement is noisy.
            if ((this.NetValue != calibrationValue || this.GrossValue != calibrationValue))
            {
                Console.Write("Wait for setting the nomnial weight into the WTX.");
                this.Async_Call(0x00, DataReceivedTimer);
            }
            else
                if (this.NetValue > (calibrationValue + 10) || (this.NetValue < (calibrationValue - 10)))
            {
                Console.Write("Wait for setting the nomnial weight into the WTX.");
                this.Async_Call(0x00, DataReceivedTimer);
            }
            else
                     if (this.GrossValue > (calibrationValue + 10) || (this.GrossValue < (calibrationValue - 10)))
            {
                Console.Write("Wait for setting the nomnial weight into the WTX.");
            }
            else
            {
                Console.Write("Calibration failed, please restart the application");
            }

        }

        private void Write_DataReceived(IDeviceData obj)
        {
            //throw new NotImplementedException();
        }

        // Calculates the values for deadload and nominal load in d from the inputs in mV/V
        // and writes the into the WTX registers.
        public void Calculate(double Preload, double Capacity)
        {
            double MultiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)

            double DPreload = Preload * MultiplierMv2D;
            double DNominalLoad = DPreload + (Capacity * MultiplierMv2D);

            this.stopTimer();

            //write reg 48, DPreload;         

            this.writeOutputWordS32(Convert.ToInt32(DPreload), 48, Write_DataReceived);

            this.SyncCall_Write_Command(0, 0x80, Write_DataReceived);

            //write reg 50, DNominalLoad;          

            this.writeOutputWordS32(Convert.ToInt32(DNominalLoad), 50, Write_DataReceived);

            this.SyncCall_Write_Command(0, 0x100, Write_DataReceived);

            this.isCalibrating = true;

            this.restartTimer();
        }

        public void MeasureZero()
        {
            this.stopTimer();

            //todo: write reg 48, 0x7FFFFFFF

            this.writeOutputWordS32(0x7FFFFFFF, 48, Write_DataReceived);

            Console.Write(".");

            this.SyncCall_Write_Command(0, 0x80, Write_DataReceived);

            if ((this.NetValue != 0 || this.GrossValue != 0))
            {
                Console.Write("Wait for setting the dead load into the WTX.");
                this.Async_Call(0x00, DataReceivedTimer);
            }
            else
            if (this.NetValue > (0 + 10) || (this.NetValue < (0 - 10)))
            {
                Console.Write("Wait for setting the dead load into the WTX.");
                this.Async_Call(0x00, DataReceivedTimer);
            }
            else
              if (this.GrossValue > (0 + 10) || (this.GrossValue < (0 - 10)))
            {
                Console.Write("Wait for setting the dead load into the WTX.");
            }
        }

        public bool Calibrating
        {
            get { return this.isCalibrating; }
            set { this.isCalibrating = value; }
        }
    }
}

