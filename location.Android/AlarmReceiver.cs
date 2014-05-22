using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Location.Droid.Services;

namespace Location.Droid
{
    [BroadcastReceiver]
    public class AlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                    /*
                    Toast.MakeText
                    (
                        context,
                        "alarm received",
                        ToastLength.Long
                    ).Show();
                    */
                    Intent serviceintent = new Intent(Android.App.Application.Context, typeof(LocationService));
                    serviceintent.PutExtra("alarmWentOff", true);
                    Android.App.Application.Context.StartService(serviceintent);



                
            }
            catch (Exception exception)
            {
                Toast.MakeText
                    (
                        context,
                        "error on alarm: " + exception.Message,
                        ToastLength.Short
                    ).Show();
            }
        }
    }
}