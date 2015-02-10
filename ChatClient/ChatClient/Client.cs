using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace ChatClient
{
    class Client
    {
        string Username { get; set; }
        string Password { get; set; }
        string IP { get; set; }
        public Socket clientSocket;
        public string strName;
        public List<string> exceptions = new List<string>();
        private byte[] byteData = new byte[1024];
        private List<string> lstChatters = new List<string>();
        public Client(string username, string password, string ip)
        {
            this.Username = username;
            this.Password = password;
            this.IP = ip;

        }

        public Client(){}

        public void connect()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(this.IP);
                //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                //Connect to the server
                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
                Console.WriteLine(this.Username);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex.Message);
            }

        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);
                //We are connected so we login into the server
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Login;
                msgToSend.strName = this.Username;
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte();

                //Send the message to the server
                clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex.Message);
            }
        }
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
                strName = this.Username;

            }
            catch (Exception ex)
            {
                exceptions.Add(ex.Message);

            }
        }


        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);

                Data msgReceived = new Data(byteData);
                //Accordingly process the message received
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        lstChatters.Add(msgReceived.strName);
                        break;

                    case Command.Logout:
                        lstChatters.Remove(msgReceived.strName);
                        break;

                    case Command.Message:
                        break;

                    case Command.List:
                        lstChatters.AddRange(msgReceived.strMessage.Split('*'));
                        lstChatters.RemoveAt(lstChatters.Count - 1);
                        //txtChatBox.Text += "<<<" + strName + " has joined the room>>>\r\n";
                        break;
                }

                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
                    //txtChatBox.Text += msgReceived.strMessage + "\r\n";

                byteData = new byte[1024];

                clientSocket.BeginReceive(byteData,
                                          0,
                                          byteData.Length,
                                          SocketFlags.None,
                                          new AsyncCallback(OnReceive),
                                          null);

            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message, "SGSclientTCP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void sendMessage(Data msgToSend)
        {
            byte[] byteData = msgToSend.ToByte();
            try
            {
             //Send it to the server
                clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (Exception)
            {
                exceptions.Add("Unable to send message to the server.");
            }  
            



        }

    }
}
