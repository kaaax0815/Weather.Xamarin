using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Essentials;
using Syncfusion.SfPullToRefresh;
using Syncfusion.Android.ProgressBar;
using AlertDialog = Android.App.AlertDialog;
using System.Net.Http;
using System.Collections.Generic;
using Android.Text;
using Android.Support.V4.Text;
using Android.Content;
using Android.Support.V7.Widget;

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
            SfPullToRefresh pullToRefresh = FindViewById<SfPullToRefresh>(Resource.Id.sfPullToRefresh1);
            pullToRefresh.Refreshing += PullToRefresh_Refreshing;
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
            SfLinearProgressBar sfLinearProgressBar = new SfLinearProgressBar(this);
            sfLinearProgressBar.LayoutParameters = new RelativeLayout.LayoutParams(
                this.Resources.DisplayMetrics.WidthPixels,
                this.Resources.DisplayMetrics.HeightPixels / 18);
            sfLinearProgressBar.IsIndeterminate = true;
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
                    var values = new Dictionary<string, string>
                    {
                    { "text", ex.ToString() },
                    { "private", "1" }
                    };

                    var content = new FormUrlEncodedContent(values);
                    var response_error = await client.PostAsync("https://nopaste.chaoz-irc.net/api/create", content);
                    var response_error_String = await response_error.Content.ReadAsStringAsync();
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
                    Toast.MakeText(ApplicationContext, excep.ToString(), ToastLength.Long).Show();
                    sfLinearProgressBar.Visibility = ViewStates.Gone;
                    return;
                }
                return;
            }
            try
            {
                WebRequest request = HttpWebRequest.Create("https://api.openweathermap.org/data/2.5/weather?lat=" + lat + "&lon=" + lon + "&lang=" + lang + "&appid=" + key + "&mode=xml&units=metric");
                request.ContentType = "application/xml";
                request.Method = "GET";
                using HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                    Toast.MakeText(Application.Context, "Error fetching data. Server returned status code: " + response.StatusCode, ToastLength.Short).Show();
                XmlSerializer serializer = new XmlSerializer(typeof(Current));
                Current i;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    i = (Current)serializer.Deserialize(reader);
                }
                FindViewById<TextView>(Resource.Id.city_txt).Text = i.City.Name;
                DateTime thisDay = DateTime.Today;
                FindViewById<TextView>(Resource.Id.date_txt).Text = thisDay.ToString("D");
                string url = "https://openweathermap.org/img/wn/" + i.Weather.Icon + "@4x.png";
                Picasso.Get().Load(url).Into(FindViewById<ImageView>(Resource.Id.weather_img));
                FindViewById<TextView>(Resource.Id.temp_txt).Text = i.Temperature.Value + "°C";
                FindViewById<TextView>(Resource.Id.feelslike_txt).Text = "Feels like: " + i.Feels_like.Value + "°C";
                FindViewById<TextView>(Resource.Id.sunrise_txt).Text = DateTime.Parse(i.City.Sun.Rise, null, DateTimeStyles.AssumeUniversal).ToString("g");
                FindViewById<TextView>(Resource.Id.sunset_txt).Text = DateTime.Parse(i.City.Sun.Set, null, DateTimeStyles.AssumeUniversal).ToString("g");
                FindViewById<TextView>(Resource.Id.humidity_txt).Text = i.Humidity.Value + i.Humidity.Unit;
                FindViewById<TextView>(Resource.Id.pressure_txt).Text = i.Pressure.Value + i.Pressure.Unit;
                FindViewById<TextView>(Resource.Id.speed_txt).Text = i.Wind.Speed.Value + i.Wind.Speed.Unit;
                FindViewById<TextView>(Resource.Id.direction_txt).Text = i.Wind.Direction.Value + " " + i.Wind.Direction.Code;
                FindViewById<TextView>(Resource.Id.lastupdate_txt).Text = "Last Updated: " + DateTime.Parse(i.Lastupdate.Value, null, DateTimeStyles.AssumeUniversal).ToString("g");
                if (i.Precipitation.Mode != "no")
                {
                    FindViewById<RelativeLayout>(Resource.Id.rain_layout).Visibility = ViewStates.Visible;
                    FindViewById<TextView>(Resource.Id.rain_txt).Text = " " + i.Precipitation.Value.AsSpan(0, 4).ToString() + Resources.GetString(Resource.String.rain) + i.Precipitation.Unit;

                }
                sfLinearProgressBar.Visibility = ViewStates.Gone;
            }
            catch (Exception except)
            {
                Toast.MakeText(ApplicationContext, except.ToString(), ToastLength.Long).Show();
                sfLinearProgressBar.Visibility = ViewStates.Gone;
                return;
            }
        }
    }
    [XmlRoot(ElementName = "coord")]
    public class Coord
    {
        [XmlAttribute(AttributeName = "lon")]
        public string Lon { get; set; }
        [XmlAttribute(AttributeName = "lat")]
        public string Lat { get; set; }
    }

    [XmlRoot(ElementName = "sun")]
    public class Sun
    {
        [XmlAttribute(AttributeName = "rise")]
        public string Rise { get; set; }
        [XmlAttribute(AttributeName = "set")]
        public string Set { get; set; }
    }

    [XmlRoot(ElementName = "city")]
    public class City
    {
        [XmlElement(ElementName = "coord")]
        public Coord Coord { get; set; }
        [XmlElement(ElementName = "country")]
        public string Country { get; set; }
        [XmlElement(ElementName = "timezone")]
        public string Timezone { get; set; }
        [XmlElement(ElementName = "sun")]
        public Sun Sun { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "temperature")]
    public class Temperature
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "min")]
        public string Min { get; set; }
        [XmlAttribute(AttributeName = "max")]
        public string Max { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
    }

    [XmlRoot(ElementName = "feels_like")]
    public class Feels_like
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
    }

    [XmlRoot(ElementName = "humidity")]
    public class Humidity
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
    }

    [XmlRoot(ElementName = "pressure")]
    public class Pressure
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
    }

    [XmlRoot(ElementName = "speed")]
    public class Speed
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "direction")]
    public class Direction
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "wind")]
    public class Wind
    {
        [XmlElement(ElementName = "speed")]
        public Speed Speed { get; set; }
        [XmlElement(ElementName = "gusts")]
        public string Gusts { get; set; }
        [XmlElement(ElementName = "direction")]
        public Direction Direction { get; set; }
    }

    [XmlRoot(ElementName = "clouds")]
    public class Clouds
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "visibility")]
    public class Visibility
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "precipitation")]
    public class Precipitation
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "mode")]
        public string Mode { get; set; }
        [XmlAttribute(AttributeName = "unit")]
        public string Unit { get; set; }
    }

    [XmlRoot(ElementName = "weather")]
    public class Weather
    {
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "icon")]
        public string Icon { get; set; }
    }

    [XmlRoot(ElementName = "lastupdate")]
    public class Lastupdate
    {
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "current")]
    public class Current
    {
        [XmlElement(ElementName = "city")]
        public City City { get; set; }
        [XmlElement(ElementName = "temperature")]
        public Temperature Temperature { get; set; }
        [XmlElement(ElementName = "feels_like")]
        public Feels_like Feels_like { get; set; }
        [XmlElement(ElementName = "humidity")]
        public Humidity Humidity { get; set; }
        [XmlElement(ElementName = "pressure")]
        public Pressure Pressure { get; set; }
        [XmlElement(ElementName = "wind")]
        public Wind Wind { get; set; }
        [XmlElement(ElementName = "clouds")]
        public Clouds Clouds { get; set; }
        [XmlElement(ElementName = "visibility")]
        public Visibility Visibility { get; set; }
        [XmlElement(ElementName = "precipitation")]
        public Precipitation Precipitation { get; set; }
        [XmlElement(ElementName = "weather")]
        public Weather Weather { get; set; }
        [XmlElement(ElementName = "lastupdate")]
        public Lastupdate Lastupdate { get; set; }
    }
}
