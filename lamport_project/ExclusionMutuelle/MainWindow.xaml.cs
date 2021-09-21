using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Lamport
{
    public partial class MainWindow : MetroWindow
    {
        const int BASE_PORTS = 2070;

        private int Nb_site = 0;

        private int min_D_S_C = 0;
        private int max_D_S_C = 0;
        
        private int min_A_H_S_C = 0;
        private int max_A_H_S_C = 0;
        
        private int min_T_A = 0;
        private int max_T_A = 0;
        
        private int min_D_P = 0;
        private int max_D_P = 0;
        
        private double Proba_P = 0;

        Thread th;
        Thread thread_simu;
        Thread thread_quit;

        MainViewModel vm;

        public MainWindow()
        {
            InitializeComponent();
            th = new Thread(Function);
            th.Start();
            D_P.LowerValue = 10;
            D_P.LowerValue = 15;
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Nb_site = (int)Math.Floor(slider1.Value);
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Proba_P = Math.Round(slider2.Value,2);
        }

        void Function()
        {
            int sauv1 = 0;
            double sauv2 = 0;
            while (true)
            {
                if (sauv1 != Nb_site)
                {
                    sauv1 = Nb_site;
                    slider1_res.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        slider1_res.Text = sauv1.ToString();
                    }
                    ));
                }
                if (sauv2 != Proba_P)
                {
                    sauv2 = Proba_P;
                    slider2_res.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        slider2_res.Text = sauv2.ToString();
                    }
                    ));
                }
            }
        }

        private void quit_thread()
        {
            ProgrssBar.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        ProgrssBar.Visibility = Visibility.Visible;
                    }
                    ));
            System.Diagnostics.Process[] exs;
            exs = System.Diagnostics.Process.GetProcessesByName("site");
            foreach (System.Diagnostics.Process ex in exs)
            {
                ex.Kill();
                System.Threading.Thread.Sleep(1000);
            }

            exs = System.Diagnostics.Process.GetProcessesByName("detecteur");
            foreach (System.Diagnostics.Process ex in exs)
            {
                ex.Kill();
                System.Threading.Thread.Sleep(1000);
            }


            ProgrssBar.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        ProgrssBar.Visibility = Visibility.Hidden;
                    }
                    ));
            thread_quit.Abort();
        }

        private void lancer_simu_thread()
        {
            ProgrssBar.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        ProgrssBar.Visibility = Visibility.Visible;
                    }
                    ));

            string arg = Nb_site.ToString() + " " + min_D_S_C.ToString() + " " + max_D_S_C.ToString() + " " + min_A_H_S_C.ToString() + " " + max_A_H_S_C.ToString() + " " + min_T_A.ToString() + " " + max_T_A.ToString() + " " + min_D_P.ToString() + " " + max_D_P.ToString() + " " + Proba_P.ToString();

            string sites="";
            for (int i = 0; i < Nb_site; i++)
            {
                sites += "127.0.0.1 "; //vm.Messages[i].ip + " ";
            }

            for (int i = 0; i < Nb_site; i++)
            {
                //string machine = vm.Messages[i].ip;
                try
                {
                    //TcpClient client = new TcpClient(machine, BASE_PORTS + Int32.Parse(machine.Split('.')[3]));
                    //Stream stream = client.GetStream();
                    //StreamWriter sw = new StreamWriter(stream);
                    //sw.AutoFlush = true;
                    //sw.WriteLine(arg + " " + i + " " + sites);

                    string chemin_site = "site.exe";

                    ProcessStartInfo processInfo = new ProcessStartInfo(chemin_site, arg + " " + i + " " + sites);
                    Process myProcess = Process.Start(processInfo);

                    //stream.Close();
                    //client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                System.Threading.Thread.Sleep(1500);
            }

            Process.Start(new ProcessStartInfo("detecteur.exe", arg + " " + sites));

            ProgrssBar.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        ProgrssBar.Visibility = Visibility.Hidden;
                    }
                    ));
            thread_simu.Abort(); 
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            th.Abort();

            min_D_S_C = (int)D_S_C.LowerValue;
            max_D_S_C = (int)D_S_C.UpperValue;

            min_A_H_S_C = (int)A_H_S_C.LowerValue;
            max_A_H_S_C = (int)A_H_S_C.UpperValue;

            min_T_A = (int)T_A.LowerValue;
            max_T_A = (int)T_A.UpperValue;

            min_D_P = (int)D_P.LowerValue;
            max_D_P = (int)D_P.UpperValue;

            Proba_P = slider2.Value;

            thread_simu = new Thread(lancer_simu_thread);
            thread_simu.Start();
        }

        private void close(object sender, CancelEventArgs e)
        {
            th.Abort();
            thread_quit = new Thread(quit_thread);
            thread_quit.Start();
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            vm = new MainViewModel(Nb_site);
            this.DataContext = vm;
        }

        private void D_S_C_RangeSelectionChanged(object sender, RangeSelectionChangedEventArgs e)
        {

        }
    }

    public class DataItem : INotifyPropertyChanged
    {
        public string _num_machine;
        
        public string num_machine
        {
            get
            {
                return _num_machine;
            }
            set
            {
                if (value != _num_machine)
                {
                    _num_machine = value;
                    OnPropertyChanged("num_machine");
                }
            }
        }

        public string _ip;
        
        public string ip
        {
            get
            {
                return _ip;
            }
            set
            {
                if (value != _ip)
                {
                    _ip = value;
                    OnPropertyChanged("ip");
                }
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public void OnPropertyChanged(string p)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(p));
            }
        }

        public DataItem(string num_machine, string ip)
        {

        }

        public DataItem()
        {

        }
    }

    public class MainViewModel
    {
        public ObservableCollection<DataItem> Messages { get; set; }

        public MainViewModel(int c)
        {
            Messages = new ObservableCollection<DataItem>();
            for (int i = 0; i < c; i++) Messages.Add(new DataItem() { num_machine = (i+1).ToString(), ip = "192.168.1.1" });
        }
    }
}
