using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class MongoImage
{
     [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        //id to send to the queue
        public string imageID { get; set; }

        //image categorie
        public string imageCategorie1 { get; set; }
        public string imageCategorie2 { get; set; }
        public string imageCategorie3 { get; set; }
        public string imageCategorie4 { get; set; }
        public string imageCategorie5 { get; set; }

        //storing the percentage value for each categories
        public decimal imagePercentage1 { get; set; }
        public decimal imagePercentage2 { get; set; }
        public decimal imagePercentage3 { get; set; }
        public decimal imagePercentage4 { get; set; }
        public decimal imagePercentage5 { get; set; }

        //image in bytes
        public byte[] contentImage { get; set; }
}