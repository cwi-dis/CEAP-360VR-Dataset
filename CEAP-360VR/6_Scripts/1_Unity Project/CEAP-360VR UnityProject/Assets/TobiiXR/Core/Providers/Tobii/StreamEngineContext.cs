using System;

namespace Tobii.XR
{
    public class StreamEngineContext
    {
        public IntPtr Device { get; private set; }
        public IntPtr Api { get; private set; }

        public string Url { get; private set; }

        public StreamEngineContext(IntPtr apiContext, IntPtr deviceContext, string url)
        {
            Api = apiContext;
            Device = deviceContext;
            Url = url;
        }
    }
}