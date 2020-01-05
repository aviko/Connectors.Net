using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TcpConnectors.Utils
{
    public class BlockingRequestResponseHandler<KEY, RES> : IDisposable
    {
        private struct Response<RES1>
        {
            internal RES1 _res;
            internal Exception _exception;
        }

        private ConcurrentDictionary<KEY, ManualResetEvent> _waitHandleMap { get; set; }
        private ConcurrentDictionary<KEY, Response<RES>> _responseMap { get; set; }
        protected bool _disposed = false;

        public BlockingRequestResponseHandler()
        {
            _waitHandleMap = new ConcurrentDictionary<KEY, ManualResetEvent>();
            _responseMap = new ConcurrentDictionary<KEY, Response<RES>>();
        }

        public RES Request(KEY key, Action action, int timeOutMilliseconds = -1)
        {

            ManualResetEvent mrEvent = new ManualResetEvent(false);
            _waitHandleMap[key] = mrEvent;

            //perform action here
            action();

            if (mrEvent.WaitOne(timeOutMilliseconds) == false)//wait to Response
            {
                _waitHandleMap.TryRemove(key, out var unused);
                throw new TimeoutException();
            }


            _responseMap.TryRemove(key, out var response);

            if (response._exception != null)
            {
                throw response._exception;
            }

            return response._res;
        }

        public void HandleResponse(KEY key, RES response)
        {
            if (_disposed) return;
            ManualResetEvent mrEvent = null;
            _waitHandleMap.TryRemove(key, out mrEvent);

            if (mrEvent != null)
            {
                _responseMap[key] = new Response<RES> { _res = response, _exception = null };
                mrEvent.Set(); //continue ServerCommand
            }
        }
        public void HandleExceptionResponse(KEY key, Exception exception)
        {
            if (_disposed) return;
            _HandleExceptionResponse(key, exception);
        }

        private void _HandleExceptionResponse(KEY key, Exception exception)
        {
            ManualResetEvent mrEvent = null;
            _waitHandleMap.TryRemove(key, out mrEvent);

            if (mrEvent != null)
            {
                _responseMap[key] = new Response<RES> { _res = default(RES), _exception = exception };
                mrEvent.Set(); //continue ServerCommand
            }
        }

        public void HandleExceptionResponseForAll(Exception exception)
        {
            foreach (var key in _waitHandleMap.Keys)
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
                HandleExceptionResponseForAll(new ObjectDisposedException("BlockingRequestResponseHandler"));
            }
            catch { }
        }
    }
}
