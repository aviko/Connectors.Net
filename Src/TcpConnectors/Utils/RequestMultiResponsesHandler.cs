using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TcpConnectors.Utils
{
    public class RequestMultiResponsesHandler<KEY> : IDisposable
    {
        private class RequestRecord
        {
            internal Action<object, bool, Exception> _action;
        }

        private ConcurrentDictionary<KEY, RequestRecord> _requestsMap { get; set; }
        protected bool _disposed = false;

        public RequestMultiResponsesHandler()
        {
            _requestsMap = new ConcurrentDictionary<KEY, RequestRecord>();
        }

        public void Request(KEY key, Action actionReq, Action<object, bool, Exception> actionRes, int timeOutMilliseconds = -1)
        {

            _requestsMap[key] = new RequestRecord { _action = actionRes };

            //perform action here
            actionReq();
        }

        public void HandleResponse(KEY key, object response, bool isLast)
        {
            if (_disposed) return;
            _requestsMap.TryGetValue(key, out var requestRecord);

            if (requestRecord != null)
            {
                requestRecord._action(response, isLast, null);
            }

            if (isLast) _requestsMap.TryRemove(key, out requestRecord);
        }

        public void HandleExceptionResponse(KEY key, Exception exception)
        {
            if (_disposed) return;
            _HandleExceptionResponse(key, exception);
        }

        private void _HandleExceptionResponse(KEY key, Exception exception)
        {
            _requestsMap.TryRemove(key, out var requestRecord);

            if (requestRecord != null)
            {
                requestRecord._action(null, true, exception);
            }
        }

        public void HandleExceptionResponseForAll(Exception exception)
        {
            foreach (var key in _requestsMap.Keys)
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
                HandleExceptionResponseForAll(new ObjectDisposedException("RequestMultiResponsesHandler"));
            }
            catch { }
        }
    }
}
