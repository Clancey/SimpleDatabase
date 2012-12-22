Installation
================

Installation: Extract the .zip file and copy the *.dll file into your project directory. Then simply "Add Reference…" the the *.dll into your MonoTouch.


Using the Addon
================

	var hud = new MBProgressHUD (this.View.Window);
	hud.Mode = MBProgressHUDMode.Indeterminate;
	hud.TitleText = "Loading";
	hud.DetailText = "We'll be back shortly...";
	this.View.Window.AddSubview(hud);
	hud.Show (true);

Documentation
================

- Project Page: https://github.com/detroitpro/MBProgressHUD-MonoTouch/

Contact / Discuss
================

- Issue Tracker: https://github.com/detroitpro/MBProgressHUD-MonoTouch/issues