using System;
using System.Collections.Generic;
using EasyHook;
using Overlay.Engine.Debug;

namespace Overlay.Engine.DirectX {
    public abstract class BaseDeviceInterface : IDeviceInterface {
        #region "Local Variables"

        private List<LocalHook> _InstalledHooks;

        #endregion

        public BaseDeviceInterface() {
            _InstalledHooks = new List<LocalHook>();
        }

        #region "Class Properties"

        protected List<LocalHook> InstalledHooks {
            get { return _InstalledHooks; }
        }

        #endregion

        public abstract void Load();

        public virtual void Unload() {
            try
            {
                LocalHook hook;
                for (int i = 0; i < InstalledHooks.Count; i++)
                {
                    hook = InstalledHooks[i];
                    InstalledHooks.RemoveAt(i);

                    hook.Dispose();
                    hook = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Failed to unload hooks: {0}", ex.Message);
                System.Diagnostics.Debug.Assert(false, "Failed to unload all hooks because of an exception.", ex.Message);
            }
        }

        public virtual void Dispose() {
            Unload();
        }
    }
}
