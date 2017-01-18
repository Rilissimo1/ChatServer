using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ChatClient
{
    interface ICommand
    {
        void Execute(string[] hParams);
    }

    class ConnectCommand : ICommand
    {
        private MainWindow m_hOwner;
        public ConnectCommand(MainWindow hOwner)
        {
            m_hOwner = hOwner;
        }

        public void Execute(string[] hParams)
        {
            m_hOwner.Client = new Client();
            m_hOwner.Client.Connect(hParams[1], int.Parse(hParams[2]));
            m_hOwner.Client.MessageReceived += m_hOwner.OnMessageReceived;
        }
    }

    class LoginCommand : ICommand
    {
        private MainWindow m_hOwner;

        public LoginCommand(MainWindow hOwner)
        {
            m_hOwner = hOwner;
        }

        public void Execute(string[] hParams)
        {
            if (hParams.Length != 3) 
                throw new Exception("Invalid Login Attempt");
            
            bool bRes = m_hOwner.Client.Login(hParams[1], hParams[2]);

            if (bRes)
                m_hOwner.m_hTextBlock.Text += "Login Successfull!" + Environment.NewLine;
            else
                m_hOwner.m_hTextBlock.Text += "Login Failed" + Environment.NewLine;

        }
    }

    class JoinCommand : ICommand
    {
        private MainWindow m_hOwner;

        public JoinCommand(MainWindow hOwner)
        {
            m_hOwner = hOwner;
        }

        public void Execute(string[] hParams)
        {
            bool bRes = m_hOwner.Client.Join(hParams[1]);
            
            if (bRes)
                m_hOwner.m_hTextBlock.Text += "Join Successfull!" + Environment.NewLine;
            else
                m_hOwner.m_hTextBlock.Text += "Join Failed" + Environment.NewLine;

        }
    }

    class LeaveCommand : ICommand {
        private MainWindow m_hOwner;

        public LeaveCommand(MainWindow hOwner)
        {
            m_hOwner = hOwner;
        }

        public void Execute(string[] hParams)
        {
            bool bRes = m_hOwner.Client.Leave();

            if (bRes)
                m_hOwner.m_hTextBlock.Text += "Leave Successfull!" + Environment.NewLine;
            else
                m_hOwner.m_hTextBlock.Text += "Leave Failed" + Environment.NewLine;

        }
    }

    public partial class MainWindow : Window
    {
        private Dictionary<string, ICommand> m_hCommands;
        public Client Client { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            Closing += OnWindowClose;
            m_hCommands = new Dictionary<string, ICommand>();

            m_hCommands.Add("\\connect",    new ConnectCommand(this));
            m_hCommands.Add("\\login",      new LoginCommand(this));
            m_hCommands.Add("\\join",       new JoinCommand(this));
            m_hCommands.Add("\\leave",      new LeaveCommand(this));
        }

        private void OnWindowClose(object sender, System.ComponentModel.CancelEventArgs e) {
            
        }

        private void OnTextboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {                
                string sCommand = m_hTextBox.Text;

                try
                {
                    if (sCommand.IsCommand())
                    {
                        string[] sParams = sCommand.ToLower().Split(new char[] { ' ' });

                        m_hCommands[sParams[0]].Execute(sParams);
                    }
                    else
                    {
                        Client.Message(sCommand);
                        m_hTextBlock.Text += m_hTextBox.Text + Environment.NewLine;
                    }


                }
                catch (Exception hEx)
                {
                    m_hTextBlock.Text += hEx.Message + Environment.NewLine;
                }
                finally
                {
                    m_hTextBox.Clear();
                }
            }
        }

        internal void OnMessageReceived(string obj) {
            //Thread Marshalling (non possiamo invokare metodi sugli oggetti della GUI da un Thread che non e' quello principale)
            Dispatcher.Invoke(() => m_hTextBlock.Text += obj + Environment.NewLine);
        }
    }


    public static class GUIExtensions
    {
        public static bool IsCommand(this string hThis)
        {            
            return hThis.Length > 0 && hThis[0] == '\\';
        }
    }
}
