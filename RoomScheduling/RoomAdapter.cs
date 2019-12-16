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

namespace RoomScheduling
{
    public class RoomAdapter : BaseAdapter<Room>
    {
        Android.Content.Context context;
        List<Room> objects;
        public RoomAdapter(Android.Content.Context context, System.Collections.Generic.List<Room> objects)
        {
            this.context = context;
            this.objects = objects;
        }
        public List<Room> GetList()
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
        public override Room this[int position]
        {

            get { return this.objects[position]; }

        }

        public void updated()
        {
            NotifyDataSetChanged();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)

        {

            Android.Views.LayoutInflater layoutInflater = ((MainActivity)context).LayoutInflater;

            Android.Views.View view = layoutInflater.Inflate(Resource.Layout.RoomListItem, parent, false);

            TextView roomNameTextView = view.FindViewById<TextView>(Resource.Id.roomName);

            Room temp = objects[position];
            if (temp != null)
            {
                roomNameTextView.Text = temp.roomName;
            }
            return view;

        }
    }
}