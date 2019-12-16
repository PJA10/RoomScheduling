using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace RoomClasses
{
    [Table("AppUser")]
    [Serializable]
        public class AppUser
        {
            [PrimaryKey, Column("_userName")]
            public String userName { get; set; }
            public String password { get; set; }
            public String firstName { get; set; }
            public String lastName { get; set; }            
            public String email { get; set; }
            public String phoneNumber { get; set; }


        public AppUser() { }
            public AppUser(String userName, String password, String firstName, String lastName, String email, String phoneNumber)
            {
                this.userName = userName;
                this.password = password;
                this.phoneNumber = phoneNumber;
                this.email = email;
                this.firstName = firstName;
                this.lastName = lastName;
            }
        }
}