public class EnvVar{
    readonly IConfiguration _config;
    public EnvVar(IConfiguration config)
    {
       _config = config;
    }
public string MongoDbConnection { get{return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MongoDbConnection"))?_config["MongoDbConnection"]:Environment.GetEnvironmentVariable("MongoDbConnection");} }
public string RabbitMQConnection { get{return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RabbitMQConnection"))?_config["RabbitMQConnection"]:Environment.GetEnvironmentVariable("RabbitMQConnection");} }
public string RabbitMQUser { get{return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RabbitMQUser"))?_config["RabbitMQUser"]:Environment.GetEnvironmentVariable("RabbitMQUser");}  }
public string RabbitMQPassword { get{return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RabbitMQPassword"))?_config["RabbitMQPassword"]:Environment.GetEnvironmentVariable("RabbitMQPassword");}  }
}