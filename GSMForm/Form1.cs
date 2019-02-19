using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace GSMForm
{
    public partial class Form1 : Form
    {
        private SerialPort port = new SerialPort();
        public Form1()
        {
            InitializeComponent();
            port = new SerialPort();
            port.BaudRate = 921600;
            port.PortName = "COM7";
            port.Parity = Parity.None;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.ReadTimeout = 3000;
            port.WriteTimeout = 3000;

            //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.Open();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string cmd = "AT+CUSD=1,\"*152#\"" + ",15\r";
                ////_port.Write("AT+CUSD=1," + textBox1.Text + ",15");
                //port.Write("AT+CUSD=1,\"" + "*778#" + "\",15" + "\r");
                //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
               string result= ExecCommand(port, cmd, 300, "Failed to read the messages.");
                this.Invoke(new Action(() => textBox2.Text = (result.ToString())));
                //MessageBox.Show(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }
        //private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    try
        //    {
        //        // read the response.
        //        var response = ((SerialPort)sender).ReadLine();

        //        // Need to update the txtMessage on the UI thread.
        //        this.Invoke(new Action(() => textBox2.Text = (response.ToString())));
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
        public static String UnicodeStr2HexStr(String strMessage)
        {
            byte[] ba = Encoding.BigEndianUnicode.GetBytes(strMessage);
            String strHex = BitConverter.ToString(ba);
            strHex = strHex.Replace("-", "");
            return strHex;
        }

        public static String HexStr2UnicodeStr(String strHex)
        {
            byte[] ba = HexStr2HexBytes(strHex);
            return HexBytes2UnicodeStr(ba);
        }

        //================> Used to decoding GSM UCS2 message  
        public static String HexBytes2UnicodeStr(byte[] ba)
        {
            var strMessage = Encoding.BigEndianUnicode.GetString(ba, 0, ba.Length);
            return strMessage;
        }

        public static byte[] HexStr2HexBytes(String strHex)
        {
            strHex = strHex.Replace(" ", "");
            int nNumberChars = strHex.Length / 2;
            byte[] aBytes = new byte[nNumberChars];
            using (var sr = new StringReader(strHex))
            {
                for (int i = 0; i < nNumberChars; i++)
                    aBytes[i] = Convert.ToByte(new String(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return aBytes;
        }
        //Execute AT Command
        public string ExecCommand(
            SerialPort port,
            string command,
            int responseTimeout,
            string errorMessage
            )
        {
            try
            {
                port.DiscardOutBuffer();
                port.DiscardInBuffer();
                receiveNow.Reset();
                port.Write(command);

                string input = null;
                input = ReadResponse(port, responseTimeout);
                if ((input.Length == 0) || ((!input.EndsWith("\r\n> ")) && (!input.EndsWith("\r\nOK\r\n"))))
                    throw new ApplicationException("No success message was received.");

                return input;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string responseUSSD = "";
        public string ReadResponse(SerialPort port, int timeout)
        {
            string buffer = string.Empty;

            try
            {
                do
                {
                    if (receiveNow.WaitOne(timeout, true))
                    {
                        string t = port.ReadExisting();
                        buffer += t;
                        //Debug.WriteLine(t);
                        responseUSSD += t;
                    }
                    else
                    {
                        if (buffer.Length > 0)
                            //throw new ApplicationException("Response received is incomplete.");
                            return buffer;
                        else
                            return buffer;
                        //throw new ApplicationException("No data received from phone.");
                    }
                }
                while (!buffer.EndsWith("\r\nOK\r\n") && !buffer.EndsWith("\r\n> ") && !buffer.EndsWith("\r\nERROR\r\n"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return buffer;
        }
        public AutoResetEvent receiveNow= new AutoResetEvent(false);
    }
}
