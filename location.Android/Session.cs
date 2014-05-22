using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Location.Droid
{
    class Session : Application
    {
        public Session()
        {
        }

        private static bool addNewTrackSegment = true;
        private static float autoEmailDelay;
        private static long autoEmailTimeStamp;
        private static String currentFileName;
        private static Android.Locations.Location currentLocationInfo;
        private static bool emailReadyToBeSent = false;
        private static bool gpsEnabled;
        public static String imeiID = null;
        private static bool isBound;
        private static bool _isStarted;
        private static bool _isUsingGps;
        private static long latestTimeStamp;
        private static bool notificationVisible;
        private static int satellites;
        private static bool towerEnabled;
        public static String url = null;

        public static float getAutoEmailDelay()
        {
            return autoEmailDelay;
        }

        public static long getAutoEmailTimeStamp()
        {
            return autoEmailTimeStamp;
        }

        public static String getCurrentFileName()
        {
            return currentFileName;
        }

        public static double getCurrentLatitude()
        {
            if (getCurrentLocationInfo() != null)
            {
                return getCurrentLocationInfo().Latitude;
            }
            else
            {
                return 0.0D;
            }
        }

        public static Android.Locations.Location getCurrentLocationInfo()
        {
            return currentLocationInfo;
        }

        public static double getCurrentLongitude()
        {
            if (getCurrentLocationInfo() != null)
            {
                return getCurrentLocationInfo().Longitude;
            }
            else
            {
                return 0.0D;
            }
        }

        public static String getIMEI()
        {
            return imeiID;
        }

        public static long getLatestTimeStamp()
        {
            return latestTimeStamp;
        }

        public static int getSatelliteCount()
        {
            return satellites;
        }

        public static String getURL()
        {
            return url;
        }

        public static bool hasValidLocation()
        {
            return getCurrentLocationInfo() != null && getCurrentLatitude() != 0.0D && getCurrentLongitude() != 0.0D;
        }

        public static bool isBoundToService()
        {
            return isBound;
        }

        public static bool isEmailReadyToBeSent()
        {
            return emailReadyToBeSent;
        }

        public static bool isGpsEnabled()
        {
            return gpsEnabled;
        }

        public static bool isNotificationVisible()
        {
            return notificationVisible;
        }

        public static bool isStarted()
        {
            return _isStarted;
        }

        public static bool isTowerEnabled()
        {
            return towerEnabled;
        }

        public static bool isUsingGps()
        {
            return _isUsingGps;
        }

        public static void setAddNewTrackSegment(bool flag)
        {
            addNewTrackSegment = flag;
        }

        public static void setAutoEmailDelay(float f)
        {
            autoEmailDelay = f;
        }

        public static void setAutoEmailTimeStamp(long l)
        {
            autoEmailTimeStamp = l;
        }

        public static void setBoundToService(bool flag)
        {
            isBound = flag;
        }

        public static void setCurrentFileName(String s)
        {
            currentFileName = s;
        }

        public static void setCurrentLocationInfo(Android.Locations.Location location)
        {
            currentLocationInfo = location;
        }

        public static void setEmailReadyToBeSent(bool flag)
        {
            emailReadyToBeSent = flag;
        }

        public static void setGpsEnabled(bool flag)
        {
            gpsEnabled = flag;
        }

        public static void setIMEI(String s)
        {
            imeiID = s;
        }

        public static void setLatestTimeStamp(long l)
        {
            latestTimeStamp = l;
        }

        public static void setNotificationVisible(bool flag)
        {
            notificationVisible = flag;
        }

        public static void setSatelliteCount(int i)
        {
            satellites = i;
        }

        public static void setStarted(bool flag)
        {
            _isStarted = flag;
        }

        public static void setTowerEnabled(bool flag)
        {
            towerEnabled = flag;
        }

        public static void setURL(String s)
        {
            url = s;
        }

        public static void setUsingGps(bool flag)
        {
            _isUsingGps = flag;
        }

        public static bool shouldAddNewTrackSegment()
        {
            return addNewTrackSegment;
        }
    }
}