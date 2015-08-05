using System;

namespace Overlay.Engine.Events {
    [Serializable]
    public class RegisterOverlayComponentEventArgs : MarshalByRefObject {
        public RegisterOverlayComponentEventArgs(Guid overlayComponentId, string template, string layout) {
            OverlayComponentId = overlayComponentId;
            Template = template;
            Layout = layout;
        }

        public Guid OverlayComponentId { get; set; }
        public string Template { get; set; }
        public string Layout { get; set; }
    }
}
