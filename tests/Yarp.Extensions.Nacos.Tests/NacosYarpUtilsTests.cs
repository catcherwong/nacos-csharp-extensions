namespace Yarp.Extensions.Nacos.Tests
{
    using System.Collections.Generic;
    using Xunit;

    public class NacosYarpUtilsTests
    {
        [Fact]
        public void BuildServiceSet_Should_Succeed()
        {
            Dictionary<string, List<string>> dict = new();
            List<string> list = new List<string> { "A", "B", "A" };
            dict.Add("g1", list);
            dict.Add("g2", list);

            var set = NacosYarpUtils.BuildServiceSet(dict);

            Assert.Equal(4, set.Count);

            Assert.Contains("g1@@A", set);
            Assert.Contains("g1@@B", set);
            Assert.Contains("g2@@A", set);
            Assert.Contains("g2@@B", set);
        }
    }
}