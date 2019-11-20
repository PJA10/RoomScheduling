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
using static RoomClasses;

namespace RoomScheduling
{
    public class RoomUserAdapter : BaseAdapter<RoomUser>
    {
        Android.Content.Context context;
        List<RoomUser> objects;
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

            RoomUser temp = objects[position];
            if (temp != null)
            {
                nameTextView.Text = temp.userName;
                startTimeTV.Text = temp.startTime.ToShortTimeString();
                endTimeTV.Text = temp.endTime.ToShortTimeString();
            }
            return view;

        }
    }
}