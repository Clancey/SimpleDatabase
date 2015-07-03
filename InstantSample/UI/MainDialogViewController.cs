using System;
using MonoTouch.Dialog;
using System.Threading;
using UIKit;
using System.Threading.Tasks;

namespace InstantSample
{
	public class MainDialogViewController : DialogViewController
	{
		public MainDialogViewController () : base (null, false)
		{
			Root = CreateRoot ();
		}

		RootElement CreateRoot ()
		{
			return new RootElement ("Instant Database Sample") {
				new Section () {
					new StringElement ("View Contacts", delegate {
						this.NavigationController.PushViewController (new ContactViewController (), true);
					}),
					new StringElement ("View Songs", delegate {
						this.NavigationController.PushViewController (new SongViewController (), true);
					}), 
				},
				new Section ("Populate database") {
					new StringElement ("Add 100 people", delegate {
						insertPeople (100);
					}),
					new StringElement ("Add 1000 people", delegate {
						insertPeople (1000);
					}),
				}
			};
		}

		async Task insertPeople (int numberOfPeople)
		{
			BigTed.BTProgressHUD.Show ();
			try {
				var people = await NameGenerator.GetPeopleAsync (numberOfPeople);
				var records = await Database.Main.InsertAllAsync (people);
				Database.Main.UpdateInstant<Person> ();
			} catch (Exception ex) {
				(new UIAlertView ("Error", "There was an error inserting people.", null, "Ok")).Show ();
			} finally {
				BigTed.BTProgressHUD.Dismiss ();

			}
		}
	}
}

