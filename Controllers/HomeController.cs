using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using REDHAT_DEMO.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using SharpCompress.Common;
using System;
using System.IO;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace REDHAT_DEMO.Controllers;

public class HomeController : Controller 
{

    private readonly ILogger<HomeController> _logger;
    private  IConfiguration configuration;
    readonly IStreamFileUploadService _streamFileUploadService;
    readonly EnvVar _envvar;

    //result table for all the db values
    List<Array> results = new List<Array>();

    public HomeController(ILogger<HomeController> logger,IStreamFileUploadService streamFileUploadService,IConfiguration iConfig,EnvVar envvar)
    {
        _streamFileUploadService = streamFileUploadService;
         _logger = logger;
         configuration = iConfig;  
         _envvar = envvar;
    }

    [ActionName("Index")]
    public IActionResult Index()
    {
        List<Models.Home> returnImage = getResultsList();
        //getting the list of results
        return View(returnImage);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    public List<Models.Home>  getResultsList() {
        List<Models.Home> list= new List<Models.Home>();
        //openening a connection to the DB
        string connectionString = _envvar.MongoDbConnection;
        string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".log";
        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
        clientSettings.SdamLogFilename = fileName;
        var creds = MongoCredential.CreateCredential("admin","pl_rhel_demo", "password1!");
        var settings = new MongoClientSettings
        {
            Servers = new[]
            {
                new MongoServerAddress("host1", 27017),
                new MongoServerAddress("host2", 27017),
                new MongoServerAddress("host3", 27017)
            },
            ConnectionMode = ConnectionMode.ReplicaSet,
            ReplicaSetName = "pl_rhel_demo",
            Credential = creds
        };
        MongoClient dbClient = new MongoClient(connectionString);
        var database = dbClient.GetDatabase("pl_rhel_demo");
        IGridFSBucket bucket = new GridFSBucket(database);
        var filter = Builders<GridFSFileInfo>.Filter.Empty;

       var cursor = bucket.Find(filter);
        
            var fileNames = cursor.ToList();
            
            int count = 50;
        foreach(var item in fileNames){
            if(count>0){
             Models.Home imageList= new Models.Home();
            imageList.imageID =item.Id;
            imageList.uploadDate = item.UploadDateTime;
            imageList.contentImage=bucket.DownloadAsBytes(item.Id);
            imageList.stringByte = Convert.ToBase64String(imageList.contentImage);
            if(item.Metadata!=null){
                Console.WriteLine(item.Metadata["predictions"]);
                imageList.Metadata= item.Metadata["predictions"].ToString();
                imageList.imageCategorie1=item.Metadata["prediction1"].ToString();
                imageList.imageCategorie2=item.Metadata["prediction2"].ToString();
                imageList.imageCategorie3=item.Metadata["prediction3"].ToString();
                imageList.imageCategorie4=item.Metadata["prediction4"].ToString();
                imageList.imageCategorie5=item.Metadata["prediction5"].ToString();

                imageList.imagePercentage1=decimal.Round((item.Metadata["prediction1_percentage"].ToDecimal()*(100)), 2, MidpointRounding.AwayFromZero) + " %";
                imageList.imagePercentage2=decimal.Round((item.Metadata["prediction2_percentage"].ToDecimal()*(100)), 2, MidpointRounding.AwayFromZero) + " %";
                imageList.imagePercentage3=decimal.Round((item.Metadata["prediction3_percentage"].ToDecimal()*(100)), 2, MidpointRounding.AwayFromZero) + " %";
                imageList.imagePercentage4=decimal.Round((item.Metadata["prediction4_percentage"].ToDecimal()*(100)), 2, MidpointRounding.AwayFromZero) + " %";
                imageList.imagePercentage5=decimal.Round((item.Metadata["prediction5_percentage"].ToDecimal()*(100)), 2, MidpointRounding.AwayFromZero) + " %";
            }
            list.Add(imageList);
            }
            count--;
        }
        //reversing the list to get it by the latest
        list.Reverse(); 
        return list;
    }
    
    public void sendQueueID(string mongoImageID) { 
         //Sending message to RABBITMQ image generator queue
       List<string> rabbitHosts = _envvar.RabbitMQConnection.Split(',').ToList();
       var factory = new RabbitMQ.Client.ConnectionFactory() { UserName = _envvar.RabbitMQUser, Password = _envvar.RabbitMQPassword };
                using (var connection = factory.CreateConnection(rabbitHosts))
                {
                    using (var channel = connection.CreateModel())
                    {
                            IDictionary<string, object> arguments = new Dictionary<string, object>();
                            arguments.Add("x-queue-type","quorum");
                            channel.QueueDeclare(queue: "Image_generator_Queue",
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: arguments);

                            var imageID = mongoImageID;
                            var body = Encoding.UTF8.GetBytes(imageID.ToString());

                            channel.BasicPublish(exchange: string.Empty,
                                            routingKey: "Image_generator_Queue",
                                            basicProperties: null,
                                            body: body);
                            Console.WriteLine($" [x] Sent {imageID}");

                            //Console.ReadLine();
                    }
                }

    }

    [HttpPost]
    public IActionResult insertImage(byte [] image) {

        //openening a connection to the DB
        string connectionString = configuration.GetSection("MongoDbConnection").Value;
        MongoClient dbClient = new MongoClient(connectionString);
        var database = dbClient.GetDatabase("pl_rhel_demo");
        IGridFSBucket bucket = new GridFSBucket(database);
        var mongoImageID = bucket.UploadFromBytes("file", image);

        //send image ID to the queue
        sendQueueID(mongoImageID.ToString());
        List<Models.Home> returnImage = getResultsList();
        return RedirectToAction("Index", returnImage);
    }

    
    [HttpPost]
    public async Task<IActionResult> SaveFileToPhysicalFolder(IFormFile file)
    {
        string bytearray = string.Empty;
        var a = Request;
            if (file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    byte [] fileBytes = ms.ToArray();
                     bytearray = Convert.ToBase64String(fileBytes);
                    // act on the Base64 data
                    return insertImage(fileBytes);
                }
            }
        return View();
    }
    
    [HttpPost]
public async Task<List<Models.Home>> updateModel()
{
    List<Models.Home> returnImage=getResultsList();
    Console.WriteLine("in");
    return returnImage;
    
}
}

