using System;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using BuddySDK;

namespace AlbumsSample
{
	[Activity(Label = "Albums")]
    public class AlbumsActivity : Activity
    {
		private Button _createAlbumButton;
		private EditText _createAlbumEditText;
		private ListView _albumsListView;

		public static BuddySDK.Album SelectedAlbum;

		protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			SetContentView(Resource.Layout.Albums);

			InitializeCreateAlbumButton ();

			InitializeCreateAlbumText ();

			InitializeAlbumsListView ();

			await RefreshAlbumList ();
        }

		private void InitializeCreateAlbumButton()
		{
			_createAlbumButton = FindViewById<Button>(Resource.Id.createAlbumButton);

			_createAlbumButton.Click += async (sender, e) =>
			{
				await GetAlbums ();
				ResetAdapterControls ();
				await RefreshAlbumList ();
			};
		}

		private void InitializeCreateAlbumText()
		{
			_createAlbumEditText = FindViewById<EditText> (Resource.Id.createAlbumEditText);

			_createAlbumEditText.KeyPress += (sender, keyEventArgs) => {
				_createAlbumButton.Enabled = _createAlbumEditText.Text.Length > 0;

				keyEventArgs.Handled = false;
			};
		}

		private void InitializeAlbumsListView()
		{
			_albumsListView = FindViewById<ListView> (Resource.Id.albumsListView);

			_albumsListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {

				var item = ((AlbumAdapter)_albumsListView.Adapter)[e.Position];

				SelectedAlbum = (BuddySDK.Album)item;

				StartActivity(typeof(AlbumItemsActivity));
			};
		}

		private async Task GetAlbums()
		{
			await Buddy.Albums.AddAsync (_createAlbumEditText.Text, "", null);
		}

		private void ResetAdapterControls()
		{
			_createAlbumEditText.Text = "";
			_createAlbumButton.Enabled = false;
		}

		private async Task RefreshAlbumList()
		{
			var albums = await Buddy.Albums.FindAsync ();

			_albumsListView.Adapter = new AlbumAdapter (this, albums.PageResults);
		}
    }
}