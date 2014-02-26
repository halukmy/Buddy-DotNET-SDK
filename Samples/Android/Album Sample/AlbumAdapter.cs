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

using BuddySDK;

namespace AlbumsSample
{
	public class AlbumAdapter : BaseAdapter<Album> {

		private readonly Activity context;
		private readonly IEnumerable<Album> albums;

		public AlbumAdapter(Activity c, IEnumerable<Album> a) : base() {
			context = c;
			albums = a;
		}

		public override long GetItemId(int position) {
			return position;
		}

		public override Album this[int position] {  
			get { return albums.Skip(position).First(); }
		}

		public override int Count {
			get { return albums.Count(); }
		}

		public override View GetView(int position, View view, ViewGroup parent)
		{
			if (view == null)
				view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);

			view.FindViewById<TextView> (Android.Resource.Id.Text1).Text = albums.Skip (position).First ().Name;

			return view;
		}
	}
}