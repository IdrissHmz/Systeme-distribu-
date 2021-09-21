using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ExclusionMutuelle
{
    //Implémentation de la classe Site
    public class Site
    {       
        public const int BASE_PORTS = 2050;     //La base des ports
        public static string[] sites;   //Les adresses IP des sites

        public static MainWindow GUI;   //La GUI
        public static TcpListener Serveur;  //Le serveur
        public static Random Generateur;    //Le générateur des nombres aléatoires
        public static Object Verrou_Traitements;    //Le verrous des traitements
        public static Object[] Verrou_Envoyer;  //Le verrous d'envoi

        public static Thread DemarrerThread;    //Le thread principal
        public static Thread[] RecevoirThread;  //Les threads de réception

        //Les paramètres de simulation
            public static int Nb_site = 2;
            public static int min_D_S_C = 5;   
            public static int max_D_S_C = 10;
            public static int min_A_H_S_C = 5;
            public static int max_A_H_S_C = 10;
            public static int min_T_A = 1;
            public static int max_T_A = 1;
            public static int min_D_P = 5;
            public static int max_D_P = 10;
            public static double Proba_P = 0.5;

        //Les paramèteres de l'algorithme de Lamport
            public static int num;
            public static int horloge;
            public static Message[] messages;
            public static bool[] actifs;
            public static bool[] valides;
            public static bool panne;

        public static void Demarrer()
        {
            //Initialisations
            horloge = 0;
            
            messages = new Message[Nb_site];
            for (int j = 0; j < Nb_site; j++)
                messages[j] = new Message(1, 0, j);   // mettre  release pour tout les sites
            
            actifs = new bool[Nb_site];
            for (int j = 0; j < Nb_site; j++)
                actifs[j] = true;

            valides = new bool[Nb_site];
            for (int j = 0; j < Nb_site; j++)
                valides[j] = false;

            panne = false;

            Serveur = new TcpListener(BASE_PORTS + num);
            Serveur.Start();

            DemarrerThread = Thread.CurrentThread;
            RecevoirThread = new Thread[Nb_site];
            for (int j = 0; j < Nb_site; j++)
            {
                RecevoirThread[j] = new Thread(new ThreadStart(Recevoir));
                RecevoirThread[j].Start();
            }

            Generateur = new Random();
            Verrou_Traitements = new Object();
            Verrou_Envoyer = new Object[Nb_site+1];
            for (int i=0; i < Nb_site+1; i++)
                Verrou_Envoyer[i] = new Object();

            Thread.Sleep(2000*(Nb_site-num-1));

            //La boucle principale
            while (true)
            {
                if (Proba_P > 0)
                    if (Generateur.NextDouble() < Proba_P)
                        panne = true;
                
                if (panne)
                {
                    GUI.nbr_panne++; GUI.Setstat_nbr_panne();
                    GUI.TraiterPanne();
                    Serveur.Stop();
                    Thread.CurrentThread.Suspend();
                }
                
                Thread.Sleep(1000 * Generateur.Next(min_A_H_S_C, max_A_H_S_C));
                DSC();

                lock (Verrou_Traitements)
                {
                    GUI.SetSC();
                    GUI.etat = "SC"; GUI.SetEtat();
                    GUI.nbr_sc++; GUI.Setstat_nbr_sc();
                    GUI.Ecrire_log("Entrée en SC");
                }
                
                Thread.Sleep(1000*Generateur.Next(min_D_S_C, max_D_S_C));

                lock (Verrou_Traitements)
                {
                    GUI.Ecrire_log("Sortie de la SC");
                    GUI.SetHorsSC();
                    GUI.etat = "Hors SC"; GUI.SetEtat();
                }                
                LSC();
            }
        }

        //Méthode d'envoi des messages
        public static void Envoyer(Message m, int Pj, int duree)
        {
            if (Pj==Nb_site || actifs[Pj])
            lock(Verrou_Envoyer[Pj])
            {
                try
                {
                    TcpClient client = new TcpClient(sites[Pj], BASE_PORTS + Pj);
                    Stream stream = client.GetStream();
                    Thread.Sleep(duree);
                    StreamWriter sw = new StreamWriter(stream);
                    sw.AutoFlush = true;
                    sw.WriteLine(m.ToString());
                    stream.Close();
                    client.Close();
                }
                catch (Exception e)
                {
                    
                }
            }
        }

        //Méthode de diffusion des messages
        public static void Diffuser(Message m)
        {
            switch (m.Type)
            {
                case 0:
                    GUI.Ecrire_log(horloge.ToString() + " : --> (req, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
                case 1:
                    GUI.Ecrire_log(horloge.ToString() + " : --> (rel, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
                case 2:
                    GUI.Ecrire_log(horloge.ToString() + " : --> (ack, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
                case 4:
                    GUI.Ecrire_log(horloge.ToString() + " : --> (rentrée, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
            }

            for (int j = 0; j < Nb_site; j++)
            {
                if (j!=num)
                    Envoyer(m, j, Generateur.Next(min_T_A, max_T_A)/Nb_site);
            }
        }

        //Méthode de réception des messages
        public static void Recevoir()
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
                        string s = sr.ReadLine();

                        lock (Verrou_Traitements)
                        {
                            Message m = new Message(s);

                            if (m.Site<Nb_site) valides[m.Site] = true;
                            switch (m.Type)
                            {
                                case 0:
                                    Maj(m.Horloge);
                                    messages[m.Site] = m;
                                    GUI.m = m; GUI.SetMessage();
                                    Envoyer(new Message(2, horloge, num), m.Site, Generateur.Next(min_T_A, max_T_A));
                                    break;
                                case 1:
                                    Maj(m.Horloge);
                                    messages[m.Site] = m;
                                    GUI.m = m; GUI.SetMessage();
                                    break;
                                case 2:
                                    Maj(m.Horloge);
                                    if (messages[m.Site].Type != 0)
                                    {
                                        messages[m.Site] = m;
                                        GUI.m = m; GUI.SetMessage();
                                    }
                                    break;
                                
                                case 3:
                                    GUI.Pi = m.Site; GUI.SetPanne();
                                    actifs[m.Site] = false;
                                    break;
                                case 4:
                                    GUI.Pj = m.Site; GUI.SetSain();
                                    actifs[m.Site] = true;
                                    Envoyer(messages[num], m.Site, Generateur.Next(min_T_A, max_T_A));
                                    break;
                            }

                            switch (m.Type)
                            {
                                case 0:
                                    GUI.Ecrire_log(horloge.ToString() + " : <-- (req, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
                                case 1:
                                    GUI.Ecrire_log(horloge.ToString() + " : <-- (rel, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
                                case 2:
                                    GUI.Ecrire_log(horloge.ToString() + " : <-- (ack, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
                                case 3:
                                    GUI.Ecrire_log(horloge.ToString() + " : <-- (absent, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
                                case 4:
                                    GUI.Ecrire_log(horloge.ToString() + " : <-- (rentrée, " + m.Horloge.ToString() + ", " + (m.Site + 1).ToString() + ")"); break;
                            }
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

        //Demande d'entrée en SC
        public static void DSC()
        {
            lock (Verrou_Traitements)
            {
                horloge++;
                GUI.h = horloge; GUI.SetHorloge();
                Diffuser(new Message(0, horloge, num));
                messages[num]=new Message(0, horloge, num);
                GUI.m = new Message(0, horloge, num); GUI.SetMessage();
            }

            //attendre [i <> j , estampille_de (f[i]) < estampille_de (f[j])];
            while (true)
            {
                int j;
                for (j=0; j<Nb_site; j++)
                    if (j!=num && actifs[j])
                        if ((messages[num].Horloge > messages[j].Horloge) || ((messages[num].Horloge == messages[j].Horloge) && num > j))
                            break;
                if (j == Nb_site)
                    break;
                Thread.Sleep(50);
            }
        }

        //Libération de la SC
        public static void LSC()
        {
            lock (Verrou_Traitements)
            {
                horloge++;
                GUI.h = horloge; GUI.SetHorloge();
                Diffuser(new Message(1, horloge, num));
                messages[num] = new Message(1, horloge, num); 
                GUI.m = new Message(1, horloge, num); GUI.SetMessage();
            }
        }

        //Mise à jour de l'horloge
        public static void Maj(int k)
        {
            if (horloge < k) horloge = k;
            horloge++;
            GUI.h = horloge; GUI.SetHorloge();
        }

        //Reprise de l'exécution après panne
        public static void Reprendre()
        {
            for (int j = 0; j < Nb_site; j++)
                if (RecevoirThread[j] != null) RecevoirThread[j].Abort();
            Serveur.Start();
            
            for (int j = 0; j < Nb_site; j++)
            {
                RecevoirThread[j] = new Thread(new ThreadStart(Recevoir));
                RecevoirThread[j].Start();
            }

            for (int j = 0; j < Nb_site; j++)
                valides[j] = false;

            for (int j = 0; j < Nb_site; j++)
            {
                GUI.Pj = j; GUI.SetSain();
                actifs[j] = true;
            }
            
            Envoyer(new Message(4, 0, num), Nb_site, Generateur.Next(min_T_A, max_T_A));

            while (true)
            {
                int j;
                for (j = 0; j < Nb_site; j++)
                    if (j != num && actifs[j] && !valides[j])
                        break;
                if (j == Nb_site)
                    break;
                Thread.Sleep(50);
            }

            DemarrerThread.Resume();
        }
    }
}
