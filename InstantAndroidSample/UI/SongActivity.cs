
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

namespace InstantSample
{
	[Activity (Label = "SongActivity", MainLauncher = true)]			
	public class SongActivity : ListActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			ListAdapter = new SongViewModel (this, ListView);
		}
	}
}

