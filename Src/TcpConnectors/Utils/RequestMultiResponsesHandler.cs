using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TcpConnectors.Utils
{
    public delegate void RequestMultiResponsesCallback(object packet, bool isLast, int nProgress, int nTotal, Exception ex);

    public class RequestMultiResponsesHandler<KEY> : IDisposable
    {

        private ConcurrentDictionary<KEY, RequestMultiResponsesCallback> _requestsMap { get; set; }
        protected bool _disposed = false;

        public RequestMultiResponsesHandler()
        {
            _requestsMap = new ConcurrentDictionary<KEY, RequestMultiResponsesCallback>();
        }

        public void Request(
            KEY key,
            Action actionReq,
            RequestMultiResponsesCallback actionRes,
            //object requestData = null,
            int timeOutMilliseconds = -1)
        {

            _requestsMap[key] = actionRes;

            //perform action here
            actionReq();
        }

        public void HandleResponse(KEY key, object response, bool isLast, int nRecieved, int nTotal)
        {
            if (_disposed) return;

            RequestMultiResponsesCallback action;

            if (isLast == false) _requestsMap.TryGetValue(key, out action); //get and keep for next packets...
            else _requestsMap.TryRemove(key, out action); //get and remove, no more packets

            action?.Invoke(response, isLast, nRecieved, nTotal, null);

        }

        public void HandleExceptionResponse(KEY key, Exception exception)
        {
            if (_disposed) return;
            _HandleExceptionResponse(key, exception);
        }

        private void _HandleExceptionResponse(KEY key, Exception exception)
        {
            _requestsMap.TryRemove(key, out var action);

            action?.Invoke(null, true, 0, 0, exception);
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
