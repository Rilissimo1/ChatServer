using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient {
    public class Client {
        //per stabilire la connessione
        private Socket m_hSocket;

        //x la ricezione dedichiamo un thread a se stante
        private Thread m_hRecvThread;

        private NetworkStream m_hNS;
        private BinaryWriter m_hWriter;
        private BinaryReader m_hReader;

        private AutoResetEvent m_hEvent;
        private static bool m_bLastResult;
        public event Action<string> OnMessageReceived;

        public Client()
        {
            //settiamo l'autoresetevent
            m_hEvent = new AutoResetEvent(false);

            OnMessageReceived += (x) => Console.WriteLine(x);
        }


        public void Connect(string sAddr, int iPort)
        {
            m_hSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.m_hSocket.ReceiveBufferSize = 1024;
            m_hSocket.Connect(sAddr, iPort);

            m_hNS = new NetworkStream(m_hSocket);

            m_hWriter = new BinaryWriter(m_hNS);
            m_hReader = new BinaryReader(m_hNS);

            m_hRecvThread = new Thread(RecvThread);
            m_hRecvThread.Start();
        }

        public bool Login(string sUsername, string sPassword)
        {
            //Scriviamo Header e dati utilizzando lo stream
            m_hWriter.Write((byte)1);
            m_hWriter.Write((ushort)(Encoding.UTF8.GetByteCount(sUsername) + Encoding.UTF8.GetByteCount(sPassword)));
            m_hWriter.Write(sUsername);
            m_hWriter.Write(sPassword);

            m_hWriter.Flush();              //tutti i stream espongono questo metodo, che blocca fino a quando la scrittura sul dispositivo non è stata ultimata

            m_hEvent.WaitOne();

            return m_bLastResult;
        }

        public bool Join(string sChannelID)
        {
            m_hWriter.Write((byte)2);
            m_hWriter.Write((ushort)Encoding.UTF8.GetByteCount(sChannelID));
            m_hWriter.Write(sChannelID);
            m_hWriter.Flush();
            m_hEvent.WaitOne();
            return m_bLastResult;
        }

        public void Message(string sMessage)
        {
            m_hWriter.Write((byte)1);
            m_hWriter.Write((ushort)Encoding.UTF8.GetByteCount(sMessage));
            m_hWriter.Write(sMessage);
            m_hWriter.Flush();
        }

        public void Leave()
        {
        }

        private void RecvThread()
        {
            while (true)
            {
                //In fase di ricezione leggiamo prima il byte iniziale

                byte id = m_hReader.ReadByte();

                ushort size = m_hReader.ReadUInt16();


                switch (id)
                {
                    //Ack, NACK
                    case 2:
                        m_bLastResult = m_hReader.ReadBoolean();
                        m_hEvent.Set(); //è sempre TReceiver a comunicare in direzione di Main
                        break;
                    case 4:
                        OnMessageReceived?.Invoke(m_hReader.ReadString());
                        break;

                }

            }
        }
    }
}
