using System;

namespace Overlay.Engine {
    public class OverlayInterface : MarshalByRefObject {
        public bool IsConnected() {
            return true;
        }
    }
}
