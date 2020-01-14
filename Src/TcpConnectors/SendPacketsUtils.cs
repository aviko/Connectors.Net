using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnectors
{
    public static class SendPacketsUtils
    {
        public interface IClient2ServerPacket
        {
        }

        public interface IServer2ClientPacket
        {
        }

        public interface IMultiResponseListPacket<T> : IServer2ClientPacket
        {
            List<T> Records { get; set; }
        }

        public static Dictionary<Tuple<int, int>, Type> GetClient2ServerMapping(Assembly assembly)
        {
            return GetMapping(assembly, typeof(IClient2ServerPacket));
        }

        public static Dictionary<Tuple<int, int>, Type> GetServer2ClientMapping(Assembly assembly)
        {
            return GetMapping(assembly, typeof(IServer2ClientPacket));
        }

        private static Dictionary<Tuple<int, int>, Type> GetMapping(Assembly assembly, Type interfaceType)
        {
            var lookupTypes = assembly.GetTypes()
                .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface)
                .ToList();

            var res = new Dictionary<Tuple<int, int>, Type>();

            foreach (var lookupType in lookupTypes)
            {
                GetModuleCommandValues(lookupType, out var module, out var command);
                if (module > 0 && command > 0)
                {
                    res.Add(new Tuple<int, int>(module, command), lookupType);
                }
            }

            return res;
        }

        public static void GetModuleCommandValues(Type type, out byte module, out byte command)
        {
            module = 0;
            command = 0;

            var consts = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(byte))
                .Select(x => new { Name = x.Name, Value = (byte)x.GetRawConstantValue() })
                .ToDictionary(x => x.Name, x => x.Value);

            consts.TryGetValue("MODULE", out module);
            consts.TryGetValue("COMMAND", out command);


        }


        public static void Send(this ClientConnector clientConnector, IClient2ServerPacket client2ServerPacket)
        {
            GetModuleCommandValues(client2ServerPacket.GetType(), out var module, out var command);
            clientConnector.Send(module, command, client2ServerPacket);
        }

        public static object SendRequest(this ClientConnector clientConnector, IClient2ServerPacket client2ServerPacket)
        {
            GetModuleCommandValues(client2ServerPacket.GetType(), out var module, out var command);
            return clientConnector.SendRequest(module, command, client2ServerPacket);
        }

        public static async Task<object> SendRequestAsync(this ClientConnector clientConnector, IClient2ServerPacket client2ServerPacket)
        {
            GetModuleCommandValues(client2ServerPacket.GetType(), out var module, out var command);
            return await clientConnector.SendRequestAsync(module, command, client2ServerPacket);
        }

        public static void Send(this ServerConnectors serverConnectors, Func<ServerConnectorContext, bool> filter, IServer2ClientPacket server2ClientPacket)
        {
            GetModuleCommandValues(server2ClientPacket.GetType(), out var module, out var command);
            serverConnectors.Send(filter, module, command, server2ClientPacket);
        }

        public static void SendMultiResponse<T>(this ServerConnectorContext serverConnectorContext, int module, int command, int requestId,
            RequestMultiResponsesServerCallback callback, IMultiResponseListPacket<T> packet, List<T> list, int chunkSize = 1000)
        {

            var chunks = Split(list, chunkSize);

            int count = 0; 
            foreach (var chunk in chunks)
            {
                packet.Records = chunk;
                count += chunk.Count;

                callback(
                    serverConnectorContext, module, command, requestId,
                    packet,
                    count == list.Count,
                    count,
                    list.Count, 
                    null);
            }
        }


        private static List<List<T>> Split<T>(IEnumerable<T> collection, int size)
        {
            var chunks = new List<List<T>>();
            var count = 0;
            var temp = new List<T>();

            foreach (var element in collection)
            {
                if (count++ == size)
                {
                    chunks.Add(temp);
                    temp = new List<T>();
                    count = 1;
                }
                temp.Add(element);
            }
            chunks.Add(temp);

            return chunks;
        }


    }
}
