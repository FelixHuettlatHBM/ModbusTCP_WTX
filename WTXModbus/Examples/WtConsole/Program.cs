
using HBM.WT.API;
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
        private static BaseWtDevice _wtxObj;

        private static String _ipAddr;
        private static int _timerInterval;
        private static bool _compareTest;
        private static string[] _previousDataStrArr;

        private static ConsoleKeyInfo _valueExitapplication;

        private static string _previousNetValue;
        //private static string[] data_str_arr;

        static string _menuRequest = "folow instructions: \n \r"
            + "<read> <parameter> \n"
            + "<write> <parameter> <value> \n"
            + "<test> \n"
            + "<show> <filter> \n"
            + "<quit> \n";

        delegate int SelectFunktion(string[] args);
        static Dictionary<string, SelectFunktion> _funktonSelect = new Dictionary<string, SelectFunktion> {
            { "read", ReadParameter },
            { "write", WriteParameter },
            { "show" , ShowProperties },
            { "test" , TestDeviceLayer },
        };

        static JetBusConnection _jetConnection;
        static ModbusTcpConnection _modbusConnection;

        static void Main(string[] args) {

            _previousNetValue = "";
            _timerInterval = 500;
            _compareTest = true;

            Thread thread1 = new Thread(new ThreadStart(InputOutput));

            try {

                switch (args[0]) {
                    /*
                    case "-can":
                        ICANCommon hw = new CANPeak(new object[] { 0x51 });
                        CANDaemon can = CANDaemonManager.CreateInstance(hw);

                        CANBaudrate baudrate = CANBaudrate.B0125;

                        switch (args[1]) 
                        {
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

                        _ipAddr = "wss://" + args[1];
                        Console.Write("Initialize Jet-Peer to address " + _ipAddr + "...");

                        _jetConnection = new JetBusConnection(_ipAddr, "Administrator", "wtx", delegate { return true; });

                        Console.WriteLine("OK");
 
                        //s_Connection.BusActivityDetection += S_Connection_BusActivityDetection;

                        Console.WriteLine("Parameter are fetching: ");
                        Console.Write((_jetConnection as JetBusConnection).BufferToString());
                        
                        _wtxObj = new HBM.WT.API.WTX.WtxJet(_jetConnection);

                        Console.WriteLine("Parameter fetched");
                        Console.WriteLine("Net value : "   + _wtxObj.NetValue);
                        Console.WriteLine("Gross value : " + _wtxObj.GrossValue);                    
                        Console.WriteLine("Decimals : "    + _wtxObj.Decimals);

                        //Console.WriteLine("Weight moving : " + WTXObj.weight_moving);

                        //Console.WriteLine("dosingCounter : " +  WTXObj.dosing_count);
                        //Console.WriteLine("dosingStatus  : "  + WTXObj.dosing_process_status);
                        //Console.WriteLine("dosingResult  : "  + WTXObj.dosing_result);

                        Console.ReadLine();

                        break;

                    case "-modbus":

                        _ipAddr = args[1];
                        
                        _modbusConnection = new ModbusTcpConnection(_ipAddr);


                        _wtxObj = new HBM.WT.API.WTX.WtxModbus(_modbusConnection, _timerInterval);

                        // Konstruktor neu : Obj. von ModbusTCPConnection, Timer Intervall

                        _previousDataStrArr = new string[59];

                        for (int i = 0; i < 59; i++)
                            _previousDataStrArr[i] = "0";

                        // Start asynchronous data transfer : Method - Nur bei einer Änderung Werte ausgeben - Was abrufbar ist aus der Klasse Program zu WTX120_Modbus

                        _wtxObj.Connect();
                        
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

                        
                        _wtxObj.DataUpdateEvent += AsyncUpdateData;

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
                            Console.Write(_menuRequest);
                            Console.Write("/> ");
                            input = Console.ReadLine().Split(new char[] { ' ' });

                            if (_funktonSelect.ContainsKey(input[0].ToLower()))
                            {

                                _funktonSelect[input[0].ToLower()](input);
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
                _valueExitapplication = Console.ReadKey();
        }

        private static void S_Connection_BusActivityDetection(object sender, EventArgs e)
        {
            Console.WriteLine((e as DataEvent).Args.ToString());
        }


        // This method prints the values of the device (as a integer and the interpreted string) as well as the description of each bit. 
        private static void AsyncUpdateData(object arg1, DataEvent arg2)
        {
            
            _compareTest = true;
            if (_previousNetValue != _wtxObj.GetDataStr[0])                   
                _compareTest = false;
            else
                _compareTest = true;
        
            if ( (_compareTest == false) && ( _wtxObj.DeviceValues!=null ))
            //if(! (previous_data_str_arr.Equals(WTXObj.getDataStr) ))
            //if(ParamValues != null)
            {

                Console.Clear();
            
                Console.WriteLine("Net value:                     " + _wtxObj.GetDataStr[0] +   "\t  As an Integer:  " + _wtxObj.DeviceValues.NetValue);        
                Console.WriteLine("Gross value:                   " + _wtxObj.GetDataStr[1] +   "\t  As an Integer:  " + _wtxObj.DeviceValues.GrossValue);    
                Console.WriteLine("General weight error:          " + _wtxObj.GetDataStr[2] +   "\t  As an Integer:  " + _wtxObj.DeviceValues.GeneralWeightError);    
                Console.WriteLine("Scale alarm triggered:         " + _wtxObj.GetDataStr[3] +   "\t  As an Integer:  " + _wtxObj.DeviceValues.LimitStatus);    
                Console.WriteLine("Scale seal is open:            " + _wtxObj.GetDataStr[6]   + "\t  As an Integer:  " + _wtxObj.DeviceValues.ScaleSealIsOpen);
                Console.WriteLine("Manual tare:                   " + _wtxObj.GetDataStr[7]   + "\t  As an Integer:  " + _wtxObj.DeviceValues.ManualTare);
                Console.WriteLine("Weight type:                   " + _wtxObj.GetDataStr[8]   + "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightType);
                Console.WriteLine("Scale range:                   " + _wtxObj.GetDataStr[9]   + "\t  As an Integer:  " + _wtxObj.DeviceValues.ScaleRange);
                Console.WriteLine("Zero required/True zero:       " + _wtxObj.GetDataStr[10]  + "\t  As an Integer:  " + _wtxObj.DeviceValues.ZeroRequired);
                Console.WriteLine("Weight within center of zero:  " + _wtxObj.GetDataStr[11]  + "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightWithinTheCenterOfZero);
                Console.WriteLine("Weight in zero range:          " + _wtxObj.GetDataStr[12]  + "\t  As an Integer:  " + _wtxObj.DeviceValues.WeightWithinTheCenterOfZero);
                Console.WriteLine("Application mode:              " + _wtxObj.GetDataStr[13]  + "\t  As an Integer:  " + _wtxObj.DeviceValues.ApplicationMode);
                Console.WriteLine("Decimal places:                " + _wtxObj.GetDataStr[14]  + "\t  As an Integer:  " + _wtxObj.DeviceValues.Decimals);
                Console.WriteLine("Unit:                          " + _wtxObj.GetDataStr[15]  + "\t  As an Integer:  " + _wtxObj.DeviceValues.Unit);
                Console.WriteLine("Handshake:                     " + _wtxObj.GetDataStr[16]  + "\t  As an Integer:  " + _wtxObj.DeviceValues.Handshake);
                Console.WriteLine("Status:                        " + _wtxObj.GetDataStr[17]  + "\t  As an Integer:  " + _wtxObj.DeviceValues.Status);
                Console.WriteLine("Limit status:                  " + _wtxObj.GetDataStr[4]  + "  As an Integer:  "    + _wtxObj.DeviceValues.LimitStatus);
                Console.WriteLine("Weight moving:                 " + _wtxObj.GetDataStr[5]  + "  As an Integer:"      + _wtxObj.DeviceValues.WeightMoving);

                Console.WriteLine("\n Press any key to exit");

                
            }
            _previousDataStrArr = _wtxObj.GetDataStr;
            _previousNetValue = _wtxObj.GetDataStr[0];            
        }

            private static int ReadParameter(string[] args) {
            Console.Write(args[0] + "Read... ");
            if (args.Length < 2) return -1;
            int intValue = _jetConnection.Read<int>(args[1]);
            
            Console.WriteLine(intValue);
            return 0;
        }

        private static int WriteParameter(string[] args) {
            Console.Write(args[0] + "Write... ");
            if (args.Length < 3) return -1;

            int value = Convert.ToInt32(args[2]);
            _jetConnection.Write<int>(args[1], value);
            Console.WriteLine("OK");

            return 0;
        }

        private static int ShowProperties (string[] args) {

            BaseWtDevice parameter = new HBM.WT.API.WTX.WtxJet(_jetConnection);

            Type type = parameter.GetType();

            PropertyInfo[] properties = type.GetProperties();
            foreach(PropertyInfo prop in properties) {
                Console.WriteLine(prop.ToString());
                                
            }

            return 0;
           
        }

        private static uint StringToId(string arg) {
            if (arg.Contains("0x")) {
                return Convert.ToUInt32(arg, 16);
            } else {
                return Convert.ToUInt32(arg, 10);
            }
        }

        private static int TestDeviceLayer(string[] arg) {
            //Hbm.Wt.WTXInterface.WTX120_Jet.ParameterProperty parameter = 
            //    new Hbm.Wt.WTXInterface.WTX120_Jet.ParameterProperty(s_Connection);

            BaseWtDevice parameter;

            if (true)
            {

                parameter = new HBM.WT.API.WTX.WtxJet(_jetConnection);

            } else
            {
                //parameter = new HBM.WT.API.WTX.WTXJet(s_Connection);
               
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
