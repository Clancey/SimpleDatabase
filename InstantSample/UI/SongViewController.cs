using System;
using MonoTouch.UIKit;

namespace InstantSample
{
	public class SongViewController : UITableViewController
	{
		public SongViewController ()
		{
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.TableView.Source = new SongViewModel ();
		}
	}
}

