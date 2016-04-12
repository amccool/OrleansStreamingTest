using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;
using System.Threading.Tasks;
using StreamingGrainInterfaces;
using DTOData;
using System.Collections.Generic;
using System.Linq;
using Orleans.Streams;
using Orleans;
using System.Diagnostics;
using System.IO;

namespace StreamerUnitTests
{
    [DeploymentItem("OrleansConfigurationForTesting.xml")]
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [DeploymentItem("StreamerGrains.dll")]
    [DeploymentItem("OrleansProviders.dll")]
    [DeploymentItem("PubSubStoreSQLStorageProvider.dll")]

    //all the supporting assemblies
    [DeploymentItem("Microsoft.Azure.SqlDatabase.ElasticScale.Client.dll")]



    [TestClass]
    public class UnitTest1 : TestingSiloHost
    {
        private const string strmProvName = "SMSProvider";
        private const int numToLoop = 70;
        private readonly Guid streamId = Guid.NewGuid(); 

        public UnitTest1()
            : base(new TestingSiloOptions
            {
                StartFreshOrleans = true,
                SiloConfigFile = new FileInfo("OrleansConfigurationForTesting.xml"),
            },
            new TestingClientOptions()
            {
                ClientConfigFile = new FileInfo("ClientConfigurationForTesting.xml")
            })
        {
        }



        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            StopAllSilos();
        }


        [TestMethod]
        public void SingleIngestionTest()
        {
            var mouthName = "TestMouth";

            IIngestionGrain grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            grain.PrepareFoodRoute(Guid.NewGuid()).Wait();

            var fedtask  = grain.FeedMe(new DTOData.Food()
            {
                name = "carrot",
                order = 0,
                type = DTOData.FoodPyramid.Vegatable
            });

            fedtask.Wait();

            //Assert.IsNotNull(reply, "Grain replied with some message");
            //string expected = string.Format("You said: '{0}', I say: Hello!", greeting);
            //Assert.AreEqual(expected, reply, "Grain replied with expected message");

        }

        [TestMethod]
        public void GrainToGrainStreamCountTest()
        {
            var rnd = new Random();
            var mouthName = "TestMouth" + rnd.Next().ToString();

            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            grain.PrepareFoodRoute(Guid.NewGuid()).Wait();

            var feeds = new List<Task>();
            for (int i = 0; i < numToLoop; i++)
            {
                feeds.Add(grain.FeedMe(new DTOData.Food()
                {
                    name = "carrot",
                    order = i,
                    type = DTOData.FoodPyramid.Vegatable
                }));
            }

            var expul = GrainFactory.GetGrain<IExpulsionGrain>(mouthName);
            Task<List<Waste>> taskwastes = expul.Dump();
            Task.WhenAll(feeds).Wait();
            taskwastes.Wait();

            Assert.AreEqual<int>(numToLoop, taskwastes.Result.Count());
        }



        [TestMethod]
        public void GrainToGrainStreamCountAwaiterTest()
        {
            var rnd = new Random();
            var mouthName = "TestMouth" + rnd.Next().ToString();

            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            grain.PrepareFoodRoute(Guid.NewGuid()).Wait();

            var expGrain = GrainFactory.GetGrain<IExpulsionGrain>(mouthName);

            var dumpTask = expGrain.Dump();

            var feeds = new List<Task>();
            for (int i = 0; i < numToLoop; i++)
            {
                feeds.Add(grain.FeedMe(new DTOData.Food()
                {
                    name = "carrot",
                    order = i,
                    type = DTOData.FoodPyramid.Vegatable
                }));
            }

            Task.WhenAll(feeds).Wait();

            Assert.AreEqual<int>(numToLoop, dumpTask.GetAwaiter().GetResult().Count());
        }


        [TestMethod]
        public void ClientGrainStreamProviderConfiguationTest()
        {
            var streamProvs = Orleans.GrainClient.GetStreamProviders();

            var strprov = Orleans.GrainClient.GetStreamProvider("SMSProvider");
            var strm = strprov.GetStream<int>(Guid.Parse("ffffffffffffffffffff"), "topic");


            Assert.AreEqual<int>(1, streamProvs.Count());
        }


        [TestMethod]
        public void ClientStreamFromGrainTest()
        {
            StreamSequenceToken tok;
            IStreamProvider clientSMSProv = Orleans.GrainClient.GetStreamProvider(strmProvName);

            var rnd = new Random();
            var mouthName = "TestMouth" + rnd.Next().ToString();

            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            var strmId = Guid.NewGuid();
            grain.PrepareFoodRoute(strmId).Wait();

            var foodstream = clientSMSProv.GetStream<Food>(strmId, mouthName);

            var foods = new List<Food>();
            var subHandle = foodstream.SubscribeAsync(
                (a,b) =>
            {
                foods.Add(a);
                tok = b;
                return TaskDone.Done;
            });

            var feeds = new List<Task>();
            for (int i = 0; i < numToLoop; i++)
            {
                feeds.Add(grain.FeedMe(new Food()
                {
                    name = "carrot",
                    order = i,
                    type = FoodPyramid.Vegatable
                }));
            }


            subHandle.Wait();
            Task.WhenAll(feeds).Wait();

            Assert.AreEqual<int>(numToLoop, foods.Count());

        }

        [TestMethod]
        public void EnumerableGenerateFoodsTest()
        {
            var rnd = new Random();
            var mouthName = "TestMouth" + rnd.Next().ToString();
            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            grain.PrepareFoodRoute(Guid.NewGuid()).Wait();
            var looper = Enumerable.Range(0, numToLoop);

            IEnumerable<Task> foodTasksQuery =
    from x in looper
    select grain.FeedMe(new Food()
    {
        name = "carrot",
        order = x,
        type = FoodPyramid.Vegatable
    });

            // Use ToArray to execute the query and start the download tasks.
            Task[] foodTasks = foodTasksQuery.ToArray();
            Task.WhenAll(foodTasks).Wait();
        }



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






        }

    }
}
