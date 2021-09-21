using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

namespace Manager
{
    class Program
    {
        const int BASE_PORTS = 2070;
        public static TcpListener Serveur;
        public static string chemin_site = "C:\\Users\\Ahmed\\Desktop\\SYSR\\TP\\Bin\\site.exe";

        static void Main(string[] args)
        {
            Serveur = new TcpListener(BASE_PORTS + Int32.Parse(args[0]));
            Serveur.Start();

            while (true)
            {
                try
                {
                    Socket socket = Serveur.AcceptSocket();
                    try
                    {
                        Stream stream = new NetworkStream(socket);
                        StreamReader sr = new StreamReader(stream);
                        string cmd = sr.ReadLine();

                        Console.WriteLine(cmd);
                        ProcessStartInfo processInfo = new ProcessStartInfo(chemin_site, cmd);
                        Process myProcess = Process.Start(processInfo);

                        stream.Close();
                    }

                    catch (Exception e)
                    {

                    }
                    socket.Close();
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }
    }
}
