// 
//  Copyright 2012  Xamarin Inc  (http://www.xamarin.com)
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Threading;

namespace MBProgressHudSample
{
	using MBProgressHUD;
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		DialogViewController dvc;
		MBProgressHUD progress;
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			dvc = new DialogViewController(CreateRoot());
			window.RootViewController = dvc;
			
			window.MakeKeyAndVisible ();
			progress = new MBProgressHUD();
			return true;
		}
		public RootElement CreateRoot()
		{
			return new RootElement("MBProgressHud"){
				new Section(){
					new StringElement("Start Timer",delegate{
						StartTimer(5);
					}),
				}
			};
		}
		public void StartTimer(double seconds)
		{
			progress.Mode = MBProgressHUDMode.Determinate;
			progress.TitleText = "Count down";
			progress.Show(true);
			ThreadPool.QueueUserWorkItem(delegate{
				int count = 0;
				while(count <= seconds)
				{
					Thread.Sleep(1000);
					progress.Progress = (float)(count/seconds);
					count ++;
				}
				progress.Hide(true);
			});
		}
	}
}

