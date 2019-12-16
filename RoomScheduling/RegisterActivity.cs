using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace RoomScheduling
{
    [Activity(Theme = "@style/AppTheme")]
    public class RegisterActivity : Activity
    {
        private static int UserName_Minimum_Length = 3;
        EditText userName;
        EditText password;
        EditText repeatPassword;
        EditText firstName;
        EditText familyName;
        EditText email;
        EditText phoneNumber;
        Button createAccountBtn;
        Button backToLoginBtn;
        Socket socket;
        ProgressDialog loadingDialog;
        EditText[] editTextArr;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Register);

            userName = FindViewById<EditText>(Resource.Id.userName);
            password = FindViewById<EditText>(Resource.Id.password);
            repeatPassword = FindViewById<EditText>(Resource.Id.repeatPassword);
            firstName = FindViewById<EditText>(Resource.Id.firstName);
            familyName = FindViewById<EditText>(Resource.Id.familyName);
            email = FindViewById<EditText>(Resource.Id.email);
            phoneNumber = FindViewById<EditText>(Resource.Id.phoneNumber);
            createAccountBtn = FindViewById<Button>(Resource.Id.createAccount);
            backToLoginBtn = FindViewById<Button>(Resource.Id.backToLogin);
            editTextArr = new EditText[] { userName, password , repeatPassword,firstName , familyName, email, phoneNumber};

            createAccountBtn.Click += createAccountClicked;
            backToLoginBtn.Click += backToLoginClicked;

        }
        void backToLoginClicked(object sender, EventArgs e)
        {
            StartActivity(new Intent(this, typeof(LoginActivity)));
            Finish();
        }
        public override void OnBackPressed()
        {
            StartActivity(new Intent(this, typeof(LoginActivity)));
            Finish();
        }
        void createAccountClicked(object sender, EventArgs e)
        {
            MainActivity.showLoadingScreen(this, ref loadingDialog);
            string toToast = "";
            bool newActivity = false;
            new Thread(new ThreadStart(delegate
            {
                foreach (var editText in editTextArr)
                {
                    editText.SetBackgroundColor(Color.White);
                }
                if (PasswordPolicy.IsValid(password.Text).CompareTo("password is valid") != 0)
                {
                    toToast = PasswordPolicy.IsValid(password.Text);
                    password.SetBackgroundColor(Color.Red);
                }
                else if (password.Text != repeatPassword.Text)
                {
                    toToast = "Passwords don't mutch";
                    repeatPassword.SetBackgroundColor(Color.Red);

                }
                else if (userName.Text.Length <= UserName_Minimum_Length)
                {
                    toToast = string.Format("User name must be more then {0} characters", UserName_Minimum_Length);
                    userName.SetBackgroundColor(Color.Red);
                }
                else if (IsValidEmail(email.Text) == false)
                {
                    toToast = "email address isn't valid";
                    email.SetBackgroundColor(Color.Red);
                }
                else if (IsPhoneNumber(phoneNumber.Text) == false)
                {
                    toToast = "phone number isn't valid";
                    phoneNumber.SetBackgroundColor(Color.Red);
                }
                else
                {
                    // check if the userName is all ready taken
                    MainActivity.connectToServer(ref socket, MainActivity.IP, MainActivity.port);
                    MainActivity.sendString(socket, string.Format("login \r\n{0}\r\n{1}<EOF>", userName.Text, ""));
                    string loginStatus = MainActivity.recvString(socket);
                    if (loginStatus.CompareTo("login failed \"there is'nt a user with that name\"") != 0)
                    {
                        toToast = "user name is all ready taken, maybe try to login?";
                    }
                    else
                    { // if all data is valid

                        MainActivity.sendString(socket, string.Format("register \r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}<EOF>", userName.Text, password.Text, firstName.Text, familyName.Text, email.Text, phoneNumber.Text));
                        string registerStatus = MainActivity.recvString(socket);
                        if (registerStatus.CompareTo("register accepted") == 0)
                        {
                            toToast = registerStatus;
                            Intent mainActivityIntent = new Intent(this, typeof(MainActivity));
                            mainActivityIntent.PutExtra("appUser-userName", userName.Text);
                            StartActivity(mainActivityIntent);
                            newActivity = true;
                        }
                    }
                    MainActivity.close_conn(socket);

                }
                RunOnUiThread(() => {
                    Toast.MakeText(this, toToast, ToastLength.Short).Show();

                    loadingDialog.Hide();
                    loadingDialog.Dismiss();
                    if (newActivity)
                    {
                        Finish();
                    }
                });

            })).Start();

        }
        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"^([0-9]{10})$").Success;
        }
        public override bool OnTouchEvent(MotionEvent e)
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(userName.WindowToken, 0);
            imm.HideSoftInputFromWindow(password.WindowToken, 0);
            imm.HideSoftInputFromWindow(repeatPassword.WindowToken, 0);
            imm.HideSoftInputFromWindow(firstName.WindowToken, 0);
            imm.HideSoftInputFromWindow(familyName.WindowToken, 0);
            imm.HideSoftInputFromWindow(email.WindowToken, 0);
            imm.HideSoftInputFromWindow(phoneNumber.WindowToken, 0);
            return base.OnTouchEvent(e);
        }
    }
    public class PasswordPolicy
    {
        private static int Minimum_Length = 5;
        private static int Upper_Case_length = 0;
        private static int Lower_Case_length = 0;
        private static int NonAlpha_length = 0;
        private static int Numeric_length = 0;

        public static string IsValid(string Password)
        {
            if (Password.Length < Minimum_Length)
                return string.Format("password must be more then {0} characters", Minimum_Length);
            if (UpperCaseCount(Password) < Upper_Case_length)
                return string.Format("password must have at lest {0} upper case letter", Upper_Case_length);
            if (LowerCaseCount(Password) < Lower_Case_length)
                return string.Format("password must have at lest {0} lower case letter", Lower_Case_length);
            if (NumericCount(Password) < Numeric_length)
                return string.Format("password must have at lest {0} numeric characters", Numeric_length);
            if (NonAlphaCount(Password) < NonAlpha_length)
                return string.Format("password must have at lest {0} non - alpha characters", NonAlpha_length);

            return "password is valid";
        }

        private static int UpperCaseCount(string Password)
        {
            return Regex.Matches(Password, "[A-Z]").Count;
        }

        private static int LowerCaseCount(string Password)
        {
            return Regex.Matches(Password, "[a-z]").Count;
        }
        private static int NumericCount(string Password)
        {
            return Regex.Matches(Password, "[0-9]").Count;
        }
        private static int NonAlphaCount(string Password)
        {
            return Regex.Matches(Password, @"[^0-9a-zA-Z\._]").Count;
        }
    }
}