using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using HotChocolate;
using HotChocolate.Language;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon;
using Amazon.Extensions.NETCore.Setup;


namespace MessageProcessor
{
    public interface IGraphQLServiceWorker
    {
        void DoWork();
    }

 
    public class GraphQLServiceWorker : IGraphQLServiceWorker
    {
        private int executionCount = 0;
        private readonly ILogger _logger;
        private Channel<string> _channel;
      
        public GraphQLServiceWorker(ILogger logger, Channel<string> channel)
        {
            _channel = channel;
            _logger = logger;
        }

        public async IAsyncEnumerator<string> GetAsyncEnumerator(
          CancellationToken cancellationToken = default)
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return await _channel.Reader.ReadAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }


        public async void DoWork()
        {
           // ExecuteStepFunctionUsingDefaultProfileWithIAMStepFunctionsFullAccessInIAMConsole();

            while (true)
            {
                var en = GetAsyncEnumerator();
                await en.MoveNextAsync();
                string val = "";
                if(en.Current != null && !en.Current.Contains("connection_init"))
                {
                    Console.WriteLine("await en " + en.Current);
                    //   var docNode = ParseMessage(en.Current);
                    val = en.Current;
                    var amazonStepFunctionsConfig = new AmazonStepFunctionsConfig { RegionEndpoint = RegionEndpoint.USEast1 };
                    StartExecutionRequest s = new StartExecutionRequest();
                    s.StateMachineArn = "arn:aws:states:us-east-1:280449388741:stateMachine:StateMachine-TJxMe1Ai0Gqk";

                    s.Input = "{\" Name \" : " + val + "}";
                    s.Name = $"SchedulingEngine_{Guid.NewGuid().ToString("N")}";
           ;

                    var client = new AmazonStepFunctionsClient( amazonStepFunctionsConfig);
                    Console.WriteLine($"Record: {val}");

                    ListStateMachinesRequest l = new ListStateMachinesRequest();
                    var resp = await client.ListStateMachinesAsync(l);
                    _logger.LogInformation($"RESP {resp.StateMachines[0]}");
                    //  client.StartExecutionAsync(s).Wait();
                    var taskStartExecutionResponse = client.StartExecutionAsync(s).ConfigureAwait(false).GetAwaiter().GetResult();
                      Console.WriteLine(taskStartExecutionResponse);
                   
                    //  ProcessGraphQLType();
                }
            }
           
        }

        private void ProcessGraphQLType()
        {
            _logger.LogInformation("here in logger");
            var schemaCode = @"type Query { message: Message }
            type MessageFrom { id: String displayName: String }
            type Message { content: String sentAt: String messageFrom: MessageFrom }";
            ISchema schema = CreateSchema(schemaCode);
            _logger.LogInformation($"Schema:{schema.Description}");
            
            FieldNodeMapper mapper = new FieldNodeMapper();
            mapper.ParseTypesFromSchema(schema);
            IObservable<IFieldResolver> fieldStream = mapper.GetFieldStream();
           // fieldStream.Subscribe(val => Console.WriteLine(val));
               /*         var server = new RedisProcessor<string, Message>(redisReceiver, objGraph, "{message {content messageFrom {id displayName} sentAt }}", schemaCode,
                            "MessageAdded", document, handler);
                        await server.Start();*/
        }

        private ISchema CreateSchema(string SchemaCode)
        {
            ISchema schema = SchemaBuilder.New()
               .AddDocument(sp =>
                   Utf8GraphQLParser.Parse(SchemaCode))
               .Use(next => context =>
               {
                   context.Result = "foo";
                   return default;
               })
               .Create();

            return schema;
        }

        private  DocumentNode ParseMessage(string req)
        {
            byte[] msg_ = Encoding.UTF8.GetBytes(req);
            var parser = new Utf8GraphQLParser(
              msg_, ParserOptions.Default);
            DocumentNode document = parser.Parse();
            return document;
        }
    }
}
