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
using RoomClasses;
using Newtonsoft.Json;


namespace RoomScheduling
{
    [Serializable]
    
    public class refreshHandler : Handler
    {
        public AppReceiver appReceiver;
        public refreshHandler(RoomActivity receiver)
        {
            appReceiver = receiver;
        }
        public override void HandleMessage(Message msg)
        {
            base.HandleMessage(msg);
            if (msg.What == refreshService.STATUS_ROOM_LIST)
            {
                List<RoomUser> recevedRoomUserslist = (List<RoomUser>)helperClass.ByteArrayToObject((byte[])msg.Obj);
                appReceiver.updateRoomList(recevedRoomUserslist);
            }
        }

        public void updateRoomList(List<RoomUser> recevedRoomUserslist)
        {
            appReceiver.updateRoomList(recevedRoomUserslist);
        }
        public interface AppReceiver
        {
            void updateRoomList(List<RoomUser> recevedRoomUserslist);
        }
    }
}