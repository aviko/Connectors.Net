using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpConnectors.Utils
{
    public class AsyncRequestResponseHandler<KEY, RES> : IDisposable
    {

        private ConcurrentDictionary<KEY, TaskCompletionSource<RES>> _taskCompletionMap { get; set; }
        protected bool _disposed = false;

        public AsyncRequestResponseHandler()
        {
            _taskCompletionMap = new ConcurrentDictionary<KEY, TaskCompletionSource<RES>>();
        }


        public async Task<RES> Request(KEY key, Action action, int timeOutMilliseconds = Timeout.Infinite)
        {

            var taskCompletion = new TaskCompletionSource<RES>();

            _taskCompletionMap[key] = taskCompletion;

            if (timeOutMilliseconds != Timeout.Infinite)
            {
                var ct = new CancellationTokenSource(timeOutMilliseconds);
                ct.Token.Register(() => TaskCompletionCancelled(key), useSynchronizationContext: false);
            }

            //perform action here
            action();

            return await taskCompletion.Task;
        }
        private void TaskCompletionCancelled(KEY key)
        {
            _taskCompletionMap.TryRemove(key, out var taskCompletion);

            if (taskCompletion != null)
            {
                taskCompletion.TrySetCanceled();
            }
        }

        public void HandleResponse(KEY key, RES response)
        {
            if (_disposed) return;
            _taskCompletionMap.TryRemove(key, out var taskCompletion);

            if (taskCompletion != null)
            {
                taskCompletion.SetResult(response);
            }
        }
        public void HandleExceptionResponse(KEY key, Exception exception)
        {
            if (_disposed) return;
            _HandleExceptionResponse(key, exception);
        }

        private void _HandleExceptionResponse(KEY key, Exception exception)
        {
            _taskCompletionMap.TryRemove(key, out var taskCompletion);
            if (taskCompletion != null)
            {
                taskCompletion.SetException(exception);
            }
        }

        public void HandleExceptionResponseForAll(Exception exception)
        {
            foreach (var key in _taskCompletionMap.Keys)
            {
                _HandleExceptionResponse(key, exception);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            Dispose(true);
        }
        public virtual void Dispose(bool disposing)
        {
            try
            {
                HandleExceptionResponseForAll(new ObjectDisposedException("AsyncRequestResponseHandler"));
            }
            catch { }
        }
    }
}
