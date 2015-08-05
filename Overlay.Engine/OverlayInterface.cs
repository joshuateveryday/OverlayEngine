using System;
using Overlay.Engine.Events;

namespace Overlay.Engine {
    [Serializable]
    public delegate void RegisterOverlayComponentEvent(RegisterOverlayComponentEventArgs args);

    [Serializable]
    public delegate void PushComponentDataEvent(PushComponentDataEventArgs args);

    public class OverlayInterface : MarshalByRefObject {
        public event RegisterOverlayComponentEvent RegisterOverlayComponentHandler;
        public event PushComponentDataEvent PushComoponentDataHandler;

        public bool IsConnected() {
            return true;
        }

        public void RegisterOverlayComponent(Guid overlayComponentId, string template, string layout) {
            SafeInvokeRegisterOverlayComponent(new RegisterOverlayComponentEventArgs(overlayComponentId, template, layout));
        }

        public void PushComponentData(Guid overlayComponentId, string data) {
            SafeInvokePushComponentData(new PushComponentDataEventArgs(overlayComponentId, data));
        }

        private void SafeInvokeRegisterOverlayComponent(RegisterOverlayComponentEventArgs args) {
            if (RegisterOverlayComponentHandler == null)
                return;

            RegisterOverlayComponentEvent listener = null;
            Delegate[] invocationList = RegisterOverlayComponentHandler.GetInvocationList();

            foreach (Delegate @delegate in invocationList) {
                try {
                    listener = (RegisterOverlayComponentEvent) @delegate;
                    listener.Invoke(args);
                }
                catch (Exception) {
                    RegisterOverlayComponentHandler -= listener;
                }
            }
        }

        private void SafeInvokePushComponentData(PushComponentDataEventArgs args) {
            if (PushComoponentDataHandler == null)
                return;

            PushComponentDataEvent listener = null;
            Delegate[] invocationList = PushComoponentDataHandler.GetInvocationList();

            foreach (Delegate @delegate in invocationList) {
                try {
                    listener = (PushComponentDataEvent) @delegate;
                    listener.Invoke(args);
                }
                catch (Exception) {
                    PushComoponentDataHandler -= listener;
                }
            }
        }
    }
}
