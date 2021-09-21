using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Detecteur
{
    //Implémentation de la classe du site détecteur de panne
    public class Detecteur
    {
        const int BASE_PORTS = 2050;    //La base des ports
        public static string[] sites;   //Les adresses IP des processus

        public static Object Verrou_Traitements;    //Un verrou pour les traitements
        public static Object[] Verrou_Envoyer;      //Un verrou pour l'envoi

        public static int NB_SITES;     //Le nombre des sites
        public static int num;          //Le numéro du site détecteur (=NB_SITE)
        public static bool[] actifs;    //La liste des sites actifs

        public static TcpListener Serveur;  //Le serveur TCP
        public static Thread[] RecevoirThread;  //Les threads de réception

        //Le programme principal
        static void Main(string[] args)
        {
            //Initialisations
            NB_SITES = Int32.Parse(args[0]);
            num = NB_SITES;

            sites = new string[NB_SITES];
            for (int i = 0; i < NB_SITES; i++)
                sites[i] = args[10 + i];

            actifs = new bool[NB_SITES];
            for (int j = 0; j < NB_SITES; j++)
                actifs[j] = true;

            Verrou_Traitements = new Object();
            Verrou_Envoyer = new Object[NB_SITES];
            for (int i = 0; i < NB_SITES; i++)
                Verrou_Envoyer[i] = new Object();
            
            Serveur = new TcpListener(BASE_PORTS + num);
            Serveur.Start();

            RecevoirThread = new Thread[NB_SITES];
            for (int j = 0; j < NB_SITES; j++)
            {
                RecevoirThread[j] = new Thread(new ThreadStart(Recevoir));
                RecevoirThread[j].Start();
            }

            //La boucle principale
            while (true)
            {
                Diffuser(new Message(5, 0, num));
                Thread.Sleep(100);
            }
        }

        //La méthode d'envoi de message
        static void Envoyer(Message m, int Pj)
        {
            lock (Verrou_Envoyer[Pj])
            {
                if (actifs[Pj])
                    try
                    {
                        TcpClient client = new TcpClient(sites[Pj], BASE_PORTS + Pj);
                        Stream stream = client.GetStream();
                        StreamWriter sw = new StreamWriter(stream);
                        sw.AutoFlush = true;
                        sw.WriteLine(m.ToString());
                        stream.Close();
                        client.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Detection de panne au niveau du site P" + (Pj + 1).ToString());
                        actifs[Pj] = false;
                        Diffuser(new Message(3, 0, Pj));
                    }
            }
        }

        //La méthode de diffusion
        static void Diffuser(Message m)
        {
            for (int j = 0; j < NB_SITES; j++)
            {
                if (m.Site!=j)
                    Envoyer(m, j);
            }
        }

        //La méthode de réception
        static void Recevoir()
        {
            while (true)
            {
                try
                {
                    Socket socket = Serveur.AcceptSocket();
                    try
                    {
                        Stream stream = new NetworkStream(socket);
                        StreamReader sr = new StreamReader(stream);
                        Message m = new Message(sr.ReadLine());

                        switch (m.Type)
                        {
                            //A la réception d'un message <Rentrée>
                            case 4:
                                actifs[m.Site] = true;
                                Diffuser(m);

                                lock (Verrou_Traitements)
                                {
                                    for (int i = 0; i < NB_SITES; i++)
                                        if (!actifs[i]) Envoyer(new Message(3, 0, i), m.Site);
                                }

                                Console.WriteLine("Hello");
                                break;
                        }
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
