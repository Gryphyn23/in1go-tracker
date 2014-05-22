using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace Location.Droid.Orm
{
    class TracksDatabase: SQLiteConnection
    {
        static object locker = new object();

        public static string DatabaseFilePath
        {
            get
            {
                var sqliteFilename = "tracks.db";

#if NETFX_CORE
			var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, sqliteFilename);
#else

#if SILVERLIGHT
			// Windows Phone expects a local path, not absolute
			var path = sqliteFilename;
#else

#if __ANDROID__
			// Just use whatever directory SpecialFolder.Personal returns
			string libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); ;
#else
                // we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
                // (they don't want non-user-generated data in Documents)
                string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
                string libraryPath = Path.Combine(documentsPath, "../Library/"); // Library folder
#endif
                //var path = Path.Combine(libraryPath, sqliteFilename);
                var path = Android.OS.Environment.ExternalStorageDirectory + "/Centrix/" + sqliteFilename;
#endif

#endif
                //Console.WriteLine(path);
                return path;
            }
        }

        public TracksDatabase(string path)
            : base(path)
        {
            // create the tables
            CreateTable<Tracks>();
        }

        public IEnumerable<Tracks> GetAllTracks()
        {
            lock (locker)
            {
                return (
                        from i in Table<Tracks>()
                        orderby i.GpsTime ascending
                        select i
                        )
                        .ToList();
            }
        }

        public IEnumerable<Tracks> GetTracks(int limit, string order = "descending")
        {
            lock (locker)
            {
                if (order == "descending")
                {
                    return (

                        from i in Table<Tracks>()
                        orderby i.GpsTime descending 
                        select i
                        )
                        .Take(limit).ToList();
                }
                else
                {
                    return (

                        from i in Table<Tracks>()
                        orderby i.GpsTime ascending 
                        select i
                        )
                        .Take(limit).ToList();
                }
                
            }
        }

        public Tracks GetTrack(int id)
        {
            lock (locker)
            {
                return Table<Tracks>().FirstOrDefault(x => x.Id == id);
            }
        }

        public int SaveTrack(Tracks item)
        {
            lock (locker)
            {
                if (item.Id != 0)
                {
                    Update(item);
                    return item.Id;
                }
                else
                {
                    return Insert(item);
                }
            }
        }

        //		public int DeleteTracks(int id) 
        //		{
        //			lock (locker) {
        //				return Delete<Tracks> (new Tracks () { Id = id });
        //			}
        //		}
        public int DeleteTrack(Tracks Tracks)
        {

            lock (locker)
            {
                return Delete<Tracks>(Tracks.Id);
            }
        }

        public int DeleteAllTracks()
        {
            lock (locker)
            {
                
                return DeleteAll<Tracks>();
            }
        }
    }
}