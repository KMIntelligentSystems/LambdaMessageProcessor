using System;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

using Amazon.Lambda.Serialization.SystemTextJson;

using Amazon.DynamoDBv2;

using Amazon.ApiGatewayManagementApi;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Logging;
using GraphQLWorkerService;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Threading;

using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Amazon;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using System.IO;
using System.Text;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace MessageProcessor
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// MyHttpGatewayApi::MyHttpGatewayApi.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
    {
        string ConnectionMappingTable { get; set; }

        /// <summary>
        /// DynamoDB service client used to store and retieve connection information from the ConnectionMappingTable
        /// </summary>
        IAmazonDynamoDB DDBClient { get; set; }
        IAmazonSQS SQSClient { get; set; }
        
        Channel<string> channel = Channel.CreateUnbounded<string>();
        GraphQLServiceWorker graphQLProcessor;
        Thread messageThread;
        public const string ConnectionIdField = "ConnectionIdField";

        Func<string, IAmazonApiGatewayManagementApi> ApiGatewayManagementApiClientFactory { get; set; }

        private readonly GraphQLServiceWorker _service;
      


        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>();
        }

        /// <summary>
        /// Use this override to customize the services registered with the IHostBuilder. 
        /// 
        /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
        /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IHostBuilder builder)
        {
            SQSClient = new AmazonSQSClient();

            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            Configure(loggerFactory);
            ILogger logger = loggerFactory.CreateLogger<GraphQLServiceWorker>();

            

            /*  graphQLProcessor = new GraphQLServiceWorker(logger,channel);
              messageThread = new Thread(graphQLProcessor.DoWork);
              messageThread.IsBackground = true;
              messageThread.Start();*/


            /*   builder.ConfigureServices((hostContext, services) =>
                {
            //    services.AddLogging(l => l.AddConfiguration(configuration));

                services.AddSingleton<IGraphQLServiceWorker, GraphQLServiceWorker>();

                    services.AddScoped<GraphQLServiceWorker>(factory =>
                    {
                        return new GraphQLServiceWorker(channel);
                    });

                    services.AddHostedService<Worker>();
                });

               var consumer = Task.Run(async () =>
               {
                   while (await channel.Reader.WaitToReadAsync())
                       Console.WriteLine(await channel.Reader.ReadAsync());
               });*/

        }

        public void Configure(ILoggerFactory loggerFactory)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: true)
                .Build();

            var loggerOptions = new LambdaLoggerOptions(configuration);

            // Configure Lambda logging
            loggerFactory
                .AddLambdaLogger(loggerOptions);
        }

        public async ValueTask GraphQLMessageHandler(SQSEvent sqsEvent, ILambdaContext context)
        {
            
            try
            {

                var awsCredentials = new BasicAWSCredentials("XXXXXXXXXXXXXXXOY5R5", "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
                var region = RegionEndpoint.GetBySystemName("us-east-1");

              

                //await Task.Run(async () =>
                 //{
                 
                     foreach (var record in sqsEvent.Records)
                     {

                         Console.WriteLine($"RECORD: {record.Body}");

                    //await channel.Writer.WriteAsync(record.Body);

                    /*     var amazonStepFunctionsConfig = new AmazonStepFunctionsConfig { RegionEndpoint = RegionEndpoint.USEast1 };
                         StartExecutionRequest s = new StartExecutionRequest();
                         s.StateMachineArn = "arn:aws:states:us-east-1:280449388741:stateMachine:StateMachine-TJxMe1Ai0Gqk";

                         s.Input = "{\" Name \" : " + record.Body + "}";
                        s.Name = $"SchedulingEngine_{Guid.NewGuid().ToString("N")}";

                        var clientSt = new AmazonStepFunctionsClient("AKIAUCTAWCDCVW4OY5R5", "Nh6KeyODYrPhYCikuq8obbfAH6Rjj/JbPX5TCaNb", amazonStepFunctionsConfig);
                        var res = Task.Run(() => _ = clientSt.StartExecutionAsync(s));
                        var f = res.GetAwaiter().GetResult();*/

                    DefaultLambdaJsonSerializer s = new DefaultLambdaJsonSerializer();
                    
                    dynamic parsedJson = JsonConvert.DeserializeObject(record.Body);
                        string res = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                        var bytes = Encoding.UTF8.GetBytes(res.ToCharArray());
                        MemoryStream memStream = new MemoryStream();
                        memStream.Write(bytes,0, bytes.Length);
                    using (var client = new AmazonLambdaClient(awsCredentials, region))
                         { 
                             var request_ = new InvokeRequest
                             {
                                 FunctionName = "HotchocolateService-GraphQLReceivedMessageFunction-1LOFB2RM9KTPS", // "GreetingTest-GreetingTask-17FWFH6J9RKGF",
                                 InvocationType = InvocationType.RequestResponse,
                                 LogType = LogType.Tail,
                                 Payload = record.Body
                             };

                             var result = Task.Run(() => _ = client.InvokeAsync(request_));
                             var response = result.GetAwaiter().GetResult();
                             bytes = new byte[response.Payload.Length];
                             response.Payload.Seek(0, SeekOrigin.Begin);
                             await response.Payload.ReadAsync(bytes, 0, bytes.Length);
                             var resp = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                             Console.WriteLine($"Response: {resp}");
                         }
                     }

               //  });
               
            }
            catch (Exception e) { Console.WriteLine($"Error: {e.Message}"); };
        }

        //ProcessGraphType
  
    }
}
