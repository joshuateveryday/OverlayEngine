using System.Collections.Generic;
using EasyHook;

namespace Overlay.Engine.DirectX {
    public abstract class BaseDeviceInterface : IDeviceInterface {
        #region "Local Variables"

        private List<LocalHook> _InstalledHooks;

        #endregion

        public BaseDeviceInterface(OverlayInterface overlayInterface) {
            _InstalledHooks = new List<LocalHook>();
        }

        #region "Class Properties"

        protected List<LocalHook> InstalledHooks {
            get { return _InstalledHooks; }
        }

        #endregion

        public abstract void Load();
        public abstract void Unload();
        public abstract void Dispose();
    }
}
