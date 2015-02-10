using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for RChatWindow.xaml
    /// </summary>
    /// 
    //The commands for interaction between the server and the client

    public partial class RChatWindow : Window
    {
        public Socket clientSocket;
        public string strName;
        //Client client = new Client();
        private byte[] byteData = new byte[1024];

        public RChatWindow(string username)
        {
          //  this.strName = username;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
        }

        //Broadcast the message typed by the user to everyone
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.strMessage = mytextBox.Text;
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                //Send it to the server
                clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

               mytextBox.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "SGSclientTCP: " + strName);
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclientTCP: " );
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
                        chatters.Items.Add(msgReceived.strName);
                        break;

                    case Command.Logout:
                        chatters.Items.Remove(msgReceived.strName);
                        break;

                    case Command.Message:
                        break;

                    case Command.List:
                        chatters.Items.Add(msgReceived.strMessage.Split('*'));
                        chatters.Items.RemoveAt(chatters.Items.Count - 1);
                        //chatters.Text += "<<<" + strName + " has joined the room>>>\r\n";
                        break;
                }

                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
                    yourscrollViewer1.Content += msgReceived.strMessage + "\r\n";

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
                MessageBox.Show(ex.Message, "SGSclientTCP: " + strName);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //this.Text = "SGSclientTCP: " + strName;

            //The user has logged into the system so we now request the server to send
            //the names of all users who are in the chat room
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = strName;
            msgToSend.strMessage = null;

            byteData = msgToSend.ToByte();

            clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

            byteData = new byte[1024];
            //Start listening to the data asynchronously
            clientSocket.BeginReceive(byteData,
                                       0,
                                       byteData.Length,
                                       SocketFlags.None,
                                       new AsyncCallback(OnReceive),
                                       null);

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
          MessageBox.Show("Are you sure you want to leave the chat room? ");
            try
            {
                //Send a message to logout of the server
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Logout;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte();
                clientSocket.Send(b, 0, b.Length, SocketFlags.None);
                clientSocket.Close();
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }










        }

        private void chatters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

           
        }

    }
}