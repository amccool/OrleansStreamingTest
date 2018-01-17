using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;
using System.Threading.Tasks;
//using StreamingGrainInterfaces;
using DTOData;
using System.Collections.Generic;
using System.Linq;
using Orleans.Streams;
using Orleans;
using System.Diagnostics;
using System.IO;
using Orleans.Runtime.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace StreamerUnitTests
{
    //[DeploymentItem("OrleansConfigurationForTesting.xml")]
    //[DeploymentItem("ClientConfigurationForTesting.xml")]
    //[DeploymentItem("StreamerGrains.dll")]
    //[DeploymentItem("OrleansProviders.dll")]
    //[DeploymentItem("PubSubStoreSQLStorageProvider.dll")]
    ////this fixed the "deserializer" problem
    //[DeploymentItem("DTOData.dll")]

    ////all the supporting assemblies
    //[DeploymentItem("Microsoft.Azure.SqlDatabase.ElasticScale.Client.dll")]



    public class UnitTest1 : IClassFixture<UnitTest1.Fixture>
    {

        private readonly Fixture _fixture;
        private readonly ITestOutputHelper output;

        public class Fixture : BaseTestClusterFixture
        {
            protected override TestCluster CreateTestCluster()
            {
                TimeSpan _timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(10);

                //TODO: the RGS config is still all static fields so we have to limit the test cluster to 1 node for now
                var options = new TestClusterOptions(1);
                //WARN: a better test would be to use the default () for 2 silos!

                options.ClusterConfiguration.AddMemoryStorageProvider("Facility");
                //options.ClusterConfiguration.AddMemoryStorageProvider("NopRefresher");
                //options.ClusterConfiguration.AddMemoryStorageProvider("PubSubStore");
                //options.ClusterConfiguration.AddMemoryStorageProvider("ACE");
                //options.ClusterConfiguration.AddMemoryStorageProvider("Annunciation");
                //options.ClusterConfiguration.AddMemoryStorageProvider("Tag");
                //options.ClusterConfiguration.AddMemoryStorageProvider("CancelRegistration");
                //options.ClusterConfiguration.AddMemoryStorageProvider("ServiceSetByPCC");

                //var connectionString = @"Data Source=.;Database=OrleansStorage;Integrated Security=True;";
                //options.ClusterConfiguration.AddSimpleSQLStorageProvider("Facility", connectionString, "true");
                //options.ClusterConfiguration.AddSimpleSQLStorageProvider("NopRefresher", connectionString, "true");
                //options.ClusterConfiguration.AddSimpleSQLStorageProvider("PubSubStore", connectionString, "true");
                //options.ClusterConfiguration.AddSimpleSQLStorageProvider("ACE", connectionString, "true");
                //options.ClusterConfiguration.AddSimpleSQLStorageProvider("Annunciation", connectionString, "true");
                //options.ClusterConfiguration.AddSimpleSQLStorageProvider("Tag", connectionString, "true");
                //options.ClusterConfiguration.AddSimpleSQLStorageProvider("CancelRegistration", connectionString, "true");
                //options.ClusterConfiguration.AddSimpleSQLStorageProvider("ServiceSetByPCC", connectionString, "true");

                //options.ClusterConfiguration.AddSimpleMessageStreamProvider(BrcStreamNamespaceHelpers.StreamProvider);
                //options.ClusterConfiguration.AddSimpleMessageStreamProvider(R5ClientStreamNamespaceHelpers.StreamProvider);
                //options.ClusterConfiguration.AddSimpleMessageStreamProvider("PHONEMOCKOUTPUT");

                options.ClusterConfiguration.Globals.ClientDropTimeout = _timeout;
                options.ClusterConfiguration.ApplyToAllNodes(o => o.DefaultTraceLevel = Orleans.Runtime.Severity.Warning);

                //options.ClientConfiguration.AddSimpleMessageStreamProvider(BrcStreamNamespaceHelpers.StreamProvider);
                //options.ClientConfiguration.AddSimpleMessageStreamProvider(R5ClientStreamNamespaceHelpers.StreamProvider);
                //options.ClientConfiguration.AddSimpleMessageStreamProvider("PHONEMOCKOUTPUT");

                options.ClientConfiguration.DefaultTraceLevel = Orleans.Runtime.Severity.Warning;
                options.ClientConfiguration.ClientDropTimeout = _timeout;

                //dependency injection for the Facility Grain
                //even if we dont actually ask for a facility grain, the implicit stream subscription will start it
                //options.ClusterConfiguration.UseStartupType<TestStartup>();

                return new TestCluster(options);
            }
        }



        private const string strmProvName = "SMSProvider";
        private const int numToLoop = 70;
        private readonly Guid streamId = Guid.NewGuid(); 


        public UnitTest1(UnitTest1.Fixture fixture, ITestOutputHelper output)
        {
            this.output = output;
            this._fixture = fixture;
        }


        //[TestMethod]
        //public void SingleIngestionTest()
        //{
        //    var mouthName = "TestMouth";

        //    IIngestionGrain grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
        //    grain.PrepareFoodRoute(Guid.NewGuid()).Wait();

        //    var fedtask  = grain.FeedMe(new DTOData.Food()
        //    {
        //        name = "carrot",
        //        order = 0,
        //        //type = DTOData.FoodPyramid.Vegatable
        //    });

        //    fedtask.Wait();

        //    //Assert.IsNotNull(reply, "Grain replied with some message");
        //    //string expected = string.Format("You said: '{0}', I say: Hello!", greeting);
        //    //Assert.AreEqual(expected, reply, "Grain replied with expected message");

        //}

        //[TestMethod]
        //public void GrainToGrainStreamCountTest()
        //{
        //    var rnd = new Random();
        //    var mouthName = "TestMouth" + rnd.Next().ToString();

        //    var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
        //    grain.PrepareFoodRoute(Guid.NewGuid()).Wait();

        //    var feeds = new List<Task>();
        //    for (int i = 0; i < numToLoop; i++)
        //    {
        //        feeds.Add(grain.FeedMe(new DTOData.Food()
        //        {
        //            name = "carrot",
        //            order = i,
        //            //type = DTOData.FoodPyramid.Vegatable
        //        }));
        //    }

        //    var expul = GrainFactory.GetGrain<IExpulsionGrain>(mouthName);
        //    Task<IEnumerable<Waste>> taskwastes = expul.Dump();
        //    Task.WhenAll(feeds).Wait();
        //    taskwastes.Wait();

        //    Assert.AreEqual<int>(numToLoop, taskwastes.Result.Count());
        //}



        //[TestMethod]
        //public void GrainToGrainStreamCountAwaiterTest()
        //{
        //    var rnd = new Random();
        //    var mouthName = "TestMouth" + rnd.Next().ToString();

        //    var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
        //    grain.PrepareFoodRoute(Guid.NewGuid()).Wait();

        //    var expGrain = GrainFactory.GetGrain<IExpulsionGrain>(mouthName);

        //    var dumpTask = expGrain.Dump();

        //    var feeds = new List<Task>();
        //    for (int i = 0; i < numToLoop; i++)
        //    {
        //        feeds.Add(grain.FeedMe(new DTOData.Food()
        //        {
        //            name = "carrot",
        //            order = i,
        //            //type = DTOData.FoodPyramid.Vegatable
        //        }));
        //    }

        //    Task.WhenAll(feeds).Wait();

        //    Assert.AreEqual<int>(numToLoop, dumpTask.GetAwaiter().GetResult().Count());
        //}


        //[TestMethod]
        //public void ClientGrainStreamProviderConfiguationTest()
        //{
        //    var streamProvs = Orleans.GrainClient.GetStreamProviders();

        //    var strprov = Orleans.GrainClient.GetStreamProvider("SMSProvider");
        //    var strm = strprov.GetStream<int>(Guid.Parse("ffffffffffffffffffff"), "topic");


        //    Assert.AreEqual<int>(1, streamProvs.Count());
        //}


        //[TestMethod]
        //public void ClientStreamFromGrainTest()
        //{
        //    StreamSequenceToken tok;
        //    IStreamProvider clientSMSProv = Orleans.GrainClient.GetStreamProvider(strmProvName);

        //    var rnd = new Random();
        //    var mouthName = "TestMouth" + rnd.Next().ToString();

        //    var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
        //    var strmId = Guid.NewGuid();
        //    grain.PrepareFoodRoute(strmId).Wait();

        //    var foodstream = clientSMSProv.GetStream<Food>(strmId, mouthName);

        //    var foods = new List<Food>();
        //    var subHandle = foodstream.SubscribeAsync(
        //        (a,b) =>
        //    {
        //        foods.Add(a);
        //        tok = b;
        //        return TaskDone.Done;
        //    });

        //    var feeds = new List<Task>();
        //    for (int i = 0; i < numToLoop; i++)
        //    {
        //        feeds.Add(grain.FeedMe(new Food()
        //        {
        //            name = "carrot",
        //            order = i,
        //            //type = FoodPyramid.Vegatable
        //        }));
        //    }


        //    subHandle.Wait();
        //    Task.WhenAll(feeds).Wait();

        //    Assert.AreEqual<int>(numToLoop, foods.Count());

        //}

    //    [TestMethod]
    //    public void EnumerableGenerateFoodsTest()
    //    {
    //        var rnd = new Random();
    //        var mouthName = "TestMouth" + rnd.Next().ToString();
    //        var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
    //        grain.PrepareFoodRoute(Guid.NewGuid()).Wait();
    //        var looper = Enumerable.Range(0, numToLoop);

    //        IEnumerable<Task> foodTasksQuery =
    //from x in looper
    //select grain.FeedMe(new Food()
    //{
    //    name = "carrot",
    //    order = x,
    //    //type = FoodPyramid.Vegatable
    //});

    //        // Use ToArray to execute the query and start the download tasks.
    //        Task[] foodTasks = foodTasksQuery.ToArray();
    //        Task.WhenAll(foodTasks).Wait();
    //    }



        //[TestMethod]
        //public void StreamingTests_Consumer_Producer()
        //{
        // consumer joins first, producer later
        //var consumer = GrainFactory.GetGrain<ISampleStreaming_ConsumerGrain>(Guid.NewGuid());
        //await consumer.BecomeConsumer(streamId, StreamNamespace, streamProvider);



        //var producer = GrainFactory.GetGrain<ISampleStreaming_ProducerGrain>(Guid.NewGuid());
        //await producer.BecomeProducer(streamId, StreamNamespace, streamProvider);

        //await producer.StartPeriodicProducing();

        //await Task.Delay(TimeSpan.FromMilliseconds(1000));

        //await producer.StopPeriodicProducing();

        //await TestingUtils.WaitUntilAsync(lastTry => CheckCounters(producer, consumer, lastTry), _timeout);

        //await consumer.StopConsuming();
        //}


        [TestMethod]
        public void DifferentTypesInAStreamTest()
        {
            StreamSequenceToken tok;
            IStreamProvider clientSMSProv = Orleans.GrainClient.GetStreamProvider(strmProvName);

            var strmId = Guid.NewGuid();

            var foodstream = clientSMSProv.GetStream<Food>(strmId, "Food");
            var substancestream = clientSMSProv.GetStream<Substance>(strmId, "Substance");
            var wastestream = clientSMSProv.GetStream<Waste>(strmId, "Waste");
            var vitaminstream = clientSMSProv.GetStream<Vitamin>(strmId, "Vitamin");

            var foods = new List<Food>();
            var foodHandle = foodstream.SubscribeAsync(
                (a, b) =>
                {
                    foods.Add(a);
                    tok = b;
                    return TaskDone.Done;
                });

            var substances = new List<Substance>();
            var subHandle = substancestream.SubscribeAsync(
                (a, b) =>
                {
                    substances.Add(a);
                    tok = b;
                    return TaskDone.Done;
                });


            var wastes = new List<Waste>();
            var wasteHandle = wastestream.SubscribeAsync(
                (a, b) =>
                {
                    wastes.Add(a);
                    tok = b;
                    return TaskDone.Done;
                });

            var vitamins = new List<Vitamin>();
            var vitaminHandle = vitaminstream.SubscribeAsync(
                (a, b) =>
                {
                    vitamins.Add(a);
                    tok = b;
                    return TaskDone.Done;
                });


            foodstream.OnNextAsync(new Food { name = "c", order = 1,
                //type = FoodPyramid.Dairy 
            }).Wait();

            substancestream.OnNextAsync(new Substance { vitamins = new List<Vitamin>() }).Wait();

            wastestream.OnNextAsync(new Waste { length = 1, numPoops = 1, width = 1 }).Wait();



        }

    }
}
