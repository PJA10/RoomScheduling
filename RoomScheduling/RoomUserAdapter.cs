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
using Android.Views;
using Android.Widget;
using Java.Lang;
using RoomClasses;
using Xamarin.Essentials;

namespace RoomScheduling
{
    public class RoomUserAdapter : BaseAdapter<RoomUser>
    {
        Android.Content.Context context;
        List<RoomUser> objects;
        static Socket secondSender;
        public RoomUserAdapter(Android.Content.Context context, System.Collections.Generic.List<RoomUser> objects)
        {
            this.context = context;
            this.objects = objects;
        }
        public List<RoomUser> GetList()
        {
            return this.objects;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override int Count
        {

            get { return this.objects.Count; }

        }
        public override RoomUser this[int position]
        {

            get { return this.objects[position]; }

        }

        public void updated()
        {
            NotifyDataSetChanged();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)

        {

            Android.Views.LayoutInflater layoutInflater = ((RoomActivity)context).LayoutInflater;

            Android.Views.View view = layoutInflater.Inflate(Resource.Layout.RoomUserListItem, parent, false);

            TextView nameTextView = view.FindViewById<TextView>(Resource.Id.name);
            TextView startTimeTV = view.FindViewById<TextView>(Resource.Id.stTV);
            TextView endTimeTV = view.FindViewById<TextView>(Resource.Id.etTV);
            Button callBtn = view.FindViewById<Button>(Resource.Id.callBtn);
            Button deleteBtn = view.FindViewById<Button>(Resource.Id.deleteBtn);

            RoomUser temp = objects[position];
            if (temp != null)
            {
                if (MainActivity.appUserName != temp.userName)
                {
                    deleteBtn.Visibility = ViewStates.Invisible;   
                }
                else
                {
                    callBtn.Visibility = ViewStates.Invisible;
                }
                callBtn.Tag = new parametersWrapper(temp.phoneNumber);
                deleteBtn.Tag = new parametersWrapper(temp);
                callBtn.Click += callClicked;
                deleteBtn.Click += deleteClicked;
                nameTextView.Text = temp.userName;
                startTimeTV.Text = temp.startTime.ToShortTimeString();
                endTimeTV.Text = temp.endTime.ToShortTimeString();
            }
            return view;

        }
        private void callClicked(object sender, EventArgs e)
        {
            var phoneNumber = ((parametersWrapper)((Button)sender).Tag).getParam1();

            PhoneDialer.Open((string)phoneNumber);
        }
        private void deleteClicked(object sender, EventArgs e)
        {
            string toast = "";
            System.Boolean finished = false;
            RoomUser roomUser = (RoomUser)((parametersWrapper)((Button)sender).Tag).getParam1();
            new System.Threading.Thread(new ThreadStart(delegate
            {
                try
                {
                    MainActivity.connectToServer(ref secondSender, MainActivity.IP, MainActivity.port);
                    // Connect Socket to the remote 
                    // endpoint using method Connect() 
                    MainActivity.sendString(secondSender, string.Format("Del \r\n{0}\r\n{1}\r\n{2}\r\n{3}<EOF>", roomUser.roomId, roomUser.userName, roomUser.startTime.ToShortTimeString(), roomUser.endTime.ToShortTimeString()));
                    string recvedString = MainActivity.recvString(secondSender);
                    toast = string.Format(recvedString);
                    MainActivity.close_conn(secondSender);

                }
                catch (System.Exception er)
                {
                    toast = er.ToString();
                }
                finished = true;
                
            })).Start();

            while (!finished)
            {
                System.Threading.Thread.Sleep(1000);
            }
            Toast.MakeText(context, toast, ToastLength.Long).Show();

        }
    }

    internal class parametersWrapper : Java.Lang.Object
    {
        private object param1;

        public parametersWrapper(object param1)
        {
            this.param1 = param1;
        }
        public object getParam1()
        {
            return param1;
        }
    }
}