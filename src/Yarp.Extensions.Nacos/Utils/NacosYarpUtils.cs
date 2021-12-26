namespace Yarp.Extensions.Nacos
{
    using System.Collections.Generic;

    internal static class NacosYarpUtils
    {
        internal static string CreateClusterId(string groupName, string serviceName) => $"{groupName}@@{serviceName}";

        internal static HashSet<string> BuildServiceSet(Dictionary<string, List<string>> dict)
        {
            var set = new HashSet<string>();
            foreach (var item in dict)
            {
                var groupName = item.Key;
                var services = item.Value;

                foreach (var service in services)
                {
                    set.Add(CreateClusterId(groupName, service));
                }
            }

            return set;
        }
    }
}
