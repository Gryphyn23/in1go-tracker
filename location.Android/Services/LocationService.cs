using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Net;
using Android.Support.V4.App;
using Android.Util;
using Android.Content;
using Android.OS;
using Android.Locations;
using Android.Widget;
using in1go.tracker;
using Java.Text;
using Location.Droid.Orm;

namespace Location.Droid.Services
{
	[Service]
	public class LocationService : Service, ILocationListener
	{
		public event EventHandler<LocationChangedEventArgs> LocationChanged = delegate { };
		public event EventHandler<ProviderDisabledEventArgs> ProviderDisabled = delegate { };
		public event EventHandler<ProviderEnabledEventArgs> ProviderEnabled = delegate { };
		public event EventHandler<StatusChangedEventArgs> StatusChanged = delegate { };

        private NotificationManager gpsNotifyManager;
        private const int NOTIFICATION_ID = 1000;

        private const long minimumseconds = 10000;
        private const string SERVER_IP = "54.251.249.117";
        private const string SERVER_PORT = "12000";

        Intent alarmIntent;

        long LatestTimeStamp;

		public LocationService() 
		{
		}

        // Set our location manager as the system location service
        protected LocationManager GPSLocMgr = Android.App.Application.Context.GetSystemService("location") as LocationManager;
        protected LocationManager TowerLocMgr = Android.App.Application.Context.GetSystemService("location") as LocationManager;


		readonly string logTag = "LocationService";
		IBinder binder;

		public override void OnCreate ()
		{
			base.OnCreate ();
            HandleIntent(alarmIntent);
			Log.Debug (logTag, "OnCreate called in the Location Service");
		}

		// This gets called when StartService is called in our App class
		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Debug (logTag, "LocationService started");
            HandleIntent(intent);
			return StartCommandResult.Sticky;
		}

		// This gets called once, the first time any client bind to the Service
		// and returns an instance of the LocationServiceBinder. All future clients will
		// reuse the same instance of the binder
		public override IBinder OnBind (Intent intent)
		{
			Log.Debug (logTag, "Client now bound to service");

			binder = new LocationServiceBinder (this);
			return binder;
		}

        public async void HandleIntent(Intent intent)
        {
            
            if (intent != null)
            {
                bool flag = intent.Extras.GetBoolean("immediate", false);
                bool flag2 = intent.Extras.GetBoolean("alarmWentOff", false);

                if (flag)
                {
                    Console.WriteLine("Auto starting logging");
                }
                if (flag2)
                {
                    Log.Debug(logTag, "Times up!");

                    //add sending of saved tracks
                    await SendTracks();
                    ResetManagersIfRequired();
                }
                return;
            }
            else
            {
                Console.WriteLine("Service restarted with null intent. Start logging.");
                return;
            }
        }


        public void StartTimer()
        {

            long now = SystemClock.CurrentThreadTimeMillis();
            alarmIntent = new Intent(this, typeof(Location.Droid.AlarmReceiver));
            PendingIntent pi = PendingIntent.GetBroadcast(this, 0, alarmIntent, 0);

            AlarmManager am = (AlarmManager)this.GetSystemService(Context.AlarmService);
            am.SetRepeating(AlarmType.ElapsedRealtimeWakeup, now + 1000, 10000, pi);
        }


		// Handle location updates from the location manager
		public void StartLocationUpdates () 
		{


            Notify();
            StartTimer();

            CheckTowerAndGpsStatus();


            if (Session.isGpsEnabled())
            {
                Log.Debug(logTag, "Requesting GPS location updates");
                /*
                Toast.MakeText
                (
                    this,
                    String.Format("Requesting GPS location updates"),
                   ToastLength.Long
                ).Show();
                */

                GPSLocMgr.RequestLocationUpdates("gps", minimumseconds, 0, this);
                Session.setUsingGps(true);

            }
            else if (Session.isTowerEnabled())
            {
                Log.Debug(logTag, "Requesting tower location updates");
                /*
                Toast.MakeText
               (
                   this,
                   String.Format("Requesting tower location updates"),
                  ToastLength.Long
               ).Show();
                 * */
                Session.setUsingGps(false);
                TowerLocMgr.RequestLocationUpdates("network", minimumseconds, 0, this);
            }
            else
            {

                Log.Debug(logTag, "No provider available");
                /*
                Toast.MakeText
               (
                   this,
                   String.Format("No provider available"),
                  ToastLength.Long
               ).Show();
                 * */
                Session.setUsingGps(false);
                return;
            }

            Log.Debug(logTag, "Now sending location updates");
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			Log.Debug (logTag, "Service has been terminated");
		}

		#region ILocationListener implementation
		// ILocationListener is a way for the Service to subscribe for updates
		// from the System location Service

		public async void OnLocationChanged (Android.Locations.Location location)
		{
			this.LocationChanged (this, new LocationChangedEventArgs (location));

			// This should be updating every time we request new location updates
			// both when teh app is in the background, and in the foreground
			Log.Debug (logTag, String.Format ("Latitude is {0}", location.Latitude));
			Log.Debug (logTag, String.Format ("Longitude is {0}", location.Longitude));
			Log.Debug (logTag, String.Format ("Altitude is {0}", location.Altitude));
			Log.Debug (logTag, String.Format ("Speed is {0}", location.Speed));
			Log.Debug (logTag, String.Format ("Accuracy is {0}", location.Accuracy));
			Log.Debug (logTag, String.Format ("Bearing is {0}", location.Bearing));

            /*
            Toast.MakeText
               (
                   this,
                   String.Format("{0} {1}", location.Latitude, location.Longitude),
                  ToastLength.Long
               ).Show();
            */

            Session.setCurrentLocationInfo(location);
            Notify();
            await SendTracks(location);

            ResetManagersIfRequired();


            /*
            ISharedPreferences sharedpreferences;
            sharedpreferences = GetSharedPreferences("LatestTimeStamp", 0);
            LatestTimeStamp = sharedpreferences.GetLong("LatestTimeStamp", 0L);

            if ((System.Environment.TickCount - LatestTimeStamp) < minimumseconds)
            {
                Log.Debug(logTag, String.Format("skip track"));
                return;
            }
            else
            {
                this.LocationChanged(this, new LocationChangedEventArgs(location));
                Log.Debug(logTag, String.Format("New location obtained"));
                Android.Content.ISharedPreferencesEditor editor = sharedpreferences.Edit();
                editor.PutLong("LatestTimeStamp", System.Environment.TickCount);
                editor.Commit();
                Session.setCurrentLocationInfo(location);
                Notify();

                //SaveTrackToDBAndSend(location);

                await SendTracks(location);

                ResetManagersIfRequired();
            }
             * */
            
		}

        public async Task SendTracks(Android.Locations.Location track)
        {
            Log.Debug(logTag, String.Format("Sending Tracks"));

            ConnectivityManager cm = (ConnectivityManager)GetSystemService(Android.Content.Context.ConnectivityService);
            NetworkInfo wifi = cm.GetNetworkInfo(ConnectivityType.Wifi);
            NetworkInfo mobile = cm.GetNetworkInfo(ConnectivityType.Mobile);

            if ((wifi.IsAvailable && wifi.IsConnected) || (mobile.IsAvailable && mobile.IsConnected))
            {

                if (ConnectionAvailable())
                {

                    Log.Debug(logTag, String.Format("now sending"));
                    using (HttpClient client = new HttpClient())
                    {

                        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        dtDateTime = dtDateTime.AddMilliseconds(track.Time).ToLocalTime();

                        try
                        {
                            var postData = new List<KeyValuePair<string, string>>();
                            postData.Add(new KeyValuePair<string, string>("latitude", track.Latitude.ToString()));
                            postData.Add(new KeyValuePair<string, string>("longitude", track.Longitude.ToString()));
                            postData.Add(new KeyValuePair<string, string>("imei", DeviceIMEI.GetDeviceId(this)));
                            postData.Add(new KeyValuePair<string, string>("gps_time",dtDateTime.ToString("yyyy-MM-dd HH:mm:ss")));
                            postData.Add(new KeyValuePair<string, string>("speed", track.Speed.ToString()));
                            postData.Add(new KeyValuePair<string, string>("head", "0"));
                            postData.Add(new KeyValuePair<string, string>("valid", "1"));
                            postData.Add(new KeyValuePair<string, string>("accuracy", track.Accuracy.ToString()));
                            postData.Add(new KeyValuePair<string, string>("altitude", track.Altitude.ToString()));
                            postData.Add(new KeyValuePair<string, string>("bearing", track.Bearing.ToString()));

                            HttpContent content = new FormUrlEncodedContent(postData);

                            HttpResponseMessage response = await client.PostAsync("http://" + SERVER_IP + ":" + SERVER_PORT + "/", content);

                            //response.EnsureSuccessStatusCode();

                            string responseBody = await response.Content.ReadAsStringAsync();

                        }
                        catch (HttpRequestException hre)
                        {
                            Log.Debug(logTag, String.Format("HttpRequestException is {0}", hre.ToString()));
                        }
                        catch (Exception ex)
                        {
                            Log.Debug(logTag, String.Format("Exception responseBody is {0}", ex.ToString()));
                        }
                    }
                }
                else
                {
                    Log.Debug(logTag, String.Format("cannot ping,save to db"));
                    SaveTrackToDB(track);
                }
            }
            else
            {
                Log.Debug(logTag, String.Format("no connection,save to db"));
                SaveTrackToDB(track);
            }
        }

        public async Task SendTracks()
        {
            ConnectivityManager cm = (ConnectivityManager)GetSystemService(Android.Content.Context.ConnectivityService);
            NetworkInfo wifi = cm.GetNetworkInfo(ConnectivityType.Wifi);
            NetworkInfo mobile = cm.GetNetworkInfo(ConnectivityType.Mobile);

            if ((wifi.IsAvailable && wifi.IsConnected) || (mobile.IsAvailable && mobile.IsConnected))
            {
                if (ConnectionAvailable())
                {

                    Log.Debug(logTag, String.Format("Sending Tracks"));

                    var tracks = TracksRepository.GetTracks(5);

                    Log.Debug(logTag,String.Format("tracks to be sent: {0}",tracks.Count()));

                    using (HttpClient client = new HttpClient())
                    {
                        foreach (var track in tracks)
                        {
                            Log.Debug(logTag,
                                String.Format("tracks: {0} {1}", track.GpsTime.ToLongTimeString(), track.Id));
                            try
                            {
                                var postData = new List<KeyValuePair<string, string>>();
                                postData.Add(new KeyValuePair<string, string>("latitude", track.Latitude));
                                postData.Add(new KeyValuePair<string, string>("longitude", track.Longitude));
                                postData.Add(new KeyValuePair<string, string>("imei", track.Imei));
                                postData.Add(new KeyValuePair<string, string>("gps_time",
                                    track.GpsTime.ToString("yyyy-MM-dd HH:mm:ss")));
                                postData.Add(new KeyValuePair<string, string>("speed", track.Speed));
                                postData.Add(new KeyValuePair<string, string>("head", track.Head));
                                postData.Add(new KeyValuePair<string, string>("valid", track.Valid));
                                postData.Add(new KeyValuePair<string, string>("accuracy", track.Accuracy));
                                postData.Add(new KeyValuePair<string, string>("altitude", track.Altitude));
                                postData.Add(new KeyValuePair<string, string>("bearing", track.Bearing));

                                HttpContent content = new FormUrlEncodedContent(postData);

                                HttpResponseMessage response =
                                    await client.PostAsync("http://" + SERVER_IP + ":" + SERVER_PORT + "/", content);

                                //response.EnsureSuccessStatusCode();

                                string responseBody = await response.Content.ReadAsStringAsync();

                                if (responseBody == "100")
                                {
                                    //record_count--;
                                    TracksRepository.DeleteTrack(track);
                                }
                                else if (response.StatusCode == (HttpStatusCode) 501 &&
                                         responseBody.Contains("ER_DUP_ENTRY"))
                                {
                                    Log.Debug(logTag,
                                        String.Format("response {0} - {1} - {2}", response.StatusCode,
                                            response.ReasonPhrase, responseBody));
                                    TracksRepository.DeleteTrack(track);
                                }

                            }
                            catch (HttpRequestException hre)
                            {
                                Log.Debug(logTag, String.Format("HttpRequestException is {0}", hre.ToString()));
                            }
                            catch (Exception ex)
                            {
                                Log.Debug(logTag, String.Format("Exception responseBody is {0}", ex.ToString()));
                            }

                        }
                    }
                }
                else
                {
                    Log.Debug(logTag, String.Format("cannot connect to server"));
                }

            }
            else
            {
                Log.Debug(logTag, String.Format("no connection"));
            }

        }

        public static bool ConnectionAvailable()
        {
            try
            {
                HttpWebRequest reqFP = (HttpWebRequest)HttpWebRequest.Create("http://" + SERVER_IP);
                reqFP.Timeout = 5000;
                HttpWebResponse rspFP = (HttpWebResponse)reqFP.GetResponse();
                if (HttpStatusCode.OK == rspFP.StatusCode)
                {
                    // HTTP = 200 - Internet connection available, server online

                    rspFP.Close();
                    return true;
                }
                else
                {
                    // Other status - Server or connection not available
                    rspFP.Close();
                    return false;
                }
            }
            catch (WebException)
            {
                // Exception - connection not available
                return false;
            }
        }

		public void OnProviderDisabled (string provider)
		{
			this.ProviderDisabled (this, new ProviderDisabledEventArgs (provider));
		}

		public void OnProviderEnabled (string provider)
		{
			this.ProviderEnabled (this, new ProviderEnabledEventArgs (provider));
		}

		public void OnStatusChanged (string provider, Availability status, Bundle extras)
		{
			this.StatusChanged (this, new StatusChangedEventArgs (provider, status, extras));
		} 

		#endregion

        private void Notify()
        {
            Log.Debug(logTag, "GpsService.Notify called");
            gpsNotifyManager = (NotificationManager)GetSystemService(Context.NotificationService);
            ShowNotification();
        }

        private void ShowNotification()
        {
            Log.Debug(logTag, "GpsService.ShowNotification called");
            Intent intent = new Intent(this, typeof(MainActivity));
            PendingIntent pendingintent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);
            DecimalFormat decimalformat = new DecimalFormat("###.######"); ;

            string s = "Waiting for next location. Please wait...";
            if (Session.hasValidLocation())
            {
                s = String.Format("Lat {0} Lon {1}", decimalformat.Format(Session.getCurrentLatitude()), decimalformat.Format(Session.getCurrentLongitude()));

            }
            // Build the notification
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
                .SetAutoCancel(true) // dismiss the notification from the notification area when the user clicks on it
                //.SetContentIntent(pendingintent) // start up this activity when the user clicks the intent.
                .SetContentTitle("in1go Tracker running...") // Set the title
                .SetSmallIcon(Resource.Drawable.icon) // This is the icon to display
                .SetContentText(s); // the message to display.

            gpsNotifyManager.Notify(NOTIFICATION_ID, builder.Build());
            StartForeground(NOTIFICATION_ID, builder.Build());

        }

        private void CheckTowerAndGpsStatus()
        {
            Session.setTowerEnabled(TowerLocMgr.IsProviderEnabled("network"));
            Session.setGpsEnabled(GPSLocMgr.IsProviderEnabled("gps"));
        }

        public void ResetManagersIfRequired()
        {

            Log.Debug(logTag, "ResetManagersIfRequired");
            CheckTowerAndGpsStatus();
            if (Session.isUsingGps() && !Session.isGpsEnabled())
            {
                RestartGpsManagers();
            }
            else
            {
                if (Session.isGpsEnabled() && !Session.isUsingGps())
                {
                    RestartGpsManagers();
                    return;
                }

            }

        }

        public void RestartGpsManagers()
        {
            Session.setCurrentLocationInfo(null);
            Notify();
            Log.Debug(logTag, "GpsService.RestartGpsManagers");
            StopGpsManager();
            StartLocationUpdates();
        }

        public void StopGpsManager()
        {
            Log.Debug(logTag, "GpsService.StopGpsManager");
            if (TowerLocMgr != null)
            {
                TowerLocMgr.RemoveUpdates(this);
            }
            if (GPSLocMgr != null)
            {
                GPSLocMgr.RemoveUpdates(this);
            }

        }

        public void SaveTrackToDB(Android.Locations.Location location)
        {
            if (location == null)
            {
                return;
            }

            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(location.Time).ToLocalTime();

            var track = new Tracks();
            track.Latitude = location.Latitude.ToString();
            track.Longitude = location.Longitude.ToString();
            track.Imei = DeviceIMEI.GetDeviceId(this);
            track.GpsTime = dtDateTime;
            track.Speed = System.Math.Round(1.9438444924406D * (double)location.Speed).ToString();
            track.Head = "0";
            track.Valid = "1";
            track.Accuracy = location.Accuracy.ToString();
            track.Altitude = location.Altitude.ToString();
            track.Bearing = location.Bearing.ToString();
            TracksRepository.SaveTrack(track);


        }

	}
}

