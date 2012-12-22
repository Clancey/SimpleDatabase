Quick and easy modal progress view!

[code:csharp] 

var hud = new MBProgressHUD (this.View.Window);
hud.Mode = MBProgressHUDMode.Indeterminate;
hud.TitleText = "Loading";
hud.DetailText = "We'll be back shortly...";
this.View.Window.AddSubview(hud);
hud.Show (true);

[code] 

![MbProgressHud](screen-1.png)
