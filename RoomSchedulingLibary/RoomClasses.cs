﻿using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using SQLite;
using Newtonsoft.Json;


namespace RoomClasses
{
    [Table("RoomUsers")]
    [Serializable]
    public class RoomUser
    {
        [PrimaryKey, AutoIncrement, Column("_id")]

        public int Id { get; set; }
        public int roomId { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string userName { get; set; }
        public string phoneNumber { get; set; }
        

        public RoomUser() { }
        public RoomUser(int roomId, string userName, DateTime startTime, DateTime endTime, string phoneNumber)
        {
            this.userName = userName;
            this.startTime = startTime;
            this.endTime = endTime;
            this.roomId = roomId;
            this.phoneNumber = phoneNumber;
        }

        public static bool isValidUserData(string userName, DateTime startTime, DateTime endTime)
        {
            /*Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (r.IsMatch(userName))
            {
                return false;
            }*/
            if (DateTime.Compare(startTime, endTime) >= 0)
            {
                return false;
            }
            return true;
        }

        public String ToString2()
        {
            return (String.Format("{0} at {1} - {2}", userName, startTime.ToShortTimeString(), endTime.ToShortTimeString()));
        }
        public bool Equals(RoomUser other) 
        {
            if (userName.CompareTo(other.userName) != 0)
            {
                return false;
            }
            if (roomId.CompareTo(other.roomId) != 0)
            {
                return false;
            }
            if (DateTime.Compare(startTime, other.startTime) != 0)
            {
                return false;
            }
            if (DateTime.Compare(endTime, other.endTime) != 0)
            {
                return false;
            }
            if (phoneNumber.CompareTo(other.phoneNumber) != 0)
            {
                return false;
            }
            return true;
        }
        public override bool Equals(object other)
        {
            return this.Equals((RoomUser)other);
        }

        public override int GetHashCode() => (userName, startTime, endTime).GetHashCode();

    }



    [Table("Rooms")]
    [Serializable]
    public class Room
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        //[JsonProperty("Id")]
        public int Id { get; set; }
        //[JsonProperty("roomName")]
        public string roomName { get; set; }

        public Room()
        {

        }
        public Room(string roomName)
        {
            this.roomName = roomName;
        }
        
        public override String ToString()
        {
            String s = "Room: " + roomName  + "\n";
            return s;
        }
        public bool Equals(Room other) 
        {
            if (roomName.CompareTo(other.roomName) != 0)
            {
                return false;
            }
            return true;
        }
        public override bool Equals(object other)
        {
            return this.Equals((Room)other);
        }
        public override int GetHashCode() => (roomName).GetHashCode();
    }
    public static class helperClass
    {
        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            if (arrBytes.Length < 2)
            {
                return null;
            }
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj2 = binForm.Deserialize(memStream);
            Object obj = (Object)obj2;

            return obj;
        }
    }
}
