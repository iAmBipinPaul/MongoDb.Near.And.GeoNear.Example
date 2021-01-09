namespace MongoDb.Near.And.GeoNear.Example
{
    public  class PlainZipCode
    {
        public string CountryCode { get; set; }
        public string ZipCode { get; set; }
        public string PlaceName { get; set; }
        public string AdminName1 { get; set; }
        public string AdminCode1 { get; set; }
        public string AdminName2 { get; set; }
        public string AdminCode2 { get; set; }
        public string AdminName3 { get; set; }
        public string AdminCode3 { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? Accuracy { get; set; }
    }
}