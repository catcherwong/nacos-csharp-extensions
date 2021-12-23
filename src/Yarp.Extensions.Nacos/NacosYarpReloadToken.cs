namespace Yarp.Extensions.Nacos
{
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Threading;

    public class NacosYarpReloadToken : IChangeToken
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public bool ActiveChangeCallbacks => true;

        public bool HasChanged => _cts.IsCancellationRequested;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => _cts.Token.Register(callback, state);

        public void OnReload() => _cts.Cancel();
    }
}