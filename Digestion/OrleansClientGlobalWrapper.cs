using Orleans.Runtime;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digestion
{
    class OrleansClientGlobalWrapper : IDisposable
    {
        private readonly static TraceSource _traceSource = new TraceSource("ProvingGroundActorDirectMessaging.OrleansClientGlobalWrapper", SourceLevels.Error);

        public static bool IsInitialized
        {
            get
            {
                return Instance._isInitialized;
            }
            private set
            {
                Instance._isInitialized = value;
            }
        }

        //public bool IsConnected
        //{
        //	get
        //	{
        //		return _isConnected;
        //	}
        //	private set
        //	{
        //		_isConnected = value;
        //	}
        //}

        public static event EventHandler<EventArgs> OrleansInitComplete;

        private const int MaxOrleansInitRetries = 5000;
        private bool _isInitialized = false;
        //private bool _isConnected = false;
        private volatile bool IsTryingInitialize = false;

        #region Singleton
        private static readonly Lazy<OrleansClientGlobalWrapper> lazy =
            new Lazy<OrleansClientGlobalWrapper>(() => new OrleansClientGlobalWrapper());

        private static OrleansClientGlobalWrapper Instance { get { return lazy.Value; } }
        #endregion Singleton

        private OrleansClientGlobalWrapper()
        {
        }

        public static bool EnsureInitialized()
        {
            if (!IsInitialized)
            {
                Instance.BeginInitOrleans();
            }
            return IsInitialized;
        }

        private void BeginInitOrleans()
        {
            Task t =
            Task.Run(() =>
            {
                PrepareOrleansClient();
            });

            // 			try
            // 			{
            //                 PrepareOrleansClient();
            //             }
            //             catch (Exception ex)
            //             {
            //                 _traceSource.TraceData(TraceEventType.Error, 0, ex);
            //                 throw;
            //             }
        }


        private void PrepareOrleansClient()
        {
            if (!IsTryingInitialize)
            {
                IsTryingInitialize = true;

                //var startupPolicyCircuitBreaker = Policy
                //    .Handle<AggregateException>(a =>
                //    {
                //        _traceSource.TraceData(TraceEventType.Error, 0, a);
                //        return true;
                //    })
                //    .Or<SiloUnavailableException>(a=>
                //    {
                //        _traceSource.TraceData(TraceEventType.Error, 0, a);
                //        return true;
                //    })
                //    .CircuitBreaker(3, TimeSpan.FromSeconds(15));

                var startupPolicy = Policy
                    .Handle<Exception>()
                    .Or<SiloUnavailableException>()
                    .WaitAndRetry(
                    5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, context) =>
                    {
                        _traceSource.TraceData(TraceEventType.Error, 0, exception);
                    }
                    );

                startupPolicy
                    .Execute(() =>
                    {
                        if (Orleans.GrainClient.IsInitialized)
                        {
                            Orleans.GrainClient.Uninitialize();
                        }

                        var config = Orleans.Runtime.Configuration.ClientConfiguration.StandardLoad();

                        Orleans.GrainClient.Initialize(config);
                    });

                IsInitialized = true;
                IsTryingInitialize = false;
                FireOrleansInitComplete();
            }
        }

        protected void FireOrleansInitComplete()
        {
            EventHandler<EventArgs> threadSafeEvent = OrleansInitComplete;
            if (threadSafeEvent != null)
            {
                try
                {
                    threadSafeEvent(this, new EventArgs());
                }
                catch (Exception exc)
                {   //log and MUTE
                    _traceSource.TraceEvent(TraceEventType.Error, 57, "FireOrleansInitComplete: " + exc.Message);
                    // developer: fix your code - you probably did an AddWatcher 
                    // without a corresponding RemoveWatcher
                    Debug.Assert(false);
                }
            }
        }

        internal static void RequestReinitialization()
        {
            _traceSource.TraceEvent(TraceEventType.Error, 57, "Reinitialize started");

            IsInitialized = false;

            //Instance.BeginInitOrleans();
            Instance.PrepareOrleansClient();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~OrleansClientGlobalWrapper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
