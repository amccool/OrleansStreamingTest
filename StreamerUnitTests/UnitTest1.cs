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
        public void TestIngestion()
        {
            var mouthName = "TestMouth";

            IIngestionGrain grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            grain.PrepareFoodRoute().Wait();

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
        public void FoodStreams()
        {
            var rnd = new Random();

            var mouthName = "TestMouth" + rnd.Next().ToString();

            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            grain.PrepareFoodRoute().Wait();

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


            var expul = GrainFactory.GetGrain<IExpulsionGrain>(mouthName);


            expul.LinkToDigestion(streamId);


            Task<List<Waste>> taskwastes = expul.Dump();

            taskwastes.Wait();

            Assert.AreEqual<int>(numToLoop, taskwastes.Result.Count());
        }

        [TestMethod]
        public void TestMouthFeeding()
        {
            var rnd = new Random();

            var mouthName = "TestMouth" + rnd.Next().ToString();

            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            grain.PrepareFoodRoute().Wait();

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


            var expGrain = GrainFactory.GetGrain<IExpulsionGrain>(mouthName);

            Assert.AreEqual<int>(numToLoop, expGrain.Dump().GetAwaiter().GetResult().Count());
        }


        [TestMethod]
        public void TestClientGrainConnection()
        {
            var streamProvs = Orleans.GrainClient.GetStreamProviders();

            Assert.AreEqual<int>(1, streamProvs.Count());
        }


        [TestMethod]
        public void TestClientStreamHookup()
        {
            StreamSequenceToken tok;
            IStreamProvider clientSMSProv = Orleans.GrainClient.GetStreamProvider(strmProvName);

            var rnd = new Random();
            var mouthName = "TestMouth" + rnd.Next().ToString();

            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            var strmId = grain.PrepareFoodRoute();
            strmId.Wait();

            var foodstream = clientSMSProv.GetStream<Food>(strmId.Result, mouthName);

            List<Food> foods = new List<Food>();
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
        public void GenerateFoods()
        {
            var rnd = new Random();
            var mouthName = "TestMouth" + rnd.Next().ToString();
            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);
            grain.PrepareFoodRoute().Wait();
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

    }
}
