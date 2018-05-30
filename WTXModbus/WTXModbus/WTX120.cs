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
    /*
     *  This class inherits from the abstract class 'DeviceAbstract', which also inherits from interface 'IDeviceValues' presetting 
     *  all necessary values for your application. 
     * 
     *  This class realizes the handling of the read data from your WTX120 weighting terminal. It needs an object of ModbusConnection to 
     *  have access to the read data and to instruct writing to the WTX120. 
     *  This class implements as well a timer to read the values periodic according to the timer interval given to the constructor of this
     *  class. The timer is started in the constructor and the data is read after the interval in the method 'OnTimedEvent(..)' by calling
     *  the method 'Async_Call(0x00,DataReceivedTimer)'. 
     *  
     *  The method 'DataReceivedTimer' is a callback method, which updates the interface IDeviceValues with the new data read from the WTX120.
     *  The reading of the WTX registers is done asynchronously by the implementation of a 'Backgroundworker' which allows to define
     *  eventhandler methods while reading and when the reading is done. 
     *  
     *  The method 'UpdateEvent(..)' has the data from the WTX120, it gets the data by the call of 'Async_Call(0x00,DataReceivedTimer)' and
     *  the adding of 'UpdatedEvent(..)' to 'RaiseDataEvent(..)' from class ModbusConnection, meaning that a reading of a register has been done. 
     *  In 'UpdateEvent(..)' the data is interpreted and converted to strings. For example the value '2' in word 4, 
     *  bit .2-.3 stands for 'Higher than maximum capacity' or the value '0' in word 5, bit .0-.1 for 'Standard', the application mode. 
     *  Therefore there are methods like : 'measurement_with_comma(..)', 'comment_weight_moving(..)', 'comment_unit(..)' and so on. 
     *  The values can be called simply by 'this.NetValue' or 'this.status', because this class inherits from interface IDeviceValues having
     *  all necessary values. 
     *  
     *  In method 'UpdateEvent' a event is triggered as well to signalize the GUI or console application that the data is ready to be
     *  printed on the console or GUI (on the DataGrid). By the eventHandler 'DataUpdateEvent' it is signalized that the data is ready to
     *  be printed out.
     *  
     *  The methods 'Calibrate' and 'Calculate' are also given in this class, which do the calibration with a nominal load, a dead load and 
     *  a individual weight.
     */
    public class WTX120 : DeviceAbstract     
    {
        private string[] dataStr;
        private ushort[] data;

        private ushort[] previousData;

        private System.Timers.Timer aTimer;
        private bool isNet;
        private bool isCalibrating;
        private bool isRefreshed;

        private ModbusConnection ModbusConnObj;
        private IDeviceValues thisValues;
        
        private ushort command;
        private bool compareDataChanged;

        private Action<IDeviceValues> callback_obj;
        ushort[] data_written;

        public override event EventHandler<NetConnectionEventArgs<ushort[]>> DataUpdateEvent;

        /*
         * Constructor of the class WTX120Modbus, which inherits from class DeviceAbstract. 
         * Therefore you have an extended constructor (with " : base(..)") 
         *
         * @param : connection - object of class ModbusConnection, which is created in the console or GUI application.
         * @param : paramTimerInterval - timer interval for the periodic reading of the values, in milli-seconds. 
         */
        public WTX120(ModbusConnection connection, int paramTimerInterval) : base(connection,paramTimerInterval)
        {
            this.ModbusConnObj = connection;
            
            this.data         = new ushort[59];
            this.previousData = new ushort[59];
            this.dataStr      = new string[59];
            this.data_written = new ushort[2];

            this.compareDataChanged = false;
            this.isCalibrating      = false;
            this.isRefreshed        = false;

            for (int i = 0; i < 59; i++)
            {
                this.dataStr[i] = "0";
                this.data[i] = 0;
                this.previousData[i] = 0;
            }
            
            this.initialize_timer(paramTimerInterval);          // Initializing and starting the timer. 
        }


        /* 
         *  This is a auto-property to get the object of ModbusConnection, which is committed to the constructor of this class. 
         */
        public override ModbusConnection getConnection
        {
            get
            {
                return this.ModbusConnObj;
            }

        }

        /*
         * This method realizes an asynchronous call to read (command=0x00) and to write(else command!=0x00). It uses the 'BackgroundWorker Class' 
         * to execute an operation on a seperate thread. To set up background operations f.e. you have to raise the 'DoWork' event (here in 'DoWorkEventHandler')
         * for the ongoing reading operation (ongoing because of the timer). 
         * To signalize if the operation(reading or writing) has been completed and finished you have to raise a 'RunWorkerCompleted' event.
         * With the interface you get in the method 'ReadCompleted(..)' you have already the data, but in method 'UpdateEvent(..)' the data is interpreted
         * and converted to strings, which is a preferred way in this application.  
         * 
         */
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

        /*
         * This method implements synchrounous writing to the WTX120 for the purpose of calibration, because you have to send several commands in a row to the
         * WTX120 to calibrate and wait after each command is finished. 
         * (Like writing 0x7FFFFFFF for the zero load, writing the calibration value and writing 0x7FFFFFFF again for setting the nominal load.)
         * So you have a polling to wait till the handshake bit is set to 1 (=1, if the command is set) and 0 (=0,if the command is reset).
         * @param : wordnumber - the word of the register which should be rewritten. commandParam - the command, 0x00=Reading everthying else is Writing
         * @param : callbackParam - the callback method which is called once the writing is finished (not used here for the synchronous reading). 
         */
        public void SyncCall_Write_Command(ushort wordNumber, ushort commandParam, Action<IDeviceValues> callbackParam)     
        {
            this.command = commandParam;
            this.callback_obj = callbackParam;

            if (this.command == 0x00)
                getConnection.Read();

            else
            {
                // (1) Sending of a command:        
                getConnection.Write(wordNumber, this.command);  

                while (this.handshake == 0)
                {
                    getConnection.Read();
                }

                // (2) If the handshake bit is equal to 0, the command has to be set to 0x00.
                if (this.handshake == 1)
                {
                    getConnection.Write(wordNumber, 0x00);    
                }
                while (this.handshake == 1)
                {
                    getConnection.Read();
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
        
        public IDeviceValues syncReadData()
        {
            getConnection.Read();

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
            this.callback_obj((IDeviceValues)e.Result);         // Commit the interface
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
            this.callback_obj(this);         // Commit the interface
        }

        public void writeOutputWordS32(int load_written,ushort wordNumber, Action<IDeviceValues> callbackParam)
        {
            this.callback_obj = callbackParam;

            data_written[0] = (ushort)((load_written & 0xffff0000) >> 16);
            data_written[1] = (ushort)(load_written & 0x0000ffff);

            getConnection.Write(wordNumber, data_written);
        }

        public void writeOutputWordU08(int load_written, ushort wordNumber, Action<IDeviceValues> callbackParam)
        {
            this.callback_obj = callbackParam;

            data_written[0] = (ushort)((load_written & 0x000000ff));

            getConnection.Write(wordNumber, data_written[0]);
        }

        public void writeOutputWordU16(int load_written, ushort wordNumber, Action<IDeviceValues> callbackParam)
        {
            this.callback_obj = callbackParam;

            data_written[0] = (ushort)((load_written & 0xffff0000) >> 16);

            getConnection.Write(wordNumber, data_written[0]);
        }

        // This method initializes the with the timer interval as a parameter: 
        public override void initialize_timer(int timer_interval)
        {
            // Create a timer with an interval of 500ms. 
            aTimer = new System.Timers.Timer(timer_interval);

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
        }

        /*
         * This method is a callback method, which is called once the reading is done and completed. 
         * It also couples the method 'UpdateEvent(..)' to the eventHandler 'RaiseDataEvent' from the class ModbusConnection,
         * signalizing that the data is read from the WTX device. 
         */
        private void DataReceivedTimer(IDeviceValues Device_Values)
        {
            getConnection.RaiseDataEvent += UpdateEvent;   // Subscribe to the event.

            thisValues = Device_Values;

            int previousNetValue = Device_Values.NetValue;

        }

        public override void Calibration(ushort command)
        {
            // Set zero, set nominal, set calibration weight... siehe anderen Code. 
        }

        public override void UpdateEvent(object sender, NetConnectionEventArgs<ushort[]> e)
        {
            this.data = e.Args;

            this.dataStr[0] = this.measurement_with_comma(this.NetValue, this.decimals);
            this.dataStr[1] = this.measurement_with_comma(this.GrossValue, this.decimals);

            this.dataStr[2] = this.generalWeightError.ToString();
            this.dataStr[3] = this.scaleAlarmTriggered.ToString();
            this.dataStr[4] = this.comment_limit_status();
            this.dataStr[5] = this.comment_weight_moving();

            this.dataStr[6] = this.scaleSealIsOpen.ToString();
            this.dataStr[7] = this.manualTare.ToString();
            this.dataStr[8] = this.comment_weight_type();
            this.dataStr[9] = this.comment_scale_range();

            this.dataStr[10] = this.zeroRequired.ToString();
            this.dataStr[11] = this.weightWithinTheCenterOfZero.ToString();
            this.dataStr[12] = this.weightInZeroRange.ToString();
            this.dataStr[13] = this.comment_application_mode();

            this.dataStr[14] = this.decimals.ToString();
            this.dataStr[15] = this.comment_unit();
            this.dataStr[16] = this.handshake.ToString();
            this.dataStr[17] = this.comment_status();

            this.dataStr[18] = this.input1.ToString();
            this.dataStr[19] = this.input2.ToString();
            this.dataStr[20] = this.input3.ToString();
            this.dataStr[21] = this.input4.ToString();

            this.dataStr[22] = this.output1.ToString();
            this.dataStr[23] = this.output2.ToString();
            this.dataStr[24] = this.output3.ToString();
            this.dataStr[25] = this.output4.ToString();

            if (this.applicationMode == 0)
            {
                this.dataStr[26] = this.limitStatus1.ToString();
                this.dataStr[27] = this.limitStatus2.ToString();
                this.dataStr[28] = this.limitStatus3.ToString();
                this.dataStr[29] = this.limitStatus4.ToString();

                this.dataStr[30] = this.weightMemDay.ToString();
                this.dataStr[31] = this.weightMemMonth.ToString();
                this.dataStr[32] = this.weightMemYear.ToString();
                this.dataStr[33] = this.weightMemSeqNumber.ToString();
                this.dataStr[34] = this.weightMemGross.ToString();
                this.dataStr[35] = this.weightMemNet.ToString();
            }
            else
                if (this.applicationMode == 2 || this.applicationMode == 0) // in filler mode 
            {
                this.dataStr[26] = this.coarseFlow.ToString();
                this.dataStr[27] = this.fineFlow.ToString();
                this.dataStr[28] = this.ready.ToString();
                this.dataStr[29] = this.reDosing.ToString();

                this.dataStr[30] = this.emptying.ToString();
                this.dataStr[31] = this.flowError.ToString();
                this.dataStr[32] = this.alarm.ToString();
                this.dataStr[33] = this.ADC_overUnderload.ToString();

                this.dataStr[34] = this.maxDosingTime.ToString();
                this.dataStr[35] = this.legalTradeOp.ToString();
                this.dataStr[36] = this.toleranceErrorPlus.ToString();
                this.dataStr[37] = this.toleranceErrorMinus.ToString();

                this.dataStr[38] = this.status.ToString();
                this.dataStr[39] = this.generalScaleError.ToString();
                this.dataStr[40] = this.fillingProcessStatus.ToString();
                this.dataStr[41] = this.numberDosingResults.ToString();

                this.dataStr[42] = this.dosingResult.ToString();
                this.dataStr[43] = this.meanValueDosingResults.ToString();
                this.dataStr[44] = this.standardDeviation.ToString();
                this.dataStr[45] = this.totalWeight.ToString();

                this.dataStr[46] = this.fineFlowCutOffPoint.ToString();
                this.dataStr[47] = this.coarseFlowCutOffPoint.ToString();
                this.dataStr[48] = this.actualDosingTime.ToString();
                this.dataStr[49] = this.actualCoarseFlowTime.ToString();

                this.dataStr[50] = this.actualFineFlowTime.ToString();
                this.dataStr[51] = this.parameterSetProduct.ToString();

                this.dataStr[52] = this.filler_weight_memory_day.ToString();
                this.dataStr[53] = this.filler_weight_memory_month.ToString();
                this.dataStr[54] = this.filler_weight_memory_year.ToString();
                this.dataStr[55] = this.filler_weight_memory_seq_number.ToString();
                this.dataStr[56] = this.filler_weight_memory_gross.ToString();
                this.dataStr[57] = this.filler_weight_memory_net.ToString();
            }

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

            if ((this.compareDataChanged == true) || (this.isCalibrating == true) || this.isRefreshed==true)   // 'isCalibrating' indicates if a calibration is done just before ...
                                                                                                               // and the data should be send to the GUI/console and be printed out. 
                                                                                                               // If the GUI has been refreshed, the values should also be send to the GUI/Console and be printed out. 
            {
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
            get { return this.compareDataChanged;  }
            set { this.compareDataChanged = value; }
        }

        public bool Calibrating
        {
            get { return this.isCalibrating; }
            set { this.isCalibrating = value; }
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
        public override  int limitStatus
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
        public override  int scaleSealIsOpen
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
        public override  int manualTare
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
        public override  int weightType
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
        public override  int scaleRange
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
        public override  int zeroRequired
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
        public override  int weightWithinTheCenterOfZero
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
        public override  int weightInZeroRange
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
        public override  int applicationMode
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

        public override  string[] getDataStr
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

        public override  int input1
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
        public override  int input2
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
        public override  int input3
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
        public override  int input4
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
        public override  int output2
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
        public override  int output3
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
        public override  int output4
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
        public override  int limitStatus2
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
        public override  int limitStatus3
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
        public override  int limitStatus4
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
        public override  int weightMemDay
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
        public override  int weightMemMonth
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
        public override  int weightMemYear
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
        public override  int weightMemSeqNumber
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
        public override  int weightMemGross
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
        public override  int weightMemNet
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
        public override  int coarseFlow
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
        public override  int fineFlow
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
        public override  int reDosing
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
        public override  int flowError
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
        public override  int ADC_overUnderload
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
        public override  int maxDosingTime
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
        public override  int legalTradeOp
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
        public override  int toleranceErrorPlus
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
        public override  int toleranceErrorMinus
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
        public override  int statusInput1
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
        public override  int generalScaleError
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
        public override  int fillingProcessStatus
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
        public override  int numberDosingResults
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
        public override  int dosingResult
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
        public override  int meanValueDosingResults
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
        public override  int standardDeviation
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
        public override  int totalWeight
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
        public override  int actualDosingTime
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
        public override  int actualCoarseFlowTime
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
        public override  int actualFineFlowTime
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

        public bool get_is_net
        {
            get
            {
                return this.isNet;
            }
        }



        /* In the following methods the different options for the single integer values are used to define and
         *interpret the value. Finally a string should be returned from the methods to write it onto the GUI Form. 
         */
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
            if (this.weightMoving == 0)
                return "0=Weight is not moving.";
            else
                if (this.weightMoving == 1)
                return "1=Weight is moving";
            else
                return "Error";
        }
        private string comment_limit_status()
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
        private string comment_weight_type()
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
        private string comment_scale_range()
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
        private string comment_application_mode()
        {
            if (this.applicationMode == 0)
                return "Standard";
            else

                if (this.applicationMode == 2 || this.applicationMode == 1)  // Will be changed to '2', so far '1'. 
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

        // Calculates the values for deadload and nominal load in d from the inputs in mV/V
        // and writes the into the WTX registers.
        public void Calculate(double Preload, double Capacity)
        {
            double MultiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)

            double DPreload = Preload * MultiplierMv2D;
            double DNominalLoad = DPreload + (Capacity * MultiplierMv2D);

            //write reg 48, DPreload;         

            this.writeOutputWordS32(Convert.ToInt32(DPreload), 48, Write_DataReceived);

            this.SyncCall_Write_Command(0, 0x80, Write_DataReceived);

            //write reg 50, DNominalLoad;          

            this.writeOutputWordS32(Convert.ToInt32(DNominalLoad), 50, Write_DataReceived);

            this.SyncCall_Write_Command(0, 0x100, Write_DataReceived);

            this.isCalibrating = true;

            this.restartTimer();
        }

        private void Write_DataReceived(IDeviceValues obj)
        {
            throw new NotImplementedException();
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
            if ((this.NetValue!=calibrationValue || this.GrossValue != calibrationValue)) 
            {
                Console.Write("Wait for setting the nomnial weight into the WTX.");
                this.Async_Call(0x00, DataReceivedTimer);
            }
            else
                if(this.NetValue > (calibrationValue+10) || (this.NetValue < (calibrationValue -10)))
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

        public void MeasureZero()
        {
            //todo: write reg 48, 0x7FFFFFFF
            
            this.writeOutputWordS32(0x7FFFFFFF, 48,Write_DataReceived);

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


    }
}
