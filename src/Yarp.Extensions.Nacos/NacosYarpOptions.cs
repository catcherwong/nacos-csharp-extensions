namespace Yarp.Extensions.Nacos
{
    using System.Collections.Generic;

    public class NacosYarpOptions
    {
        /// <summary>
        /// Specify group name list that need to linsten
        /// </summary>
        public List<string> GroupNameList { get; set; } = new List<string>();

        /// <summary>
        /// Specify the count that query the service from nacos server
        /// </summary>
        public int PreCount { get; set; } = 50;

        /// <summary>
        /// Whether to enable auto refresh service from nacos server
        /// </summary>
        public bool EnableAutoRefreshService { get; set; } = false;

        /// <summary>
        /// When enable auto refresh, set the period to query service
        /// </summary>
        public int AutoRefreshPeriod { get; set; } = 1800;
    }
}