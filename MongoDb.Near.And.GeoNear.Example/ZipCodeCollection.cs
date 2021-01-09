using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MongoDb.Near.And.GeoNear.Example
{
    public class ZipCodeCollection
    {
        [BsonId] public Guid Id { get; set; }
        public string CountryCode { get; set; }
        public string ZipCode { get; set; }
        public string PlaceName { get; set; }
        public string AdminName1 { get; set; }
        public string AdminCode1 { get; set; }
        public string AdminName2 { get; set; }
        public string AdminCode2 { get; set; }
        public string AdminName3 { get; set; }
        public string AdminCode3 { get; set; }
        public int? Accuracy { get; set; }
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
        public double Distance { get; set; }
    }
}