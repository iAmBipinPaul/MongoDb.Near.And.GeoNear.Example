using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;

namespace MongoDb.Near.And.GeoNear.Example
{
    public class Program
    {
        private static readonly MongoClient Client =
            new MongoClient("[Enter Mongo Db Connection string]");
        private static readonly IMongoDatabase Database = Client.GetDatabase("test");
        private static readonly IMongoCollection<ZipCodeCollection> Collection =
            Database.GetCollection<ZipCodeCollection>("ZipCodeCollection");

        static async Task Main(string[] args)
        {
            
            Console.WriteLine("Creating Index if not exit");
            var builder = Builders<ZipCodeCollection>.IndexKeys;
            var keys = builder.Geo2DSphere(tag => tag.Location);
            await Collection.Indexes.CreateOneAsync(new CreateIndexModel<ZipCodeCollection>(keys));

            Console.WriteLine("Adding data to Mongo Db Collection");
            await AddZipCodeDataIfCollectionIsEmpty();
          
            Console.WriteLine("Getting Lat and Lng for ZipCode 10001 (New York)");
            var locationQuery = new FilterDefinitionBuilder<ZipCodeCollection>().Where(c=>c.ZipCode== "10001");
            var query =  Collection.Find(locationQuery);
         
            var zipcode =await query.FirstAsync();
            var lat = zipcode.Location.Coordinates.Latitude;
            var lng = zipcode.Location.Coordinates.Longitude;
             var  nearByZipCodeWithOutDistance= await GetNearByZipCodeWithOutDistance(lat, lng);
            var nearByZipCodeWithDistance = await GetNearByZipCodeWithDistance(lat, lng);

            Console.WriteLine("Nearby 50  ZipCode to Zip Code  10001 without distance");
            foreach (var s in nearByZipCodeWithOutDistance)
            {
                Console.WriteLine($"Zipcode: {s.ZipCode}");
            }
            Console.WriteLine("Nearby 50  Zip Code to Zip Code  10001 with distance");
            foreach (var s in nearByZipCodeWithDistance)
            {
                Console.WriteLine($"Zip Code: {s.ZipCode} Distance {s.Distance/1609} mile");
            }
        }

        private static async Task AddZipCodeDataIfCollectionIsEmpty()
        {
            var result = await Collection.CountDocumentsAsync(new BsonDocument());
            if (result == 0)
            {
                var usZipCodeJsonData = await File.ReadAllTextAsync(
                    @"C:\Users\iambi\RiderProjects\MongoDBNearBy\MongoDb.Near.And.GeoNear.Example\USZipCode.json");
                var zipCodeValue = JsonConvert.DeserializeObject<List<PlainZipCode>>(usZipCodeJsonData);

                var zipCodeCollectionList = new List<ZipCodeCollection>();

                foreach (var eventIds in zipCodeValue)
                {
                    var zipCodeCollection = new ZipCodeCollection
                    {
                        CountryCode = eventIds.CountryCode,
                        ZipCode = eventIds.ZipCode,
                        PlaceName = eventIds.PlaceName,
                        AdminName1 = eventIds.AdminName1,
                        AdminCode1 = eventIds.AdminCode1,
                        AdminName2 = eventIds.AdminName2,
                        AdminCode2 = eventIds.AdminCode2,
                        AdminName3 = eventIds.AdminName3,
                        AdminCode3 = eventIds.AdminCode3,
                        Accuracy = eventIds.Accuracy
                    };
                    if (eventIds.Latitude.HasValue && eventIds.Longitude.HasValue)
                    {
                        zipCodeCollection.Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                            new GeoJson2DGeographicCoordinates(eventIds.Longitude.Value,
                                eventIds.Latitude.Value));
                    }

                    zipCodeCollectionList.Add(zipCodeCollection);
                }

                var listWrites = new List<WriteModel<ZipCodeCollection>>();
                foreach (var f in zipCodeCollectionList)
                {
                    listWrites.Add(new InsertOneModel<ZipCodeCollection>(f));
                }

                var resultWrites = await Collection.BulkWriteAsync(listWrites);
            }
        }

        private static async Task<List<ZipCodeCollection>> GetNearByZipCodeWithDistance(double lat, double lng)
        {
            BsonDocument geoPoint = new BsonDocument
            {
                {"type", "Point"},
                {
                    "coordinates", new BsonArray(new double[]

                        {lng, lat}
                    )
                }
            };
            BsonDocument geoNearOptions = new BsonDocument
            {
                {"spherical", true},
                {"near", geoPoint},
                {"distanceField", "Distance"}
            };
            var stage = new BsonDocumentPipelineStageDefinition<ZipCodeCollection, ZipCodeCollection>(new BsonDocument
            {
                {"$geoNear", geoNearOptions}
            });
            var result = await Collection.Aggregate().AppendStage(stage).Limit(50).ToListAsync();
            var nearByZipcodes = result;
            return nearByZipcodes;
        }
        private static async Task<List<ZipCodeCollection>> GetNearByZipCodeWithOutDistance(double lat, double lng)
        {
            var point = GeoJson.Point(GeoJson.Geographic(lng, lat));
            var locationQuery = new FilterDefinitionBuilder<ZipCodeCollection>().Near(tag => tag.Location, point);
            var query = Collection.Find(locationQuery)
                .Limit(50); //Limit the query to return only the top 10 results.
            return await query.ToListAsync();
        }
    }
}