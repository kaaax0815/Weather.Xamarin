using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using System.Net;
using Xamarin.Essentials;

namespace Weather.Xamarin
{
    [Service]
    public class UpdateService : Service
    {
        [System.Obsolete]
        public override void OnStart(Intent intent, int startId)
		{
			// Build the widget update for today
			RemoteViews updateViews = BuildUpdate(this);

			// Push update for this widget to the home screen
			ComponentName thisWidget = new ComponentName(this, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
			AppWidgetManager manager = AppWidgetManager.GetInstance(this);
			manager.UpdateAppWidget(thisWidget, updateViews);
		}

		public override IBinder OnBind(Intent intent)
		{
			// We don't need to bind to this service
			return null;
		}

		private Bitmap GetImageBitmapFromUrl(string url)
		{
			Bitmap imageBitmap = null;

			using (WebClient webClient = new WebClient())
			{
                byte[] imageBytes = webClient.DownloadData(url);
				if (imageBytes != null && imageBytes.Length > 0)
				{
					imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
				}
			}

			return imageBitmap;
		}

		public RemoteViews BuildUpdate(Context context)
		{
			RemoteViews updateViews = new RemoteViews(context.PackageName, Resource.Layout.widget);
			if (Preferences.ContainsKey("offline_weather"))
			{
				OneClickApi i = JsonConvert.DeserializeObject<OneClickApi>(Preferences.Get("offline_weather", ""));
				
				updateViews.SetTextViewText(Resource.Id.widgettemperatur, GetString(Resource.String.temp) + i.current.temp.ToString() + "°C");
				updateViews.SetTextViewText(Resource.Id.widgetfeelslike, GetString(Resource.String.feelslike) + i.current.feels_like.ToString() + "°C");
				updateViews.SetTextViewText(Resource.Id.widgetlastupdate, GetString(Resource.String.lastupdate) + Preferences.Get("offline_time", "Error"));
                string url = "https://openweathermap.org/img/wn/" + i.current.weather[0].icon + "@4x.png";
                Bitmap imageBitmap = GetImageBitmapFromUrl(url);
				updateViews.SetImageViewBitmap(Resource.Id.widgetimage, imageBitmap);

			}
			else
            {
				updateViews.SetTextViewText(Resource.Id.widgettemperatur, "Error");
				updateViews.SetTextViewText(Resource.Id.widgetfeelslike, "Error");
				updateViews.SetTextViewText(Resource.Id.widgetlastupdate, "Error");
			}
			if (true)
			{
				Intent defineIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("market://details?id=de.kaaaxcreators.weather_xamarin"));

				PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, defineIntent, 0);
				updateViews.SetOnClickPendingIntent(Resource.Id.widgetBackground, pendingIntent);
			}

			return updateViews;
		}
	}
}