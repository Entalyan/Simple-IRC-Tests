using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_IRC_Test
{
    class Program
    {
        static int port;
        static string buf, nick, owner, server, chan, pass;
        static System.Net.Sockets.TcpClient sock = new System.Net.Sockets.TcpClient();
        static System.IO.TextReader input;
        static System.IO.TextWriter output;

        public static void Main(string[] args)
        {
            nick = "SecureIRC";
            owner = "SecureIRC";
            server = "irc.entalyan.com";
            port = 6999;
            chan = "#SecureIRC";
            pass = ""; //Enter just the password

            //Connect to irc server and get input and output text streams from TcpClient.
            sock.Connect(server, port);
            if (!sock.Connected)
            {
                Console.WriteLine("Failed to connect!");
                return;
            }
            input = new System.IO.StreamReader(sock.GetStream());
            output = new System.IO.StreamWriter(sock.GetStream());

            //Starting USER and NICK login commands 
            output.Write(
                "PASS " + nick + ":" + pass + "\r\n" +
                "USER " + nick + " 0 * :" + owner + "\r\n" +
                "NICK " + nick + "\r\n" +
                "PRIVMSG #SecureIRC Successful login at: " + DateTime.Now.ToString() + "\r\n"
            );
            output.Flush();

            Listen();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

        }

        private async static void Listen()
        {
            await ReadAsync();
        }


        private async static Task ReadAsync()
        {
            int linesRead = 0;

            do
            {
                var buffer = new char[4096];
                try
                {
                    linesRead = await input.ReadBlockAsync(buffer, 0, 4096);

                    //Display received irc message
                    Console.WriteLine(buf);

                    //Send pong reply to any ping messages
                    if (buf.StartsWith("PING ")) { output.Write(buf.Replace("PING", "PONG") + "\r\n"); output.Flush(); }
                    if (buf[0] != ':') continue;

                    /* IRC commands come in one of these formats:
                     * :NICK!USER@HOST COMMAND ARGS ... :DATA\r\n
                     * :SERVER COMAND ARGS ... :DATA\r\n
                     */

                    //After server sends 001 command, we can set mode to bot and join a channel
                    if (buf.Split(' ')[1] == "001")
                    {
                        await output.WriteAsync(
                            "MODE " + nick + " +B\r\n" +
                            "JOIN " + chan + "\r\n"
                        );
                        await output.FlushAsync();

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.InnerException.Message);
                }
            } while (linesRead != 0);
        }
    }
}
