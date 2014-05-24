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

namespace Location.Droid.Orm
{
    class TracksRepository
    {
        private TracksDatabase db = null;

        protected static TracksRepository me;

        static TracksRepository ()
		{
			me = new TracksRepository();
		}
		protected TracksRepository()
		{
			db = new TracksDatabase(TracksDatabase.DatabaseFilePath);
		}

		public static Tracks GetTrack(int id)
		{
			return me.db.GetTrack(id);
		}

        public static IEnumerable<Tracks> GetTracks(int limit, string order = "descending")
		{
            return me.db.GetTracks(limit, order);
		}

        public static IEnumerable<Tracks> GetAllTracks()
        {
            return me.db.GetAllTracks();
        }
		
		public static int SaveTrack (Tracks item)
		{
			return me.db.SaveTrack(item);
		}
		
		public static int DeleteTrack(Tracks item)
		{
			return me.db.DeleteTrack(item);
		}

        public static int DeleteAllTracks()
        {            
            return me.db.DeleteAllTracks();
        }

    }
}