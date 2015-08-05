using System;
using EasyHook;
using Overlay.Engine.Debug;
using Overlay.Engine.Events;

namespace Overlay.Engine.DirectX {
    public class Direct3D11 : BaseDeviceInterface {
        private OverlayInterfaceProxy InterfaceProxy = new OverlayInterfaceProxy();

        public Direct3D11(OverlayInterface overlayInterface) : base(overlayInterface) {
            Interface = overlayInterface;

            Interface.RegisterOverlayComponentHandler += InterfaceProxy.RegisterOverlayComponentProxyHandler;
            InterfaceProxy.RegisterOverlayComponentHandler += InterfaceProxy_RegisterOverlayComponentHandler;

            Interface.PushComoponentDataHandler += InterfaceProxy.PushComponentDataProxyHandler;
            InterfaceProxy.PushComponentDataHandler += InterfaceProxy_PushComponentDataHandler;
        }

        private OverlayInterface Interface { get; }

        private void InterfaceProxy_RegisterOverlayComponentHandler(RegisterOverlayComponentEventArgs args) {
            throw new NotImplementedException();
        }

        private void InterfaceProxy_PushComponentDataHandler(PushComponentDataEventArgs args) {
            throw new NotImplementedException();
        }

        public override void Load() {
            throw new NotImplementedException();
        }

        public override void Unload() {
            try {
                LocalHook hook;
                for (int i = 0; i < InstalledHooks.Count; i++) {
                    hook = InstalledHooks[i];
                    InstalledHooks.RemoveAt(i);

                    hook.Dispose();
                    hook = null;
                }
            }
            catch (Exception ex) {
                Log.Write("Failed to unload hooks: {0}", ex.Message);
                System.Diagnostics.Debug.Assert(false, "Failed to unload all hooks because of an exception.", ex.Message);
            }
        }

        public override void Dispose() {
            try {
                Interface.RegisterOverlayComponentHandler -= InterfaceProxy.RegisterOverlayComponentProxyHandler;
                Interface.PushComoponentDataHandler -= InterfaceProxy.PushComponentDataProxyHandler;
            }
            catch (Exception ex) {
                Log.Write("Failed to decouple listeners: {0}", ex.Message);
            }
            finally {
                Unload();
            }
        }
    }
}
