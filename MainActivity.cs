using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Square.Picasso;
using Syncfusion.Android.ProgressBar;
using Syncfusion.SfPullToRefresh;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using AlertDialog = Android.App.AlertDialog;

namespace Weather.Xamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        readonly string key = "89f453dd00317568c5655dddece7f2a7";
        readonly string lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        private static readonly HttpClient client = new HttpClient();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzc1OTk1QDMxMzgyZTM0MmUzMEpGUi96NGFrK2xrU0o2emJ1cHpmYm5mZkNqVEpQUEQ0MW1sbHNjUnJaZWs9");
            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            // SfPullToRefresh pullToRefresh = FindViewById<SfPullToRefresh>(Resource.Id.sfPullToRefresh1);
            // pullToRefresh.Refreshing += PullToRefresh_Refreshing;
            GetWeather();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
        private async void PullToRefresh_Refreshing(object sender, RefreshingEventArgs e)
        {
            GetWeather();
            await Task.Delay(1000);
            e.Refreshed = true;
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public async void GetWeather()
        {
            RelativeLayout relativeLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            SfLinearProgressBar sfLinearProgressBar = new SfLinearProgressBar(this)
            {
                LayoutParameters = new RelativeLayout.LayoutParams(
                this.Resources.DisplayMetrics.WidthPixels,
                this.Resources.DisplayMetrics.HeightPixels / 18),
                IsIndeterminate = true
            };
            relativeLayout.AddView(sfLinearProgressBar);
            string lat = "0";
            string lon = "0";
            try
            {
                GeolocationRequest locrequest = new GeolocationRequest(GeolocationAccuracy.Best);
                Location loc = await Geolocation.GetLocationAsync(locrequest);

                if (loc != null)
                {
                    // Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}
                    lat = loc.Latitude.ToString();
                    lon = loc.Longitude.ToString();
                }
                else
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Error while getting Location");
                    alert.SetMessage("There was an error getting your Location. Please retry");
                    alert.SetIcon(Resource.Drawable.main_warning);
                    alert.SetNeutralButton("OK", (senderAlert, args) =>
                    {
                        sfLinearProgressBar.Visibility = ViewStates.Gone;
                        return;
                    });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                }
            }
            catch (FeatureNotEnabledException)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Error while getting Location");
                alert.SetCancelable(false);
                alert.SetMessage("There was an error getting your Location. Please enable Location Services!");
                alert.SetIcon(Resource.Drawable.main_warning);
                alert.SetNeutralButton("OK", (senderAlert, args) =>
                {
                    sfLinearProgressBar.Visibility = ViewStates.Gone;
                    StartActivity(new Intent(Android.Provider.Settings.ActionLocationSourceSettings));
                    return;
                });
                Dialog dialog = alert.Create();
                dialog.Show();
                return;
            }
            catch (Exception ex)
            {
                try
                {
                    Dictionary<string, string> values = new Dictionary<string, string>
                    {
                        { "text", ex.ToString() },
                        { "private", "1" },
                        { "lang", "java" }
                    };

                    FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                    HttpResponseMessage response_error = await client.PostAsync("https://nopaste.chaoz-irc.net/api/create", content);
                    string response_error_String = await response_error.Content.ReadAsStringAsync();
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Error while getting Location");
                    alert.SetCancelable(false);
                    alert.SetMessage("There was an error getting your Location. Please share this url with Developers: \n" + response_error_String);
                    alert.SetIcon(Resource.Drawable.main_warning);
                    alert.SetNeutralButton("OK", (senderAlert, args) =>
                    {
                        sfLinearProgressBar.Visibility = ViewStates.Gone;
                        return;
                    });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                }
                catch (Exception excep)
                {
                    Toast.MakeText(ApplicationContext, "Error while making Error and uploading Log: " + excep.ToString(), ToastLength.Long).Show();
                    sfLinearProgressBar.Visibility = ViewStates.Gone;
                    return;
                }
                return;
            }
            try
            {
                Task task = client.GetAsync("https://api.openweathermap.org/data/2.5/onecall?lat=" + lat + "&lon=" + lon + "&appid=" + key + "&lang" + lang + "&unit=metric")
                  .ContinueWith((taskwithresponse) =>
                  {
                      HttpResponseMessage response = taskwithresponse.Result;
                      Task<string> jsonString = response.Content.ReadAsStringAsync();
                      if (!jsonString.IsFaulted)
                      {
                          jsonString.Wait();
                          Root i;
                          i = JsonConvert.DeserializeObject<Root>(jsonString.Result);
                          FindViewById<TextView>(Resource.Id.city_txt).Text = i.lat + " " + i.lon;
                          DateTime thisDay = DateTime.Today;
                          FindViewById<TextView>(Resource.Id.date_txt).Text = thisDay.ToString("D");
                          string url = "https://openweathermap.org/img/wn/" + i.current.weather[0].icon + "@4x.png";
                          Picasso.Get().Load(url).Into(FindViewById<ImageView>(Resource.Id.weather_img));
                          FindViewById<TextView>(Resource.Id.temp_txt).Text = i.current.temp + "°C";
                          FindViewById<TextView>(Resource.Id.feelslike_txt).Text = "Feels like: " + i.current.feels_like + "°C";
                          FindViewById<TextView>(Resource.Id.sunrise_txt).Text = DateTime.Parse(i.current.sunrise.ToString(), null, DateTimeStyles.AssumeUniversal).ToString("g");
                          FindViewById<TextView>(Resource.Id.sunset_txt).Text = DateTime.Parse(i.current.sunrise.ToString(), null, DateTimeStyles.AssumeUniversal).ToString("g");
                          FindViewById<TextView>(Resource.Id.humidity_txt).Text = i.current.humidity + "%";
                          FindViewById<TextView>(Resource.Id.pressure_txt).Text = i.current.pressure + "hPa";
                          FindViewById<TextView>(Resource.Id.speed_txt).Text = i.current.wind_speed + "m/s";
                          FindViewById<TextView>(Resource.Id.direction_txt).Text = i.current.wind_deg + "°";
                          if (i.current.rain._1h.GetValueOrDefault() != 0)
                          {
                              FindViewById<RelativeLayout>(Resource.Id.rain_layout).Visibility = ViewStates.Visible;
                              FindViewById<TextView>(Resource.Id.rain_txt).Text = " " + i.current.rain._1h + Resources.GetString(Resource.String.rain) + "1h";

                          }
                          else if (i.current.rain._3h.GetValueOrDefault() != 0)
                          {
                              FindViewById<RelativeLayout>(Resource.Id.rain_layout).Visibility = ViewStates.Visible;
                              FindViewById<TextView>(Resource.Id.rain_txt).Text = " " + i.current.rain._3h + Resources.GetString(Resource.String.rain) + "3h";

                          }
                          else if (i.current.snow._1h.GetValueOrDefault() != 0)
                          {
                              FindViewById<RelativeLayout>(Resource.Id.rain_layout).Visibility = ViewStates.Visible;
                              FindViewById<TextView>(Resource.Id.rain_txt).Text = " " + i.current.snow._1h + Resources.GetString(Resource.String.snow) + "1h";

                          }
                          else if (i.current.snow._3h.GetValueOrDefault() != 0)
                          {
                              FindViewById<RelativeLayout>(Resource.Id.rain_layout).Visibility = ViewStates.Visible;
                              FindViewById<TextView>(Resource.Id.rain_txt).Text = " " + i.current.snow._3h + Resources.GetString(Resource.String.snow) + "3h";

                          }
                          sfLinearProgressBar.Visibility = ViewStates.Gone;
                      }
                  });
                task.Wait();
            }
            catch (Exception except)
            {
                sfLinearProgressBar.Visibility = ViewStates.Gone;
                try
                {
                    Dictionary<string, string> values = new Dictionary<string, string>
                    {
                        { "text", except.ToString() },
                        { "private", "1" },
                        { "lang", "java" }
                    };

                    FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                    HttpResponseMessage response_error = await client.PostAsync("https://nopaste.chaoz-irc.net/api/create", content);
                    string response_error_String = await response_error.Content.ReadAsStringAsync();
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Error while making Request and setting Values");
                    alert.SetCancelable(false);
                    alert.SetMessage("Error while making Request and setting Values. Please share this url with Developers: \n" + response_error_String);
                    alert.SetIcon(Resource.Drawable.main_warning);
                    alert.SetNeutralButton("OK", (senderAlert, args) =>
                    {
                        sfLinearProgressBar.Visibility = ViewStates.Gone;
                        return;
                    });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                }
                catch (Exception excep)
                {
                    Toast.MakeText(ApplicationContext, "Error while making Error and uploading Log: " + excep.ToString(), ToastLength.Long).Show();
                    sfLinearProgressBar.Visibility = ViewStates.Gone;
                    return;
                }
                return;
            }
        }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Current
    {
        public int dt { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public int uvi { get; set; }
        public int clouds { get; set; }
        public int visibility { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public double wind_gust { get; set; }
        public List<Weather> weather { get; set; }
        public Rain rain { get; set; }
        public Snow snow { get; set; }
    }

    public class Rain
    {
        public double? _1h { get; set; }
        public double? _3h { get; set; }
    }
    public class Snow
    {
        public double? _1h { get; set; }
        public double? _3h { get; set; }
    }

    public class Minutely
    {
        public int dt { get; set; }
        public int precipitation { get; set; }
    }

    public class Weather2
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Hourly
    {
        public int dt { get; set; }
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public double uvi { get; set; }
        public int clouds { get; set; }
        public int visibility { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public List<Weather2> weather { get; set; }
        public double pop { get; set; }
        public Snow snow { get; set; }
    }

    public class Temp
    {
        public double day { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public double night { get; set; }
        public double eve { get; set; }
        public double morn { get; set; }
    }

    public class FeelsLike
    {
        public double day { get; set; }
        public double night { get; set; }
        public double eve { get; set; }
        public double morn { get; set; }
    }

    public class Weather3
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Daily
    {
        public int dt { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public Temp temp { get; set; }
        public FeelsLike feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public List<Weather3> weather { get; set; }
        public int clouds { get; set; }
        public double pop { get; set; }
        public double rain { get; set; }
        public double snow { get; set; }
        public double uvi { get; set; }
    }

    public class Alert
    {
        public string sender_name { get; set; }
        public string @event { get; set; }
        public int start { get; set; }
        public int end { get; set; }
        public string description { get; set; }
    }

    public class Root
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public int timezone_offset { get; set; }
        public Current current { get; set; }
        public List<Minutely> minutely { get; set; }
        public List<Hourly> hourly { get; set; }
        public List<Daily> daily { get; set; }
        public List<Alert> alerts { get; set; }
    }


}
