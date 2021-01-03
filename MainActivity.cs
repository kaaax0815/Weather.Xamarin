using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Plugin.Geolocator;
using Square.Picasso;
using Syncfusion.Android.ProgressBar;
using Syncfusion.SfPullToRefresh;
using Com.Syncfusion.Charts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using AlertDialog = Android.App.AlertDialog;
using System.Collections.ObjectModel;
using Android.Net;
using Android.Util;

namespace Weather.Xamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        readonly string key = "89f453dd00317568c5655dddece7f2a7";
        readonly string iqkey = "pk.0ced00bd926dbd6d1ea64941491f228a";
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
            FindViewById<Button>(Resource.Id.nointernet_retry).Click += Retry_Click;
            GetWeather();
        }

        private void Retry_Click(object sender, EventArgs e)
        {
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
            if (id == Resource.Id.action_refresh)
            {
                GetWeather();
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

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public async void GetWeather()
        {
            if (!IsOnline())
            {
                DisplayMetrics displayMetrics = new DisplayMetrics();
                LinearLayout nonetwork = FindViewById<LinearLayout>(Resource.Id.nonetwork_layout);
                nonetwork.Visibility = ViewStates.Visible;
                return;
            }
            else
            {
                FindViewById<LinearLayout>(Resource.Id.nonetwork_layout).Visibility = ViewStates.Gone;
            }
            // Progress Bar
            RelativeLayout relativeLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            SfLinearProgressBar sfLinearProgressBar = new SfLinearProgressBar(this)
            {
                LayoutParameters = new RelativeLayout.LayoutParams(
                this.Resources.DisplayMetrics.WidthPixels,
                this.Resources.DisplayMetrics.HeightPixels / 18),
                IsIndeterminate = true
            };
            relativeLayout.AddView(sfLinearProgressBar);
            string lat;
            string lon;
            try
            {
                // Get Current Location
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 50;
                var loc = await locator.GetPositionAsync();


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
                    lat = "0"; lon = "0";
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
                    // Send Error to Nopaste
                    FormUrlEncodedContent errorlog = new FormUrlEncodedContent(values);
                    HttpResponseMessage response_error = await client.PostAsync("https://nopaste.chaoz-irc.net/api/create", errorlog);
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
                // Reverse Geocoding
                WebRequest iqrequest = HttpWebRequest.Create("https://us1.locationiq.com/v1/reverse.php?key=" + iqkey + "&lat=" + lat.Replace(",", ".") + "&lon=" + lon.Replace(",", ".") + "&format=json");
                iqrequest.ContentType = "application/json";
                iqrequest.Method = "GET";
                using HttpWebResponse iqresponse = iqrequest.GetResponse() as HttpWebResponse;
                if (iqresponse.StatusCode != HttpStatusCode.OK)
                    Toast.MakeText(Application.Context, "Error fetching data. Server returned status code: " + iqresponse.StatusCode, ToastLength.Short).Show();
                using StreamReader iqreader = new StreamReader(iqresponse.GetResponseStream());
                var iqcontent = iqreader.ReadToEnd();
                var loc = JsonConvert.DeserializeObject<ReverseGeocoding>(iqcontent);
                // Weather Data
                WebRequest request = HttpWebRequest.Create("https://api.openweathermap.org/data/2.5/onecall?lat=" + lat + "&lon=" + lon + "&lang=" + lang + "&appid=" + key + "&units=metric");
                request.ContentType = "application/json";
                request.Method = "GET";
                using HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                    Toast.MakeText(Application.Context, "Error fetching data. Server returned status code: " + response.StatusCode, ToastLength.Short).Show();
                using StreamReader reader = new StreamReader(response.GetResponseStream());
                var content = reader.ReadToEnd();
                var i = JsonConvert.DeserializeObject<OneClickApi>(content);
                // Current Weather
                if (loc.address.city != null)
                    FindViewById<TextView>(Resource.Id.city_txt).Text = loc.address.city;
                else if (loc.address.village != null)
                    FindViewById<TextView>(Resource.Id.city_txt).Text = loc.address.village;
                DateTime thisDay = DateTime.Today;
                FindViewById<TextView>(Resource.Id.date_txt).Text = thisDay.ToString("D");
                string url = "https://openweathermap.org/img/wn/" + i.current.weather[0].icon + "@4x.png";
                Picasso.Get().Load(url).Into(FindViewById<ImageView>(Resource.Id.weather_img));
                FindViewById<TextView>(Resource.Id.temp_txt).Text = i.current.temp + "°C";
                FindViewById<TextView>(Resource.Id.feelslike_txt).Text = "Feels like: " + i.current.feels_like + "°C";
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                FindViewById<TextView>(Resource.Id.sunrise_txt).Text = dtDateTime.AddSeconds(i.current.sunrise).ToLocalTime().ToString("g");
                FindViewById<TextView>(Resource.Id.sunset_txt).Text = dtDateTime.AddSeconds(i.current.sunset).ToLocalTime().ToString("g");
                FindViewById<TextView>(Resource.Id.humidity_txt).Text = i.current.humidity + "%";
                FindViewById<TextView>(Resource.Id.pressure_txt).Text = i.current.pressure + "hPa";
                FindViewById<TextView>(Resource.Id.speed_txt).Text = i.current.wind_speed + "m/s";
                FindViewById<TextView>(Resource.Id.direction_txt).Text = i.current.wind_deg + "°";
                if (i.current.rain != null && i.current.rain._1h != 0.0)
                {
                    FindViewById<RelativeLayout>(Resource.Id.rain_layout).Visibility = ViewStates.Visible;
                    FindViewById<TextView>(Resource.Id.rain_txt).Text = " " + i.current.rain._1h + Resources.GetString(Resource.String.rain) + "1h";

                }
                else if (i.current.snow != null && i.current.snow._1h != 0.0)
                {
                    FindViewById<RelativeLayout>(Resource.Id.rain_layout).Visibility = ViewStates.Visible;
                    FindViewById<TextView>(Resource.Id.rain_txt).Text = " " + i.current.snow._1h + Resources.GetString(Resource.String.snow) + "1h";

                }
                // Forecast
                FindViewById<TextView>(Resource.Id.forecast1_date).Text = dtDateTime.AddSeconds(i.daily[1].dt).ToLocalTime().ToString("d");
                string forecast1_url = "https://openweathermap.org/img/wn/" + i.daily[1].weather[0].icon + "@4x.png";
                Picasso.Get().Load(forecast1_url).Fit().CenterCrop().Into(FindViewById<ImageView>(Resource.Id.forecast1_img));
                FindViewById<TextView>(Resource.Id.forecast1_max).Text = "Max: " + i.daily[1].temp.max.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast1_min).Text = "Min: " + i.daily[1].temp.min.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast1_pop).Text = i.daily[1].pop.ToString() + "%";
                FindViewById<TextView>(Resource.Id.forecast2_date).Text = dtDateTime.AddSeconds(i.daily[2].dt).ToLocalTime().ToString("d");
                string forecast2_url = "https://openweathermap.org/img/wn/" + i.daily[2].weather[0].icon + "@4x.png";
                Picasso.Get().Load(forecast2_url).Fit().CenterCrop().Into(FindViewById<ImageView>(Resource.Id.forecast2_img));
                FindViewById<TextView>(Resource.Id.forecast2_max).Text = "Max: " + i.daily[2].temp.max.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast2_min).Text = "Min: " + i.daily[2].temp.min.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast2_pop).Text = i.daily[2].pop.ToString() + "%";
                FindViewById<TextView>(Resource.Id.forecast3_date).Text = dtDateTime.AddSeconds(i.daily[3].dt).ToLocalTime().ToString("d");
                string forecast3_url = "https://openweathermap.org/img/wn/" + i.daily[3].weather[0].icon + "@4x.png";
                Picasso.Get().Load(forecast3_url).Fit().CenterCrop().Into(FindViewById<ImageView>(Resource.Id.forecast3_img));
                FindViewById<TextView>(Resource.Id.forecast3_max).Text = "Max: " + i.daily[3].temp.max.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast3_min).Text = "Min: " + i.daily[3].temp.min.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast3_pop).Text = i.daily[3].pop.ToString() + "%";
                FindViewById<TextView>(Resource.Id.forecast4_date).Text = dtDateTime.AddSeconds(i.daily[4].dt).ToLocalTime().ToString("d");
                string forecast4_url = "https://openweathermap.org/img/wn/" + i.daily[4].weather[0].icon + "@4x.png";
                Picasso.Get().Load(forecast4_url).Fit().CenterCrop().Into(FindViewById<ImageView>(Resource.Id.forecast4_img));
                FindViewById<TextView>(Resource.Id.forecast4_max).Text = "Max: " + i.daily[4].temp.max.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast4_min).Text = "Min: " + i.daily[4].temp.min.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast4_pop).Text = i.daily[4].pop.ToString() + "%";
                FindViewById<TextView>(Resource.Id.forecast5_date).Text = dtDateTime.AddSeconds(i.daily[5].dt).ToLocalTime().ToString("d");
                string forecast5_url = "https://openweathermap.org/img/wn/" + i.daily[5].weather[0].icon + "@4x.png";
                Picasso.Get().Load(forecast5_url).Fit().CenterCrop().Into(FindViewById<ImageView>(Resource.Id.forecast5_img));
                FindViewById<TextView>(Resource.Id.forecast5_max).Text = "Max: " + i.daily[5].temp.max.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast5_min).Text = "Min: " + i.daily[5].temp.min.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast5_pop).Text = i.daily[5].pop.ToString() + "%";
                FindViewById<TextView>(Resource.Id.forecast6_date).Text = dtDateTime.AddSeconds(i.daily[6].dt).ToLocalTime().ToString("d");
                string forecast6_url = "https://openweathermap.org/img/wn/" + i.daily[6].weather[0].icon + "@4x.png";
                Picasso.Get().Load(forecast6_url).Fit().CenterCrop().Into(FindViewById<ImageView>(Resource.Id.forecast6_img));
                FindViewById<TextView>(Resource.Id.forecast6_max).Text = "Max: " + i.daily[6].temp.max.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast6_min).Text = "Min: " + i.daily[6].temp.min.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast6_pop).Text = i.daily[6].pop.ToString() + "%";
                FindViewById<TextView>(Resource.Id.forecast7_date).Text = dtDateTime.AddSeconds(i.daily[7].dt).ToLocalTime().ToString("d");
                string forecast7_url = "https://openweathermap.org/img/wn/" + i.daily[7].weather[0].icon + "@4x.png";
                Picasso.Get().Load(forecast7_url).Fit().CenterCrop().Into(FindViewById<ImageView>(Resource.Id.forecast7_img));
                FindViewById<TextView>(Resource.Id.forecast7_max).Text = "Max: " + i.daily[7].temp.max.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast7_min).Text = "Min: " + i.daily[7].temp.min.ToString() + "°C";
                FindViewById<TextView>(Resource.Id.forecast7_pop).Text = i.daily[7].pop.ToString() + "%";
                // Chart
                SfChart chart = FindViewById<SfChart>(Resource.Id.sfChart1);
                //Initializing Primary Axis
                CategoryAxis primaryAxis = new CategoryAxis();
                chart.PrimaryAxis = primaryAxis;
                //Initializing Secondary Axis
                NumericalAxis secondaryAxis = new NumericalAxis();
                chart.SecondaryAxis = secondaryAxis;
                // Populate Temp Series
                ObservableCollection<TempChart> tempchart = new ObservableCollection<TempChart>
                {
                    new TempChart(dtDateTime.AddSeconds(i.daily[0].dt).ToLocalTime().ToString("d"), i.daily[0].temp.morn),
                    new TempChart(dtDateTime.AddSeconds(i.daily[0].dt).ToLocalTime().ToString("d"), i.daily[0].temp.day),
                    new TempChart(dtDateTime.AddSeconds(i.daily[0].dt).ToLocalTime().ToString("d"), i.daily[0].temp.eve),
                    new TempChart(dtDateTime.AddSeconds(i.daily[0].dt).ToLocalTime().ToString("d"), i.daily[0].temp.night),
                    new TempChart(dtDateTime.AddSeconds(i.daily[1].dt).ToLocalTime().ToString("d"), i.daily[1].temp.morn),
                    new TempChart(dtDateTime.AddSeconds(i.daily[1].dt).ToLocalTime().ToString("d"), i.daily[1].temp.day),
                    new TempChart(dtDateTime.AddSeconds(i.daily[1].dt).ToLocalTime().ToString("d"), i.daily[1].temp.eve),
                    new TempChart(dtDateTime.AddSeconds(i.daily[1].dt).ToLocalTime().ToString("d"), i.daily[1].temp.night),
                    new TempChart(dtDateTime.AddSeconds(i.daily[2].dt).ToLocalTime().ToString("d"), i.daily[2].temp.morn),
                    new TempChart(dtDateTime.AddSeconds(i.daily[2].dt).ToLocalTime().ToString("d"), i.daily[2].temp.day),
                    new TempChart(dtDateTime.AddSeconds(i.daily[2].dt).ToLocalTime().ToString("d"), i.daily[2].temp.eve),
                    new TempChart(dtDateTime.AddSeconds(i.daily[2].dt).ToLocalTime().ToString("d"), i.daily[2].temp.night),
                    new TempChart(dtDateTime.AddSeconds(i.daily[3].dt).ToLocalTime().ToString("d"), i.daily[3].temp.morn),
                    new TempChart(dtDateTime.AddSeconds(i.daily[3].dt).ToLocalTime().ToString("d"), i.daily[3].temp.day),
                    new TempChart(dtDateTime.AddSeconds(i.daily[3].dt).ToLocalTime().ToString("d"), i.daily[3].temp.eve),
                    new TempChart(dtDateTime.AddSeconds(i.daily[3].dt).ToLocalTime().ToString("d"), i.daily[3].temp.night),
                    new TempChart(dtDateTime.AddSeconds(i.daily[4].dt).ToLocalTime().ToString("d"), i.daily[4].temp.morn),
                    new TempChart(dtDateTime.AddSeconds(i.daily[4].dt).ToLocalTime().ToString("d"), i.daily[4].temp.day),
                    new TempChart(dtDateTime.AddSeconds(i.daily[4].dt).ToLocalTime().ToString("d"), i.daily[4].temp.eve),
                    new TempChart(dtDateTime.AddSeconds(i.daily[4].dt).ToLocalTime().ToString("d"), i.daily[4].temp.night),
                    new TempChart(dtDateTime.AddSeconds(i.daily[5].dt).ToLocalTime().ToString("d"), i.daily[5].temp.morn),
                    new TempChart(dtDateTime.AddSeconds(i.daily[5].dt).ToLocalTime().ToString("d"), i.daily[5].temp.day),
                    new TempChart(dtDateTime.AddSeconds(i.daily[5].dt).ToLocalTime().ToString("d"), i.daily[5].temp.eve),
                    new TempChart(dtDateTime.AddSeconds(i.daily[5].dt).ToLocalTime().ToString("d"), i.daily[5].temp.night),
                    new TempChart(dtDateTime.AddSeconds(i.daily[6].dt).ToLocalTime().ToString("d"), i.daily[6].temp.morn),
                    new TempChart(dtDateTime.AddSeconds(i.daily[6].dt).ToLocalTime().ToString("d"), i.daily[6].temp.day),
                    new TempChart(dtDateTime.AddSeconds(i.daily[6].dt).ToLocalTime().ToString("d"), i.daily[6].temp.eve),
                    new TempChart(dtDateTime.AddSeconds(i.daily[6].dt).ToLocalTime().ToString("d"), i.daily[6].temp.night),
                    new TempChart(dtDateTime.AddSeconds(i.daily[7].dt).ToLocalTime().ToString("d"), i.daily[7].temp.morn),
                    new TempChart(dtDateTime.AddSeconds(i.daily[7].dt).ToLocalTime().ToString("d"), i.daily[7].temp.day),
                    new TempChart(dtDateTime.AddSeconds(i.daily[7].dt).ToLocalTime().ToString("d"), i.daily[7].temp.eve),
                    new TempChart(dtDateTime.AddSeconds(i.daily[7].dt).ToLocalTime().ToString("d"), i.daily[7].temp.night)
                };
                AreaSeries tempseries = (new AreaSeries()
                {
                    ItemsSource = tempchart,
                    XBindingPath = "Date",
                    YBindingPath = "Temperature"
                });
                tempseries.TooltipEnabled = true;
                tempseries.Label = "Temp in °C";
                tempseries.VisibilityOnLegend = Visibility.Visible;
                tempseries.Color = Android.Graphics.Color.Red;
                chart.Series.Add(tempseries);
                ObservableCollection<RainChart> rainchart = new ObservableCollection<RainChart>
                {
                    new RainChart(dtDateTime.AddSeconds(i.daily[0].dt).ToLocalTime().ToString("d"), i.daily[0].rain + i.daily[0].snow),
                    new RainChart(dtDateTime.AddSeconds(i.daily[1].dt).ToLocalTime().ToString("d"), i.daily[1].rain + i.daily[1].snow),
                    new RainChart(dtDateTime.AddSeconds(i.daily[2].dt).ToLocalTime().ToString("d"), i.daily[2].rain + i.daily[2].snow),
                    new RainChart(dtDateTime.AddSeconds(i.daily[3].dt).ToLocalTime().ToString("d"), i.daily[3].rain + i.daily[3].snow),
                    new RainChart(dtDateTime.AddSeconds(i.daily[4].dt).ToLocalTime().ToString("d"), i.daily[4].rain + i.daily[4].snow),
                    new RainChart(dtDateTime.AddSeconds(i.daily[5].dt).ToLocalTime().ToString("d"), i.daily[5].rain + i.daily[5].snow),
                    new RainChart(dtDateTime.AddSeconds(i.daily[6].dt).ToLocalTime().ToString("d"), i.daily[6].rain + i.daily[6].snow),
                    new RainChart(dtDateTime.AddSeconds(i.daily[7].dt).ToLocalTime().ToString("d"), i.daily[7].rain + i.daily[7].snow)
                };
                AreaSeries rainseries = (new AreaSeries()
                {
                    ItemsSource = rainchart,
                    XBindingPath = "Date",
                    YBindingPath = "Rain"
                });
                rainseries.TooltipEnabled = true;
                rainseries.Label = "Rain and Snow in mm";
                rainseries.VisibilityOnLegend = Visibility.Visible;
                rainseries.Color = Android.Graphics.Color.Blue;
                chart.Legend.Visibility = Visibility.Visible;
                chart.Series.Add(rainseries);
                // Finished All Tasks --> Remove Loading Bar
                sfLinearProgressBar.Visibility = ViewStates.Gone;
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
                    // Send Error to Nopaste
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
    // Chart Data
    public class TempChart

    {

        public TempChart(string date, double temperature)
        {

            this.Date = date;

            this.Temperature = temperature;

        }

        public string Date { get; set; }

        public double Temperature { get; set; }

    }
    public class RainChart

    {

        public RainChart(string date, double rain)
        {

            this.Date = date;

            this.Rain = rain;

        }

        public string Date { get; set; }

        public double Rain { get; set; }

    }
    // Deserialization
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
        public double uvi { get; set; }
        public int clouds { get; set; }
        public int visibility { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public double wind_gust { get; set; }
        public List<Weather> weather { get; set; }
        public Rain rain { get; set; }
        public Snow snow { get; set; }
    }

    public class Snow
    {
        public double _1h { get; set; }
    }

    public class Rain
    {
        public double _1h { get; set; }
    }

    public class Minutely
    {
        public int dt { get; set; }
        public double precipitation { get; set; }
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

    public class OneClickApi
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
    // Reverse Geocoding
    public class Address
    {
        public string house_number { get; set; }
        public string museum { get; set; }
        public string road { get; set; }
        public string suburb { get; set; }
        public string village { get; set; }
        public string city_district { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string postcode { get; set; }
        public string country_code { get; set; }
    }

    public class ReverseGeocoding
    {
        public string place_id { get; set; }
        public string licence { get; set; }
        public string osm_type { get; set; }
        public string osm_id { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
        public Address address { get; set; }
        public List<string> boundingbox { get; set; }
    }
}
