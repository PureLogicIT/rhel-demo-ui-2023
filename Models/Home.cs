using System;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace REDHAT_DEMO.Models
{
	public class Home
	{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        //id to send to the queue
        public ObjectId imageID { get; set; }

        public string Metadata {get;set;}

        public System.DateTime uploadDate {get; set;}

        public System.TimeSpan Diff {get{
            return DateTime.UtcNow - uploadDate;
        }}

        //image categorie
        public string imageCategorie1 { get; set; }
        public string imageCategorie2 { get; set; }
        public string imageCategorie3 { get; set; }
        public string imageCategorie4 { get; set; }
        public string imageCategorie5 { get; set; }

        //storing the percentage value for each categories
        public string imagePercentage1 { get; set; }
        public string imagePercentage2 { get; set; }
        public string imagePercentage3 { get; set; }
        public string imagePercentage4 { get; set; }
        public string imagePercentage5 { get; set; }


        //image in bytes
        public byte[] contentImage { get; set; }

        public string stringByte { get; set; }

    }
}

