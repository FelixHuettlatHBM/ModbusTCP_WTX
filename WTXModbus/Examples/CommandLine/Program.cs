
/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120_Modbus | 01/2018
 * 
 * Author : Felix Huettl 
 * 
 * Console application for WTX120_Modbus MODBUS TCPIP 
 * 
 *  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using System.Timers;
using System.Globalization;

using HBM.Weighing.API;
using HBM.Weighing.API.WTX;
using HBM.Weighing.API.WTX.Jet;
using HBM.Weighing.API.WTX.Modbus;

namespace WTXModbus
{
    /// <summary>
    /// This class implements a console application. An Object of the class 'ModbusTcpConnection' or 'JetBusConnection' and 'BaseWTDevice'('WTXJet' or WTXModbus') 
    /// are initialized as a publisher and subscriber. Afterwards a connection to the device is established and the timer/sending interval is set. 
    /// A timer with for example 500ms is created. After 500ms an event is triggered, which executes the method "Update" reading the register of the device
    /// by an asynchronous call in the method '_wtxDevice.Async_Call'. As soon as the reading is finisihed, the callback method "Read_DataReceived" takes over the
    /// new data , which have already been interpreted in class 'WTX120_Modbus', so the data is given as a string array. 
    /// The data is also printed on the console in the callback method "Read_DataReceived". 
    /// Being in the while-loop it is possible to select commands to the device. For example taring, change from gross to net value, stop dosing, zeroing and so on. 
    /// 
    /// This is overall just a simple application as an example. A significantly broad presentation is given by the windows form, but for starters it is recommended. 
    /// </summary>

    static class Program
    {

        #region Locales
        private const string DEFAULT_IP_ADDRESS = "192.168.100.88";

        private static string mode = "";
        private const string MESSAGE_CONNECTION_FAILED = "Connection failed!";
        private const string MESSAGE_CONNECTING = "Connecting...";

        private const int WAIT_DISCONNECT = 2000; 
        
        private static BaseWtDevice _wtxDevice;
        //private static ModbusTcpConnection _modbusObj;
        //private static JetBusConnection _jetObj;
        
        private static string _ipAddress = DEFAULT_IP_ADDRESS ;     // IP-adress, set as the first argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 
        private static int _timerInterval = 200;                    // timer interval, set as the second argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 
        private static ushort _inputMode;                           // inputMode (number of input bytes), set as the third argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 

        private static ConsoleKeyInfo _valueOutputwords;
        private static ConsoleKeyInfo _valueExitapplication;

        private static string _calibrationWeight = "0";

        private static string _preloadStr, _capacityStr;

        private static double _preload, _capacity;
        private static IFormatProvider _provider;

        private const double MULTIPLIER_MV2_D = 500000;      //   2 / 1000000; // 2mV/V correspond 1 million digits (d)
        private static string _strCommaDot ="";
        private static double _doubleCalibrationWeight =0.0, _potenz=0.0;

        private static bool _isCalibrating = false;        // For checking if the WTX120_Modbus device is calculating at a moment after the command has been send. If 'isCalibrating' is true, the values are not printed on the console. 
        private static bool _showAllInputWords  = false;
        private static bool _showAllOutputWords = false;

        #endregion

        static void Main(string[] args)
        {
            // Input for the ip adress, the timer interval and the input mode: 

            _inputMode = 6;
            _timerInterval = 200;

            DefineInputs(args);

            // Initialize:

            _provider = CultureInfo.InvariantCulture;

            Console.WriteLine("\nTCPModbus Interface for weighting terminals of HBM\nEnter e to exit the application");

            do // do-while loop for the connection establishment. If the connection is established successfully, the do-while loop is left/exit. 
            {
                InitializeConnection();

                _wtxDevice.DataUpdateEvent += Update;   // To get updated values from the WTX, use method Update(..). 

            } while (_wtxDevice.getConnection.IsConnected==false);


            MenuCases();

            //Get a single value from the WTX device : 
            int x = _wtxDevice.NetValue; 

        } // end main  



        private static void DefineInputs(string[] args)
        {
            if (args.Length > 0)
                mode = args[0];
            else
                mode = "Jetbus";

            if (args.Length > 1)
                _ipAddress = args[1];
            else
            {
                Console.WriteLine(" WTX - For connection establishment : No IP adress is given. \n please enter the IP adress to the WTX device, format : aaa.bbb.ccc.dd");
                _ipAddress = Console.ReadLine();
            }
            if (args.Length > 2)
                _timerInterval = Convert.ToInt32(args[2]);
            else
                _timerInterval = 200;

        }

        #region Connection
        // This method connects to the given IP address
        private static void InitializeConnection()
        {

            if (mode == "Modbus" || mode == "modbus")    // If 'Modbus/Tcp' is selected: 
            {
                // Creating objects of ModbusTcpConnection and WTXModbus: 
                ModbusTcpConnection _modbusConection = new ModbusTcpConnection(_ipAddress);

                _wtxDevice = new WtxModbus(_modbusConection, _timerInterval);

                _wtxDevice.getConnection.NumofPoints = 6;
            }
            else
            {
                if (mode == "Jet" || mode == "jet" || mode == "Jetbus" || mode == "jetbus")  // If 'JetBus' is selected: 
                {
                    // Creating objects of JetBusConnection and WTXJet: 
                    JetBusConnection _jetConnection = new JetBusConnection(_ipAddress, "Administrator", "wtx");

                    _wtxDevice = new WtxJet(_jetConnection);
                }
            }

            // Connection establishment via Modbus or Jetbus :  
            try
            {
                _wtxDevice.Connect();
            }
            catch (Exception)
            {
                Console.WriteLine(MESSAGE_CONNECTION_FAILED);
            }


            if (_wtxDevice.getConnection.IsConnected == true)
            {
                // Coupling the data via an event-based call - If the event in class WTX120_Modbus is triggered, the values are updated on the console: Already done in main for presentation. 
                //_wtxDevice.DataUpdateEvent += Update;

                Update(null, null);

                Console.WriteLine("\nThe connection has been established successfully.\nThe values of the WTX device are printed on the console ... :");

                WTXModbusExamples.Properties.Settings.Default.IPaddress = _ipAddress;
                WTXModbusExamples.Properties.Settings.Default.Save();
            }
            else
            {
                Console.WriteLine(MESSAGE_CONNECTION_FAILED);
                Console.WriteLine("\nFailure : The connection has not been established successfully.\nPlease enter a correct IP Adress for the connection establishment...");               
                _ipAddress = Console.ReadLine();
            }

        } // End method Connect() 


        private static void MenuCases()
        {
            // This while loop is repeated till the user enters 'e' (=e meaning exit). After the timer interval the register of the device is read out. 
            // In the while-loop the user can select commands, which are send immediately to the device. 
            while (_valueExitapplication.KeyChar != 'e')
            {
                _isCalibrating = false;
                _valueOutputwords = Console.ReadKey();
                int valueOutput = Convert.ToInt32(_valueOutputwords.KeyChar);

                if (mode == "Modbus" || mode == "modbus")
                {
                    switch (_valueOutputwords.KeyChar)
                    {

                        case '0': _wtxDevice.taring(Write_DataReceived); break;                  // Taring 
                        case '1': _wtxDevice.gross(Write_DataReceived); break;                   // Gross/Net
                        case '2': _wtxDevice.zeroing(Write_DataReceived); break;                 // Zeroing
                        case '3': _wtxDevice.adjustZero(Write_DataReceived); break;              // Adjust zero 
                        case '4': _wtxDevice.adjustNominal(Write_DataReceived); break;           // Adjust nominal
                        case '5': _wtxDevice.activateData(Write_DataReceived); break;            // Activate data
                        case '6': _wtxDevice.manualTaring(Write_DataReceived); break;            // Manual taring
                        case '7': _wtxDevice.recordWeight(Write_DataReceived); break;            // Record Weight

                        // 'c' for writing on multiple registers, which is necessary for the calibration. 
                        case 'c':       // Calculate Calibration
                            CalculateCalibration();
                            break;
                        case 'w':       // Calculation with weight 
                            CalibrationWithWeight();
                            break;
                        case 'a':  // Show all input words in the filler application. 
                            if (_showAllInputWords == false)
                            {
                                _showAllInputWords = true;
                                //_wtxDevice.Refreshed = true;
                            }
                            else
                                if (_showAllInputWords == true)
                            {
                                _showAllInputWords = false;
                                //_wtxDevice.Refreshed = true;
                            }
                            break;

                        case 'o': // Writing of the output words

                            if (_showAllOutputWords == false)
                            {
                                _showAllOutputWords = true;
                                //_wtxDevice.Refreshed = true;
                            }
                            else
                            if (_showAllOutputWords == true)
                            {
                                _showAllOutputWords = false;
                                //_wtxDevice.Refreshed = true;
                            }

                            break;

                        // Change connection from Modbus to Jetbus: 
                        case 'j':
                            _wtxDevice.DataUpdateEvent -= Update;   // Delete Callback method 'Update' from the Eventhandler 'DataUpdateEvent'.

                            mode = "Jetbus";

                            if (_wtxDevice != null)    // Necessary to check if the object of BaseWtDevice have been created and a connection exists. 
                            {
                                _wtxDevice.getConnection.Disconnect();
                                _wtxDevice = null;
                            }

                            Thread.Sleep(WAIT_DISCONNECT);     // Wait for 2 seconds till the disconnection request is finished. 

                            InitializeConnection();
                            _wtxDevice.DataUpdateEvent += Update;   // To get updated values from the WTX, use method Update(..). 
                            break;


                        default: break;

                    }   // end switch-case
                }

                else
                    if(mode=="Jet" || mode=="Jetbus" || mode =="jet" || mode =="jetbus")
                    {
                    switch (_valueOutputwords.KeyChar)
                    {
                        case '0': _wtxDevice.taring(Write_DataReceived); break;                  // Taring 
                        case '1': _wtxDevice.gross(Write_DataReceived); break;                   // Gross/Net
                        case '2': _wtxDevice.zeroing(Write_DataReceived); break;                 // Zeroing
                        case '3': _wtxDevice.adjustZero(Write_DataReceived); break;              // Adjust zero 
                        case '4': _wtxDevice.adjustNominal(Write_DataReceived); break;           // Adjust nominal
                        case '5': _wtxDevice.activateData(Write_DataReceived); break;            // Activate data
                        case '6': _wtxDevice.manualTaring(Write_DataReceived); break;            // Manual taring
                        case '7': _wtxDevice.recordWeight(Write_DataReceived); break;            // Record Weight

                        // 'c' for writing on multiple registers, which is necessary for the calibration. 
                        case 'c':       // Calculate Calibration
                            CalculateCalibration();
                            break;
                        case 'w':       // Calculation with weight 
                            CalibrationWithWeight();
                            break;
                        // Change connection from Jetbus to Modbus: 
                        case 'j':
                           
                            _wtxDevice.DataUpdateEvent -= Update;   // Delete Callback method 'Update' from the Eventhandler 'DataUpdateEvent'.

                            mode = "Modbus";

                            if (_wtxDevice != null)    // Necessary to check if the object of BaseWtDevice have been created and a connection exists. 
                            {
                                _wtxDevice.getConnection.Disconnect();
                                _wtxDevice = null;
                            }

                            Thread.Sleep(WAIT_DISCONNECT);     // Wait for 2 seconds till the disconnection request is finished. 

                            InitializeConnection();
                            _wtxDevice.DataUpdateEvent += Update;   // To get updated values from the WTX, use method Update(..). 
                            break;

                        default: break;

                    }   // end switch-case

                } // end if

                //int valueOutput = Convert.ToInt32(value_outputwords.KeyChar);
                int value = 0;
                if (valueOutput >= 9)
                {// switch-case for writing the additional output words of the filler application: 
                    //InputWriteOutputWordsFiller((ushort)valueOutput, value);
                }

                _valueExitapplication = Console.ReadKey();
                if (_valueExitapplication.KeyChar == 'e')
                    break;

            }// end while
        } // end method MenuCases() 


        // This method sets the number of read bytes (words) in the register of the device. 
        // You can set '1' for the net value. '2','3' or '4' for the net and gross value. '5' for the net,gross value and the weight status(word[4]) for the bits representing the weight status like weight moving, weight type, scale range and so ... 
        // You can set '6' for reading the previous bytes(gross/net values, weight status) and for enabling to write on the register. With '6', bit "application mode", "decimals", "unit", "handshake" and "status" is read. 
        // Especially the handshake and status bit is used for writing. 
        private static void set_number_inputs()
        {

            Console.WriteLine("Please enter how many words(bytes) you want to read from the register\nof the device. See the following table for choosing:");

            Console.WriteLine("\nEnter '1'     : Enables reading of ... \n\t\t  word[0]- netto value.\n");
            Console.WriteLine("Enter '2','3',4': Enables reading of ... \n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");
            Console.WriteLine("Enter '5'       : Enables reading of ... \n\t\t  word[4]- weight moving,weight type,scale range,..(see manual)\n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");
            Console.WriteLine("Enter '6'       : Enables writing to the register and reading of ... \n\t\t  word[5]- application mode,decimals,unit,handshake,status bit\n\t\t  word[4]- weight moving,weight type,scale range,..(see manual)\n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");

            Console.WriteLine("It is recommended to use at least '6' for writing and reading. \nDefault setting for the full application in filler mode : '38'\nPlease tip the button 'Enter' after you typed in the number '1' or '2' or...'6'");

            _inputMode = (ushort)Convert.ToInt32(Console.ReadLine());

        }
        /*
         * This method calcutes the values for a dead load and a nominal load(span) in a ratio in mV/V and write in into the WTX registers. 
         */
         
        private static void CalculateCalibration()
        {
            _isCalibrating = true;

            zero_load_nominal_load_input();
           
            _wtxDevice.Calculate(_preload,_capacity);
            
            _isCalibrating = false;           
        }

        /*
         * This method does a calibration with an individual weight to the WTX.  
         * First you tip the value for the calibration weight, then you set the value for the dead load (method ‚MeasureZero‘), 
         * finally you set the value for the nominal weight in the WTX (method ‚Calibrate(calibrationValue)‘).
         */
        private static void CalibrationWithWeight()
        {
            _isCalibrating = true;

            //WTXObj.stopTimer();    // The timer is stopped in the method 'Calculate(..)' in class WTX120_Modbus.

            Console.Clear();
            Console.WriteLine("\nPlease tip the value for the calibration weight and tip enter to confirm : ");
            _calibrationWeight = Console.ReadLine();

            Console.WriteLine("\nTo start : Set zero load and press any key for measuring zero and wait.");
            string another = Console.ReadLine();

            _wtxDevice.MeasureZero();
            Console.WriteLine("\n\nDead load measured.Put weight on scale, press any key and wait.");

            string another2 = Console.ReadLine();

            _wtxDevice.Calibrate(PotencyCalibrationWeight(), _calibrationWeight);

            //WTXObj.restartTimer();   // The timer is restarted in the method 'Calculate(..)' in class WTX120_Modbus.

            _isCalibrating = false;
        }  

        /*
         * This method potentiate the number of the values decimals and multiply it with the calibration weight(input) to get
         * an integer which is in written into the WTX registers by the method Calibrate(potencyCalibrationWeight()). 
         */
        private static int PotencyCalibrationWeight()
        {

            _strCommaDot = _calibrationWeight.Replace(".", ","); // Transformation into a floating-point number.Thereby commas and dots can be used as input for the calibration weight.
            _doubleCalibrationWeight = double.Parse(_strCommaDot);                  

            _potenz = Math.Pow(10, _wtxDevice.Decimals); // Potentisation by 10^(decimals). 
            
            return (int) (_doubleCalibrationWeight * _potenz); // Multiplying of the potentiated values with the calibration weight, ...
                                                             // ...casting to integer (easily possible because of the multiplying with ... 
                                                             // ...the potensied value) and returning of the value. 
        }

        // This method prints the values of the device (as a integer and the interpreted string) as well as the description of each bit. 
        private static void Update(object sender, DataEvent e)
        {
            // The description and the value of the WTX are only printed on the console if the Interface, containing all auto-properties of the values is 
            // not null (respectively empty) and if no calibration is done at that moment.

            if (_wtxDevice != null/* && (_isCalibrating==false)*/)
            {
                Console.Clear();               

                if (_wtxDevice.ApplicationMode == 0)  // If the WTX device is in standard application/mode.
                {
                    Console.WriteLine("0-Taring | 1-Gross/net  | 2-Zeroing  | 3- Adjust zero | 4-Adjust nominal |\n5-Activate Data \t| 6-Manual taring \t      | 7-Weight storage\n");
                }
                else
                    if (_wtxDevice.ApplicationMode == 1 || _wtxDevice.ApplicationMode == 2) // If the WTX device is in filler application/mode.
                    {

                    if(_showAllInputWords==false && mode=="Modbus")
                    Console.WriteLine("\n0-Taring  | 1-Gross/net  | 2-Clear dosing  | 3- Abort dosing | 4-Start dosing| \n5-Zeroing | 6-Adjust zero| 7-Adjust nominal| 8-Activate data | 9-Weight storage|m-Manual redosing | j-Connection to Jetbus | a-Show all input words 0 to 37 |\no-Show output words 9-44 | b-Bytes read from the register |\nc-Calculate Calibration | w-Calibration with weight | e-Exit the application\n");
                    else
                    if (_showAllInputWords == true && mode == "Modbus")
                        Console.WriteLine("\n0-Taring  | 1-Gross/net  | 2-Clear dosing  | 3- Abort dosing | 4-Start dosing| \n5-Zeroing | 6-Adjust zero| 7-Adjust nominal| 8-Activate data | 9-Weight storage|m-Manual redosing | j-Connection to Modbus | a-Show only input word 0 to 5 |\nb-Bytes read from the register |\nc-Calculate Calibration | w-Calibration with weight | e-Exit the application\n");

                    if (mode == "Jet" || mode == "Jetbus" || mode == "jet" || mode == "jetbus")
                        Console.WriteLine("\n0-Taring  | 1-Gross/net  | 2-Clear dosing  | 3- Abort dosing | 4-Start dosing| \n5-Zeroing | 6-Adjust zero| 7-Adjust nominal| 8-Activate data | 9-Weight storage|m-Manual redosing | j-Connection to Modbus | \nc-Calculate Calibration | w-Calibration with weight | e-Exit the application\n");

                }

                if (_wtxDevice.ApplicationMode == 0)   // If the device is in the standard mode (standard=0; filler=1 or filler=2) 
                {

                    // The values are printed on the console according to the input - "numInputs": 

                    if (_inputMode == 1)
                    {
                        Console.WriteLine("Net value:                     " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.NetValue, _wtxDevice.Decimals) + "\t  As an Integer:  " + _wtxDevice.NetValue);
                    }
                    else
                        if (_inputMode == 2 || _inputMode == 3 || _inputMode == 4)
                    {
                        Console.WriteLine("Net value:                     " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.NetValue, _wtxDevice.Decimals) +   "\t  As an Integer:  " + _wtxDevice.NetValue);
                        Console.WriteLine("Gross value:                   " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.GrossValue, _wtxDevice.Decimals) + "\t  As an Integer:  " + _wtxDevice.GrossValue);
                    }
                    else
                            if (_inputMode == 5)
                    {
                        Console.WriteLine("Net value:                     " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.NetValue, _wtxDevice.Decimals) +   "\t  As an Integer:  " + _wtxDevice.NetValue);
                        Console.WriteLine("Gross value:                   " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.GrossValue, _wtxDevice.Decimals) + "\t  As an Integer:  " + _wtxDevice.GrossValue);
                        Console.WriteLine("General weight error:          " + _wtxDevice.GeneralWeightError.ToString() + "\t  As an Integer:  " + _wtxDevice.GeneralWeightError);
                        Console.WriteLine("Scale alarm triggered:         " + _wtxDevice.LimitStatus.ToString() +        "\t  As an Integer:  " + _wtxDevice.LimitStatus);
                        Console.WriteLine("Scale seal is open:            " + _wtxDevice.ScaleSealIsOpen.ToString() +    "\t  As an Integer:  " + _wtxDevice.ScaleSealIsOpen);
                        Console.WriteLine("Manual tare:                   " + _wtxDevice.ManualTare.ToString() +         "\t  As an Integer:  " + _wtxDevice.ManualTare);
                        Console.WriteLine("Weight type:                   " + _wtxDevice.WeightType + "\t  As an Integer:  " + _wtxDevice.WeightType);        //_wtxDevice.WeightTypeStringComment()
                        Console.WriteLine("Scale range:                   " + _wtxDevice.ScaleRange + "\t  As an Integer:  " + _wtxDevice.ScaleRange);        //_wtxDevice.ScaleRangeStringComment()
                        Console.WriteLine("Zero required/True zero:       " + _wtxDevice.ZeroRequired.ToString() +       "\t  As an Integer:  " + _wtxDevice.ZeroRequired);
                        Console.WriteLine("Weight within center of zero:  " + _wtxDevice.WeightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + _wtxDevice.WeightWithinTheCenterOfZero);
                        Console.WriteLine("Weight in zero range:          " + _wtxDevice.WeightInZeroRange.ToString() +  "\t  As an Integer:  " + _wtxDevice.WeightInZeroRange);

                        Console.WriteLine("Limit status:                  " + _wtxDevice.LimitStatus.ToString() + "        As an Integer:  " + _wtxDevice.LimitStatus);               //_wtxDevice.LimitStatusStringComment()
                        Console.WriteLine("Weight moving:                 " + _wtxDevice.WeightMoving.ToString() + "          As an Integer: " + _wtxDevice.WeightMoving);                //_wtxDevice.WeightMovingStringComment()
                    }
                    else
                    if (_inputMode == 6 || _inputMode == 38)
                    { 
                        Console.WriteLine("Net value:                     " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.NetValue, _wtxDevice.Decimals) +  "\t  As an Integer:  " + _wtxDevice.NetValue);
                        Console.WriteLine("Gross value:                   " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.GrossValue, _wtxDevice.Decimals)+ "\t  As an Integer:  " + _wtxDevice.GrossValue);
                        Console.WriteLine("General weight error:          " + _wtxDevice.GeneralWeightError.ToString() +                  "\t  As an Integer:  " + _wtxDevice.GeneralWeightError);
                        Console.WriteLine("Scale alarm triggered:         " + _wtxDevice.LimitStatus.ToString() +                         "\t  As an Integer:  " + _wtxDevice.LimitStatus);
                        Console.WriteLine("Scale seal is open:            " + _wtxDevice.ScaleSealIsOpen.ToString() +                     "\t  As an Integer:  " + _wtxDevice.ScaleSealIsOpen);
                        Console.WriteLine("Manual tare:                   " + _wtxDevice.ManualTare.ToString() +        "\t  As an Integer:  " + _wtxDevice.ManualTare);
                        Console.WriteLine("Weight type:                   " + _wtxDevice.WeightType + "\t  As an Integer:  " + _wtxDevice.WeightType);        //_wtxDevice.WeightTypeStringComment()
                        Console.WriteLine("Scale range:                   " + _wtxDevice.ScaleRange + "\t  As an Integer:  " + _wtxDevice.ScaleRange);        //_wtxDevice.ScaleRangeStringComment()
                        Console.WriteLine("Zero required/True zero:       " + _wtxDevice.ZeroRequired.ToString() +      "\t  As an Integer:  " + _wtxDevice.ZeroRequired);
                        Console.WriteLine("Weight within center of zero:  " + _wtxDevice.WeightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + _wtxDevice.WeightWithinTheCenterOfZero);
                        Console.WriteLine("Weight in zero range:          " + _wtxDevice.WeightInZeroRange.ToString() + "\t  As an Integer:  " + _wtxDevice.WeightInZeroRange);
                        Console.WriteLine("Application mode:              " + _wtxDevice.ApplicationMode.ToString() + "\t  As an Integer:  " + _wtxDevice.ApplicationMode);  //_wtxDevice.ApplicationModeStringComment()
                        Console.WriteLine("Decimal places:                " + _wtxDevice.Decimals.ToString() +          "\t  As an Integer:  " + _wtxDevice.Decimals);
                        Console.WriteLine("Unit:                          " + _wtxDevice.UnitStringComment() +                       "\t  As an Integer:  " + _wtxDevice.Unit);
                        Console.WriteLine("Handshake:                     " + _wtxDevice.Handshake.ToString() +         "\t  As an Integer:  " + _wtxDevice.Handshake);
                        Console.WriteLine("Status:                        " + statusCommentMethod() + "\t  As an Integer:  " + _wtxDevice.Status);     //_wtxDevice.StatusStringComment()

                        Console.WriteLine("Limit status:                  " + limitCommentMethod() + "       As an Integer:  " + _wtxDevice.LimitStatus);               //_wtxDevice.LimitStatusStringComment()
                        Console.WriteLine("Weight moving:                 " + _wtxDevice.WeightMoving.ToString() + "         As an Integer: " + _wtxDevice.WeightMoving);                //_wtxDevice.WeightMovingStringComment()

                    }
                    else
                        Console.WriteLine("\nWrong input for the number of bytes, which should be read from the register!\nPlease enter 'b' to choose again.");
                }
                else
                    if (_wtxDevice.ApplicationMode == 2 || _wtxDevice.ApplicationMode == 1)
                    {
                    Console.WriteLine("Net value:                     " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.NetValue, _wtxDevice.Decimals) +   "\t  As an Integer:  " + _wtxDevice.NetValue);
                    Console.WriteLine("Gross value:                   " + _wtxDevice.NetGrossValueStringComment(_wtxDevice.GrossValue, _wtxDevice.Decimals) + "\t  As an Integer:  " + _wtxDevice.GrossValue);
                    Console.WriteLine("General weight error:          " + _wtxDevice.GeneralWeightError.ToString() +                   "\t  As an Integer:  " + _wtxDevice.GeneralWeightError);
                    Console.WriteLine("Scale alarm triggered:         " + _wtxDevice.LimitStatus.ToString() +     "\t  As an Integer:  " + _wtxDevice.LimitStatus);
                    Console.WriteLine("Scale seal is open:            " + _wtxDevice.ScaleSealIsOpen.ToString() + "\t  As an Integer:  " + _wtxDevice.ScaleSealIsOpen);
                    Console.WriteLine("Manual tare:                   " + _wtxDevice.ManualTare.ToString() +      "\t  As an Integer:  " + _wtxDevice.ManualTare);
                    Console.WriteLine("Weight type:                   " + _wtxDevice.WeightType +               "\t  As an Integer:  " + _wtxDevice.WeightType);        //_wtxDevice.WeightTypeStringComment()
                    Console.WriteLine("Scale range:                   " + _wtxDevice.ScaleRange +               "\t  As an Integer:  " + _wtxDevice.ScaleRange);        //_wtxDevice.ScaleRangeStringComment()
                    Console.WriteLine("Zero required/True zero:       " + _wtxDevice.ZeroRequired.ToString() +    "\t  As an Integer:  " + _wtxDevice.ZeroRequired);
                    Console.WriteLine("Weight within center of zero:  " + _wtxDevice.WeightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + _wtxDevice.WeightWithinTheCenterOfZero);
                    Console.WriteLine("Weight in zero range:          " + _wtxDevice.WeightInZeroRange.ToString() +           "\t  As an Integer:  " + _wtxDevice.WeightInZeroRange);
                    Console.WriteLine("Application mode:              " + _wtxDevice.ApplicationMode.ToString() +           "\t  As an Integer:  " + _wtxDevice.ApplicationMode);  //_wtxDevice.ApplicationModeStringComment()
                    Console.WriteLine("Decimal places:                " + _wtxDevice.Decimals.ToString() +         "\t  As an Integer:  " + _wtxDevice.Decimals);
                    Console.WriteLine("Unit:                          " + _wtxDevice.UnitStringComment() +                      "\t  As an Integer:  " + _wtxDevice.Unit);
                    Console.WriteLine("Handshake:                     " + _wtxDevice.Handshake.ToString() +        "\t  As an Integer:  " + _wtxDevice.Handshake);
                    Console.WriteLine("Status:                        " + statusCommentMethod() +                    "\t  As an Integer:  " + _wtxDevice.Status);     //_wtxDevice.StatusStringComment()

                    Console.WriteLine("Limit status:                  " + limitCommentMethod() +  "     As an Integer:  " + _wtxDevice.LimitStatus);               //_wtxDevice.LimitStatusStringComment()
                    Console.WriteLine("Weight moving:                 " + _wtxDevice.WeightMoving.ToString() + "          As an Integer:  " + _wtxDevice.WeightMoving);                //_wtxDevice.WeightMovingStringComment()

                    if (_showAllInputWords == true)
                    {

                        Console.WriteLine("Digital input  1:              " + _wtxDevice.Input1.ToString() + "\t  As an Integer:  " + _wtxDevice.Input1);
                        Console.WriteLine("Digital input  2:              " + _wtxDevice.Input2.ToString() + "\t  As an Integer:  " + _wtxDevice.Input2);
                        Console.WriteLine("Digital input  3:              " + _wtxDevice.Input3.ToString() + "\t  As an Integer:  " + _wtxDevice.Input3);
                        Console.WriteLine("Digital input  4:              " + _wtxDevice.Input4.ToString() + "\t  As an Integer:  " + _wtxDevice.Input4);

                        Console.WriteLine("Digital output 1:              " + _wtxDevice.Output1.ToString() + "\t  As an Integer:  " + _wtxDevice.Output1);
                        Console.WriteLine("Digital output 2:              " + _wtxDevice.Output2.ToString() + "\t  As an Integer:  " + _wtxDevice.Output2);
                        Console.WriteLine("Digital output 3:              " + _wtxDevice.Output3.ToString() + "\t  As an Integer:  " + _wtxDevice.Output3);
                        Console.WriteLine("Digital output 4:              " + _wtxDevice.Output4.ToString() + "\t  As an Integer:  " + _wtxDevice.Output4);

                        Console.WriteLine("Coarse flow:                   " + _wtxDevice.CoarseFlow.ToString() +  "\t  As an Integer:  " + _wtxDevice.CoarseFlow);
                        Console.WriteLine("Fine flow:                     " + _wtxDevice.FineFlow.ToString() +    "\t  As an Integer:  " + _wtxDevice.FineFlow);
                        Console.WriteLine("Ready:                         " + _wtxDevice.Ready.ToString() +       "\t  As an Integer:  " + _wtxDevice.Ready);
                        Console.WriteLine("Re-dosing:                     " + _wtxDevice.ReDosing.ToString() +    "\t  As an Integer:  " + _wtxDevice.ReDosing);

                        Console.WriteLine("Emptying:                      " + _wtxDevice.Emptying.ToString() +          "\t  As an Integer:  " + _wtxDevice.Emptying);
                        Console.WriteLine("Flow error:                    " + _wtxDevice.FlowError.ToString() +         "\t  As an Integer:  " + _wtxDevice.FlowError);
                        Console.WriteLine("Alarm:                         " + _wtxDevice.Alarm.ToString() +             "\t  As an Integer:  " + _wtxDevice.Alarm);
                        Console.WriteLine("ADC Overload/Unterload:        " + _wtxDevice.AdcOverUnderload.ToString() + "\t  As an Integer:  " + _wtxDevice.AdcOverUnderload);

                        Console.WriteLine("Max.Dosing time:               " + _wtxDevice.MaxDosingTime.ToString() +          "\t  As an Integer:  " + _wtxDevice.MaxDosingTime);
                        Console.WriteLine("Legal-for-trade operation:     " + _wtxDevice.LegalTradeOp.ToString() +           "\t  As an Integer:  " + _wtxDevice.LegalTradeOp);
                        Console.WriteLine("Tolerance error+:              " + _wtxDevice.ToleranceErrorPlus.ToString() +     "\t  As an Integer:  " + _wtxDevice.ToleranceErrorPlus);
                        Console.WriteLine("Tolerance error-:              " + _wtxDevice.ToleranceErrorMinus.ToString() +    "\t  As an Integer:  " + _wtxDevice.ToleranceErrorMinus);
                                            
                        Console.WriteLine("Status digital input 1:        " + _wtxDevice.StatusInput1.ToString() +           "\t  As an Integer:  " + _wtxDevice.StatusInput1);
                        Console.WriteLine("General scale error:           " + _wtxDevice.GeneralScaleError.ToString() +      "\t  As an Integer:  " + _wtxDevice.GeneralScaleError);
                        Console.WriteLine("Filling process status:        " + _wtxDevice.FillingProcessStatus.ToString() +   "\t  As an Integer:  " + _wtxDevice.FillingProcessStatus);
                        Console.WriteLine("Number of dosing results:      " + _wtxDevice.NumberDosingResults.ToString() +    "\t  As an Integer:  " + _wtxDevice.NumberDosingResults);

                        Console.WriteLine("Dosing result:                 " + _wtxDevice.DosingResult.ToString() +           "\t  As an Integer:  " + _wtxDevice.DosingResult);
                        Console.WriteLine("Mean value of dosing results:  " + _wtxDevice.MeanValueDosingResults.ToString() + "\t  As an Integer:  " + _wtxDevice.MeanValueDosingResults);
                        Console.WriteLine("Standard deviation:            " + _wtxDevice.StandardDeviation.ToString() +      "\t  As an Integer:  " + _wtxDevice.StandardDeviation);
                        Console.WriteLine("Total weight:                  " + _wtxDevice.TotalWeight.ToString() +            "\t  As an Integer:  " + _wtxDevice.TotalWeight);

                        Console.WriteLine("Fine flow cut-off point:       " + _wtxDevice.FineFlowCutOffPoint.ToString() +    "\t  As an Integer:  " + _wtxDevice.FineFlowCutOffPoint);
                        Console.WriteLine("Coarse flow cut-off point:     " + _wtxDevice.CoarseFlowCutOffPoint.ToString() +  "\t  As an Integer:  " + _wtxDevice.CoarseFlowCutOffPoint);
                        Console.WriteLine("Current dosing time:           " + _wtxDevice.CurrentDosingTime.ToString() +      "\t  As an Integer:  " + _wtxDevice.CurrentDosingTime);
                        Console.WriteLine("Current coarse flow time:      " + _wtxDevice.CurrentCoarseFlowTime.ToString() +  "\t  As an Integer:  " + _wtxDevice.CurrentCoarseFlowTime);
                        Console.WriteLine("Current fine flow time:        " + _wtxDevice.CurrentFineFlowTime.ToString() +    "\t  As an Integer:  " + _wtxDevice.CurrentFineFlowTime);

                        Console.WriteLine("Parameter set (product):       " + _wtxDevice.ParameterSetProduct.ToString() + "\t  As an Integer:  " + _wtxDevice.ParameterSetProduct);
                        Console.WriteLine("Weight memory, Day:            " + _wtxDevice.WeightMemDay.ToString() +        "\t  As an Integer:  " + _wtxDevice.WeightMemDay);
                        Console.WriteLine("Weight memory, Month:          " + _wtxDevice.WeightMemMonth.ToString() +      "\t  As an Integer:  " + _wtxDevice.WeightMemMonth);
                        Console.WriteLine("Weight memory, Year:           " + _wtxDevice.WeightMemYear.ToString() +       "\t  As an Integer:  " + _wtxDevice.WeightMemYear);
                        Console.WriteLine("Weight memory, Seq.Number:     " + _wtxDevice.WeightMemSeqNumber.ToString() +  "\t  As an Integer:  " + _wtxDevice.WeightMemSeqNumber);
                        Console.WriteLine("Weight memory, gross:          " + _wtxDevice.WeightMemGross.ToString() +      "\t  As an Integer:  " + _wtxDevice.WeightMemGross);
                        Console.WriteLine("Weight memory, net:            " + _wtxDevice.WeightMemNet.ToString() +        "\t  As an Integer:  " + _wtxDevice.WeightMemNet);

                        Console.WriteLine("\nPress 'a' again to hide the input words.");
                    }
                    
                    if(_showAllOutputWords==true)
                    {
                        Console.WriteLine("\nOutput words:\n");
                  
                        Console.WriteLine(" 9) Residual flow time:            " + _wtxDevice.ResidualFlowTime      + " Press '9' and a value to write");
                        Console.WriteLine("10) Target filling weight:         " + _wtxDevice.TargetFillingWeight   + " Press '10' and a value to write");
                        Console.WriteLine("12) Coarse flow cut-off point:     " + _wtxDevice.CoarseFlowCutOffPoint + " Press '12' and a value to write");
                        Console.WriteLine("14) Fine flow cut-off point:       " + _wtxDevice.FineFlowCutOffPoint   + " Press '14' and a value to write");

                        Console.WriteLine("16) Minimum fine flow:             " + _wtxDevice.MinimumFineFlow   + " Press '16' and a value to write");
                        Console.WriteLine("18) Optimization of cut-off points:" + _wtxDevice.OptimizationOfCutOffPoints + " Press '18' and a value to write");
                        Console.WriteLine("19) Maximum dosing time:           " + _wtxDevice.MaxDosingTime     + " Press '19' and a value to write");
                        Console.WriteLine("20) Start with fine flow:          " + _wtxDevice.StartWithFineFlow + " Press '20' and a value to write");

                        Console.WriteLine("21) Coarse lockout time:           " + _wtxDevice.CoarseLockoutTime + " Press '21' and a value to write");
                        Console.WriteLine("22) Fine lockout time:             " + _wtxDevice.FineLockoutTime   + " Press '22' and a value to write");
                        Console.WriteLine("23) Tare mode:                     " + _wtxDevice.TareMode + " Press '23' and a value to write");
                        Console.WriteLine("24) Upper tolerance limit + :      " + _wtxDevice.UpperToleranceLimit + " Press '24' and a value to write");

                        Console.WriteLine("26) Lower tolerance limit -:       " + _wtxDevice.LowerToleranceLimit + " Press '26' and a value to write");
                        Console.WriteLine("28) Minimum start weight:          " + _wtxDevice.MinimumStartWeight  + " Press '28' and a value to write");
                        Console.WriteLine("30) Empty weight:                  " + _wtxDevice.EmptyWeight + " Press '30' and a value to write");
                        Console.WriteLine("32) Tare delay:                    " + _wtxDevice.TareDelay   + " Press '32' and a value to write");

                        Console.WriteLine("33) Coarse flow monitoring time:   " + _wtxDevice.CoarseFlowMonitoringTime + " Press '33' and a value to write");
                        Console.WriteLine("34) Coarse flow monitoring:        " + _wtxDevice.CoarseFlowMonitoring   + " Press '34' and a value to write");
                        Console.WriteLine("36) Fine flow monitoring:          " + _wtxDevice.FineFlowMonitoring     + " Press '36' and a value to write");
                        Console.WriteLine("38) Fine flow monitoring time:     " + _wtxDevice.FineFlowMonitoringTime + " Press '38' and a value to write");

                        Console.WriteLine("40) Delay time after fine flow:    " + _wtxDevice.DelayTimeAfterFineFlow + " Press '40' and a value to write");
                        Console.WriteLine("41) Systematic difference:         " + _wtxDevice.SystematicDifference + " Press '41' and a value to write");
                        Console.WriteLine("42) Downwards dosing:              " + _wtxDevice.DownwardsDosing + " Press '42' and a value to write");
                        Console.WriteLine("43) Valve control:                 " + _wtxDevice.ValveControl   + " Press '43' and a value to write");
                        Console.WriteLine("44) Emptying mode:                 " + _wtxDevice.EmptyingMode   + " Press '44' and a value to write");

                        Console.WriteLine("\nPress 'o' again to hide the output words.");

                    }
                    
                }
            }
        }

        private static string statusCommentMethod()
        {

            if (mode == "Jetbus" || mode == "Jet" || mode == "jet" || mode == "jetbus")
            {
                if (_wtxDevice.Status == 1634168417)
                    return "Command on go";

                if (_wtxDevice.Status == 1801543519)
                    return "Command ok";
            }
            else
                if (mode == "Modbus" || mode == "modbus")
                {
                    if (_wtxDevice.Status == 0)
                        return "Command on go";

                    if (_wtxDevice.Status == 1)
                        return "Command ok";
                 }
            return "Command on go";
        }

        private static string limitCommentMethod()
        {

            switch(_wtxDevice.LimitStatus)
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
                    return "Not defined";
            }
        }

        private static void zero_load_nominal_load_input()
        {

            Console.Clear();

            Console.WriteLine("\nPlease tip the value for the zero load/dead load and tip enter to confirm : ");

            _preloadStr = Console.ReadLine();
            _strCommaDot = _preloadStr.Replace(".", ",");           // For converting into a floating-point number.
            _preload = double.Parse(_strCommaDot);                   // By using the method 'double.Parse(..)' you can use dots and commas.


            Console.WriteLine("\nPlease tip the value for the span/nominal load and tip enter to confirm : ");

            _capacityStr = Console.ReadLine();
            _strCommaDot = _capacityStr.Replace(".", ",");           // For converting into a floating-point number.
            _capacity = double.Parse(_strCommaDot);                   // By using the method 'double.Parse(..)' you can use dots and commas.

        }

        // This method prints the table to choose how many byte of the register are read. 
        // The parameter "beginning" stands for the moment of the program, in which is this program is called Either on the beginning(=true) or during the execution while the timer is running (=false).
        private static void print_table_for_register_words(bool beginning)
        {
            Console.Clear();

            Console.WriteLine("TCPModbus Interface for weighting terminals of HBM\n");

            if (beginning == true)
                Console.WriteLine("Timer/Sending interval received.\nPlease enter how many words(bytes) you want to read from the register\nof the device. See the following table for choosing:");

            if (beginning == false)
                Console.WriteLine("Executing was interrupted to choose another number of bytes, which will be read from the register.\nPlease enter how many words(bytes) you want to read from the register\nof the device. See the following table for choosing:");

            Console.WriteLine("\nEnter '1'     : Enables reading of ... \n\t\t  word[0]- netto value.\n");
            Console.WriteLine("Enter '2','3',4': Enables reading of ... \n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");
            Console.WriteLine("Enter '5'       : Enables reading of ... \n\t\t  word[4]- weight moving,weight type,scale range,..(see manual)\n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");
            Console.WriteLine("Enter '6'       : Enables writing to the register and reading of ... \n\t\t  word[5]- application mode,decimals,unit,handshake,status bit\n\t\t  word[4]- weight moving,weight type,scale range,..(see manual)\n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");

            Console.WriteLine("It is recommended to use at least '6' for writing and reading. \nDefault setting for the full application in filler mode : '38'\nPlease tip the button 'Enter' after you typed in the number '1' or '2' or...'6'");
        }

        /*
         * This method is a callback method for the asnchronous writing via the method 'Async_Call', which is called once the writing is done. Here, in that case the 
         * callback method 'Write_DataReceived' is empty, there is no need to print the values on the console twice because the timer in class 'WTX120_Modbus' does that already 
         * in a short time intervall. 
         * If you do not want a timer you can put f.e. the printing method into 'Write_DataReceived' f.e. .
         */
         
        private static void Write_DataReceived(IDeviceData obj)
        {
            throw new NotImplementedException();
        }
        

    }
}

#endregion
