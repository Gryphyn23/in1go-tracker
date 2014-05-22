using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Location.Droid.Services;

namespace Location.Droid
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class StartupReceiver : BroadcastReceiver
    {
        protected readonly string logTag = "StartupReceiver";

        protected LocationServiceConnection locationServiceConnection;

        // events
        public event EventHandler<ServiceConnectedEventArgs> LocationServiceConnected = delegate { };

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                if (intent.Action == Intent.ActionBootCompleted)
                {
                    /*
                    // start our main service
                    Intent serviceintent = new Intent(Android.App.Application.Context, typeof(LocationService));
                    serviceintent.PutExtra("immediate", true);
                    Android.App.Application.Context.StartService(serviceintent);

                    // create a new service connection so we can get a binder to the service
                    this.locationServiceConnection = new LocationServiceConnection(null);

                    // this event will fire when the Service connectin in the OnServiceConnected call 
                    this.locationServiceConnection.ServiceConnected += (object sender, ServiceConnectedEventArgs e) => this.LocationServiceConnected(this, e);

                    // bind our service (Android goes and finds the running service by type, and puts a reference
                    // on the binder to that service)
                    // The Intent tells the OS where to find our Service (the Context) and the Type of Service
                    // we're looking for (LocationService)
                    Intent locationServiceIntent = new Intent(Android.App.Application.Context, typeof(LocationService));

                    // Finally, we can bind to the Service using our Intent and the ServiceConnection we
                    // created in a previous step.
                    Android.App.Application.Context.BindService(locationServiceIntent, locationServiceConnection, Bind.AutoCreate);
                    */

                    App.Current.LocationServiceConnected += (object sender, ServiceConnectedEventArgs e) =>
                    {
                    };
                }
            }
            catch (Exception exception)
            {
                Toast.MakeText
                    (
                        context,
                        "error on receiver: " + exception.Message,
                        ToastLength.Short
                    ).Show();
            }
        }

        
    }
}