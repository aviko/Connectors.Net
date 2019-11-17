using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
    }
}
