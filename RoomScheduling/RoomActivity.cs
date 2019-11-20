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
using static RoomClasses;
using Android.Support.V7.App;
using System.Threading.Tasks;

namespace RoomScheduling
{
    [Activity(Label = "RoomActivity")]
    class RoomActivity : AppCompatActivity
    {
        List<RoomUser> usersList;
        Button StartTimeInput;
        Button EndTimeInput;
        Button submit;
        EditText NameInput;
        int pos = -1;
        Timer myTimer;
        Socket sender = null;
        RoomUserAdapter roomUserAdapter = null;
        ListView roomUsersLV;
        int roomId;
        ProgressDialog loadingDialog;
        //

        async protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.room_main);

            showLoadingScreen();
            await Task.Run(() => RunOnUiThread(() =>
            {
                pos = Intent.GetIntExtra("pos", -1);
                roomId = MainActivity.roomList[pos].Id;

                MainActivity.connectToServer(ref sender, MainActivity.IP, MainActivity.port);
                GET_users();
                roomUserAdapter = new RoomUserAdapter(this, usersList);
                roomUsersLV = FindViewById<ListView>(Resource.Id.roomUsersLV);
                roomUsersLV.Adapter = roomUserAdapter;

                StartTimeInput = FindViewById<Button>(Resource.Id.StartTimeInput);
                EndTimeInput = FindViewById<Button>(Resource.Id.EndTimeInput);
                StartTimeInput.Click += TimeSelectOnClick;
                EndTimeInput.Click += TimeSelectOnClick;

                NameInput = FindViewById<EditText>(Resource.Id.NameInput);


                submit = FindViewById<Button>(Resource.Id.submit);
                submit.Click += submitAction;

                myTimer = new Timer();
                myTimer.Elapsed += new ElapsedEventHandler(TimeUp);
                myTimer.Interval = 10000;
                myTimer.Start();
            }));
            loadingDialog.Hide();
            loadingDialog.Dismiss();
        }

        private void showLoadingScreen()
        {
            loadingDialog = ProgressDialog.Show(this, "just a sec", "Loading please wait...", true);
            loadingDialog.SetCancelable(true);
            loadingDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);

            loadingDialog.SetMessage("Loading...");

            loadingDialog.Show();
        }

        async private void submitAction(object sender, EventArgs e)
        {
            String UserName = NameInput.Text;
            String RoomName = MainActivity.roomList[pos].roomName;
            Socket secondSender = null;
            MainActivity.connectToServer(ref secondSender, MainActivity.IP, MainActivity.port);

            try
            {

                // Connect Socket to the remote 
                // endpoint using method Connect() 
                MainActivity.sendString(secondSender, String.Format("Request {0} \"{1}\" {2} {3}", roomId, UserName, StartTimeInput.Text, EndTimeInput.Text));
                String recvedString = MainActivity.recvString(secondSender);
                string toast = string.Format(recvedString);
                Toast.MakeText(this, toast, ToastLength.Long).Show();
                MainActivity.close_conn(secondSender);
                GET_users();

            }
            catch (Exception error)
            {
                Toast.MakeText(this, error.ToString(), ToastLength.Long).Show(); 
            }


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
        public override bool OnTouchEvent(MotionEvent e)
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(NameInput.WindowToken, 0);
            return base.OnTouchEvent(e);
        }
        public void TimeUp(object source, ElapsedEventArgs e)
        {
            GET_users();
        }

        private void GET_users()
        {

            List<RoomUser> recevedRoomUserslist = (List<RoomUser>)MainActivity.GET(sender, "GET " + roomId.ToString() + "<EOF>");
            if (usersList != null)
            {
                if (!Enumerable.SequenceEqual(usersList, recevedRoomUserslist))
                {
                    foreach (RoomUser curr_user in recevedRoomUserslist)
                    {
                        if (!usersList.Contains(curr_user))
                        {
                            usersList.Add(curr_user);
                        }
                    }
                    roomUserAdapter.updated();
                }
                
            }
            else
            {
                usersList = recevedRoomUserslist;
            }
        }
        protected override void OnStop()
        {
            myTimer.Stop();
            MainActivity.close_conn(sender);
            base.OnStop();
        }
    }
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