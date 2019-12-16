using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using RoomClasses;
using System.Timers;
using Newtonsoft.Json;


namespace RoomScheduling
{
    [Service]
    public class refreshService : IntentService
    {
        public static int STATUS_ROOM_LIST = 0;

        int roomId = -1;
        Socket sender;
        Messenger messenger;
        List<RoomUser> recevedRoomUserslist;
        public refreshService() : base("DemoIntentService")
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            var recv = intent.GetParcelableExtra("handler");
            messenger = (Messenger)recv;
            roomId = intent.GetIntExtra("roomId", -1);

            MainActivity.connectToServer(ref sender, MainActivity.IP, MainActivity.port);
            GET_users();

            /*Thread thr = new Thread(new ThreadStart(this.TimeUp));
            thr.Start();*/
            while (sender.Connected)
            {
                GET_users();
                Thread.Sleep(5000);
            }
        }
        public void TimeUp()
        {
            GET_users();
        }

        private void GET_users()
        {
            if (sender.Connected)
            {
                recevedRoomUserslist = (List<RoomUser>)MainActivity.GET(sender, "GET " + roomId.ToString() + "<EOF>");
                Message msg = new Message();
                msg.Obj = helperClass.ObjectToByteArray(recevedRoomUserslist);
                msg.What = STATUS_ROOM_LIST;
                messenger.Send(msg);
            }
        }
        
        public override void OnDestroy()
        {
            //myTimer.Stop();
            MainActivity.close_conn(sender);
            base.OnDestroy();
        }
    }
}