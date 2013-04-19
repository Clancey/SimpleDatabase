using System;
using MonoTouch.Dialog;
using System.Threading;
using MonoTouch.UIKit;
using ClanceysLib;

namespace InstantSample
{
	public class MainDialogViewController : DialogViewController
	{
		public MainDialogViewController () : base(null,false)
		{
			Root = CreateRoot ();
		}
		MBProgressHUD hud;
		RootElement CreateRoot()
		{
			return new RootElement ("Instant Database Sample"){
				new Section(){
					new StringElement("View Contacts",delegate{
						this.NavigationController.PushViewController(new ContactViewController(),true);
					}),
					new StringElement("View Songs",delegate{
						this.NavigationController.PushViewController(new SongViewController(),true);
					}), 
				},
				new Section("Populate database"){
					new StringElement("Add 100 people", delegate{
						insertPeople(100);
					}),
					new StringElement("Add 1000 people",delegate{
						insertPeople(1000);
					}),
				}
			};
		}
		void insertPeople(int numberOfPeople)
		{
			hud = new MBProgressHUD();
			hud.Show(true);
			NameGenerator.GetPeopleAsync(numberOfPeople).ContinueWith(t=>{
				if(t.Exception == null){
					Database.Main.InsertPeople(t.Result).ContinueWith(t2 =>{
						Database.Main.UpdateInstant<Person>();
						this.BeginInvokeOnMainThread(delegate{
							if(t2.Exception != null){
								Console.WriteLine(t2.Exception);
								(new UIAlertView("Error", "There was an error inserting people.",null,"Ok")).Show();
							}
							hud.Hide(true);
						});
					});
				}
				else
				{
					Console.WriteLine(t.Exception);
					this.BeginInvokeOnMainThread(delegate{
						(new UIAlertView("Error", "There was an error generating names.",null,"Ok")).Show();
						hud.Hide(true);
					});
				}
			});
		}
	}
}

