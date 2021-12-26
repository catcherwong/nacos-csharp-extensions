namespace Yarp.Extensions.Nacos
{
    using System;

    public class NacosYarpException : Exception
    {
        public NacosYarpException(string message)
            : base(message)
        {
        }
    }
}