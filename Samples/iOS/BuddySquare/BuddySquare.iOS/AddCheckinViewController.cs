using System;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using BuddySDK;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuddySquare.iOS
{
    public partial class AddCheckinViewController : UIViewController
    {

        UIImage _chosenImage;
        LocationsDataSource _dataSource;

        public AddCheckinViewController () : base ("AddCheckinViewController", null)
        {
        }

        public override void DidReceiveMemoryWarning ()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();
            
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Cancel);
            NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => {
                this.NavigationController.PopViewControllerAnimated(true);
            };
            NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Done);

            NavigationItem.RightBarButtonItem.Clicked += (sender, e) => {
                SaveCheckin();
            };

            mapView.DidUpdateUserLocation += (sender, e) => {
                if (mapView.UserLocation != null) {
                    CLLocationCoordinate2D coords = mapView.UserLocation.Coordinate;
                    UpdateMapLocation(coords);
                }
            };

            UITapGestureRecognizer doubletap = new UITapGestureRecognizer();
            doubletap.NumberOfTapsRequired = 1; // double tap
            doubletap.AddTarget (this, new MonoTouch.ObjCRuntime.Selector("ImageTapped"));
            imageView.AddGestureRecognizer(doubletap); 


            _dataSource = new LocationsDataSource (this);
            tableLocations.Source = _dataSource;

            txtComment.ShouldEndEditing += (tf) => {
                tf.ResignFirstResponder();
                return true;
            };



        }


        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            tableLocations.ReloadData ();
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            UpdateMapLocation (mapView.UserLocation.Location.Coordinate);
        }

        IEnumerable<Tuple<Location, BasicMapAnnotation>> _annotations;

        private void OnLocationsUpdate(IEnumerable<Location> locations) {


            if (_annotations != null) {
                var annotations = from c in _annotations
                                  select c.Item2;

                mapView.RemoveAnnotations (annotations.ToArray<NSObject> ());
                _annotations = null;

            }



            if (locations != null) {
                    var alist = new List<Tuple<Location, BasicMapAnnotation>> ();

                    foreach (var c in locations) {

                    var a = new BasicMapAnnotation(c.Location.ToCLLocation().Coordinate, c.Name,GetSubtitle(c));
                            mapView.AddAnnotation (a);
                            alist.Add(new Tuple<Location, BasicMapAnnotation>(c, a));
                    };
                    _annotations = alist;
            }
            tableLocations.ReloadData ();
        }

        private static string GetSubtitle(Location l) {
			return String.Format ("{2:0.00}km, {0}, {1}", l.City, l.Region, l.Distance / 1000.0);
        }
        Location _selected;
        private void OnLocationSelected (Location ci)
        {
            _selected = ci;
            var span = new MKCoordinateSpan(Utils.MilesToLatitudeDegrees(1.2), Utils.MilesToLongitudeDegrees(1.2, ci.Location.Latitude));
            mapView.Region = new MKCoordinateRegion(ci.Location.ToCLLocation().Coordinate, span);
        }

        [MonoTouch.Foundation.Export("ImageTapped")]
        public void ImageTapped (UIGestureRecognizer sender) {
            var imagePicker = new UIImagePickerController ();
            imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary | UIImagePickerControllerSourceType.SavedPhotosAlbum ;
            //imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes (UIImagePickerControllerSourceType.PhotoLibrary | UIImagePickerControllerSourceType.SavedPhotosAlbum);

            imagePicker.Canceled += (s, e) => {

                NavigationController.DismissViewController(true, null);
            };

         
            imagePicker.FinishedPickingMedia += (s2, e2) => {
                UIImage originalImage = e2.Info[UIImagePickerController.OriginalImage] as UIImage;

               
                _chosenImage = originalImage;
                imageView.Image = _chosenImage;
                NavigationController.DismissViewController(true, null);
            };



            NavigationController.PresentViewController (imagePicker, true, null);
        }

        void UpdateMapLocation (CLLocationCoordinate2D coords)
        {
            MKCoordinateSpan span = new MKCoordinateSpan(Utils.MilesToLatitudeDegrees(2), Utils.MilesToLongitudeDegrees(2, coords.Latitude));
            mapView.Region = new MKCoordinateRegion(coords, span);

            _dataSource.Update (coords);
        }

        private async void SaveCheckin() {


            var comment = txtComment.Text;


            var loc = mapView.UserLocation.Location.ToBuddyGeoLocation ();

			Action<Picture> finish = async (p) => {


                string photoID = null;

                if (p != null) {
                    photoID = p.ID;
                }

                // add the checkin

                if (_selected != null) {
                    loc = new BuddyGeoLocation(_selected.ID);
                }

                await Buddy.Checkins.AddAsync (comment, null, loc, photoID);

               

                this.NavigationController.PopViewControllerAnimated(true);

                PlatformAccess.Current.ShowActivity = false;

            };

            PlatformAccess.Current.ShowActivity = true;

            // if we have a photo save that first.
            //
            if (_chosenImage != null) {

                var bytes = _chosenImage.AsJPEG ();


				var result = await Buddy.Pictures.AddAsync (comment, bytes.AsStream (), "image/jpeg", loc);

                if (result.IsSuccess) {
                    finish (result.Value);
                }  
               
            } else {
                finish (null);
            }

        }

        private class LocationsDataSource : UITableViewSource {


            AddCheckinViewController _parent;


            IEnumerable<Location> _locations;
            private CLLocationCoordinate2D? _coords;

            public LocationsDataSource(AddCheckinViewController parent) {
                _parent = parent;
            }

            public void Clear() {
                _locations = null;

            }

            public void Update (CLLocationCoordinate2D coords)
            {
                Clear ();
                _coords = coords;
            }

            private async void LoadLocations() {

                if (_coords == null) {
                    return;
                }

                var r = await Buddy.Locations.FindAsync(locationRange: new BuddyGeoLocationRange(_coords.Value.Latitude,_coords.Value.Longitude, 3000));

                if (r.IsSuccess) {
                    _locations = r.PageResults;
                    _parent.OnLocationsUpdate (_locations);                
                }
            }

            private IEnumerable<Location> GetLocations() {

                if (_locations == null) {
                    LoadLocations ();
                    _locations = new Location[0];
                }
                return _locations;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            { 
                var l = GetLocations ();

                if (l == null)
                    return;
                    
                var ci = l.ElementAt (indexPath.Row);

                _parent.OnLocationSelected (ci);
                // tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
            }



            public override int RowsInSection (UITableView tableView, int section)
            {
                var t = GetLocations ();
               
                return t.Count ();
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                var t = GetLocations ();

                var ci = t.ElementAt (indexPath.Row);

                UITableViewCell cell = tableView.DequeueReusableCell ("NormalCell");
                // if there are no cells to reuse, create a new one
                if (cell == null)
                    cell = new UITableViewCell (UITableViewCellStyle.Subtitle, "NormalCell");
                cell.TextLabel.Text = ci.Name;
                cell.DetailTextLabel.Text = AddCheckinViewController.GetSubtitle (ci);

                return cell;

            }

           

    }
}
}

