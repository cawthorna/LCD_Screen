using System;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;

using OpenHardwareMonitor.Hardware;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LCD_Screen_Server
{
    class Program
    {
        protected static SerialPort port;
        protected static Computer myComp;
        protected static int comPort = -1;
        private static Timer timer;

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            //Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            //do your cleanup here
            shutdownProgram();

            //Console.WriteLine("Cleanup complete");

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
        #endregion
  
        static void Main(string[] args)
        {

            // Some biolerplate to react to close window event, CTRL-C, kill, etc
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            //Init Port
            Boolean portOpen = openPort();
            while(!portOpen)
            {
                portOpen = openPort();
                Thread.Sleep(2000);
            }

            //Connected
            //Wake and clear the screen
            writePort('w');
            writePort('c');

            //Set Console Control Handler
            

            //Open the Hardware Monitor
            myComp = new Computer();
            myComp.MainboardEnabled = true;
            myComp.CPUEnabled = true;
            myComp.GPUEnabled = true;
            myComp.RAMEnabled = true;

            myComp.Open();

            //Create timer update callback
            timer = new Timer(timer_callback, null, 0, 500);

            //Turn Screen On
            //writePort('w');

            //Setup Infinite loop with blocking read to stall program until it is quit. 
            Boolean quit = false;
            while (!quit)
            {
                //timer.GetHashCode();
                ConsoleKey key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.F1:
                        quit = true;
                        writePort('c');
                        writePort('s');
                        port.Close();
                        timer.Dispose();
                        break;
                    case ConsoleKey.F2:
                        writePort('s');
                        break;
                    case ConsoleKey.F3:
                        writePort('w');
                        break;
                }
            }

        }

        static void timer_callback(object sender)
        {
            float cpup = 999;
            float cput = 999;
            float ram = 999;
            float gpup = 999;
            float gput = 999;
            float gpur = 999;

            foreach (var hardwareItem in myComp.Hardware)
            {
                hardwareItem.Update();
                hardwareItem.GetReport();

                foreach (var sensor in hardwareItem.Sensors)
                {
					//Console.WriteLine("{0} {1} = {2}", sensor.Name, sensor.SensorType, sensor.Value);
					if (sensor.SensorType == SensorType.Load)
                    {
                        switch (sensor.Name)
                        {
                            case "CPU Total":
                                cpup = sensor.Value ?? 999;
                                //Console.WriteLine("{0} {1} = {2}", sensor.Name, sensor.SensorType, sensor.Value);
                                break;
                            case "GPU Core":
                                gpup = sensor.Value ?? 999;
                                //Console.WriteLine("{0} {1} = {2}", sensor.Name, sensor.SensorType, sensor.Value);
                                break;
                            case "Memory":
                                ram = sensor.Value ?? 999;
                                //Console.WriteLine("{0} {1} = {2}", sensor.Name, sensor.SensorType, sensor.Value);
                                break;
                            case "GPU Memory":
                                gpur = sensor.Value ?? 999;
                                //Console.WriteLine("{0} {1} = {2}", sensor.Name, sensor.SensorType, sensor.Value);
                                break;
                        }
                    }
                    else if(sensor.SensorType == SensorType.Temperature)
                    {
						//Console.WriteLine("{0} {1} = {2}", sensor.Name, sensor.SensorType, sensor.Value);
						switch (sensor.Name)
                        {
                            case "CPU Package":
                                cput = sensor.Value ?? 999;
                                //Console.WriteLine("{0} {1} = {2}", sensor.Name, sensor.SensorType, sensor.Value);
                                break;
                            case "GPU Core":
                                gput = sensor.Value ?? 999;
                                //Console.WriteLine("{0} {1} = {2}", sensor.Name, sensor.SensorType, sensor.Value);
                                break;
                        }
                    }  
                }
            }

            updateDisplay(cpup, cput, ram, gpup, gput, gpur);
        }

        private static bool writePort(char letter)
        {
            if (port.IsOpen)
            {
                try
                {
                    port.Write(letter + "");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static void updateDisplay(float fcpup, float fcput, float fram, float fgpup, float fgput, float fgpur)
        {
            String cpup = formatValue(fcpup);
            String cput = formatValue(fcput);
            String ram =  formatValue(fram);
            String gpup = formatValue(fgpup);
            String gput = formatValue(fgput);
            String gpur = formatValue(fgpur);

            //Send (p)rint command plus data.
            if (port.IsOpen)
            {
                try
                {
                    port.Write("p" + cpup + cput + ram + gpup + gput + gpur);
                }
                catch// (Exception e)
                {
                    //Console.WriteLine("Could not write to port");
                    //Console.WriteLine(e.ToString());
                    comPort = -1;
                    openPort();
                }
            }
            else
            {
                comPort = -1;
                openPort();
            }          
        }

        private static String formatValue(float value)
        {
            String temp = Convert.ToString(Math.Round(value));
            while(temp.Length < 3)
            {
                temp = " " + temp;
            }
            return temp;
        }

        private static Boolean openPort()
        {
            if (comPort != -1)
            {
                port = new SerialPort("COM" + comPort, 9600);
            }
            else
            {
                String comPortName = SetupDiWrap.ComPortNameFromFriendlyNamePrefix("USB Serial");
                if (comPortName == null)
                {
                    return false;
                }
                comPort = Int32.Parse(comPortName[comPortName.Length - 1] + "");
                port = new SerialPort(comPortName, 9600);
            }
            //Console.WriteLine("COM Port: " + comPort);
            try
            {
                port.Open();
            }
            catch //(Exception e)
            {
                return false;
            }
            return true;
        }

        private static void shutdownProgram()
        {
            writePort('c');
            writePort('s');
            port.Close();
            timer.Dispose();
        }

    }
}
