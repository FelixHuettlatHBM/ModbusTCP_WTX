
using HBM.WT.API.COMMON;
using HBM.WT.API.WTX;
using HBM.WT.API.WTX.Jet;
using HBM.WT.API.WTX.Modbus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

//using Hbm.Wt.WTXInterface.WTX120_Jet;

namespace WtConsole
{
    class Program
    {
        private static HBM.WT.API.COMMON.BaseWTDevice WTXObj;

        private static System.Timers.Timer aTimer;
        private static String ipAddr;
        private static int timer_interval;
        private static bool compare_test;
        private static string[] previous_data_str_arr;

        private static ConsoleKeyInfo value_exitapplication;

        private static string previousNetValue;
        //private static string[] data_str_arr;

        static string MENU_REQUEST = "folow instructions: \n \r"
            + "<read> <parameter> \n"
            + "<write> <parameter> <value> \n"
            + "<test> \n"
            + "<show> <filter> \n"
            + "<quit> \n";

        delegate int SelectFunktion(string[] args);
        static Dictionary<string, SelectFunktion> FUNKTON_SELECT = new Dictionary<string, SelectFunktion> {
            { "read", ReadParameter },
            { "write", WriteParameter },
            { "show" , ShowProperties },
            { "test" , TestDeviceLayer },
        };

        static INetConnection s_Connection;
        
        static void Main(string[] args) {

            previousNetValue = "";
            timer_interval = 500;
            compare_test = true;

            Thread thread1 = new Thread(new ThreadStart(InputOutput));

            try {

                switch (args[0]) {
                    /*
                    case "-can":
                        ICANCommon hw = new CANPeak(new object[] { 0x51 });
                        CANDaemon can = CANDaemonManager.CreateInstance(hw);

                        CANBaudrate baudrate = CANBaudrate.B0125;
                        switch (args[1]) {
                            case "1000":baudrate = CANBaudrate.B1000; break;
                            case "500": baudrate = CANBaudrate.B0500; break;
                            case "250": baudrate = CANBaudrate.B0250; break;
                            case "150": baudrate = CANBaudrate.B0125; break;
                        }

                        Console.Write("Init CAN-Hardware...");
                        can.Initialize(baudrate);
                        Console.WriteLine("OK");

                        Console.Write("Connect to Device...");
                        byte addr = byte.Parse(args[2]);

                        s_Connection = new CANOpenConnection(addr, can);

                        Console.WriteLine("OK");
                        break;
                        */
                    case "-jet":

                        ipAddr = "wss://" + args[1];
                        Console.Write("Initialize Jet-Peer to address " + ipAddr + "...");

                        s_Connection = new JetBusConnection(ipAddr, "Administrator", "wtx", delegate { return true; });

                        Console.WriteLine("OK");
 
                        //s_Connection.BusActivityDetection += S_Connection_BusActivityDetection;

                        Console.WriteLine("Parameter are fetching: ");
                        Console.Write((s_Connection as JetBusConnection).BufferToString());
                        
                        WTXObj = new HBM.WT.API.WTX.WTXJet(s_Connection);

                        Console.WriteLine("Parameter fetched");
                        Console.WriteLine("Net value : "   + WTXObj.NetValue);
                        Console.WriteLine("Gross value : " + WTXObj.GrossValue);                    
                        Console.WriteLine("Decimals : "    + WTXObj.decimals);

                        //Console.WriteLine("Weight moving : " + WTXObj.weight_moving);

                        //Console.WriteLine("dosingCounter : " +  WTXObj.dosing_count);
                        //Console.WriteLine("dosingStatus  : "  + WTXObj.dosing_process_status);
                        //Console.WriteLine("dosingResult  : "  + WTXObj.dosing_result);

                        Console.ReadLine();

                        break;

                    case "-modbus":

                        ipAddr = args[1];
                        
                        s_Connection = new ModbusConnection(ipAddr);


                        WTXObj = new HBM.WT.API.WTX.WTXModbus(s_Connection, 100);

                        // Konstruktor neu : Obj. von ModbusConnection, Timer Intervall

                        previous_data_str_arr = new string[59];

                        for (int i = 0; i < 59; i++)
                            previous_data_str_arr[i] = "0";

                        // Start asynchronous data transfer : Method - Nur bei einer Änderung Werte ausgeben - Was abrufbar ist aus der Klasse Program zu WTX120_Modbus

                        WTXObj.getConnection.Connect();
                        
                        WTXObj.getConnection.Sending_interval = timer_interval;

                        //WTXObj.isDataReceived = false;

                        thread1.Start();     // Thread für eine Eingabe. Wenn 'e' eingetippt wurde, wird die Anwendung bzgl. Modbus beendet. 

                        //while (value_exitapplication.KeyChar != 'e')        // Solange bis Schleife verlassen wird mit einem Kommando für Exit.
                        //{
                        //    if (WTXObj.isDataReceived == true)      // Weiterer Asynchronener Aufruf eventbasiert
                        //    {
                        //        reset_values_on_console(WTXObj.DeviceValues);
                        //        WTXObj.isDataReceived = false;      // rücksetzen in der GUI. 
                        //    }
                        //}

                        //Thread.Sleep(2000);

                        
                        WTXObj.DataUpdateEvent += AsyncUpdateData;

                        //WTXObj.Shutdown += Handler1;
                        //WTXObj.OnShutdown().Wait();

                        break;
                }


                if (args[0] == "-jet")
                {
                    string[] input = null;
                    do
                    {
                        try
                        {
                            Console.Write(MENU_REQUEST);
                            Console.Write("/> ");
                            input = Console.ReadLine().Split(new char[] { ' ' });

                            if (FUNKTON_SELECT.ContainsKey(input[0].ToLower()))
                            {

                                FUNKTON_SELECT[input[0].ToLower()](input);
                                Console.ReadLine();
                            }
                            else
                            {

                                Console.WriteLine("Unknown instruction - try again \n");
                            }
                        }
                        catch (InterfaceException operationException)
                        {

                            Console.WriteLine("Exception is thrown: " + operationException.Message);
                            Console.ReadLine();
                            continue;
                        }

                    } while (!input[0].ToUpper().Equals("QUIT"));
                    Console.WriteLine("good bye");

                }
            }
            catch (InterfaceException e) {
                Console.WriteLine("FAILED with error: " + e.Error);
                Console.ReadLine();
            }
            /*
            catch (Exception e) {
                Console.WriteLine("Jetbus:" + e.Message);
                Console.ReadLine();
            }*/
        }

        //public static async Task Handler1(object sender, EventArgs e /*EventArgs e*/)
        //
        //await WTXObj.UpdateEvent(sender, e);

        //ushort[] TESTushortarray = new ushort[100];
        //TESTushortarray = await Task.FromResult<ushort[]>(WTXObj.getValuesAsync());

        //await Task.FromResult<IDeviceData>(WTXObj.DeviceValues);

        //reset_values_on_console(WTXObj.DeviceValues);
        //}



        private static void InputOutput()
        {
                value_exitapplication = Console.ReadKey();
        }

        private static void S_Connection_BusActivityDetection(object sender, EventArgs e)
        {
            Console.WriteLine((e as NetConnectionEventArgs<string>).Args.ToString());
        }


        // This method prints the values of the device (as a integer and the interpreted string) as well as the description of each bit. 
        private static void AsyncUpdateData(object arg1, NetConnectionEventArgs<ushort[]> arg2)
        {
            
            compare_test = true;
            if (previousNetValue != WTXObj.getDataStr[0])                   
                compare_test = false;
            else
                compare_test = true;
        
            if ( (compare_test == false) && ( WTXObj.DeviceValues!=null ))
            //if(! (previous_data_str_arr.Equals(WTXObj.getDataStr) ))
            //if(ParamValues != null)
            {

                Console.Clear();
            
                Console.WriteLine("Net value:                     " + WTXObj.getDataStr[0] +   "\t  As an Integer:  " + WTXObj.DeviceValues.NetValue);        
                Console.WriteLine("Gross value:                   " + WTXObj.getDataStr[1] +   "\t  As an Integer:  " + WTXObj.DeviceValues.GrossValue);    
                Console.WriteLine("General weight error:          " + WTXObj.getDataStr[2] +   "\t  As an Integer:  " + WTXObj.DeviceValues.generalWeightError);    
                Console.WriteLine("Scale alarm triggered:         " + WTXObj.getDataStr[3] +   "\t  As an Integer:  " + WTXObj.DeviceValues.limitStatus);    
                Console.WriteLine("Scale seal is open:            " + WTXObj.getDataStr[6]   + "\t  As an Integer:  " + WTXObj.DeviceValues.scaleSealIsOpen);
                Console.WriteLine("Manual tare:                   " + WTXObj.getDataStr[7]   + "\t  As an Integer:  " + WTXObj.DeviceValues.manualTare);
                Console.WriteLine("Weight type:                   " + WTXObj.getDataStr[8]   + "\t  As an Integer:  " + WTXObj.DeviceValues.weightType);
                Console.WriteLine("Scale range:                   " + WTXObj.getDataStr[9]   + "\t  As an Integer:  " + WTXObj.DeviceValues.scaleRange);
                Console.WriteLine("Zero required/True zero:       " + WTXObj.getDataStr[10]  + "\t  As an Integer:  " + WTXObj.DeviceValues.zeroRequired);
                Console.WriteLine("Weight within center of zero:  " + WTXObj.getDataStr[11]  + "\t  As an Integer:  " + WTXObj.DeviceValues.weightWithinTheCenterOfZero);
                Console.WriteLine("Weight in zero range:          " + WTXObj.getDataStr[12]  + "\t  As an Integer:  " + WTXObj.DeviceValues.weightWithinTheCenterOfZero);
                Console.WriteLine("Application mode:              " + WTXObj.getDataStr[13]  + "\t  As an Integer:  " + WTXObj.DeviceValues.applicationMode);
                Console.WriteLine("Decimal places:                " + WTXObj.getDataStr[14]  + "\t  As an Integer:  " + WTXObj.DeviceValues.decimals);
                Console.WriteLine("Unit:                          " + WTXObj.getDataStr[15]  + "\t  As an Integer:  " + WTXObj.DeviceValues.unit);
                Console.WriteLine("Handshake:                     " + WTXObj.getDataStr[16]  + "\t  As an Integer:  " + WTXObj.DeviceValues.handshake);
                Console.WriteLine("Status:                        " + WTXObj.getDataStr[17]  + "\t  As an Integer:  " + WTXObj.DeviceValues.status);
                Console.WriteLine("Limit status:                  " + WTXObj.getDataStr[4]  + "  As an Integer:  "    + WTXObj.DeviceValues.limitStatus);
                Console.WriteLine("Weight moving:                 " + WTXObj.getDataStr[5]  + "  As an Integer:"      + WTXObj.DeviceValues.weightMoving);

                Console.WriteLine("\n Press any key to exit");

                
            }
            previous_data_str_arr = WTXObj.getDataStr;
            previousNetValue = WTXObj.getDataStr[0];            
        }

            private static int ReadParameter(string[] args) {
            Console.Write(args[0] + "Read... ");
            if (args.Length < 2) return -1;
            int intValue = s_Connection.Read<int>(args[1]);
            
            Console.WriteLine(intValue);
            return 0;
        }

        private static int WriteParameter(string[] args) {
            Console.Write(args[0] + "Write... ");
            if (args.Length < 3) return -1;

            int value = Convert.ToInt32(args[2]);
            s_Connection.Write<int>(args[1], value);
            Console.WriteLine("OK");

            return 0;
        }

        private static int ShowProperties (string[] args) {

            HBM.WT.API.COMMON.BaseWTDevice parameter = new HBM.WT.API.WTX.WTXJet(s_Connection);

            Type type = parameter.GetType();

            PropertyInfo[] properties = type.GetProperties();
            foreach(PropertyInfo prop in properties) {
                Console.WriteLine(prop.ToString());
                                
            }

            return 0;
           
        }

        private static uint StringToID(string arg) {
            if (arg.Contains("0x")) {
                return Convert.ToUInt32(arg, 16);
            } else {
                return Convert.ToUInt32(arg, 10);
            }
        }

        private static int TestDeviceLayer(string[] arg) {
            //Hbm.Wt.WTXInterface.WTX120_Jet.ParameterProperty parameter = 
            //    new Hbm.Wt.WTXInterface.WTX120_Jet.ParameterProperty(s_Connection);

            HBM.WT.API.COMMON.BaseWTDevice parameter;

            if (true) {

                parameter = new HBM.WT.API.WTX.WTXJet(s_Connection);

            } else {
                parameter = new HBM.WT.API.WTX.WTXJet(s_Connection);
               
            }

            
            /*
            Console.Write("Read Measure... ");
            int value = parameter.MeasureValue;
            Console.WriteLine(value);
            //int statusValue = parameter.MeasureValueType;

            Console.Write("Write DPT... ");
            parameter.DecimalPonit = 4;
            Console.WriteLine("OK");

            Console.WriteLine("Read Parameter success");
            */

            return 0;

        }

    }
}
