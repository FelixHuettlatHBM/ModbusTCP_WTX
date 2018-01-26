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

        private static int timer_interval;
        private static string input_IP_Adress;
        private static ushort input_numInputs;
        private static string[] data_str_arr;
        private static ushort[] previous_data_ushort_arr;

        private static System.Timers.Timer aTimer;
        private static ConsoleKeyInfo value_outputwords;
        private static ConsoleKeyInfo value_exitapplication;
        private static bool compare_data_received;
        private static bool compare_values_changed;

        static void Main()
        {
            compare_data_received = false;
            compare_values_changed = true;

            Console.WriteLine("\nTCPModbus Interface for weighting terminals of HBM\nEnter e to exit the application");
            Console.WriteLine("\n\n Please enter the IP Adress with dots (see on the device, on a tip-on note) \n Default setting: 172.19.103.8\n ");
            
            input_IP_Adress=Console.ReadLine();
            Console.Clear();


            Modbus_TCP_obj = new ModbusTCP();
            WTX_obj = new WTX120("WTX120_1", Modbus_TCP_obj);

            Modbus_TCP_obj.IP_Adress = input_IP_Adress;

            Modbus_TCP_obj.Connect();   // Connection to the timer is established. 

           
            if (Modbus_TCP_obj.is_connected)        // If a connection has been established successfully:
            {

                data_str_arr = new string[59];      // Initialize the data arrays: 
                previous_data_ushort_arr = new ushort[59];

                for (int i = 0; i < 59; i++)
                {
                    data_str_arr[i] = "0";
                    previous_data_ushort_arr[i] = 0;
                }

                Console.WriteLine("\nTCPModbus Interface for weighting terminals of HBM\n\n");
                Console.WriteLine("Connection successful!\nPlease enter the timer interval specifiying the time after the values\nare refreshed on the console(for example: 200)\nIt is recommended to choose a value between 150 and 1000. (in milli seconds)");
                timer_interval = Convert.ToInt32(Console.ReadLine());

                print_table_for_register_words(true);       // Prints the options for the number of read register. The parameter "beginning" stands for the moment of the program, in which is this program is called Either on the beginning(=true) or during the execution while the timer is running (=false).

                set_number_inputs();
         
                initialize_timer(timer_interval);

                Modbus_TCP_obj.Sending_interval = timer_interval;

                // This while loop is repeated till the user enters e. After 500ms the register of the device is read out. In the while-loop the user
                // can select commands, which are send immediately to the device. 
                while (value_exitapplication.KeyChar != 'e')
                {
                    compare_data_received = false;
                    value_outputwords = Console.ReadKey();

                    switch (value_outputwords.KeyChar)
                    {
                        case '0': WTX_obj.Async_Call(0x1, Write_DataReceived); break;     // Taring 
                        case '1': WTX_obj.Async_Call(0x2, Write_DataReceived); break;     // Gross/Net
                        case '2': WTX_obj.Async_Call(0x40, Write_DataReceived); break;    // Zeroing
                        case '3': WTX_obj.Async_Call(0x80, Write_DataReceived); break;    // Adjust zero 
                        case '4': WTX_obj.Async_Call(0x100, Write_DataReceived); break;   // Adjust nominal
                        case '5': WTX_obj.Async_Call(0x800, Write_DataReceived); break;   // Activate data
                        case '6': WTX_obj.Async_Call(0x1000, Write_DataReceived); break;  // Manual taring
                        case '7': WTX_obj.Async_Call(0x4000, Write_DataReceived); break;  // Weight storage

                        //case '8' : WTX_obj.Async_Call(0x4, Write_DataReceived);           break;   // Clear dosing results
                        //case '9' : WTX_obj.Async_Call(0x8, Write_DataReceived);           break;   // Abort dosing 
                        // ... 

                        default: break;

                    }   // end switch-case

                    value_exitapplication = Console.ReadKey();
                    if (value_exitapplication.KeyChar == 'e')
                        break;

                    if(value_exitapplication.KeyChar == 'n')   // Change number of bytes, which will be read from the register. 
                    {
                        aTimer.Stop();

                        print_table_for_register_words(false);  // The parameter stands for the moment of the program, in which is this program is called. ..
                        set_number_inputs();                    //...Either on the beginning(=true) or during the execution while the timer is running (=false).

                        aTimer.Start();
                    }

                } // end while

            } // end if
            else
            {
                Console.WriteLine("\nTCPModbus Interface for weighting terminals of HBM");
                Console.WriteLine("\nConnection establishment was not succesful!\nPlease restart and enter the right IP adress. The application is canceled.");
                Thread.Sleep(4000);
            }

        } // end main method


        // Event method, which will be triggered after a interval of the timer is elapsed- 
        // After triggering (after 500ms) the register is read. 
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (compare_data_received == false && compare_values_changed == true)
                Console.WriteLine("\n\n\n\tMeasurement at the time: {0:HH:mm:ss.fff}. \nNo values - Connection not successfully established. Exit with 'e' .\n", e.SignalTime);

            try
            {
                compare_data_received = false;
                WTX_obj.Async_Call(0x00, Read_DataReceived);
            }
            catch (System.NullReferenceException) 
            { 
                Console.WriteLine("System.NullReferenceException");
            }
        }

        // This is a callback-method, which will be called after the reading of the register is finished. 
        public static void Read_DataReceived(IDeviceValues Device_Values)
        {
        
            compare_values_changed = false; // for every iteration of this method, compare_test has to be "true" in the beginning. 

            for (int i = 0; i < WTX_obj.get_data_ushort.Length; i++)
            {
                // If one value of the data changes, the boolean value "compare_test" will be set to
                // false and the data array "data_str_arr" will be updated in the following, as well as the GUI form.
                // ("compare_test" is for the purpose of comparision.)
                if (WTX_obj.get_data_ushort[i] != previous_data_ushort_arr[i])
                {
                    compare_values_changed = true;
                }
            }

            // If the data is unequal to the previous one, the array "data_str_arr" will be updated in the following, as well as the GUI form. 
            if (compare_values_changed == true)
            {
                compare_data_received = true;
                data_str_arr = Device_Values.get_data_str;
                Console.Clear();
                reset_values_on_console(Device_Values);
            }

            
        }

        // This is a callback-method, which will be called after the writing of the register is finished. 
        public static void Write_DataReceived(IDeviceValues Device_Values)
        {
            data_str_arr = Device_Values.get_data_str;
            Console.Clear();
        }

        // This method initializes the with the timer interval as a parameter: 
        //
        private static void initialize_timer(int timer_interval) 
        {
            // Create a timer with an interval of 500ms. 

            aTimer = new System.Timers.Timer(timer_interval);

            // Connect the elapsed event for the timer. 

            aTimer.Elapsed += OnTimedEvent;

            aTimer.AutoReset = true;

            aTimer.Enabled = true;    
        }

        // This method sets the number of read bytes (words) in the register of the device. 
        // You can set '1' for the net value. '2','3' or '4' for the net and gross value. '5' for the net,gross value and the weight status(word[4]) for the bits representing the weight status like weight moving, weight type, scale range and so ... 
        // You can set '6' for reading the previous bytes(gross/net values, weight status) and for enabling to write on the register. With '6', bit "application mode", "decimals", "unit", "handshake" and "status" is read. 
        // Especially the handshake and status bit is used for writing. 
        private static void set_number_inputs()
        {
            input_numInputs = (ushort)Convert.ToInt32(Console.ReadLine());
            Modbus_TCP_obj.NumOfPoints = input_numInputs;
        }

        // This method prints the values of the device (as a integer and the interpreted string) as well as the description of each bit. 
        private static void reset_values_on_console(IDeviceValues Device_Values)
        {

            Console.WriteLine("Options to set the device : Enter the following keys:\nn-Choose the number of bytes read from the register |");
            if (Device_Values.application_mode == 0)
            {
                Console.WriteLine("0-Taring | 1-Gross/net  | 2-Zeroing  | 3- Adjust zero | 4-Adjust nominal |\n5-Activate Data \t| 6-Manual taring \t      | 7-Weight storage\n");
            }
            else
                if (Device_Values.application_mode == 1 || Device_Values.application_mode == 2)
                    Console.WriteLine("\n 0-Taring | 1-Gross/net | 2-Clear dosing | 3- Abort dosing | 4-Start dosing |\n 5-Zeroing | 6-Adjust zero | 7-Adjust nominal | 8-Activate data | 9-Weight storage | \n m-Manual redosing\n");

            if (Device_Values.application_mode == 0 || Device_Values.application_mode == 2 || Device_Values.application_mode == 1)   // If the device is a compatible mode (standard=0 or filler=1 | filler=2) 
            {

                // The values are printed on the console according to the input - "numInputs": 

                if (input_numInputs == 1)
                {
                    Console.WriteLine("Net value:                     " + data_str_arr[1] + "\t  As an Integer:  " + Device_Values.NetValue);
                }
                else
                    if (input_numInputs == 2 || input_numInputs == 3 || input_numInputs == 4)
                    {
                        Console.WriteLine("Net plus gross value:          " + data_str_arr[0] + "\t  As an Integer:  " + Device_Values.NetandGrossValue);
                        Console.WriteLine("Net value:                     " + data_str_arr[1] + "\t  As an Integer:  " + Device_Values.NetValue);
                        Console.WriteLine("Gross value:                   " + data_str_arr[2] + "\t  As an Integer:  " + Device_Values.GrossValue);
                    }
                    else
                        if (input_numInputs == 5)
                        {
                            Console.WriteLine("Net plus gross value:          " + data_str_arr[0] + "\t  As an Integer:  " + Device_Values.NetandGrossValue);
                            Console.WriteLine("Net value:                     " + data_str_arr[1] + "\t  As an Integer:  " + Device_Values.NetValue);
                            Console.WriteLine("Gross value:                   " + data_str_arr[2] + "\t  As an Integer:  " + Device_Values.GrossValue);
                            Console.WriteLine("General weight error:          " + data_str_arr[3] + "\t  As an Integer:  " + Device_Values.general_weight_error);
                            Console.WriteLine("Scale alarm triggered:         " + data_str_arr[4] + "\t  As an Integer:  " + Device_Values.limit_status);
                            Console.WriteLine("Scale seal is open:            " + data_str_arr[7] + "\t  As an Integer:  " + Device_Values.scale_seal_is_open);
                            Console.WriteLine("Manual tare:                   " + data_str_arr[8] + "\t  As an Integer:  " + Device_Values.manual_tare);
                            Console.WriteLine("Weight type:                   " + data_str_arr[9] + "\t  As an Integer:  " + Device_Values.weight_type);
                            Console.WriteLine("Scale range:                   " + data_str_arr[10] + "\t  As an Integer:  " + Device_Values.scale_range);
                            Console.WriteLine("Zero required/True zero:       " + data_str_arr[11] + "\t  As an Integer:  " + Device_Values.zero_required);
                            Console.WriteLine("Weight within center of zero:  " + data_str_arr[12] + "\t  As an Integer:  " + Device_Values.weight_within_the_center_of_zero);
                            Console.WriteLine("Weight in zero range:          " + data_str_arr[13] + "\t  As an Integer:  " + Device_Values.weight_within_the_center_of_zero);
                            Console.WriteLine("Limit status:                  " + data_str_arr[5] + "  As an Integer:  " + Device_Values.limit_status);
                            Console.WriteLine("Weight moving:                 " + data_str_arr[6] + "  As an Integer:" + Device_Values.weight_moving);
                        }
                        else
                            if (input_numInputs == 6)
                            {
                                Console.WriteLine("Net plus gross value:          " + data_str_arr[0] + "\t  As an Integer:  " + Device_Values.NetandGrossValue);
                                Console.WriteLine("Net value:                     " + data_str_arr[1] + "\t  As an Integer:  " + Device_Values.NetValue);
                                Console.WriteLine("Gross value:                   " + data_str_arr[2] + "\t  As an Integer:  " + Device_Values.GrossValue);
                                Console.WriteLine("General weight error:          " + data_str_arr[3] + "\t  As an Integer:  " + Device_Values.general_weight_error);
                                Console.WriteLine("Scale alarm triggered:         " + data_str_arr[4] + "\t  As an Integer:  " + Device_Values.limit_status);
                                Console.WriteLine("Scale seal is open:            " + data_str_arr[7] + "\t  As an Integer:  " + Device_Values.scale_seal_is_open);
                                Console.WriteLine("Manual tare:                   " + data_str_arr[8] + "\t  As an Integer:  " + Device_Values.manual_tare);
                                Console.WriteLine("Weight type:                   " + data_str_arr[9] + "\t  As an Integer:  " + Device_Values.weight_type);
                                Console.WriteLine("Scale range:                   " + data_str_arr[10] + "\t  As an Integer:  " + Device_Values.scale_range);
                                Console.WriteLine("Zero required/True zero:       " + data_str_arr[11] + "\t  As an Integer:  " + Device_Values.zero_required);
                                Console.WriteLine("Weight within center of zero:  " + data_str_arr[12] + "\t  As an Integer:  " + Device_Values.weight_within_the_center_of_zero);
                                Console.WriteLine("Weight in zero range:          " + data_str_arr[13] + "\t  As an Integer:  " + Device_Values.weight_within_the_center_of_zero);
                                Console.WriteLine("Application mode:              " + data_str_arr[14] + "\t  As an Integer:  " + Device_Values.application_mode);
                                Console.WriteLine("Decimal places:                " + data_str_arr[15] + "\t  As an Integer:  " + Device_Values.decimals);
                                Console.WriteLine("Unit:                          " + data_str_arr[16] + "\t  As an Integer:  " + Device_Values.unit);
                                Console.WriteLine("Handshake:                     " + data_str_arr[17] + "\t  As an Integer:  " + Device_Values.handshake);
                                Console.WriteLine("Status:                        " + data_str_arr[18] + "\t  As an Integer:  " + Device_Values.status);

                                Console.WriteLine("Limit status:                  " + data_str_arr[5] + "  As an Integer:  " + Device_Values.limit_status);
                                Console.WriteLine("Weight moving:                 " + data_str_arr[6] + "  As an Integer:" + Device_Values.weight_moving);
                            }
                            else
                                Console.WriteLine("\nWrong input for the number of bytes, which should be read from the register!\nPlease enter 'n' to choose again.");
            }
            else
                Console.WriteLine("\n\t No compatible mode set for the WTX-device");
        }

        // This method prints the table to choose how many byte of the register are read. 
        // The parameter "beginning" stands for the moment of the program, in which is this program is called Either on the beginning(=true) or during the execution while the timer is running (=false).
        private static void print_table_for_register_words(bool beginning)
        {
            Console.Clear();

            Console.WriteLine("TCPModbus Interface for weighting terminals of HBM\n");                  
            
            if(beginning==true)
                Console.WriteLine("Timer/Sending interval received.\nPlease enter how many words(bytes) you want to read from the register\nof the device. See the following table for choosing:");

            if (beginning == false)
                Console.WriteLine("Executing was interrupted to choose another number of bytes, which will be read from the register.\nPlease enter how many words(bytes) you want to read from the register\nof the device. See the following table for choosing:");

            Console.WriteLine("\nEnter '1'       : Enables reading of ... \n\t\t  word[0]- netto value.\n");
            Console.WriteLine(  "Enter '2','3',4': Enables reading of ... \n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");
            Console.WriteLine(  "Enter '5'       : Enables reading of ... \n\t\t  word[4]- weight moving,weight type,scale range,..(see manual)\n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");
            Console.WriteLine(  "Enter '6'       : Enables writing to the register and reading of ... \n\t\t  word[5]- application mode,decimals,unit,handshake,status bit\n\t\t  word[4]- weight moving,weight type,scale range,..(see manual)\n\t\t  word[2]- gross value. \n\t\t  Word[0]- netto value.\n");
         
            Console.WriteLine("It is recommended to use at least '6' for writing and reading. \nDefault setting for the full application in filler mode : '38'\nPlease tip the button 'Enter' after you typed in the number '1' or '2' or...'6'");
        }
    }
}

