using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;
using System.Threading.Tasks;
using StreamingGrainInterfaces;
using DTOData;
using System.Collections.Generic;
using System.Linq;

namespace StreamerUnitTests
{
    [DeploymentItem("OrleansConfigurationForTesting.xml")]
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [DeploymentItem("StreamerGrains.dll")]
    [DeploymentItem("OrleansProviders.dll")]
    [TestClass]
    public class UnitTest1 : TestingSiloHost
    {
            private readonly Guid streamId = Guid.NewGuid(); 
            private readonly string streamProvider = "xxx";

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

            var feeds = new List<Task>();
            for (int i = 0; i < 500; i++)
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

            Assert.AreEqual<int>(taskwastes.Result.Count(), 500);
        }

        [TestMethod]
        public void TestMouthFeeding()
        {
            var rnd = new Random();

            var mouthName = "TestMouth" + rnd.Next().ToString();

            var grain = GrainFactory.GetGrain<IIngestionGrain>(mouthName);

            var feeds = new List<Task>();
            for (int i = 0; i < 500; i++)
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

            Assert.AreEqual<int>(expGrain.Dump().GetAwaiter().GetResult().Count(), 500);


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
