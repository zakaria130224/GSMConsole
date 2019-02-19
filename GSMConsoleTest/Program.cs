using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;

namespace GSMConsoleTest
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
            port.DataReceived +=  new SerialDataReceivedEventHandler(port_DataReceived);

            port.Open();
            string cmd = Console.ReadLine();
            //port.Write("AT+CUSD=1,\"*152#\"" + ",15\r");
            port.Write("AT+CUSD=1," + cmd + ",15\r");

            void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                SerialPort spL = (SerialPort)sender;
                byte[] buf = new byte[spL.BytesToRead];
                spL.Read(buf, 0, buf.Length);
                string message = "";
                foreach (Byte b in buf)
                {
                    message += b.ToString();
                }

                string res = string.Empty;
                var result = Encoding.ASCII.GetString(buf);//just return OK
                //Console.WriteLine(result);
                if (result.First()=='\r')
                {
                    res = result;
                    res = res.Replace("\r\n+CUSD: 0,\"", "");
                    res = res.Replace("00", "");
                    res = res.Replace("\"", "");
                    res = res.Replace(",72\r\n", "");
                    var rtfBytes = FromHex(res);
                    var rtfText = Encoding.ASCII.GetString(rtfBytes);
                    Console.WriteLine(rtfText);
                }
                //var str = System.Text.Encoding.Default.GetString(buf);

                
                //Console.WriteLine(message);
            }

           // Console.WriteLine("Hello World!");
            Console.ReadKey();

        }
        public static byte[] FromHex(string hex)
        {
            // hex = @"0059006F007500720020006100630063006F0075006E0074002000620061006C0061006E0063006500200069007300200054004B002E002000320039002E00300039002E0059006F007500720020006100630063006F0075006E0074002000770069006C006C00200065007800700069007200650020006F006E002000310035002F00310031002F0032003000310039002C005400680061006E006B007300200046006F00720020005500730069006E0067002000540065006C006500740061006C006B002E";
            try
            {
                var result = new byte[hex.Length / 2];
                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return result;
            }
            catch (Exception ex)
            {

                return Encoding.ASCII.GetBytes("Unknown Request!!!"); ;
            }
            
        }


    }
}
