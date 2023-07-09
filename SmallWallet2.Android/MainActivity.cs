using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Support.V4.App;
using Android;

namespace SmallWallet2.Droid 
{
    [Activity(Label = "SmallWallet2", Icon = "@mipmap/wallet_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (!(CheckPermissionGranted(Manifest.Permission.ReadExternalStorage) && !CheckPermissionGranted(Manifest.Permission.WriteExternalStorage) && !CheckPermissionGranted(Manifest.Permission.ManageExternalStorage)))
                {
                    RequestPermission();
                }
                if (!(CheckPermissionGranted(Manifest.Permission.Internet)))
                {
                    RequestPermission2();
                }
            }
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void RequestPermission()
        {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage, Manifest.Permission.ManageExternalStorage }, 0);
        }
        private void RequestPermission2()
        {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Internet}, 0);
        }
        public bool CheckPermissionGranted(string Permissions)
        {
            // Check if the permission is already available.
            if (ActivityCompat.CheckSelfPermission(this, Permissions) != Permission.Granted)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}