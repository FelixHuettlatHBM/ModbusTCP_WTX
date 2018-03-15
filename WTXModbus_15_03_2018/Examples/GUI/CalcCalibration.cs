/* @@@@ HOTTINGER BALDWIN MESSTECHNIK - DARMSTADT @@@@@
 * 
 * TCP/MODBUS Interface for WTX120 | 02/2018
 * 
 * Author : Felix Retsch 
 * 
 *  */

using Hbm.Devices.WTXModbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WTXModbusGUIsimple
{
    // This class provides a window to perform a calibration without a calibration weight,
    // based on know values for dead load and nominal load in mV/V

    public partial class CalcCalibration : Form
    {
        private WTX120 WTXObj;
        private bool Finished;
        private double Preload;
        private double Capacity;
        private IFormatProvider Provider;
        private const double MultiplierMv2D = 500000; //   2 / 1000000; // 2mV/V correspond 1 million digits (d)
        private string str_comma_dot;

        private int handshake_compare;      // neu: 8.3.2018
        private int status_compare;

        private System.Windows.Forms.Timer myTimer2;      // Neu : 8.3.2018 - Idee : Timer für zyklische Abfrage der Werte nutzen. 

        public CalcCalibration(WTX120 WTXObj, bool connected)
        {
            myTimer2 = new System.Windows.Forms.Timer();
            myTimer2.Tick += new EventHandler(timerWeightCalibrationTick);

            myTimer2.Enabled = true;   // Neu : 9.3.2018
            myTimer2.Interval = 1;     // Neu : 9.3.2018

            this.handshake_compare = 0;
            this.WTXObj = WTXObj;
            Finished = false;
            //Provider for english number format
            Provider = CultureInfo.InvariantCulture;

            str_comma_dot = "";

            InitializeComponent();

            if (!connected)
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                buttonCalculate.Text = "Close";
                Finished = true;
                label5.Visible = true;
                label5.Text = "No WTX connected!";
            }
        }

        // Checks the input values of the textboxes and start the calibration calculation.
        // If the caluclation is finished, the window can be closed with the button.
        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            if (!Finished)
            {
                label5.Visible = true;
                bool abort = false;
                try
                {
                    //Preload = Convert.ToDouble(textBox1.Text, Provider);
                    str_comma_dot = textBox1.Text.Replace(".", ",");                   // Kommas durch Punkte ersetzen. 
                    Preload = double.Parse(str_comma_dot);
                    textBox1.Enabled = false;
                }
                catch (FormatException)
                {
                    label5.Text = "wrong number format";
                    abort = true;
                }
                catch (OverflowException)
                {
                    label5.Text = "Overflow! Number to big.";
                    abort = true;
                }

                try
                {
                    //Capacity = Convert.ToDouble(textBox2.Text, Provider);

                    str_comma_dot = textBox2.Text.Replace(".", ",");                   // Kommas durch Punkte ersetzen. 
                    Capacity = double.Parse(str_comma_dot);
                    textBox2.Enabled = false;
                }
                catch (FormatException)
                {
                    label5.Text = "wrong number format";
                    abort = true;
                }
                catch (OverflowException)
                {
                    label5.Text = "Overflow! Number to big.";
                    abort = true;
                }
                if (abort) return;

                Calculate();

                label5.Text = "Calibration Successful!";
                Finished = true;
                buttonCalculate.Text = "Close";
            }
            else
            {

                Close();
            }
        }

        // Limits the input of the textbox 1/2 to digits, ',' and '.'
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == ',')
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        // Calculates the values for deadload and nominal load in d from the inputs in mV/V
        // and writes the into the WTX registers.
        private void Calculate()
        {
            double DPreload = Preload * MultiplierMv2D;
            double DNominalLoad = DPreload + (Capacity * MultiplierMv2D);

            //write reg 48, DPreload;

            WTXObj.write_Zero__Calibration_Nominal_Load('z', Convert.ToInt32(DPreload), WriteDataReceived);

            WTXObj.SyncCall_Write_Command(0, 0x80, WriteDataReceived);

            //write reg 50, DNominalLoad;

            WTXObj.write_Zero__Calibration_Nominal_Load('n', Convert.ToInt32(DNominalLoad), WriteDataReceived);

            WTXObj.SyncCall_Write_Command(0, 0x100, WriteDataReceived);
        }

        // New(8.3.2018) : This is the method to run when the timer is raised.
        private void timerWeightCalibrationTick(Object myObject, EventArgs myEventArgs)
        {
            WTXObj.Async_Call(0x00, ReadDataReceived);
        }


        private void ReadDataReceived(IDeviceValues obj)
        {
            this.handshake_compare = obj.handshake;
            this.status_compare = obj.status;

        }

        private void WriteDataReceived(IDeviceValues obj)
        {
            this.handshake_compare = obj.handshake;
            this.status_compare = obj.status;
        }

        private void CalcCalibration_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}

/*
        private void Calculate()
        {
            double DPreload = Preload * MultiplierMv2D;
            double DNominalLoad = DPreload + (Capacity * MultiplierMv2D);
            
            //todo: write reg 48, DPreload;

            WTXObj.SyncCallWriteMultipleRegister('z', Convert.ToInt32(DPreload), WriteDataReceived);

            while (handshake_compare == 1)
                WTXObj.SyncCall(0, 0x00, ReadDataReceived);
            
            WTXObj.SyncCall(0, 0x80, WriteDataReceived);

            while (handshake_compare == 1)
                WTXObj.SyncCall(0, 0x00, ReadDataReceived);

            //todo: write reg 50, DNominalLoad;

            WTXObj.SyncCallWriteMultipleRegister('n', Convert.ToInt32(DNominalLoad), WriteDataReceived);

            while (handshake_compare == 1)
                WTXObj.SyncCall(0, 0x00, ReadDataReceived);

            WTXObj.SyncCall(0, 0x100, WriteDataReceived);

            // Timing Problem - Übertragungsprotokoll beachten, siehe bitte dazu Manual  
        }
    */




// Alternativ : 8.3.2018

/*
 * 
 *         private void Calculate()
    {
        double DPreload = Preload * MultiplierMv2D;
        double DNominalLoad = DPreload + (Capacity * MultiplierMv2D);

        ushort[] data2Write = new ushort[2];
        int IPreload = Convert.ToInt32(DPreload);
        int INominalLoad = Convert.ToInt32(DNominalLoad);

        data2Write[0] = (ushort)((IPreload & 0xffff0000) >> 16);
        data2Write[1] = (ushort) (IPreload & 0x0000ffff);

        handshake_compare = 0;

        //todo: write reg 48, DPreload;

        for (int index = 0; index < data2Write.Length; index++)
        {
            WTXObj.SyncCall( (ushort) (46+index), data2Write[index], WriteDataReceived);

            while (handshake_compare == 0)
                WTXObj.Async_Call(0, ReadDataReceived);
            if (handshake_compare == 1)
                WTXObj.SyncCall((ushort)(48 + index), 0, WriteDataReceived);        // Zurücksetzen. 
            while (handshake_compare == 1)
                WTXObj.Async_Call(0, ReadDataReceived);
            while (status_compare == 0)
                WTXObj.Async_Call(0, ReadDataReceived);
        }

        WTXObj.SyncCall(0, 0x80, WriteDataReceived);

        while (handshake_compare == 0)
            WTXObj.Async_Call(0, ReadDataReceived);
        if (handshake_compare == 1)
            WTXObj.SyncCall(0, 0, WriteDataReceived);       // Zurücksetzen 
        while (handshake_compare == 1)
            WTXObj.Async_Call(0, ReadDataReceived);
        while (status_compare == 0)
            WTXObj.Async_Call(0, ReadDataReceived);

        //todo: write reg 50, DNominalLoad;

        data2Write[0] = (ushort)((INominalLoad & 0xffff0000) >> 16);
        data2Write[1] = (ushort) (INominalLoad & 0x0000ffff);

        for (int index = 0; index < data2Write.Length; index++)
        {
            WTXObj.SyncCall((ushort)(50 + index), data2Write[index], WriteDataReceived);

            while (handshake_compare == 0)
                WTXObj.Async_Call(0, ReadDataReceived);
            if (handshake_compare == 1)
                WTXObj.SyncCall((ushort)(46 + index), 0, WriteDataReceived);        // Zurücksetzen. 
            while (handshake_compare == 1)
                WTXObj.Async_Call(0, ReadDataReceived);
            while (status_compare == 0)
                WTXObj.Async_Call(0, ReadDataReceived);
        }

        WTXObj.SyncCall(0, 0x100, WriteDataReceived);

    }
*/


// Alternativ: 8.3.2018

/*
 * 
 *         private void Calculate()
    {
        double DPreload = Preload * MultiplierMv2D;
        double DNominalLoad = DPreload + (Capacity * MultiplierMv2D);

        ushort[] data2Write = new ushort[2];

        int IPreload = Convert.ToInt32(DPreload);
        int INominalLoad = Convert.ToInt32(DNominalLoad);

        data2Write[0] = (ushort)((IPreload & 0xffff0000) >> 16);
        data2Write[1] = (ushort)(IPreload & 0x0000ffff);

        this.handshake_compare = 0;
        this.status_compare = 0;

        //todo: write reg 48 und 49, DPreload;

        for (int index = 0; index < data2Write.Length; index++)
        {
            WTXObj.SyncCall((ushort)(48 + index), data2Write[index], WriteDataReceived);

            while (handshake_compare == 1)
                WTXObj.Async_Call(0x00, ReadDataReceived);

        }

        WTXObj.Async_Call(0x80, WriteDataReceived);

        while (handshake_compare == 1)
            WTXObj.Async_Call(0x00, ReadDataReceived);


        //todo: write reg 50, DNominalLoad;

        data2Write[0] = (ushort)((INominalLoad & 0xffff0000) >> 16);
        data2Write[1] = (ushort)(INominalLoad & 0x0000ffff);

        for (int index = 0; index < data2Write.Length; index++)
        {

            WTXObj.SyncCall((ushort)(50 + index), data2Write[index], WriteDataReceived);

            while (handshake_compare == 1)
                WTXObj.Async_Call(0x00, ReadDataReceived);

        }

        WTXObj.Async_Call(0x100, WriteDataReceived);

        while (handshake_compare == 1 && status_compare==1) ;

        if (handshake_compare == 1 && status_compare == 1)
            Console.WriteLine("Datenaustausch erfolgreich");
    }


    */


// Alternative 12.3.2018: Asynchroner Aufruf zum Kalibrieren - Asynchronität hier sinnvoll ? 
/*
private void Calculate()
{
    double DPreload = Preload * MultiplierMv2D;
    double DNominalLoad = DPreload + (Capacity * MultiplierMv2D);

    myTimer2.Start();          // Neu : 9.3.2018

    handshake_compare = 0;

    //todo: write reg 48, DPreload;

    WTXObj.WriteMultipleRegister('z', Convert.ToInt32(DPreload), WriteDataReceived);
   
    WTXObj.Async_Call(0x80, WriteDataReceived);           // Alternative zum synchronen Aufruf. 

    //todo: write reg 50, DNominalLoad;

    WTXObj.WriteMultipleRegister('n', Convert.ToInt32(DNominalLoad), WriteDataReceived);

    WTXObj.Async_Call(0x100, WriteDataReceived);      // Alternative zum synchronen Aufruf. 

    while (this.handshake_compare == 1) ;
    myTimer2.Stop();

}
*/
