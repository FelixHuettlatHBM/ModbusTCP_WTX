
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

using HBM.WT.API.WTX.Modbus;
using HBM.WT.API.WTX;
using HBM.WT.API;


namespace WTXModbus
{
    /// <summary>
    /// This class implements a console application instead of a windows form. An Object of the class 'ModbusTCPConnection' and 'WTX120_Modbus' are initialized as a publisher
    /// and subscriber. Afterwars a connection to the device is build and the timer/sending interval is set. 
    /// A timer with for example 500ms is created. After 500ms an event is triggered, which executes the method "OnTimedEvent" reading the register of the device
    /// by an asynchronous call in the method "WTXObj.Async_Call". As soon as the reading is finisihed, the callback method "Read_DataReceived" takes over the
    /// new data , which have already been interpreted in class 'WTX120_Modbus', so the data is given as a string array. 
    /// The data is also printed on the console in the callback method "Read_DataReceived". 
    /// Being in the while-loop it is possible to select commands to the device. For example taring, change from gross to net value, stop dosing, zeroing and so on. 
    /// 
    /// This is overall just a simple application as an example. A significantly broad presentation is given by the windows form, but for starters it is recommended. 
    /// </summary>

    static class Program
    {
        private static ModbusTcpConnection _modbusObj;
        private static HBM.WT.API.WTX.WtxModbus _wtxObj;
        
        private static string _ipAddress;     // IP-adress, set as the first argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 
        private static int _timerInterval;    // timer interval, set as the second argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 
        private static ushort _inputMode;     // inputMode (number of input bytes), set as the third argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 

        private static ConsoleKeyInfo _valueOutputwords;
        private static ConsoleKeyInfo _valueExitapplication;

        private static string _calibrationWeight;

        private static string _preloadStr, _capacityStr;

        private static double _preload, _capacity;
        private static IFormatProvider _provider;

        private const double MULTIPLIER_MV2_D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)
        private static string _strCommaDot;
        private static double _doubleCalibrationWeight, _potenz;

        private static bool _isCalibrating;  // For checking if the WTX120_Modbus device is calculating at a moment after the command has been send. If 'isCalibrating' is true, the values are not printed on the console. 
        private static bool _showAllInputWords;
        private static bool _showAllOutputWords;

        static void Main(string[] args)
        {
            // Input for the ip adress, the timer interval and the input mode: 

            _ipAddress = "172.19.103.8";     // Default setting. 
            _inputMode = 6;
            _timerInterval = 200;           

            if (args.Length > 0)
            {
                _ipAddress = args[0];
            }
            if (args.Length > 1)
            {
                _timerInterval = Convert.ToInt32(args[1]);
            }
            else
                _timerInterval = 200; // Default value for the timer interval. 
            
            // Initialize:

            _provider = CultureInfo.InvariantCulture;

            _strCommaDot = "";
            _calibrationWeight = "0";

            _isCalibrating = false;
            _showAllInputWords  = false;
            _showAllOutputWords = false;

            _doubleCalibrationWeight = 0.0;
            _potenz = 0.0;

            Console.WriteLine("\nTCPModbus Interface for weighting terminals of HBM\nEnter e to exit the application");

            do // do-while loop for the connection establishment. If the connection is established successfully, the do-while loop is left/exit. 
            {
                _modbusObj = new ModbusTcpConnection(_ipAddress);
                
                _wtxObj = new HBM.WT.API.WTX.WtxModbus(_modbusObj, _timerInterval);    // timerInterval is given by the VS project properties menu as an argument.

                // The connection to the device should be established.   
                _wtxObj.Connect();                                 // Alternative : WTXObj.getConnection.Connect();  

                if (_wtxObj.Connection.IsConnected == true)
                {
                    Console.WriteLine("\nThe connection has been established successfully.\nThe values of the WTX device are printed on the console ... :");
                }
                else
                {
                    Console.WriteLine("\nFailure : The connection has not been established successfully.\nPlease enter a correct IP Adress for the connection establishment...");
                    _ipAddress = Console.ReadLine();
                }

            } while (_wtxObj.Connection.IsConnected==false);


            //thread1.Start();

            // Coupling the data via an event-based call - If the event in class WTX120_Modbus is triggered, the values are updated on the console: 
            _wtxObj.DataUpdateEvent += ValuesOnConsole;     

            // This while loop is repeated till the user enters e. After 500ms the register of the device is read out. In the while-loop the user
            // can select commands, which are send immediately to the device. 
            while (_valueExitapplication.KeyChar != 'e')
            {
                _isCalibrating = false;
                _valueOutputwords = Console.ReadKey();
                int valueOutput = Convert.ToInt32(_valueOutputwords.KeyChar);

                switch (_valueOutputwords.KeyChar)
                {
                    case '0': _wtxObj.Async_Call(0x1,   Write_DataReceived); break;    // Taring 
                    case '1': _wtxObj.Async_Call(0x2,   Write_DataReceived); break;    // Gross/Net
                    case '2': _wtxObj.Async_Call(0x40,  Write_DataReceived); break;    // Zeroing
                    case '3': _wtxObj.Async_Call(0x80,  Write_DataReceived); break;    // Adjust zero 
                    case '4': _wtxObj.Async_Call(0x100, Write_DataReceived); break;    // Adjust nominal
                    case '5': _wtxObj.Async_Call(0x800, Write_DataReceived); break;    // Activate data
                    case '6': _wtxObj.Async_Call(0x1000,Write_DataReceived); break;    // Manual taring
                    case '7': _wtxObj.Async_Call(0x4000,Write_DataReceived); break;    // Record Weight

                    // Fall für schreiben auf multiple Register:
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
                            _wtxObj.Refreshed = true;
                        }
                        else
                            if (_showAllInputWords == true)
                            {
                            _showAllInputWords = false;
                            _wtxObj.Refreshed = true;
                            }
                        break;
                  
                    case 'o': // Writing of the output words

                        if (_showAllOutputWords == false)
                        {
                            _showAllOutputWords = true;
                            _wtxObj.Refreshed = true;
                        }
                        else
                            if (_showAllOutputWords == true)
                            {
                            _showAllOutputWords = false;
                            _wtxObj.Refreshed = true;
                            }

                        break;     
                        
                    default: break;

                }   // end switch-case

         
                //int valueOutput = Convert.ToInt32(value_outputwords.KeyChar);
                int value = 0;
                if (valueOutput >= 9)
                {// switch-case for writing the additional output words of the filler application: 
                    InputWriteOutputWordsFiller((ushort)valueOutput,value);
                }

                _valueExitapplication = Console.ReadKey();
                if (_valueExitapplication.KeyChar == 'e')
                    break;

                if (_valueExitapplication.KeyChar == 'b')   // Change number of bytes, which will be read from the register. (with 'b')
                {
                    print_table_for_register_words(false);  // The parameter stands for the moment of the program, in which is this program is called. ..
                    set_number_inputs();                    //...Either on the beginning(=true) or during the execution while the timer is running (=false).

                }

            } // end while
        } // end main  

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

            //WTXObj.stopTimer();      // The timer is stopped in the method 'Calculate(..)' in class WTX120_Modbus.

            zero_load_nominal_load_input();
           
            _wtxObj.Calculate(_preload,_capacity);
            
            _isCalibrating = false;

            //WTXObj.restartTimer();   // The timer is restarted in the method 'Calculate(..)' in class WTX120_Modbus.
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

            _wtxObj.MeasureZero();
            Console.WriteLine("\n\nDead load measured.Put weight on scale, press any key and wait.");

            string another2 = Console.ReadLine();

            _wtxObj.Calibrate(PotencyCalibrationWeight(), _calibrationWeight);

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

            _potenz = Math.Pow(10, _wtxObj.Decimals); // Potentisation by 10^(decimals). 
            
            return (int) (_doubleCalibrationWeight * _potenz); // Multiplying of the potentiated values with the calibration weight, ...
                                                             // ...casting to integer (easily possible because of the multiplying with ... 
                                                             // ...the potensied value) and returning of the value. 
        }

        // This method prints the values of the device (as a integer and the interpreted string) as well as the description of each bit. 
        private static void ValuesOnConsole(object sender, NetConnectionEventArgs<ushort[]> e)
        {
            // The description and the value of the WTX are only printed on the console if the Interface, containing all auto-properties of the values is 
            // not null (respectively empty) and if no calibration is done at that moment.

            if (_wtxObj.DeviceValues != null && (_isCalibrating==false))
            {
                Console.Clear();

                Console.WriteLine("Options to set the device : Enter the following keys:\nb-Choose the number of bytes read from the register |");

                if (_wtxObj.DeviceValues.ApplicationMode == 0)  // If the WTX120_Modbus device is in standard application/mode.
                {
                    Console.WriteLine("0-Taring | 1-Gross/net  | 2-Zeroing  | 3- Adjust zero | 4-Adjust nominal |\n5-Activate Data \t| 6-Manual taring \t      | 7-Weight storage\n");
                }
                else
                    if (_wtxObj.DeviceValues.ApplicationMode == 1 || _wtxObj.DeviceValues.ApplicationMode == 2) // If the WTX120_Modbus device is in filler application/mode.
                    {

                    if(_showAllInputWords==false)
                    Console.WriteLine("\n0-Taring  | 1-Gross/net  | 2-Clear dosing  | 3- Abort dosing | 4-Start dosing| \n5-Zeroing | 6-Adjust zero| 7-Adjust nominal| 8-Activate data | 9-Weight storage|m-Manual redosing | a-Show all input words 0 to 37 | o-Show output words 9-44\nc-Calculate Calibration | w-Calibration with weight | e-Exit the application\n");

                    if (_showAllInputWords == true)
                        Console.WriteLine("\n0-Taring  | 1-Gross/net  | 2-Clear dosing  | 3- Abort dosing | 4-Start dosing| \n5-Zeroing | 6-Adjust zero| 7-Adjust nominal| 8-Activate data | 9-Weight storage|m-Manual redosing | a-Show only input word 0 to 5\nc-Calculate Calibration | w-Calibration with weight | e-Exit the application\n");
                }

                if (_wtxObj.DeviceValues.ApplicationMode == 0)   // If the device is in the standard mode (standard=0; filler=1 or filler=2) 
                {

                    // The values are printed on the console according to the input - "numInputs": 

                    if (_inputMode == 1)
                    {
                        Console.WriteLine("Net value:                     " + _wtxObj.NetGrossValueStringComment(_wtxObj.NetValue, _wtxObj.Decimals) + "\t  As an Integer:  " + _wtxObj.DeviceValues.NetValue);
                    }
                    else
                        if (_inputMode == 2 || _inputMode == 3 || _inputMode == 4)
                    {
                        Console.WriteLine("Net value:                     " + _wtxObj.NetGrossValueStringComment(_wtxObj.NetValue, _wtxObj.Decimals) +   "\t  As an Integer:  " + _wtxObj.DeviceValues.NetValue);
                        Console.WriteLine("Gross value:                   " + _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals) + "\t  As an Integer:  " + _wtxObj.DeviceValues.GrossValue);
                    }
                    else
                            if (_inputMode == 5)
                    {
                        Console.WriteLine("Net value:                     " + _wtxObj.NetGrossValueStringComment(_wtxObj.NetValue, _wtxObj.Decimals) +   "\t  As an Integer:  " + _wtxObj.DeviceValues.NetValue);
                        Console.WriteLine("Gross value:                   " + _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals) + "\t  As an Integer:  " + _wtxObj.DeviceValues.GrossValue);
                        Console.WriteLine("General weight error:          " + _wtxObj.DeviceValues.GeneralWeightError.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.GeneralWeightError);
                        Console.WriteLine("Scale alarm triggered:         " + _wtxObj.DeviceValues.LimitStatus.ToString() +        "\t  As an Integer:  " + _wtxObj.DeviceValues.LimitStatus);
                        Console.WriteLine("Scale seal is open:            " + _wtxObj.DeviceValues.ScaleSealIsOpen.ToString() +    "\t  As an Integer:  " + _wtxObj.DeviceValues.ScaleSealIsOpen);
                        Console.WriteLine("Manual tare:                   " + _wtxObj.DeviceValues.ManualTare.ToString() +         "\t  As an Integer:  " + _wtxObj.DeviceValues.ManualTare);
                        Console.WriteLine("Weight type:                   " + _wtxObj.WeightTypeStringComment() +                  "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightType);
                        Console.WriteLine("Scale range:                   " + _wtxObj.ScaleRangeStringComment() +                  "\t  As an Integer:  " + _wtxObj.DeviceValues.ScaleRange);
                        Console.WriteLine("Zero required/True zero:       " + _wtxObj.DeviceValues.ZeroRequired.ToString() +       "\t  As an Integer:  " + _wtxObj.DeviceValues.ZeroRequired);
                        Console.WriteLine("Weight within center of zero:  " + _wtxObj.DeviceValues.WeightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightWithinTheCenterOfZero);
                        Console.WriteLine("Weight in zero range:          " + _wtxObj.DeviceValues.WeightInZeroRange.ToString() +  "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightInZeroRange);

                        Console.WriteLine("Limit status:                  " + _wtxObj.LimitStatusStringComment () + "  As an Integer:  " + _wtxObj.DeviceValues.LimitStatus);
                        Console.WriteLine("Weight moving:                 " + _wtxObj.WeightMovingStringComment() + "  As an Integer:" + _wtxObj.DeviceValues.WeightMoving);
                    }
                    else
                    if (_inputMode == 6 || _inputMode == 38)
                    { 
                        Console.WriteLine("Net value:                     " + _wtxObj.NetGrossValueStringComment(_wtxObj.NetValue, _wtxObj.Decimals) +  "\t  As an Integer:  " + _wtxObj.DeviceValues.NetValue);
                        Console.WriteLine("Gross value:                   " + _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals)+ "\t  As an Integer:  " + _wtxObj.DeviceValues.GrossValue);
                        Console.WriteLine("General weight error:          " + _wtxObj.DeviceValues.GeneralWeightError.ToString() +                  "\t  As an Integer:  " + _wtxObj.DeviceValues.GeneralWeightError);
                        Console.WriteLine("Scale alarm triggered:         " + _wtxObj.DeviceValues.LimitStatus.ToString() +                         "\t  As an Integer:  " + _wtxObj.DeviceValues.LimitStatus);
                        Console.WriteLine("Scale seal is open:            " + _wtxObj.DeviceValues.ScaleSealIsOpen.ToString() +                     "\t  As an Integer:  " + _wtxObj.DeviceValues.ScaleSealIsOpen);
                        Console.WriteLine("Manual tare:                   " + _wtxObj.DeviceValues.ManualTare.ToString() +        "\t  As an Integer:  " + _wtxObj.DeviceValues.ManualTare);
                        Console.WriteLine("Weight type:                   " + _wtxObj.WeightTypeStringComment() +                 "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightType);
                        Console.WriteLine("Scale range:                   " + _wtxObj.ScaleRangeStringComment() +                 "\t  As an Integer:  " + _wtxObj.DeviceValues.ScaleRange);
                        Console.WriteLine("Zero required/True zero:       " + _wtxObj.DeviceValues.ZeroRequired.ToString() +      "\t  As an Integer:  " + _wtxObj.DeviceValues.ZeroRequired);
                        Console.WriteLine("Weight within center of zero:  " + _wtxObj.DeviceValues.WeightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightWithinTheCenterOfZero);
                        Console.WriteLine("Weight in zero range:          " + _wtxObj.DeviceValues.WeightInZeroRange.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightInZeroRange);
                        Console.WriteLine("Application mode:              " + _wtxObj.ApplicationModeStringComment() +            "\t  As an Integer:  " + _wtxObj.DeviceValues.ApplicationMode);
                        Console.WriteLine("Decimal places:                " + _wtxObj.DeviceValues.Decimals.ToString() +          "\t  As an Integer:  " + _wtxObj.DeviceValues.Decimals);
                        Console.WriteLine("Unit:                          " + _wtxObj.UnitStringComment() +                       "\t  As an Integer:  " + _wtxObj.DeviceValues.Unit);
                        Console.WriteLine("Handshake:                     " + _wtxObj.DeviceValues.Handshake.ToString() +         "\t  As an Integer:  " + _wtxObj.DeviceValues.Handshake);
                        Console.WriteLine("Status:                        " + _wtxObj.StatusStringComment() +                     "\t  As an Integer:  " + _wtxObj.DeviceValues.Status);

                        Console.WriteLine("Limit status:                  " + _wtxObj.LimitStatusStringComment()  + "  As an Integer:  " + _wtxObj.DeviceValues.LimitStatus);
                        Console.WriteLine("Weight moving:                 " + _wtxObj.WeightMovingStringComment() + "  As an Integer:" + _wtxObj.DeviceValues.WeightMoving);
                        
                    }
                    else
                        Console.WriteLine("\nWrong input for the number of bytes, which should be read from the register!\nPlease enter 'b' to choose again.");
                }
                else
                    if (_wtxObj.DeviceValues.ApplicationMode == 2 || _wtxObj.DeviceValues.ApplicationMode == 1)
                    {
                    Console.WriteLine("Net value:                     " + _wtxObj.NetGrossValueStringComment(_wtxObj.NetValue, _wtxObj.Decimals) +   "\t  As an Integer:  " + _wtxObj.DeviceValues.NetValue);
                    Console.WriteLine("Gross value:                   " + _wtxObj.NetGrossValueStringComment(_wtxObj.GrossValue, _wtxObj.Decimals) + "\t  As an Integer:  " + _wtxObj.DeviceValues.GrossValue);
                    Console.WriteLine("General weight error:          " + _wtxObj.DeviceValues.GeneralWeightError.ToString() +                   "\t  As an Integer:  " + _wtxObj.DeviceValues.GeneralWeightError);
                    Console.WriteLine("Scale alarm triggered:         " + _wtxObj.DeviceValues.LimitStatus.ToString() +     "\t  As an Integer:  " + _wtxObj.DeviceValues.LimitStatus);
                    Console.WriteLine("Scale seal is open:            " + _wtxObj.DeviceValues.ScaleSealIsOpen.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.ScaleSealIsOpen);
                    Console.WriteLine("Manual tare:                   " + _wtxObj.DeviceValues.ManualTare.ToString() +      "\t  As an Integer:  " + _wtxObj.DeviceValues.ManualTare);
                    Console.WriteLine("Weight type:                   " + _wtxObj.WeightTypeStringComment() +               "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightType);
                    Console.WriteLine("Scale range:                   " + _wtxObj.ScaleRangeStringComment() +               "\t  As an Integer:  " + _wtxObj.DeviceValues.ScaleRange);
                    Console.WriteLine("Zero required/True zero:       " + _wtxObj.DeviceValues.ZeroRequired.ToString() +    "\t  As an Integer:  " + _wtxObj.DeviceValues.ZeroRequired);
                    Console.WriteLine("Weight within center of zero:  " + _wtxObj.DeviceValues.WeightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightWithinTheCenterOfZero);
                    Console.WriteLine("Weight in zero range:          " + _wtxObj.DeviceValues.WeightInZeroRange.ToString() +           "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightInZeroRange);
                    Console.WriteLine("Application mode:              " + _wtxObj.ApplicationModeStringComment() +           "\t  As an Integer:  " + _wtxObj.DeviceValues.ApplicationMode);
                    Console.WriteLine("Decimal places:                " + _wtxObj.DeviceValues.Decimals.ToString() +         "\t  As an Integer:  " + _wtxObj.DeviceValues.Decimals);
                    Console.WriteLine("Unit:                          " + _wtxObj.UnitStringComment() +                      "\t  As an Integer:  " + _wtxObj.DeviceValues.Unit);
                    Console.WriteLine("Handshake:                     " + _wtxObj.DeviceValues.Handshake.ToString() +        "\t  As an Integer:  " + _wtxObj.DeviceValues.Handshake);
                    Console.WriteLine("Status:                        " + _wtxObj.StatusStringComment() +                    "\t  As an Integer:  " + _wtxObj.DeviceValues.Status);

                    Console.WriteLine("Limit status:                  " + _wtxObj.LimitStatusStringComment() +  "  As an Integer:  " + _wtxObj.DeviceValues.LimitStatus);
                    Console.WriteLine("Weight moving:                 " + _wtxObj.WeightMovingStringComment() + "  As an Integer:" + _wtxObj.DeviceValues.WeightMoving);

                    if (_showAllInputWords == true)
                    {

                        Console.WriteLine("Digital input  1:              " + _wtxObj.DeviceValues.Input1.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.Input1);
                        Console.WriteLine("Digital input  2:              " + _wtxObj.DeviceValues.Input2.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.Input2);
                        Console.WriteLine("Digital input  3:              " + _wtxObj.DeviceValues.Input3.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.Input3);
                        Console.WriteLine("Digital input  4:              " + _wtxObj.DeviceValues.Input4.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.Input4);

                        Console.WriteLine("Digital output 1:              " + _wtxObj.DeviceValues.Output1.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.Output1);
                        Console.WriteLine("Digital output 2:              " + _wtxObj.DeviceValues.Output2.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.Output2);
                        Console.WriteLine("Digital output 3:              " + _wtxObj.DeviceValues.Output3.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.Output3);
                        Console.WriteLine("Digital output 4:              " + _wtxObj.DeviceValues.Output4.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.Output4);

                        Console.WriteLine("Coarse flow:                   " + _wtxObj.DeviceValues.CoarseFlow.ToString() +  "\t  As an Integer:  " + _wtxObj.DeviceValues.CoarseFlow);
                        Console.WriteLine("Fine flow:                     " + _wtxObj.DeviceValues.FineFlow.ToString() +    "\t  As an Integer:  " + _wtxObj.DeviceValues.FineFlow);
                        Console.WriteLine("Ready:                         " + _wtxObj.DeviceValues.Ready.ToString() +       "\t  As an Integer:  " + _wtxObj.DeviceValues.Ready);
                        Console.WriteLine("Re-dosing:                     " + _wtxObj.DeviceValues.ReDosing.ToString() +    "\t  As an Integer:  " + _wtxObj.DeviceValues.ReDosing);

                        Console.WriteLine("Emptying:                      " + _wtxObj.DeviceValues.Emptying.ToString() +          "\t  As an Integer:  " + _wtxObj.DeviceValues.Emptying);
                        Console.WriteLine("Flow error:                    " + _wtxObj.DeviceValues.FlowError.ToString() +         "\t  As an Integer:  " + _wtxObj.DeviceValues.FlowError);
                        Console.WriteLine("Alarm:                         " + _wtxObj.DeviceValues.Alarm.ToString() +             "\t  As an Integer:  " + _wtxObj.DeviceValues.Alarm);
                        Console.WriteLine("ADC Overload/Unterload:        " + _wtxObj.DeviceValues.AdcOverUnderload.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.AdcOverUnderload);

                        Console.WriteLine("Max.Dosing time:               " + _wtxObj.DeviceValues.MaxDosingTime.ToString() +          "\t  As an Integer:  " + _wtxObj.DeviceValues.MaxDosingTime);
                        Console.WriteLine("Legal-for-trade operation:     " + _wtxObj.DeviceValues.LegalTradeOp.ToString() +           "\t  As an Integer:  " + _wtxObj.DeviceValues.LegalTradeOp);
                        Console.WriteLine("Tolerance error+:              " + _wtxObj.DeviceValues.ToleranceErrorPlus.ToString() +     "\t  As an Integer:  " + _wtxObj.DeviceValues.ToleranceErrorPlus);
                        Console.WriteLine("Tolerance error-:              " + _wtxObj.DeviceValues.ToleranceErrorMinus.ToString() +    "\t  As an Integer:  " + _wtxObj.DeviceValues.ToleranceErrorMinus);
                                            
                        Console.WriteLine("Status digital input 1:        " + _wtxObj.DeviceValues.StatusInput1.ToString() +           "\t  As an Integer:  " + _wtxObj.DeviceValues.StatusInput1);
                        Console.WriteLine("General scale error:           " + _wtxObj.DeviceValues.GeneralScaleError.ToString() +      "\t  As an Integer:  " + _wtxObj.DeviceValues.GeneralScaleError);
                        Console.WriteLine("Filling process status:        " + _wtxObj.DeviceValues.FillingProcessStatus.ToString() +   "\t  As an Integer:  " + _wtxObj.DeviceValues.FillingProcessStatus);
                        Console.WriteLine("Number of dosing results:      " + _wtxObj.DeviceValues.NumberDosingResults.ToString() +    "\t  As an Integer:  " + _wtxObj.DeviceValues.NumberDosingResults);

                        Console.WriteLine("Dosing result:                 " + _wtxObj.DeviceValues.DosingResult.ToString() +           "\t  As an Integer:  " + _wtxObj.DeviceValues.DosingResult);
                        Console.WriteLine("Mean value of dosing results:  " + _wtxObj.DeviceValues.MeanValueDosingResults.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.MeanValueDosingResults);
                        Console.WriteLine("Standard deviation:            " + _wtxObj.DeviceValues.StandardDeviation.ToString() +      "\t  As an Integer:  " + _wtxObj.DeviceValues.StandardDeviation);
                        Console.WriteLine("Total weight:                  " + _wtxObj.DeviceValues.TotalWeight.ToString() +            "\t  As an Integer:  " + _wtxObj.DeviceValues.TotalWeight);

                        Console.WriteLine("Fine flow cut-off point:       " + _wtxObj.DeviceValues.FineFlowCutOffPoint.ToString() +    "\t  As an Integer:  " + _wtxObj.DeviceValues.FineFlowCutOffPoint);
                        Console.WriteLine("Coarse flow cut-off point:     " + _wtxObj.DeviceValues.CoarseFlowCutOffPoint.ToString() +  "\t  As an Integer:  " + _wtxObj.DeviceValues.CoarseFlowCutOffPoint);
                        Console.WriteLine("Current dosing time:           " + _wtxObj.DeviceValues.CurrentDosingTime.ToString() +      "\t  As an Integer:  " + _wtxObj.DeviceValues.CurrentDosingTime);
                        Console.WriteLine("Current coarse flow time:      " + _wtxObj.DeviceValues.CurrentCoarseFlowTime.ToString() +  "\t  As an Integer:  " + _wtxObj.DeviceValues.CurrentCoarseFlowTime);
                        Console.WriteLine("Current fine flow time:        " + _wtxObj.DeviceValues.CurrentFineFlowTime.ToString() +    "\t  As an Integer:  " + _wtxObj.DeviceValues.CurrentFineFlowTime);

                        Console.WriteLine("Parameter set (product):       " + _wtxObj.DeviceValues.ParameterSetProduct.ToString() + "\t  As an Integer:  " + _wtxObj.DeviceValues.ParameterSetProduct);
                        Console.WriteLine("Weight memory, Day:            " + _wtxObj.DeviceValues.WeightMemDay.ToString() +        "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightMemDay);
                        Console.WriteLine("Weight memory, Month:          " + _wtxObj.DeviceValues.WeightMemMonth.ToString() +      "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightMemMonth);
                        Console.WriteLine("Weight memory, Year:           " + _wtxObj.DeviceValues.WeightMemYear.ToString() +       "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightMemYear);
                        Console.WriteLine("Weight memory, Seq.Number:     " + _wtxObj.DeviceValues.WeightMemSeqNumber.ToString() +  "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightMemSeqNumber);
                        Console.WriteLine("Weight memory, gross:          " + _wtxObj.DeviceValues.WeightMemGross.ToString() +      "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightMemGross);
                        Console.WriteLine("Weight memory, net:            " + _wtxObj.DeviceValues.WeightMemNet.ToString() +        "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightMemNet);

                        Console.WriteLine("\nPress 'a' again to hide the input words.");
                    }
                    
                    if(_showAllOutputWords==true)
                    {
                        Console.WriteLine("\nOutput words:\n");
                  
                        Console.WriteLine(" 9) Residual flow time:            " + _wtxObj.ResidualFlowTime      + " Press '9' and a value to write");
                        Console.WriteLine("10) Target filling weight:         " + _wtxObj.TargetFillingWeight   + " Press '10' and a value to write");
                        Console.WriteLine("12) Coarse flow cut-off point:     " + _wtxObj.CoarseFlowCutOffPoint + " Press '12' and a value to write");
                        Console.WriteLine("14) Fine flow cut-off point:       " + _wtxObj.FineFlowCutOffPoint   + " Press '14' and a value to write");

                        Console.WriteLine("16) Minimum fine flow:             " + _wtxObj.MinimumFineFlow   + " Press '16' and a value to write");
                        Console.WriteLine("18) Optimization of cut-off points:" + _wtxObj.OptimizationOfCutOffPoints + " Press '18' and a value to write");
                        Console.WriteLine("19) Maximum dosing time:           " + _wtxObj.MaxDosingTime     + " Press '19' and a value to write");
                        Console.WriteLine("20) Start with fine flow:          " + _wtxObj.StartWithFineFlow + " Press '20' and a value to write");

                        Console.WriteLine("21) Coarse lockout time:           " + _wtxObj.CoarseLockoutTime + " Press '21' and a value to write");
                        Console.WriteLine("22) Fine lockout time:             " + _wtxObj.FineLockoutTime   + " Press '22' and a value to write");
                        Console.WriteLine("23) Tare mode:                     " + _wtxObj.TareMode + " Press '23' and a value to write");
                        Console.WriteLine("24) Upper tolerance limit + :      " + _wtxObj.UpperToleranceLimit + " Press '24' and a value to write");

                        Console.WriteLine("26) Lower tolerance limit -:       " + _wtxObj.LowerToleranceLimit + " Press '26' and a value to write");
                        Console.WriteLine("28) Minimum start weight:          " + _wtxObj.MinimumStartWeight  + " Press '28' and a value to write");
                        Console.WriteLine("30) Empty weight:                  " + _wtxObj.EmptyWeight + " Press '30' and a value to write");
                        Console.WriteLine("32) Tare delay:                    " + _wtxObj.TareDelay   + " Press '32' and a value to write");

                        Console.WriteLine("33) Coarse flow monitoring time:   " + _wtxObj.CoarseFlowMonitoringTime + " Press '33' and a value to write");
                        Console.WriteLine("34) Coarse flow monitoring:        " + _wtxObj.CoarseFlowMonitoring   + " Press '34' and a value to write");
                        Console.WriteLine("36) Fine flow monitoring:          " + _wtxObj.FineFlowMonitoring     + " Press '36' and a value to write");
                        Console.WriteLine("38) Fine flow monitoring time:     " + _wtxObj.FineFlowMonitoringTime + " Press '38' and a value to write");

                        Console.WriteLine("40) Delay time after fine flow:    " + _wtxObj.DelayTimeAfterFineFlow + " Press '40' and a value to write");
                        Console.WriteLine("41) Systematic difference:         " + _wtxObj.SystematicDifference + " Press '41' and a value to write");
                        Console.WriteLine("42) Downwards dosing:              " + _wtxObj.DownardsDosing + " Press '42' and a value to write");
                        Console.WriteLine("43) Valve control:                 " + _wtxObj.ValveControl   + " Press '43' and a value to write");
                        Console.WriteLine("44) Emptying mode:                 " + _wtxObj.EmptyingMode   + " Press '44' and a value to write");

                        Console.WriteLine("\nPress 'o' again to hide the output words.");

                    }
                    
                }
            }
        }

        private static void InputWriteOutputWordsFiller(ushort wordNumberParam,int value)
        {
            int valueToWrite = 0;

            // char wordNumberChar  = writeOutputWord.KeyChar;
            // int wordNumberParam = Convert.ToInt32(wordNumberChar);

            Console.WriteLine("Tip in the value to be written: ");
            string s= Console.ReadLine();
            valueToWrite = Convert.ToInt32(s);

            switch (wordNumberParam)
            {
                case 9:  _wtxObj.WriteOutputWordU16(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 10: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 12: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 14: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 16: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 18: _wtxObj.WriteOutputWordU08(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 19: _wtxObj.WriteOutputWordU16(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 20: _wtxObj.WriteOutputWordU16(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 21: _wtxObj.WriteOutputWordU16(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 22: _wtxObj.WriteOutputWordU16(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 23: _wtxObj.WriteOutputWordU08(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 24: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 26: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 28: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 30: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 32: _wtxObj.WriteOutputWordU16(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 33: _wtxObj.WriteOutputWordU16(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 34: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 36: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 38: _wtxObj.WriteOutputWordU16(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 40: _wtxObj.WriteOutputWordU08(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 41: _wtxObj.WriteOutputWordS32(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 42: _wtxObj.WriteOutputWordU08(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 43: _wtxObj.WriteOutputWordU08(valueToWrite, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 44: _wtxObj.WriteOutputWordU08(valueToWrite, (ushort) wordNumberParam, Write_DataReceived); 
                    break;
               
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
