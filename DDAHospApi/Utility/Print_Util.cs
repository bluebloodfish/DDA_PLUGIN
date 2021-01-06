using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DDAApi.Utility
{
    public class Print_Util
    {
        public static async Task PrintDocket(string printerServer, int port, string orderNo, int docketType)
        {
            //dockettype: 0 joblist, 1 bill, 3 reprint-joblist
            await SendMessage(printerServer, port, $"{orderNo},{docketType}");
            
        }

        private static async Task SendMessage(string ip, int port, string message)
        {
            using (var client = new TcpClient())
            {
                // Asynchronsly attempt to connect to server
                await client.ConnectAsync(ip, port);
                using (var netstream = client.GetStream())
                {
                    // Optionally set a timeout
                    netstream.ReadTimeout = 5000;

                    using (var writer = new StreamWriter(netstream))
                    {
                        // AutoFlush the StreamWriter
                        // so we don't go over the buffer
                        writer.AutoFlush = true;
                        // Write a message over the TCP Connection
                        await writer.WriteLineAsync(message);
                    }


                }
            }


        }
    }
}
