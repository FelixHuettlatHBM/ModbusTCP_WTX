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

namespace WTXModbus
{   
    /// <summary>
    /// This class implements a console application instead of a windows form. An Object of the class ModbusTcp and WTX120 are initialized as a publisher
    /// and subscriber. Afterwars a connection to the device is build and the timer/sending interval is set. 
    /// A timer with for example 500ms is created. Adter 500ms an event is triggered, which executes the method "OnTimedEvent" reading the register of the device
    /// by an asynchronous call in the method "WTX_obj.Async_Call". As soon as the reading is finisihed, the callback method "Read_DataReceived" takes over the
    /// new data , which have already been interpreted in class WTX120, so the data is given as a string array. 
    /// The data is also printed on the console in the callback method "Read_DataReceived". 
    /// Being in the while-loop it is possible to select commands to the device. For example taring, change from gross to net value, stop dosing, zeroing and so on. 
    /// 
    /// This is overall just a simple application as an example. A significantly broad presentation is given by the windows form, but for starters it is recommended. 
    /// </summary>
 
    static class Program
    {
        private static ModbusTCP Modbus_TCP_obj;
        private static WTX120 WTX_obj;

        private static string[] data_str_arr;

        private static System.Timers.Timer aTimer;

        private static string t;
        private static string input_IP_Adress;

        private static ConsoleKeyInfo value_outputwords;
        private static ConsoleKeyInfo value_exitapplication;

        static void Main()
        {

            Console.WriteLine("\n\n Please enter the IP Adress with dots (see on the device, on a tip-on note) \n Default setting: 172.19.103.8\n ");
            input_IP_Adress=Console.ReadLine();

            Modbus_TCP_obj = new ModbusTCP(input_IP_Adress);
            WTX_obj = new WTX120("WTX120_1", Modbus_TCP_obj);        

            // Create a timer with an interval of 500ms. 
            aTimer = new System.Timers.Timer(500);     
            // Connect the elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            data_str_arr = new string[59];
            t = "";

            for (int i = 0; i < 59; i++)
                data_str_arr[i] = "0";
      
            Modbus_TCP_obj.Connect();   // Connection to the timer is established. 

            Modbus_TCP_obj.Sending_interval=Convert.ToInt32(aTimer.Interval);

            Console.Write("Waiting for async callback");


            // This while loop is repeated till the user enters e. After 500ms the register of the device is read out. In the while-loop the user
            // can select commands, which are send immediately to the device. 
            while (value_exitapplication.KeyChar != 'e')
            {

                value_outputwords = Console.ReadKey();

                switch (value_outputwords.KeyChar)
                {                
                    case '0': WTX_obj.Async_Call(0x1, Write_DataReceived);    break;     // Taring 
                    case '1': WTX_obj.Async_Call(0x2, Write_DataReceived);    break;     // Gross/Net
                    case '2': WTX_obj.Async_Call(0x40, Write_DataReceived);   break;     // Zeroing
                    case '3': WTX_obj.Async_Call(0x80, Write_DataReceived);   break;     // Adjust zero 
                    case '4': WTX_obj.Async_Call(0x100, Write_DataReceived);  break;     // Adjust nominal
                    case '5': WTX_obj.Async_Call(0x800, Write_DataReceived);  break;     // Activate data
                    case '6': WTX_obj.Async_Call(0x1000, Write_DataReceived); break;     // Manual taring
                    case '7': WTX_obj.Async_Call(0x4000, Write_DataReceived); break;     // Weight storage

                    //case '8' : WTX_obj.Async_Call(0x4, Write_DataReceived);           break;   // Clear dosing results
                    //case '9' : WTX_obj.Async_Call(0x8, Write_DataReceived);           break;   // Abort dosing 
                    // ... 

                    default: break;
                }
                Thread.Sleep(100);

                value_exitapplication = Console.ReadKey();
                if (value_exitapplication.KeyChar == 'e')
                    break;
            }
             
        }

        // Event method, which will be triggered after a interval of the timer is elapsed- 
        // After triggering (after 500ms) the register is read. 
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                WTX_obj.Async_Call(0x00, Read_DataReceived);
            }
            catch (System.NullReferenceException) { Console.WriteLine("System.NullReferenceException"); }

            Console.WriteLine("\n\tMeasurement at the time : {0:HH:mm:ss.fff} , exit with 'e'\n", e.SignalTime);
        }

        // This is a callback-method, which will be called after the reading of the register is finished. 
        public static void Read_DataReceived(IDeviceValues Device_Values)
        {
            data_str_arr = Device_Values.get_data_str;      // The values are updated. 

            Console.WriteLine("Setting for the output words : Enter the following keys\n");
            if (Device_Values.application_mode == 0)
            {           
                Console.WriteLine("0-Taring | 1-Gross/net  | 2-Zeroing  | 3- Adjust zero | 4-Adjust nominal |\n5-Activate Data \t| 6-Manual taring \t      | 7-Weight storage\n");
            }
            else
                if (Device_Values.application_mode == 1 || Device_Values.application_mode == 2)
                    Console.WriteLine("\n 0-Taring | 1-Gross/net | 2-Clear dosing | 3- Abort dosing | 4-Start dosing |\n 5-Zeroing | 6-Adjust zero | 7-Adjust nominal | 8-Activate data | 9-Weight storage | \n m-Manual redosing\n");

            if (Device_Values.application_mode == 0 || Device_Values.application_mode == 2 || Device_Values.application_mode == 1)   // If the device is a compatible mode (standard=0 or filler=1 | filler=2) 
            {

                // The values are printed on the console: 

                Console.WriteLine("Net and gross value:           " + data_str_arr[0] + "\t  As an Integer:  " + Device_Values.NetandGrossValue);
                Console.WriteLine("Net value:                     " + data_str_arr[1] + "\t  As an Integer:  " + Device_Values.NetValue);
                Console.WriteLine("Gross value:                   " + data_str_arr[2] + "\t  As an Integer:  " + Device_Values.GrossValue);

                Console.WriteLine("General weight error:          " + data_str_arr[3] + "\t  As an Integer:  " + Device_Values.general_weight_error);
                Console.WriteLine("Scale alarm triggered:         " + data_str_arr[4] + "\t  As an Integer:  " + Device_Values.limit_status);
                Console.WriteLine("Scale seal is open:            " + data_str_arr[7] + "\t  As an Integer:  " + Device_Values.scale_seal_is_open);
                Console.WriteLine("Manual tare:                   " + data_str_arr[8] + "\t  As an Integer:  " + Device_Values.manual_tare);
                Console.WriteLine("Weight type:                   " + data_str_arr[9] + "\t  As an Integer:  " + Device_Values.weight_type);
                Console.WriteLine("Scale range:                   " + data_str_arr[10]+ "\t  As an Integer:  " + Device_Values.scale_range);
                Console.WriteLine("Zero required/True zero:       " + data_str_arr[11]+ "\t  As an Integer:  " + Device_Values.zero_required);
                Console.WriteLine("Weight within center of zero:  " + data_str_arr[12]+ "\t  As an Integer:  " + Device_Values.weight_within_the_center_of_zero);
                Console.WriteLine("Weight in zero range:          " + data_str_arr[13]+ "\t  As an Integer:  " + Device_Values.weight_within_the_center_of_zero);

                Console.WriteLine("Application mode:              " + data_str_arr[14]+ "\t  As an Integer:  " + Device_Values.application_mode);
                Console.WriteLine("Decimal places:                " + data_str_arr[15]+ "\t  As an Integer:  " + Device_Values.decimals);
                Console.WriteLine("Unit:                          " + data_str_arr[16]+ "\t  As an Integer:  " + Device_Values.unit);
                Console.WriteLine("Handshake:                     " + data_str_arr[17]+ "\t  As an Integer:  " + Device_Values.handshake);
                Console.WriteLine("Status:                        " + data_str_arr[18]+ "\t  As an Integer:  " + Device_Values.status);

                Console.WriteLine("Limit status:                  " + data_str_arr[5] + "  As an Integer:  " + Device_Values.limit_status);
                Console.WriteLine("Weight moving:                 " + data_str_arr[6] + "  As an Integer:" + Device_Values.weight_moving);

            }
            else
                Console.WriteLine("\n\t No compatible mode set for the WTX-device");
        }

        // This is a callback-method, which will be called after the writing of the register is finished. 
        public static void Write_DataReceived(IDeviceValues Device_Values)
        {
            data_str_arr = Device_Values.get_data_str; // The values are updated. 
        }


    }
}

