using Orleans;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerGrains
{
    public class FoodObserver<T> : IAsyncObserver<T>
    {
        private readonly DigestionGrain hostingGrain;

        internal FoodObserver(DigestionGrain hostingGrain)
        {
            this.hostingGrain = hostingGrain;
        }

        public Task OnCompletedAsync()
        {
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return TaskDone.Done;
        }

        public Task OnNextAsync(T item, StreamSequenceToken token = null)
        {
            return TaskDone.Done;
        }
    }
}
