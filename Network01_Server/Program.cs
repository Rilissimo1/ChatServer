using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;       //<= il  socket è il tassello fondamentale per quanto riguarda la costruzione di programmi che comunicano in network
using System.Net;
using System.Threading;
using System.IO;
using Network01_Shared;
using System.Security;

//Un applicazione server, solitamente definisce un "servizio"
namespace Network01_Server
{
    //Creiamo il nostro handler
    public class ChatUserHandler : ServerOTPC<ChatUserHandler>.ConnectionHandler
    {
        internal string Username { get; set; }
        internal ChatServer.Channel Channel { get; set; }


        public ChatUserHandler()
        {                  
        }

        protected override void OnDataReceived(byte bId, ushort uSize, BinaryReader hReader)
        {

            switch (bId)
            {
                case 1:
                    string sUsername = hReader.ReadString();
                    string sPassword = hReader.ReadString();
                    (this.Owner as ChatServer).Login(this, sUsername, sPassword);
                    break;

                case 2:
                    string iChannel = hReader.ReadString();
                    (this.Owner as ChatServer).JoinChannel(this, iChannel);
                    break;

                case 3:
                    string sMessage = hReader.ReadString();
                    (this.Owner as ChatServer).Message(this, sMessage);
                    break;
                case 4:
                    (this.Owner as ChatServer).LeaveChannel(this);
                    break;

                //se arriva un messaggio non previsto è da considerare un potenziale tentativo di attacco
                //un kick ci sta tutto..
                default: throw new SecurityException();                    
            }
        }
    }


    //Il server
    public class ChatServer : ServerOTPC<ChatUserHandler>
    {
        private System.Collections.Concurrent.ConcurrentDictionary<int, ChatUserHandler> m_hDictionary;
        private System.Collections.Concurrent.ConcurrentDictionary<string, Channel> m_hChannels;

        public ChatServer() : base(50, 512)
        {
            m_hDictionary = new System.Collections.Concurrent.ConcurrentDictionary<int, ChatUserHandler>();
            m_hChannels = new System.Collections.Concurrent.ConcurrentDictionary<string, Channel>();

            Channel bear = new Channel();
            Channel extreme = new Channel();
            Channel job = new Channel();


            m_hChannels.TryAdd("bear", new Channel());
            m_hChannels.TryAdd("extreme", new Channel());
            m_hChannels.TryAdd("merge", new Channel());
            m_hChannels.TryAdd("f00tf3t15!", new Channel());
            m_hChannels.TryAdd("renzi", new Channel());
        }


        //L'untente che ha effettuato l'operazione è detto contesto e solitamente viene passato come parametro sull'implementazione del metodo associato
        internal void Login(ChatUserHandler hContext, string username, string password)
        {
            //TODO: Accedere al db, controllare se esiste un username e una password
            hContext.Username = username;

            //TODO: modificare in modo da disconnettere l'utente già loggato
            if (!m_hDictionary.TryAdd(hContext.Id, hContext))
                throw new Exception();

            hContext.Writer.Write((byte)2);
            hContext.Writer.Write((short)1);
            hContext.Writer.Write(true);
            hContext.Writer.Flush();
        }


        internal void JoinChannel(ChatUserHandler hContext, string iChannelID)
        {
            //TODO: completare
            if (!m_hDictionary.ContainsKey(hContext.Id))
                throw new Exception();

            try
            {
                if (hContext.Channel != null || hContext.Channel == m_hChannels[iChannelID]) {
                    LeaveChannel(hContext);
                }
                Channel result;
                if (m_hChannels.TryGetValue(iChannelID, out result))
                    result.Add(hContext);
                else
                    throw new Exception();

                hContext.Writer.Write((byte)2);     //ID
                hContext.Writer.Write((short)1);    //SIZE
                hContext.Writer.Write(true);
            }
            catch (Exception)
            {
                hContext.Writer.Write((byte)2);     //ID
                hContext.Writer.Write((short)1);    //SIZE
                hContext.Writer.Write(false);
            }
        }

        internal void Message(ChatUserHandler hContext, string sMessage)
        {
            Console.WriteLine(hContext.Username + ": " + sMessage);

            string sFinalMessage = hContext.Username + ": " + sMessage;

            hContext.Channel.DispachMessage(hContext, sMessage);

        }

        internal void LeaveChannel(ChatUserHandler hContext)
        {
            hContext.Channel.Remove(hContext);
        }

        internal void BanUser(ChatUserHandler hContext, int banId)
        {
            
        }


        internal class Channel
        {
            private System.Collections.Concurrent.ConcurrentDictionary<int, ChatUserHandler> m_hChannelUsers;

            public Channel()
            {
                m_hChannelUsers = new System.Collections.Concurrent.ConcurrentDictionary<int, ChatUserHandler>();
            }

            public void Add(ChatUserHandler hUser)
            {
                if(m_hChannelUsers.TryAdd(hUser.Id, hUser))
                    hUser.Channel = this;

             
            }

            public void Remove(ChatUserHandler hUser)
            {
                ChatUserHandler result;
                m_hChannelUsers.TryRemove(hUser.Id, out result);
                hUser.Channel = null;
            }

            internal void DispachMessage(ChatUserHandler hContext, string sMessage)
            {
       
                m_hChannelUsers.Values.AsParallel().ForAll((client) => 
                {
                    if (client != hContext)
                    {
                        client.Writer.Write((byte)4);
                        client.Writer.Write((ushort)Encoding.UTF8.GetByteCount(sMessage));
                        client.Writer.Write(hContext.Username + ": " + sMessage);
                        client.Writer.Flush();
                    }
                });
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ChatServer hServer = new ChatServer();
            
            hServer.ServerStarted       += OnServerStarted;
            hServer.ServerStopped       += OnServerStopped;
            hServer.ClientConnected     += OnClientConnected;
            hServer.ClientDisconnected  += OnClientDisconnected;

            hServer.Start(2800);

            Thread.CurrentThread.Join();
        }

        private static void OnClientDisconnected(ChatUserHandler obj)
        {
            Console.WriteLine("OnClientDisconnected");
        }

        private static void OnClientConnected(ChatUserHandler obj)
        {
            Console.WriteLine("OnClientConnected");
        }

        private static void OnServerStopped()
        {
            Console.WriteLine("OnServerStopped");
        }

        private static void OnServerStarted()
        {
            Console.WriteLine("OnServerStarted");
        }
    }
}
