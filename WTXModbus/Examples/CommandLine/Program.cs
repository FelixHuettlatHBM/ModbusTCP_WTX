
/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 01/2018
 * 
 * Author : Felix Huettl 
 * 
 * Console application for WTX120 MODBUS TCPIP 
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
using Hbm.Devices.WTXModbus;
using System.Globalization;

namespace WTXModbus
{
    /// <summary>
    /// This class implements a console application instead of a windows form. An Object of the class 'ModbusConnection' and 'WTX120' are initialized as a publisher
    /// and subscriber. Afterwars a connection to the device is build and the timer/sending interval is set. 
    /// A timer with for example 500ms is created. After 500ms an event is triggered, which executes the method "OnTimedEvent" reading the register of the device
    /// by an asynchronous call in the method "WTX_obj.Async_Call". As soon as the reading is finisihed, the callback method "Read_DataReceived" takes over the
    /// new data , which have already been interpreted in class 'WTX120', so the data is given as a string array. 
    /// The data is also printed on the console in the callback method "Read_DataReceived". 
    /// Being in the while-loop it is possible to select commands to the device. For example taring, change from gross to net value, stop dosing, zeroing and so on. 
    /// 
    /// This is overall just a simple application as an example. A significantly broad presentation is given by the windows form, but for starters it is recommended. 
    /// </summary>

    static class Program
    {
        private static ModbusConnection ModbusObj;
        private static WTX120 WTX_obj;

        private static string ipAddress;     // IP-adress, set as the first argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 
        private static int timerInterval;    // timer interval, set as the second argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 
        private static ushort inputMode;     // inputMode (number of input bytes), set as the third argument in the VS project properties menu as an argument or in the console application as an input(which is commented out in the following) 

        private static ConsoleKeyInfo value_outputwords;
        private static ConsoleKeyInfo value_exitapplication;

        private static ConsoleKeyInfo writeOutputWord;

        private static string calibration_weight;

        private static string Preload_str, Capacity_str;

        private static double Preload, Capacity;
        private static IFormatProvider Provider;

        private const double MultiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)
        private static string str_comma_dot;
        private static double DoubleCalibrationWeight, potenz;

        private static bool isCalibrating;  // For checking if the WTX120 device is calculating at a moment after the command has been send. If 'isCalibrating' is true, the values are not printed on the console. 
        private static bool ShowAllInputWords;
        private static bool ShowAllOutputWords;

        static void Main(string[] args)
        {
            // Input for the ip adress, the timer interval and the input mode: 

            ipAddress = "172.19.103.8";     // Default setting. 
            inputMode = 6;
            timerInterval = 200;           

            if (args.Length > 0)
            {
                ipAddress = args[0];
            }
            if (args.Length > 1)
            {
                timerInterval = Convert.ToInt32(args[1]);
            }
            else
                timerInterval = 200; // Default value for the timer interval. 
            
            // Initialize:

            Provider = CultureInfo.InvariantCulture;

            str_comma_dot = "";
            calibration_weight = "0";

            isCalibrating = false;
            ShowAllInputWords  = false;
            ShowAllOutputWords = false;

            DoubleCalibrationWeight = 0.0;
            potenz = 0.0;

            Console.WriteLine("\nTCPModbus Interface for weighting terminals of HBM\nEnter e to exit the application");

            do // do-while loop for the connection establishment. If the connection is established successfully, the do-while loop is left/exit. 
            {
                ModbusObj = new ModbusConnection(ipAddress);

                WTX_obj = new WTX120(ModbusObj, timerInterval);    // timerInterval is given by the VS project properties menu as an argument.

                WTX_obj.getConnection.Connect();
          
                if (WTX_obj.getConnection.is_connected == true)
                {
                    Console.WriteLine("\nThe connection has been established successfully.\nThe values of the WTX device are printed on the console ... :");
                }
                else
                {
                    Console.WriteLine("\nFailure : The connection has not been established successfully.\nPlease enter a correct IP Adress for the connection establishment...");
                    ipAddress = Console.ReadLine();
                }

            } while (WTX_obj.getConnection.is_connected==false);


            //thread1.Start();

            // Coupling the data via an event-based call - If the event in class WTX120 is triggered, the values are updated on the console: 
            WTX_obj.DataUpdateEvent += ValuesOnConsole;     

            // This while loop is repeated till the user enters e. After 500ms the register of the device is read out. In the while-loop the user
            // can select commands, which are send immediately to the device. 
            while (value_exitapplication.KeyChar != 'e')
            {
                isCalibrating = false;
                value_outputwords = Console.ReadKey();

                switch (value_outputwords.KeyChar)
                {
                    case '0': WTX_obj.Async_Call(0x1, Write_DataReceived);    break;    // Taring 
                    case '1': WTX_obj.Async_Call(0x2, Write_DataReceived);    break;    // Gross/Net
                    case '2': WTX_obj.Async_Call(0x40, Write_DataReceived);   break;    // Zeroing
                    case '3': WTX_obj.Async_Call(0x80, Write_DataReceived);   break;    // Adjust zero 
                    case '4': WTX_obj.Async_Call(0x100, Write_DataReceived);  break;    // Adjust nominal
                    case '5': WTX_obj.Async_Call(0x800, Write_DataReceived);  break;    // Activate data
                    case '6': WTX_obj.Async_Call(0x1000, Write_DataReceived); break;    // Manual taring
                    case '7': WTX_obj.Async_Call(0x4000, Write_DataReceived); break;    // Record Weight

                    // Fall für schreiben auf multiple Register:
                    case 'c':       // Calculate Calibration
                        CalculateCalibration();
                        break;
                    case 'w':       // Calculation with weight 
                        CalibrationWithWeight();
                        break;
                    case 'a':  // Show all input words in the filler application. 
                        if (ShowAllInputWords == false)
                        {
                            ShowAllInputWords = true;
                            WTX_obj.Refreshed = true;
                        }
                        else
                            if (ShowAllInputWords == true)
                            {
                            ShowAllInputWords = false;
                            WTX_obj.Refreshed = true;
                            }
                        break;
                  
                    case 'o': // Writing of the output words

                        if (ShowAllOutputWords == false)
                        {
                            ShowAllOutputWords = true;
                            WTX_obj.Refreshed = true;
                        }
                        else
                            if (ShowAllOutputWords == true)
                            {
                            ShowAllOutputWords = false;
                            WTX_obj.Refreshed = true;
                            }


                        break;             

                    default: break;

                }   // end switch-case

                // switch-case for writing the additional output words of the filler application: 
                inputWriteOutputWordsFiller();

                value_exitapplication = Console.ReadKey();
                if (value_exitapplication.KeyChar == 'e')
                    break;

                if (value_exitapplication.KeyChar == 'b')   // Change number of bytes, which will be read from the register. (with 'b')
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

            inputMode = (ushort)Convert.ToInt32(Console.ReadLine());

        }
        /*
         * This method calcutes the values for a dead load and a nominal load(span) in a ratio in mV/V and write in into the WTX registers. 
         */
         
        private static void CalculateCalibration()
        {
            isCalibrating = true;

            //WTX_obj.stopTimer();      // The timer is stopped in the method 'Calculate(..)' in class WTX120.

            zero_load_nominal_load_input();
           
            WTX_obj.Calculate(Preload,Capacity);
            
            isCalibrating = false;

            //WTX_obj.restartTimer();   // The timer is restarted in the method 'Calculate(..)' in class WTX120.
        }

        /*
         * This method does a calibration with an individual weight to the WTX.  
         * First you tip the value for the calibration weight, then you set the value for the dead load (method ‚MeasureZero‘), 
         * finally you set the value for the nominal weight in the WTX (method ‚Calibrate(calibrationValue)‘).
         */
        private static void CalibrationWithWeight()
        {
            isCalibrating = true;

            //WTX_obj.stopTimer();    // The timer is stopped in the method 'Calculate(..)' in class WTX120.

            Console.Clear();
            Console.WriteLine("\nPlease tip the value for the calibration weight and tip enter to confirm : ");
            calibration_weight = Console.ReadLine();

            Console.WriteLine("\nTo start : Set zero load and press any key for measuring zero and wait.");
            string another = Console.ReadLine();

            WTX_obj.MeasureZero();
            Console.WriteLine("\n\nDead load measured.Put weight on scale, press any key and wait.");

            string another2 = Console.ReadLine();

            WTX_obj.Calibrate(potencyCalibrationWeight(), calibration_weight);

            //WTX_obj.restartTimer();   // The timer is restarted in the method 'Calculate(..)' in class WTX120.

            isCalibrating = false;
        }  

        /*
         * This method potentiate the number of the values decimals and multiply it with the calibration weight(input) to get
         * an integer which is in written into the WTX registers by the method Calibrate(potencyCalibrationWeight()). 
         */
        private static int potencyCalibrationWeight()
        {

            str_comma_dot = calibration_weight.Replace(".", ","); // Transformation into a floating-point number.Thereby commas and dots can be used as input for the calibration weight.
            DoubleCalibrationWeight = double.Parse(str_comma_dot);                  

            potenz = Math.Pow(10, WTX_obj.decimals); // Potentisation by 10^(decimals). 
            
            return (int) (DoubleCalibrationWeight * potenz); // Multiplying of the potentiated values with the calibration weight, ...
                                                             // ...casting to integer (easily possible because of the multiplying with ... 
                                                             // ...the potensied value) and returning of the value. 
        }

        // This method prints the values of the device (as a integer and the interpreted string) as well as the description of each bit. 
        private static void ValuesOnConsole(object sender, NetConnectionEventArgs<ushort[]> e)
        {
            // The description and the value of the WTX are only printed on the console if the Interface, containing all auto-properties of the values is 
            // not null (respectively empty) and if no calibration is done at that moment.

            if (WTX_obj.DeviceValues != null && (isCalibrating==false))
            {
                Console.Clear();

                Console.WriteLine("Options to set the device : Enter the following keys:\nb-Choose the number of bytes read from the register |");

                if (WTX_obj.DeviceValues.applicationMode == 0)  // If the WTX120 device is in standard application/mode.
                {
                    Console.WriteLine("0-Taring | 1-Gross/net  | 2-Zeroing  | 3- Adjust zero | 4-Adjust nominal |\n5-Activate Data \t| 6-Manual taring \t      | 7-Weight storage\n");
                }
                else
                    if (WTX_obj.DeviceValues.applicationMode == 1 || WTX_obj.DeviceValues.applicationMode == 2) // If the WTX120 device is in filler application/mode.
                    {

                    if(ShowAllInputWords==false)
                    Console.WriteLine("\n0-Taring  | 1-Gross/net  | 2-Clear dosing  | 3- Abort dosing | 4-Start dosing| \n5-Zeroing | 6-Adjust zero| 7-Adjust nominal| 8-Activate data | 9-Weight storage|m-Manual redosing | a-Show all input words 0 to 37 | o-Show output words 9-44\nc-Calculate Calibration | w-Calibration with weight | e-Exit the application\n");

                    if (ShowAllInputWords == true)
                        Console.WriteLine("\n0-Taring  | 1-Gross/net  | 2-Clear dosing  | 3- Abort dosing | 4-Start dosing| \n5-Zeroing | 6-Adjust zero| 7-Adjust nominal| 8-Activate data | 9-Weight storage|m-Manual redosing | a-Show only input word 0 to 5\nc-Calculate Calibration | w-Calibration with weight | e-Exit the application\n");
                }

                if (WTX_obj.DeviceValues.applicationMode == 0)   // If the device is in the standard mode (standard=0; filler=1 or filler=2) 
                {

                    // The values are printed on the console according to the input - "numInputs": 

                    if (inputMode == 1)
                    {
                        Console.WriteLine("Net value:                     " + WTX_obj.netGrossValueStringComment(WTX_obj.NetValue, WTX_obj.decimals) + "\t  As an Integer:  " + WTX_obj.DeviceValues.NetValue);
                    }
                    else
                        if (inputMode == 2 || inputMode == 3 || inputMode == 4)
                    {
                        Console.WriteLine("Net value:                     " + WTX_obj.netGrossValueStringComment(WTX_obj.NetValue, WTX_obj.decimals) +   "\t  As an Integer:  " + WTX_obj.DeviceValues.NetValue);
                        Console.WriteLine("Gross value:                   " + WTX_obj.netGrossValueStringComment(WTX_obj.GrossValue, WTX_obj.decimals) + "\t  As an Integer:  " + WTX_obj.DeviceValues.GrossValue);
                    }
                    else
                            if (inputMode == 5)
                    {
                        Console.WriteLine("Net value:                     " + WTX_obj.netGrossValueStringComment(WTX_obj.NetValue, WTX_obj.decimals) +   "\t  As an Integer:  " + WTX_obj.DeviceValues.NetValue);
                        Console.WriteLine("Gross value:                   " + WTX_obj.netGrossValueStringComment(WTX_obj.GrossValue, WTX_obj.decimals) + "\t  As an Integer:  " + WTX_obj.DeviceValues.GrossValue);
                        Console.WriteLine("General weight error:          " + WTX_obj.DeviceValues.generalWeightError.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.generalWeightError);
                        Console.WriteLine("Scale alarm triggered:         " + WTX_obj.DeviceValues.limitStatus.ToString() +        "\t  As an Integer:  " + WTX_obj.DeviceValues.limitStatus);
                        Console.WriteLine("Scale seal is open:            " + WTX_obj.DeviceValues.scaleSealIsOpen.ToString() +    "\t  As an Integer:  " + WTX_obj.DeviceValues.scaleSealIsOpen);
                        Console.WriteLine("Manual tare:                   " + WTX_obj.DeviceValues.manualTare.ToString() +         "\t  As an Integer:  " + WTX_obj.DeviceValues.manualTare);
                        Console.WriteLine("Weight type:                   " + WTX_obj.weightTypeStringComment() +                  "\t  As an Integer:  " + WTX_obj.DeviceValues.weightType);
                        Console.WriteLine("Scale range:                   " + WTX_obj.scaleRangeStringComment() +                  "\t  As an Integer:  " + WTX_obj.DeviceValues.scaleRange);
                        Console.WriteLine("Zero required/True zero:       " + WTX_obj.DeviceValues.zeroRequired.ToString() +       "\t  As an Integer:  " + WTX_obj.DeviceValues.zeroRequired);
                        Console.WriteLine("Weight within center of zero:  " + WTX_obj.DeviceValues.weightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.weightWithinTheCenterOfZero);
                        Console.WriteLine("Weight in zero range:          " + WTX_obj.DeviceValues.weightInZeroRange.ToString() +  "\t  As an Integer:  " + WTX_obj.DeviceValues.weightInZeroRange);

                        Console.WriteLine("Limit status:                  " + WTX_obj.limitStatusStringComment () + "  As an Integer:  " + WTX_obj.DeviceValues.limitStatus);
                        Console.WriteLine("Weight moving:                 " + WTX_obj.weightMovingStringComment() + "  As an Integer:" + WTX_obj.DeviceValues.weightMoving);
                    }
                    else
                    if (inputMode == 6 || inputMode == 38)
                    { 
                        Console.WriteLine("Net value:                     " + WTX_obj.netGrossValueStringComment(WTX_obj.NetValue, WTX_obj.decimals) +  "\t  As an Integer:  " + WTX_obj.DeviceValues.NetValue);
                        Console.WriteLine("Gross value:                   " + WTX_obj.netGrossValueStringComment(WTX_obj.GrossValue, WTX_obj.decimals)+ "\t  As an Integer:  " + WTX_obj.DeviceValues.GrossValue);
                        Console.WriteLine("General weight error:          " + WTX_obj.DeviceValues.generalWeightError.ToString() +                  "\t  As an Integer:  " + WTX_obj.DeviceValues.generalWeightError);
                        Console.WriteLine("Scale alarm triggered:         " + WTX_obj.DeviceValues.limitStatus.ToString() +                         "\t  As an Integer:  " + WTX_obj.DeviceValues.limitStatus);
                        Console.WriteLine("Scale seal is open:            " + WTX_obj.DeviceValues.scaleSealIsOpen.ToString() +                     "\t  As an Integer:  " + WTX_obj.DeviceValues.scaleSealIsOpen);
                        Console.WriteLine("Manual tare:                   " + WTX_obj.DeviceValues.manualTare.ToString() +        "\t  As an Integer:  " + WTX_obj.DeviceValues.manualTare);
                        Console.WriteLine("Weight type:                   " + WTX_obj.weightTypeStringComment() +                 "\t  As an Integer:  " + WTX_obj.DeviceValues.weightType);
                        Console.WriteLine("Scale range:                   " + WTX_obj.scaleRangeStringComment() +                 "\t  As an Integer:  " + WTX_obj.DeviceValues.scaleRange);
                        Console.WriteLine("Zero required/True zero:       " + WTX_obj.DeviceValues.zeroRequired.ToString() +      "\t  As an Integer:  " + WTX_obj.DeviceValues.zeroRequired);
                        Console.WriteLine("Weight within center of zero:  " + WTX_obj.DeviceValues.weightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.weightWithinTheCenterOfZero);
                        Console.WriteLine("Weight in zero range:          " + WTX_obj.DeviceValues.weightInZeroRange.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.weightInZeroRange);
                        Console.WriteLine("Application mode:              " + WTX_obj.applicationModeStringComment() +            "\t  As an Integer:  " + WTX_obj.DeviceValues.applicationMode);
                        Console.WriteLine("Decimal places:                " + WTX_obj.DeviceValues.decimals.ToString() +          "\t  As an Integer:  " + WTX_obj.DeviceValues.decimals);
                        Console.WriteLine("Unit:                          " + WTX_obj.unitStringComment() +                       "\t  As an Integer:  " + WTX_obj.DeviceValues.unit);
                        Console.WriteLine("Handshake:                     " + WTX_obj.DeviceValues.handshake.ToString() +         "\t  As an Integer:  " + WTX_obj.DeviceValues.handshake);
                        Console.WriteLine("Status:                        " + WTX_obj.statusStringComment() +                     "\t  As an Integer:  " + WTX_obj.DeviceValues.status);

                        Console.WriteLine("Limit status:                  " + WTX_obj.limitStatusStringComment()  + "  As an Integer:  " + WTX_obj.DeviceValues.limitStatus);
                        Console.WriteLine("Weight moving:                 " + WTX_obj.weightMovingStringComment() + "  As an Integer:" + WTX_obj.DeviceValues.weightMoving);
                        
                    }
                    else
                        Console.WriteLine("\nWrong input for the number of bytes, which should be read from the register!\nPlease enter 'b' to choose again.");
                }
                else
                    if (WTX_obj.DeviceValues.applicationMode == 2 || WTX_obj.DeviceValues.applicationMode == 1)
                    {
                    Console.WriteLine("Net value:                     " + WTX_obj.netGrossValueStringComment(WTX_obj.NetValue, WTX_obj.decimals) +   "\t  As an Integer:  " + WTX_obj.DeviceValues.NetValue);
                    Console.WriteLine("Gross value:                   " + WTX_obj.netGrossValueStringComment(WTX_obj.GrossValue, WTX_obj.decimals) + "\t  As an Integer:  " + WTX_obj.DeviceValues.GrossValue);
                    Console.WriteLine("General weight error:          " + WTX_obj.DeviceValues.generalWeightError.ToString() +                   "\t  As an Integer:  " + WTX_obj.DeviceValues.generalWeightError);
                    Console.WriteLine("Scale alarm triggered:         " + WTX_obj.DeviceValues.limitStatus.ToString() +     "\t  As an Integer:  " + WTX_obj.DeviceValues.limitStatus);
                    Console.WriteLine("Scale seal is open:            " + WTX_obj.DeviceValues.scaleSealIsOpen.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.scaleSealIsOpen);
                    Console.WriteLine("Manual tare:                   " + WTX_obj.DeviceValues.manualTare.ToString() +      "\t  As an Integer:  " + WTX_obj.DeviceValues.manualTare);
                    Console.WriteLine("Weight type:                   " + WTX_obj.weightTypeStringComment() +               "\t  As an Integer:  " + WTX_obj.DeviceValues.weightType);
                    Console.WriteLine("Scale range:                   " + WTX_obj.scaleRangeStringComment() +               "\t  As an Integer:  " + WTX_obj.DeviceValues.scaleRange);
                    Console.WriteLine("Zero required/True zero:       " + WTX_obj.DeviceValues.zeroRequired.ToString() +    "\t  As an Integer:  " + WTX_obj.DeviceValues.zeroRequired);
                    Console.WriteLine("Weight within center of zero:  " + WTX_obj.DeviceValues.weightWithinTheCenterOfZero.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.weightWithinTheCenterOfZero);
                    Console.WriteLine("Weight in zero range:          " + WTX_obj.DeviceValues.weightInZeroRange.ToString() +           "\t  As an Integer:  " + WTX_obj.DeviceValues.weightInZeroRange);
                    Console.WriteLine("Application mode:              " + WTX_obj.applicationModeStringComment() +           "\t  As an Integer:  " + WTX_obj.DeviceValues.applicationMode);
                    Console.WriteLine("Decimal places:                " + WTX_obj.DeviceValues.decimals.ToString() +         "\t  As an Integer:  " + WTX_obj.DeviceValues.decimals);
                    Console.WriteLine("Unit:                          " + WTX_obj.unitStringComment() +                      "\t  As an Integer:  " + WTX_obj.DeviceValues.unit);
                    Console.WriteLine("Handshake:                     " + WTX_obj.DeviceValues.handshake.ToString() +        "\t  As an Integer:  " + WTX_obj.DeviceValues.handshake);
                    Console.WriteLine("Status:                        " + WTX_obj.statusStringComment() +                    "\t  As an Integer:  " + WTX_obj.DeviceValues.status);

                    Console.WriteLine("Limit status:                  " + WTX_obj.limitStatusStringComment() +  "  As an Integer:  " + WTX_obj.DeviceValues.limitStatus);
                    Console.WriteLine("Weight moving:                 " + WTX_obj.weightMovingStringComment() + "  As an Integer:" + WTX_obj.DeviceValues.weightMoving);

                    if (ShowAllInputWords == true)
                    {

                        Console.WriteLine("Digital input  1:              " + WTX_obj.DeviceValues.input1.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.input1);
                        Console.WriteLine("Digital input  2:              " + WTX_obj.DeviceValues.input2.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.input2);
                        Console.WriteLine("Digital input  3:              " + WTX_obj.DeviceValues.input3.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.input3);
                        Console.WriteLine("Digital input  4:              " + WTX_obj.DeviceValues.input4.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.input4);

                        Console.WriteLine("Digital output 1:              " + WTX_obj.DeviceValues.output1.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.output1);
                        Console.WriteLine("Digital output 2:              " + WTX_obj.DeviceValues.output2.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.output2);
                        Console.WriteLine("Digital output 3:              " + WTX_obj.DeviceValues.output3.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.output3);
                        Console.WriteLine("Digital output 4:              " + WTX_obj.DeviceValues.output4.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.output4);

                        Console.WriteLine("Coarse flow:                   " + WTX_obj.DeviceValues.coarseFlow.ToString() +  "\t  As an Integer:  " + WTX_obj.DeviceValues.coarseFlow);
                        Console.WriteLine("Fine flow:                     " + WTX_obj.DeviceValues.fineFlow.ToString() +    "\t  As an Integer:  " + WTX_obj.DeviceValues.fineFlow);
                        Console.WriteLine("Ready:                         " + WTX_obj.DeviceValues.ready.ToString() +       "\t  As an Integer:  " + WTX_obj.DeviceValues.ready);
                        Console.WriteLine("Re-dosing:                     " + WTX_obj.DeviceValues.reDosing.ToString() +    "\t  As an Integer:  " + WTX_obj.DeviceValues.reDosing);

                        Console.WriteLine("Emptying:                      " + WTX_obj.DeviceValues.emptying.ToString() +          "\t  As an Integer:  " + WTX_obj.DeviceValues.emptying);
                        Console.WriteLine("Flow error:                    " + WTX_obj.DeviceValues.flowError.ToString() +         "\t  As an Integer:  " + WTX_obj.DeviceValues.flowError);
                        Console.WriteLine("Alarm:                         " + WTX_obj.DeviceValues.alarm.ToString() +             "\t  As an Integer:  " + WTX_obj.DeviceValues.alarm);
                        Console.WriteLine("ADC Overload/Unterload:        " + WTX_obj.DeviceValues.ADC_overUnderload.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.ADC_overUnderload);

                        Console.WriteLine("Max.Dosing time:               " + WTX_obj.DeviceValues.maxDosingTime.ToString() +          "\t  As an Integer:  " + WTX_obj.DeviceValues.maxDosingTime);
                        Console.WriteLine("Legal-for-trade operation:     " + WTX_obj.DeviceValues.legalTradeOp.ToString() +           "\t  As an Integer:  " + WTX_obj.DeviceValues.legalTradeOp);
                        Console.WriteLine("Tolerance error+:              " + WTX_obj.DeviceValues.toleranceErrorPlus.ToString() +     "\t  As an Integer:  " + WTX_obj.DeviceValues.toleranceErrorPlus);
                        Console.WriteLine("Tolerance error-:              " + WTX_obj.DeviceValues.toleranceErrorMinus.ToString() +    "\t  As an Integer:  " + WTX_obj.DeviceValues.toleranceErrorMinus);
                                            
                        Console.WriteLine("Status digital input 1:        " + WTX_obj.DeviceValues.statusInput1.ToString() +           "\t  As an Integer:  " + WTX_obj.DeviceValues.statusInput1);
                        Console.WriteLine("General scale error:           " + WTX_obj.DeviceValues.generalScaleError.ToString() +      "\t  As an Integer:  " + WTX_obj.DeviceValues.generalScaleError);
                        Console.WriteLine("Filling process status:        " + WTX_obj.DeviceValues.fillingProcessStatus.ToString() +   "\t  As an Integer:  " + WTX_obj.DeviceValues.fillingProcessStatus);
                        Console.WriteLine("Number of dosing results:      " + WTX_obj.DeviceValues.numberDosingResults.ToString() +    "\t  As an Integer:  " + WTX_obj.DeviceValues.numberDosingResults);

                        Console.WriteLine("Dosing result:                 " + WTX_obj.DeviceValues.dosingResult.ToString() +           "\t  As an Integer:  " + WTX_obj.DeviceValues.dosingResult);
                        Console.WriteLine("Mean value of dosing results:  " + WTX_obj.DeviceValues.meanValueDosingResults.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.meanValueDosingResults);
                        Console.WriteLine("Standard deviation:            " + WTX_obj.DeviceValues.standardDeviation.ToString() +      "\t  As an Integer:  " + WTX_obj.DeviceValues.standardDeviation);
                        Console.WriteLine("Total weight:                  " + WTX_obj.DeviceValues.totalWeight.ToString() +            "\t  As an Integer:  " + WTX_obj.DeviceValues.totalWeight);

                        Console.WriteLine("Fine flow cut-off point:       " + WTX_obj.DeviceValues.fineFlowCutOffPoint.ToString() +    "\t  As an Integer:  " + WTX_obj.DeviceValues.fineFlowCutOffPoint);
                        Console.WriteLine("Coarse flow cut-off point:     " + WTX_obj.DeviceValues.coarseFlowCutOffPoint.ToString() +  "\t  As an Integer:  " + WTX_obj.DeviceValues.coarseFlowCutOffPoint);
                        Console.WriteLine("Current dosing time:           " + WTX_obj.DeviceValues.currentDosingTime.ToString() +      "\t  As an Integer:  " + WTX_obj.DeviceValues.currentDosingTime);
                        Console.WriteLine("Current coarse flow time:      " + WTX_obj.DeviceValues.currentCoarseFlowTime.ToString() +  "\t  As an Integer:  " + WTX_obj.DeviceValues.currentCoarseFlowTime);
                        Console.WriteLine("Current fine flow time:        " + WTX_obj.DeviceValues.currentFineFlowTime.ToString() +    "\t  As an Integer:  " + WTX_obj.DeviceValues.currentFineFlowTime);

                        Console.WriteLine("Parameter set (product):       " + WTX_obj.DeviceValues.parameterSetProduct.ToString() + "\t  As an Integer:  " + WTX_obj.DeviceValues.parameterSetProduct);
                        Console.WriteLine("Weight memory, Day:            " + WTX_obj.DeviceValues.weightMemDay.ToString() +        "\t  As an Integer:  " + WTX_obj.DeviceValues.weightMemDay);
                        Console.WriteLine("Weight memory, Month:          " + WTX_obj.DeviceValues.weightMemMonth.ToString() +      "\t  As an Integer:  " + WTX_obj.DeviceValues.weightMemMonth);
                        Console.WriteLine("Weight memory, Year:           " + WTX_obj.DeviceValues.weightMemYear.ToString() +       "\t  As an Integer:  " + WTX_obj.DeviceValues.weightMemYear);
                        Console.WriteLine("Weight memory, Seq.Number:     " + WTX_obj.DeviceValues.weightMemSeqNumber.ToString() +  "\t  As an Integer:  " + WTX_obj.DeviceValues.weightMemSeqNumber);
                        Console.WriteLine("Weight memory, gross:          " + WTX_obj.DeviceValues.weightMemGross.ToString() +      "\t  As an Integer:  " + WTX_obj.DeviceValues.weightMemGross);
                        Console.WriteLine("Weight memory, net:            " + WTX_obj.DeviceValues.weightMemNet.ToString() +        "\t  As an Integer:  " + WTX_obj.DeviceValues.weightMemNet);

                        Console.WriteLine("\nPress 'a' again to hide the input words.");
                    }
                    
                    if(ShowAllOutputWords==true)
                    {
                        Console.WriteLine("\nOutput words:\n");
                  
                        Console.WriteLine(" 9) Residual flow time:            " + WTX_obj.ResidualFlowTime      + " Press '9' and a value to write");
                        Console.WriteLine("10) Target filling weight:         " + WTX_obj.targetFillingWeight   + " Press '10' and a value to write");
                        Console.WriteLine("12) Coarse flow cut-off point:     " + WTX_obj.coarseFlowCutOffPoint + " Press '12' and a value to write");
                        Console.WriteLine("14) Fine flow cut-off point:       " + WTX_obj.fineFlowCutOffPoint   + " Press '14' and a value to write");

                        Console.WriteLine("16) Minimum fine flow:             " + WTX_obj.minimumFineFlow   + " Press '16' and a value to write");
                        Console.WriteLine("18) Optimization of cut-off points:" + WTX_obj.optimizationOfCutOffPoints + " Press '18' and a value to write");
                        Console.WriteLine("19) Maximum dosing time:           " + WTX_obj.maxDosingTime     + " Press '19' and a value to write");
                        Console.WriteLine("20) Start with fine flow:          " + WTX_obj.startWithFineFlow + " Press '20' and a value to write");

                        Console.WriteLine("21) Coarse lockout time:           " + WTX_obj.coarseLockoutTime + " Press '21' and a value to write");
                        Console.WriteLine("22) Fine lockout time:             " + WTX_obj.fineLockoutTime   + " Press '22' and a value to write");
                        Console.WriteLine("23) Tare mode:                     " + WTX_obj.tareMode + " Press '23' and a value to write");
                        Console.WriteLine("24) Upper tolerance limit + :      " + WTX_obj.upperToleranceLimit + " Press '24' and a value to write");

                        Console.WriteLine("26) Lower tolerance limit -:       " + WTX_obj.lowerToleranceLimit + " Press '26' and a value to write");
                        Console.WriteLine("28) Minimum start weight:          " + WTX_obj.minimumStartWeight  + " Press '28' and a value to write");
                        Console.WriteLine("30) Empty weight:                  " + WTX_obj.emptyWeight + " Press '30' and a value to write");
                        Console.WriteLine("32) Tare delay:                    " + WTX_obj.tareDelay   + " Press '32' and a value to write");

                        Console.WriteLine("33) Coarse flow monitoring time:   " + WTX_obj.coarseFlowMonitoringTime + " Press '33' and a value to write");
                        Console.WriteLine("34) Coarse flow monitoring:        " + WTX_obj.coarseFlowMonitoring   + " Press '34' and a value to write");
                        Console.WriteLine("36) Fine flow monitoring:          " + WTX_obj.fineFlowMonitoring     + " Press '36' and a value to write");
                        Console.WriteLine("38) Fine flow monitoring time:     " + WTX_obj.fineFlowMonitoringTime + " Press '38' and a value to write");

                        Console.WriteLine("40) Delay time after fine flow:    " + WTX_obj.delayTimeAfterFineFlow + " Press '40' and a value to write");
                        Console.WriteLine("41) Systematic difference:         " + WTX_obj.systematicDifference + " Press '41' and a value to write");
                        Console.WriteLine("42) Downwards dosing:              " + WTX_obj.downardsDosing + " Press '42' and a value to write");
                        Console.WriteLine("43) Valve control:                 " + WTX_obj.valveControl   + " Press '43' and a value to write");
                        Console.WriteLine("44) Emptying mode:                 " + WTX_obj.emptyingMode   + " Press '44' and a value to write");

                        Console.WriteLine("\nPress 'o' again to hide the output words.");

                    }
                    
                }
            }
        }

        private static void inputWriteOutputWordsFiller()
        {
            int valueParam = 0;

            char wordNumberChar  = writeOutputWord.KeyChar;
            int wordNumberParam = Convert.ToInt32(wordNumberChar);

            switch (wordNumberParam)
            {
                case 9: WTX_obj.writeOutputWordU16(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 10: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 12: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 14: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 16: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 18: WTX_obj.writeOutputWordU08(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 19: WTX_obj.writeOutputWordU16(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 20: WTX_obj.writeOutputWordU16(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 21: WTX_obj.writeOutputWordU16(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 22: WTX_obj.writeOutputWordU16(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 23: WTX_obj.writeOutputWordU08(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 24: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 26: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 28: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 30: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 32: WTX_obj.writeOutputWordU16(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 33: WTX_obj.writeOutputWordU16(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 34: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 36: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 38: WTX_obj.writeOutputWordU16(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 40: WTX_obj.writeOutputWordU08(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 41: WTX_obj.writeOutputWordS32(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 42: WTX_obj.writeOutputWordU08(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 43: WTX_obj.writeOutputWordU08(valueParam, (ushort) wordNumberParam, Write_DataReceived);
                    break;
                case 44: WTX_obj.writeOutputWordU08(valueParam, (ushort) wordNumberParam, Write_DataReceived); 
                    break;

            }
        }

        private static void zero_load_nominal_load_input()
        {

            Console.Clear();

            Console.WriteLine("\nPlease tip the value for the zero load/dead load and tip enter to confirm : ");

            Preload_str = Console.ReadLine();
            str_comma_dot = Preload_str.Replace(".", ",");           // For converting into a floating-point number.
            Preload = double.Parse(str_comma_dot);                   // By using the method 'double.Parse(..)' you can use dots and commas.


            Console.WriteLine("\nPlease tip the value for the span/nominal load and tip enter to confirm : ");

            Capacity_str = Console.ReadLine();
            str_comma_dot = Capacity_str.Replace(".", ",");           // For converting into a floating-point number.
            Capacity = double.Parse(str_comma_dot);                   // By using the method 'double.Parse(..)' you can use dots and commas.

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
         * callback method 'Write_DataReceived' is empty, there is no need to print the values on the console twice because the timer in class 'WTX120' does that already 
         * in a short time intervall. 
         * If you do not want a timer you can put f.e. the printing method into 'Write_DataReceived' f.e. .
         */
         
        private static void Write_DataReceived(IDeviceValues obj)
        {
            throw new NotImplementedException();
        }
        

    }
}
