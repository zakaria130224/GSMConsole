using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSMTEST
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort port = new SerialPort();

            port.BaudRate = 921600;
            port.PortName = "COM7";
            port.Parity = Parity.None;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.ReadTimeout = 3000;
            port.WriteTimeout = 3000;
            //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            port.Open();
            string cmd = "AT+CUSD=1,\"*152#\"" + ",15\r";
            ATCommand atc = new ATCommand();
            string res = atc.ExecCommand( port, cmd, 10000,"jsjs");
            Console.WriteLine(res);
            Console.ReadKey();
        }
    }
    internal class ATCommand
    {
        private const string comPort = "COM7";
        private const int baudRate = 115200;
        private const int dataBits = 8;
        private const int parityBit = 1;
        private const int readTimeOut = 3000;
        private const int writeTimeOut = 3000;
        public AutoResetEvent receiveNow= new AutoResetEvent(true);

        public SerialPort OpenPort(
          string p_strPortName,
          int p_uBaudRate,
          int p_uDataBits,
          int p_uReadTimeout,
          int p_uWriteTimeout)
        {
            this.receiveNow = new AutoResetEvent(false);
            SerialPort serialPort = new SerialPort();
            try
            {
                serialPort.PortName = p_strPortName;
                serialPort.BaudRate = p_uBaudRate;
                serialPort.DataBits = p_uDataBits;
                serialPort.StopBits = StopBits.One;
                serialPort.Parity = Parity.None;
                serialPort.ReadTimeout = p_uReadTimeout;
                serialPort.WriteTimeout = p_uWriteTimeout;
                serialPort.Encoding = Encoding.GetEncoding("UTF-8");
                serialPort.DataReceived += new SerialDataReceivedEventHandler(this.port_DataReceived);
                serialPort.Open();
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return serialPort;
        }

        public void closePort(SerialPort port)
        {
            port.Close();
        }

        public string ExecuteCommandSMS(SerialPort port, string command, int timeout)
        {
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            this.receiveNow.Reset();
            port.Write(command + "\r");
            return this.ReadResponse(port, timeout);
        }

        public string portStatus(SerialPort port)
        {
            return this.ExecuteCommandSMS(port, "AT", 3000);
        }

        public bool sendMsg(SerialPort port, string PhoneNo, string Message)
        {
            bool flag1 = false;
            bool flag2;
            try
            {
                string str1 = this.ExecCommand(port, "AT", 300, "No phone connected");
                str1 = this.ExecCommand(port, "AT+CMGF=1", 300, "Failed to set message format.");
                str1 = this.ExecCommand(port, "AT+CSCS=\"IRA\"", 300, "Failed to set unicode format.");
                str1 = this.ExecCommand(port, "AT+CSMP=17,167,0,25", 300, "Failed to set csmp.");
                string command1 = "AT+CMGS=\"" + PhoneNo + "\"";
                str1 = this.ExecCommand(port, command1, 30000, "Failed to accept phoneNo");
                string command2 = Message + char.ConvertFromUtf32(26) + "\r";
                string str2 = this.ExecCommand(port, command2, 10000, "Failed to send message");
                if (str2.EndsWith("\r\nOK\r\n"))
                    flag1 = true;
                else if (str2.Contains("ERROR"))
                    flag1 = false;
                flag2 = flag1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flag2;
        }

        public string ExecuteCommand(SerialPort port, string command, int timeout)
        {
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            port.Write(command + "\r");
            Thread.Sleep(3000);
            byte[] numArray = new byte[port.BytesToRead];
            port.Read(numArray, 0, numArray.Length);
            return Encoding.ASCII.GetString(numArray);
        }

        public string ExecCommand(
          SerialPort port,
          string command,
          int responseTimeout,
          string errorMessage)
        {
            string str1;
            try
            {
                port.DiscardOutBuffer();
                port.DiscardInBuffer();
                this.receiveNow.Reset();
                port.Write(command + "\r");
                string str2 = this.ReadResponse(port, responseTimeout);
                if (str2.Length == 0 || !str2.EndsWith("\r\n> ") && !str2.EndsWith("\r\nOK\r\n"))
                    throw new ApplicationException("No success message was received.");
                str1 = str2;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return str1;
        }

        public string ReadResponse(SerialPort port, int timeout)
        {
            string empty = string.Empty;
            try
            {
                while (this.receiveNow.WaitOne(timeout, false))
                {
                    string str = port.ReadExisting();
                    empty += str;
                    if (empty.EndsWith("\r\nOK\r\n") || empty.EndsWith("\r\n> ") || empty.EndsWith("\r\nERROR\r\n"))
                        return empty;
                }
                if (empty.Length > 0)
                    throw new ApplicationException("Response received is incomplete.");
                throw new ApplicationException("No data received from phone.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType != SerialData.Chars)
                    return;
                this.receiveNow.Set();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
