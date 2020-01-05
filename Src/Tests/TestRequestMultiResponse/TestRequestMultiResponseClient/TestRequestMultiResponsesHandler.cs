using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TcpConnectors.Utils;

namespace TestRequestMultiResponseClient
{
    class TestRequestMultiResponsesHandler
    {
        private RequestMultiResponsesHandler<int> _requestMultiResponsesHandler = new RequestMultiResponsesHandler<int>();

        public void Run()
        {
            _requestMultiResponsesHandler.Request(1, () => { }, ActionRes);
            while (true)
            {
                Console.WriteLine("Enter input (0-done, n-list of n, exp - exception)");

                var line = Console.ReadLine();

                if (int.TryParse(line, out var n))
                {

                    if (n > 0)
                    {
                        var list = Enumerable.Range(1, n).Select(x => (object)x).ToList();
                        _requestMultiResponsesHandler.HandleResponse(1, list, false);
                    }
                    else
                    {
                        _requestMultiResponsesHandler.HandleResponse(1, null, true);
                    }
                }

                if (line == "exp")
                {
                    _requestMultiResponsesHandler.HandleExceptionResponse(1, new Exception("Exception!"));
                    break;
                }
            }
        }

        public void ActionRes(object res, bool isLast, Exception exception)
        {
            Console.WriteLine($"ActionRes(res={JsonConvert.SerializeObject(res)}, isLast={isLast}, exception={exception?.Message})");
        }
    }
}
