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
using System.Collections.ObjectModel;
using MahApps.Metro.Controls;
using System.ComponentModel;

namespace ExclusionMutuelle
{
    public partial class MainWindow : MetroWindow
    {
        public static App config;
        
        public int h;
        public int Pi;
        public int Pj;
        public Message m;
        public string etat;

        public int nbr_sites = 0;
        public int nbr_sc = 0;
        public int nbr_panne = 0;

        private int nbr_process = 0;
        MainViewModel vm;

        string path_log = "log\\";

        public MainWindow()
        {
            InitializeComponent();
            Site.GUI = this;

            if (config.args != null)
            {
                Site.Nb_site = Int32.Parse(config.args[0]);

                Site.min_D_S_C = Int32.Parse(config.args[1]);
                Site.max_D_S_C = Int32.Parse(config.args[2]);

                Site.min_A_H_S_C = Int32.Parse(config.args[3]);
                Site.max_A_H_S_C = Int32.Parse(config.args[4]);

                Site.min_T_A = Int32.Parse(config.args[5]);
                Site.max_T_A = Int32.Parse(config.args[6]);

                Site.min_D_P = Int32.Parse(config.args[7]);
                Site.max_D_P = Int32.Parse(config.args[8]);

                Site.Proba_P = Double.Parse(config.args[9]);
                Site.num = Int32.Parse(config.args[10]);

                Site.sites = new string[Site.Nb_site+1];
                for (int i = 0; i < Site.Nb_site; i++)
                    Site.sites[i] = config.args[11 + i];
                Site.sites[Site.Nb_site] = "127.0.0.1";

                if (Site.Proba_P > 0) bt1.IsEnabled = false;
            }

            Initialize();

            vm = new MainViewModel();
            this.DataContext = vm;
            
            new Thread(new ThreadStart(Site.Demarrer)).Start();
        }

        private void Initialize()
        {
            System.Diagnostics.Process[] exs;
            exs = System.Diagnostics.Process.GetProcessesByName("site");
            foreach (System.Diagnostics.Process ex in exs)
            {
                nbr_process++;
            }
                        
            this.Title += (Site.num+1).ToString();
            if (nbr_process < 6)
            {
                this.Left = 140 + (this.Width + 15) * (nbr_process - 1);
                this.Top = 5;
            }
            else
            {
                this.Left = 140 + (this.Width + 15) * (nbr_process - 6);
                this.Top = 5 + this.Height + 10;
            }

            if (Site.Nb_site<=5) this.Top += 150;

            nbr_process--;

            ring.RenderTransform = new ScaleTransform(1,1);

            nbr_sites = Site.Nb_site; Setstat_nbr_sites();
        }

        public void ImprimerFromRichTextBox(RichTextBox rtb)
        {
            string ch = "site_" + (Site.num+1) + ".txt";
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

            System.IO.StreamWriter file = new System.IO.StreamWriter(path_log + ch);
            file.WriteLine(textRange.Text);
            file.Close();
        }

        public void Ecrire_log(string chaine)
        {
            RichTextBox1.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                delegate()
                {
                    RichTextBox1.Document.Blocks.Add(new Paragraph(new Run(chaine)));
                }
                ));
        }

        public void Setstat_nbr_sites()
        {
            //stat_nbr_sites.Dispatcher.Invoke(
            //System.Windows.Threading.DispatcherPriority.Normal,
            //new Action(
            //delegate()
            //{
            //    stat_nbr_sites.Text = nbr_sites.ToString();
            //}
            //));
        }
        
        public void Setstat_nbr_sc()
        {
            stat_nbr_sc.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                stat_nbr_sc.Text = nbr_sc.ToString();
            }
            ));
        }
        
        public void Setstat_nbr_panne()
        {
            stat_nbr_panne.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                stat_nbr_panne.Text = nbr_panne.ToString();
            }
            ));
        }

        public void SetHorloge()
        {
            tb_horloge.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                tb_horloge.Text = h.ToString();
            }
            ));

            stat_Horloge.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                delegate()
                {
                    stat_Horloge.Text = h.ToString();
                }
                ));
        }

        public void SetSC()
        {
            try
            {
                DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(Site.num);
                dg.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        row.Background = Brushes.Green;
                    }));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void SetHorsSC()
        {
            try
            {
                DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(Site.num);
                dg.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        row.Background = Brushes.Transparent;
                    }));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void SetPanne()
        {
            try
            {
                DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(Pi);
                dg.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        row.Background = Brushes.Tomato;
                    }));
            }
            catch (Exception e)
            { 
                MessageBox.Show(e.Message); 
            }
        }

        public void SetSain()
        {
            try
            {
                DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(Pj);
                dg.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate()
                        {
                            row.Background = Brushes.Transparent;
                        }));
            }
            catch (Exception e)
            { MessageBox.Show(e.Message); }
        }

        public void SetMessage()
        {
            dg.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {

                        try
                        {
                            vm.Messages[m.Site].H = m.Horloge.ToString();
                            switch (m.Type)
                            {
                                case 0: vm.Messages[m.Site].Msg = "Req"; break;
                                case 1: vm.Messages[m.Site].Msg = "Rel"; break;
                                case 2: vm.Messages[m.Site].Msg = "Acq"; break;
                            }
                        }
                        catch (Exception e)
                        { MessageBox.Show(e.Message); }
                    }
                    ));
        }
        
        public void SetEtat()
        {
            tb_etat.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                tb_etat.Text = etat;
            }
            ));
        }

        private void Quit(object sender, EventArgs e)
        {
            ImprimerFromRichTextBox(RichTextBox1);
            Site.Serveur.Stop();
            if (Site.DemarrerThread.ThreadState==System.Threading.ThreadState.Suspended) Site.DemarrerThread.Resume();
            Site.DemarrerThread.Abort();
        }

        private void panne_click(object sender, RoutedEventArgs e)
        {
            if (!Site.panne)
            {
                bt1.IsEnabled = false;
            }
            else
            {
                dg.Visibility = Visibility.Visible;
                ring.IsActive = false;
                bt1.Content = "Stop";
                tb_etat.Text = "Hors SC";
            }
            Site.panne = !Site.panne;

            if (!Site.panne)
            {
                new Thread(new ThreadStart(Site.Reprendre)).Start();
            }
        }

        public void TraiterPanne()
        {
            bt1.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                if (Site.Proba_P==0) bt1.IsEnabled = true;
                bt1.Content = "Start";
            }
            ));

            dg.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                dg.Visibility = Visibility.Hidden;
            }
            ));

            ring.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                ring.IsActive = true;
            }
            ));

            tb_etat.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                tb_etat.Text = "En panne";
            }
            ));

            if (Site.Proba_P > 0)
                new Thread(new ThreadStart(TerminerPanne)).Start();
        }

        public void TerminerPanne()
        {
            Thread.Sleep(1000 * Site.Generateur.Next(Site.min_D_P, Site.max_D_P));
            dg.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                dg.Visibility = Visibility.Visible;
            }
            ));

            bt1.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                bt1.Content = "Stop";
            }
            ));

            ring.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                ring.IsActive = false;
            }
            ));

            tb_etat.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
            delegate()
            {
                tb_etat.Text = "Hors SC";
            }
            ));
            Site.panne = false;
            new Thread(new ThreadStart(Site.Reprendre)).Start();
        }

       
    }

    public class DataItem : INotifyPropertyChanged
    {
        public string _Msg;
        public string Msg
        {
            get
            {
                return _Msg;
            }
            set
            {
                if (value != _Msg)
                {
                    _Msg = value;
                    OnPropertyChanged("Msg");
                }
            }

        }

        public string _H;
        public string H
        {
            get
            {
                return _H;
            }
            set
            {
                if (value != _H)
                {
                    _H = value;
                    OnPropertyChanged("H");
                }
            }

        }

        public string _Site;
        public string Site
        {
            get
            {
                return _Site;
            }
            set
            {
                if (value != _Site)
                {
                    _Site = value;
                    OnPropertyChanged("Site");
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

        public DataItem(string Msg, string H, string Site)
        {

        }

        public DataItem()
        {

        }
    }

    public class MainViewModel
    {
        public ObservableCollection<DataItem> Messages { get; set; }

        public MainViewModel()
        {
            Messages = new ObservableCollection<DataItem>();
            for (int i = 0; i < Site.Nb_site; i++) Messages.Add(new DataItem() { Msg = "Rel", H = "0", Site = (i + 1).ToString() });
        }
    }
}
