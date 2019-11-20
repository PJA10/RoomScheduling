using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Util;
using System;
using static RoomClasses;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Timers;
using System.Linq;
using Android.Views;
using Android.Views.InputMethods;
using Android.Content;
using Android.Content.PM;
using System.Collections.Generic;

namespace RoomScheduling
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity, ListView.IOnItemClickListener
    {
        public static List<Room> roomList { get; set; }
        //Room[] roomList = new Room[] { new Room("201"), new Room("202"), new Room("203"), new Room("204"), new Room("204"), new Room("auditorium") };
        public static string IP = "3.19.114.185";// http://:/
        public static int port = 14578;
        ListView RoomsAvailable;
        static public Socket sender;
        RoomAdapter roomAdapter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            

            connectToServer(ref sender, IP, port);
            GET_rooms();

            roomAdapter = new RoomAdapter(this, roomList);
            RoomsAvailable = FindViewById<ListView>(Resource.Id.roomsLV);
          
            RoomsAvailable.Adapter = roomAdapter;

            RoomsAvailable.OnItemClickListener = this;


        }

        public void OnItemClick(AdapterView parent, Android.Views.View view, int position, long id)
        {
            Intent intent = new Intent(this, typeof(RoomActivity));
            Room temp = MainActivity.roomList[position];
            intent.PutExtra("pos", position);
            StartActivity(intent);
        }

        protected override void OnStop()
        {
            if (sender.Connected)
            {
                close_conn(sender);
            }
            base.OnStop();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public static void connectToServer(ref Socket socket, string IP, int port)
        {

            try
            {

                // Establish the remote endpoint 
                // for the socket. This example 
                // uses port 11111 on the local 
                // computer. 
                //IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = IPAddress.Parse(IP);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

                // Creation TCP/IP Socket using 
                // Socket Class Costructor 
                socket = new Socket(ipAddr.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                try
                {

                    // Connect Socket to the remote 
                    // endpoint using method Connect() 
                    socket.Connect(localEndPoint);

                    // We print EndPoint information 
                    // that we are connected
                    //Console.WriteLine((string.Format("Socket connected to -> {0} ", socket.RemoteEndPoint.ToString())));

                    // Creation of messagge that 
                    // we will send to Server 
                }

                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    close_conn(socket);
                    //Console.WriteLine(string.Format("ArgumentNullException : {0}", ane.ToString()));
                    
                }

                catch (SocketException se)
                {

                    //Console.WriteLine(string.Format("SocketException : {0}", se.ToString()));
                    close_conn(socket);
                }

                catch (Exception e)
                {
                    //Console.WriteLine(string.Format("Unexpected exception : {0}", e.ToString()));
                    close_conn(socket);
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
        }
        public static void close_conn(Socket socket)
        {
            sendString(socket, "close connection");
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public static void sendString(Socket socket, String toSend)
        {
            byte[] messageSent = Encoding.ASCII.GetBytes(toSend + "<EOF>");
            int byteSent = socket.Send(messageSent);
        }

        public static String recvString(Socket socket)
        {
            byte[] messageReceived = new byte[1024];

            int byteRecv = socket.Receive(messageReceived);
            String recvedString = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
            return recvedString;
        }

        public static object GET(Socket sender, String send_msg)
        {
            sendString(sender, send_msg);

            // Data buffer 
            byte[] messageReceived = new byte[1024 * 2];
            int byteRecv = sender.Receive(messageReceived);
            byte[] msgLengthBytes = new byte[15];
            System.Buffer.BlockCopy(messageReceived, 0, msgLengthBytes, 0, msgLengthBytes.Length);
            System.Buffer.BlockCopy(messageReceived, 15, messageReceived, 0, messageReceived.Length - 15);

            Array.Reverse(msgLengthBytes);
            int msgLength = BitConverter.ToInt32(msgLengthBytes, 0);
            int totalbyte = byteRecv;

            while (msgLength > totalbyte - 15)
            {
                byteRecv = sender.Receive(messageReceived, byteRecv, msgLength - byteRecv - 15, SocketFlags.None);
                totalbyte += byteRecv;
            }
            return (ByteArrayToObject(messageReceived));
        }

        private void GET_rooms()
        {
            
            List<Room> recevedRoomlist = (List<Room>)GET(sender, "GET rooms");
            if (roomList != null)
            {
                if (!Enumerable.SequenceEqual(roomList, recevedRoomlist))
                {
                    roomList = recevedRoomlist;
                    roomAdapter.updated();
                }
            }
            else
            {
                roomList = recevedRoomlist;
            }
        }
    }
}



