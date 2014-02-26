using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Database;

using BuddySDK;

namespace AlbumsSample
{
	[Activity (Label = "Album Items")]			
	public class AlbumItemsActivity : Activity
	{
		private const int PickImageId = 1000;

		protected override async void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.AlbumItems);

			InitializeAddPictureButton ();

			await RefreshAlbumItemsGrid ();
		}

		private void InitializeAddPictureButton()
		{
			var addPictureButton = FindViewById<Button>(Resource.Id.addPictureButton);

			addPictureButton.Click += (object sender, EventArgs e) => 
			{
				var intent = new Intent();
				intent.SetType("image/*");
				intent.SetAction(Intent.ActionGetContent);
				StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), PickImageId);
			};
		}

		protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
			{
				string path, mimeType;
				GetPathToImage(data.Data, out path, out mimeType);

				await AddAlbumItem (path, mimeType);

				await RefreshAlbumItemsGrid ();
			}
		}

		private void GetPathToImage(Android.Net.Uri uri, out string path, out string mimeType)
		{
			path = null;
			mimeType = null;

			string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data,
											Android.Provider.MediaStore.Images.Media.InterfaceConsts.MimeType };

			using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
			{
				if (cursor != null)
				{
					cursor.MoveToFirst();
					int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
					path = cursor.GetString(columnIndex);

					columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.MimeType);
					mimeType = cursor.GetString(columnIndex);
				}
			}
		}

		private async Task AddAlbumItem(string path, string mimeType)
		{
			using (var fileStream = new FileStream (path, FileMode.Open)) {
				// Check stream for picture types other than JPEG
				var picture = await BuddySDK.Buddy.Pictures.AddAsync ("", fileStream, mimeType, null);

				await AlbumsActivity.SelectedAlbum.AddItemAsync (picture.Value.ID, "", null);
			}
		}

		private async Task RefreshAlbumItemsGrid()
		{
			var albumItems = await AlbumsActivity.SelectedAlbum.Items.FindAsync ();

			var gridview = FindViewById<GridView> (Resource.Id.albumItemsGridView);

			gridview.Adapter = new AlbumItemsAdapter (this, albumItems.PageResults.ToList());
		}
	}
}