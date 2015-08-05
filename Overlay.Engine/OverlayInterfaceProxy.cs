using System;
using System.Runtime.Remoting;
using Overlay.Engine.Events;

namespace Overlay.Engine {
    public class OverlayInterfaceProxy : MarshalByRefObject, IDisposable {
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event RegisterOverlayComponentEvent RegisterOverlayComponentHandler;
        public event PushComponentDataEvent PushComponentDataHandler;

        public void RegisterOverlayComponentProxyHandler(RegisterOverlayComponentEventArgs args) {
            RegisterOverlayComponentHandler?.Invoke(args);
        }

        public void PushComponentDataProxyHandler(PushComponentDataEventArgs args) {
            PushComponentDataHandler?.Invoke(args);
        }

        ~OverlayInterfaceProxy() {
            Dispose(false);
        }

        private void Dispose(bool disposing) {
            if (disposing)
                RemotingServices.Disconnect(this);
        }
    }
}
