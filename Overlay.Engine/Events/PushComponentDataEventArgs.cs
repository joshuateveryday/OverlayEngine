using System;

namespace Overlay.Engine.Events {
    [Serializable]
    public class PushComponentDataEventArgs : MarshalByRefObject {
        public PushComponentDataEventArgs(Guid overlayComponentId, string data) {
            OverlayComponentId = overlayComponentId;
            Data = data;
        }

        public Guid OverlayComponentId { get; set; }
        public string Data { get; set; }
    }
}
