using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using RoomClasses;
using System.Threading;
using Android.Views.InputMethods;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoomScheduling
{
    [Activity(Label = "@string/app_name", Theme = "@style/android:Theme.Holo.Light.NoActionBar", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        EditText userName;
        EditText password;
        Socket socket;
        ProgressDialog loadingDialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Login);

            //Initializing button from layout
            Button loginBtn = FindViewById<Button>(Resource.Id.login);
            Button registerBtn = FindViewById<Button>(Resource.Id.register);
            userName = FindViewById<EditText>(Resource.Id.userName);
            password = FindViewById<EditText>(Resource.Id.password);

            //Login button click action
            loginBtn.Click += loginClicked;
            registerBtn.Click += registerClicked;
        }
        void loginClicked(object sender, EventArgs e) {
            List<Room> roomsList = null;
            MainActivity.showLoadingScreen(this, ref loadingDialog);
            new Thread(new ThreadStart(delegate {
                MainActivity.connectToServer(ref socket, MainActivity.IP, MainActivity.port);
                MainActivity.sendString(socket, string.Format("login \r\n{0}\r\n{1}<EOF>", userName.Text, password.Text));
                String loginStatus = MainActivity.recvString(socket);
                if (loginStatus.CompareTo("login accepted") == 0)
                {
                    //roomsList = MainActivity.GET_rooms(socket);
                }
                MainActivity.close_conn(socket);


                RunOnUiThread(() => {
                    Toast.MakeText(this, loginStatus, ToastLength.Short).Show();

                    if (loginStatus.CompareTo("login accepted") == 0)
                    {
                        Intent mainActivityIntent = new Intent(this, typeof(MainActivity));
                        mainActivityIntent.PutExtra("appUser-userName", userName.Text);
                        //mainActivityIntent.PutExtra("room list", JsonConvert.SerializeObject(roomsList));
                        StartActivity(mainActivityIntent);
                        Finish();
                    }
                    loadingDialog.Hide();
                    loadingDialog.Dismiss();
                });

            })).Start();
            

        }


        void registerClicked(object sender, EventArgs e)
        {
            Intent registerActivityIntent = new Intent(this, typeof(RegisterActivity));
            StartActivity(registerActivityIntent);
            Finish();
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(userName.WindowToken, 0);
            imm.HideSoftInputFromWindow(password.WindowToken, 0);
            return base.OnTouchEvent(e);
        }
    }
}