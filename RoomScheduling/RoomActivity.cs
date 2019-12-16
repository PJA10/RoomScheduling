using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;
using System.Timers;
using RoomClasses;
using Android.Support.V7.App;
using System.Threading.Tasks;
using System.Threading;
using Timer = System.Timers.Timer;
using Newtonsoft.Json;
using static RoomScheduling.refreshHandler;
using Xamarin.Essentials;

namespace RoomScheduling
{
    [Activity(Theme = "@style/AppTheme")]
    public class RoomActivity : AppCompatActivity, AppReceiver
    {
        public List<RoomUser> usersList;
        Button StartTimeInput;
        Button EndTimeInput;
        Button submit;
        EditText NameInput;
        int pos = -1;
        public RoomUserAdapter roomUserAdapter = null;
        ListView roomUsersLV;
        int roomId;
        ProgressDialog loadingDialog;
        Intent service;

        async protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.room_main);

            MainActivity.showLoadingScreen(this, ref loadingDialog);
            pos = Intent.GetIntExtra("pos", -1);
            roomId = MainActivity.roomList[pos].Id;
            TextView roomNameTV = FindViewById<TextView>(Resource.Id.roomName);
            roomNameTV.Text = MainActivity.roomList[pos].roomName;

            refreshHandler handler = new refreshHandler(this);

            service = new Intent(this, typeof(refreshService));
            handler = new refreshHandler(this);
            service.PutExtra("handler", new Messenger(handler));
            service.PutExtra("roomId", roomId);
            StartService(service);


            new Thread(new ThreadStart(delegate {
                roomUsersLV = FindViewById<ListView>(Resource.Id.roomUsersLV);
                

                StartTimeInput = FindViewById<Button>(Resource.Id.StartTimeInput);
                EndTimeInput = FindViewById<Button>(Resource.Id.EndTimeInput);
                StartTimeInput.Click += TimeSelectOnClick;
                EndTimeInput.Click += TimeSelectOnClick;

                submit = FindViewById<Button>(Resource.Id.submit);
                while (usersList == null)
                {
                    Thread.Sleep(100);
                }

                RunOnUiThread(() => {
                    submit.Click += submitAction;

                    loadingDialog.Hide();
                    loadingDialog.Dismiss();
                });

            })).Start();
        }

        public int getRoomId()
        {
            return roomId;
        }

        protected override void OnResume()
        {
            StartService(service);
            base.OnResume();
        }
        public void updateRoomList(List<RoomUser> recevedRoomUserslist)
        {
            if (usersList != null)
            {
                if (usersList.Count!=recevedRoomUserslist.Count)
                {
                    foreach (RoomUser curr_user in recevedRoomUserslist)
                    {
                        if (!usersList.Contains(curr_user))
                        {
                            usersList.Add(curr_user);
                        }
                    }
                    foreach (RoomUser curr_user in usersList.ToList())
                    {
                        if (!recevedRoomUserslist.Contains(curr_user))
                        {
                            usersList.Remove(curr_user);
                        }
                    }
                    usersList.Sort((x, y) => TimeSpan.Compare(x.startTime.TimeOfDay, y.startTime.TimeOfDay));
                    roomUserAdapter.updated();
                }
            }
            else
            {
                usersList = recevedRoomUserslist;
                roomUserAdapter = new RoomUserAdapter(this, usersList);
                roomUsersLV.Adapter = roomUserAdapter;

            }
        }

        async private void submitAction(object sender, EventArgs e)
        {
            String UserName = MainActivity.appUserName;
            String RoomName = MainActivity.roomList[pos].roomName;
            Socket secondSender = null;
            string toast = "";
            MainActivity.showLoadingScreen(this, ref loadingDialog);

            new Thread(new ThreadStart(delegate {
                try
                {
                    MainActivity.connectToServer(ref secondSender, MainActivity.IP, MainActivity.port);
                    // Connect Socket to the remote 
                    // endpoint using method Connect() 
                    MainActivity.sendString(secondSender, String.Format("Request {0} \"{1}\" {2} {3}", roomId, UserName, StartTimeInput.Text, EndTimeInput.Text));
                    String recvedString = MainActivity.recvString(secondSender);
                    toast = string.Format(recvedString);
                    MainActivity.close_conn(secondSender);
                }
                catch (Exception er)
                {
                    toast = er.ToString();
                }
                RunOnUiThread(() => {
                    loadingDialog.Hide();
                    loadingDialog.Dismiss();
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                });

            })).Start();
        }

        void TimeSelectOnClick(object sender, EventArgs eventArgs)
        {
            Button TimeInput = (Button)sender;
            TimePickerFragment frag = TimePickerFragment.NewInstance(
                delegate (DateTime time)
                {
                    TimeInput.Text = time.ToShortTimeString();
                });

            frag.Show(FragmentManager, TimePickerFragment.TAG);
        }

        protected override void OnStop()
        {
            StopService(service);
            base.OnStop();
        }

        public void onReceiveResult(Message message)
        {
            throw new NotImplementedException();
        }
    }

    [Obsolete]
    public class TimePickerFragment : DialogFragment, TimePickerDialog.IOnTimeSetListener
    {
        public static readonly string TAG = "MyTimePickerFragment";
        Action<DateTime> timeSelectedHandler = delegate { };
        DateTime randomTime = new DateTime(2019, 9, 4, 0, 0, 0);
        public static TimePickerFragment NewInstance(Action<DateTime> onTimeSelected)
        {
            TimePickerFragment frag = new TimePickerFragment();
            frag.timeSelectedHandler = onTimeSelected;
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currentTime = DateTime.Now;
            bool is24HourFormat = true;
            TimePickerDialog dialog = new TimePickerDialog
                (Activity, this, currentTime.Hour, currentTime.Minute, is24HourFormat);
            return dialog;
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            DateTime selectedTime = new DateTime(randomTime.Year, randomTime.Month, randomTime.Day, hourOfDay, minute, 0);
            timeSelectedHandler(selectedTime);
        }

    }

}