using System;
using SQLite;

namespace Location.Droid.Orm
{
    public class Tracks
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string Imei { get; set; }

        public DateTime GpsTime { get; set; }

        public string Speed { get; set; }

        public string Head { get; set; }

        public string Valid { get; set; }

        public string Accuracy { get; set; }

        public string Altitude { get; set; }

        public string Bearing { get; set; }
    }
}