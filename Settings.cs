using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace Weather.Xamarin
{
    [Activity(Label = "Settings", Theme = "@style/AppTheme", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class Settings : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.content_settings);
            Button weatherapi = FindViewById<Button>(Resource.Id.weatherapi);
            Button locationapi = FindViewById<Button>(Resource.Id.locationapi);
            weatherapi.Click += Weatherapi_Click;
            locationapi.Click += Locationapi_Click;
        }

        private void Locationapi_Click(object sender, EventArgs e)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            View view = layoutInflater.Inflate(Resource.Layout.userinput_settings, null);
            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertbuilder.SetView(view);
            alertbuilder.SetTitle("Weather API Key");
            EditText userdata = view.FindViewById<EditText>(Resource.Id.dialogText);
            userdata.Hint = Preferences.Get("iqkey", "pk.0ced00bd926dbd6d1ea64941491f228a");
            alertbuilder.SetCancelable(false)
            .SetPositiveButton("Submit", delegate
            {
                Preferences.Set("iqkey", userdata.Text.ToString());
            })
            .SetNegativeButton("Cancel", delegate
            {
                alertbuilder.Dispose();
            });
            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.Show();
        }

        private void Weatherapi_Click(object sender, EventArgs e)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            View view = layoutInflater.Inflate(Resource.Layout.userinput_settings, null);
            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertbuilder.SetView(view);
            alertbuilder.SetTitle("Weather API Key");
            EditText userdata = view.FindViewById<EditText>(Resource.Id.dialogText);
            userdata.Hint = Preferences.Get("key", "89f453dd00317568c5655dddece7f2a7");
            alertbuilder.SetCancelable(false)
            .SetPositiveButton("Submit", delegate
            {
                Preferences.Set("key", userdata.Text.ToString());
            })
            .SetNegativeButton("Cancel", delegate
            {
                alertbuilder.Dispose();
            });
            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.Show();
        }
    }
}