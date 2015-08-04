using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Overlay.Engine.Debug;

using EasyHook;
using Diagnostics = System.Diagnostics.Debug;

namespace Overlay.Engine.DirectX {
    public abstract class BaseDeviceInterface : IDeviceInterface {
        #region "Local Variables"
        private List<LocalHook> _InstalledHooks;
        #endregion

        #region "Class Properties"
        protected List<LocalHook> InstalledHooks { get { return _InstalledHooks; } }
        #endregion


        public BaseDeviceInterface() {
            _InstalledHooks = new List<LocalHook>();
        }


        public abstract void Load();

        public void Unload() {
            try {
                LocalHook hook;
                for (int i = 0; i < InstalledHooks.Count; i++) {
                    hook = InstalledHooks[i];
                    InstalledHooks.RemoveAt(i);

                    hook.Dispose();
                    hook = null;
                }
            } catch(Exception ex) {
                Log.Write("Failed to unload hooks: {0}", ex.Message);
                Diagnostics.Assert(false, "Failed to unload all hooks because of an exception.", ex.Message);
            }
        }
                        
        public void Dispose() {
            Unload();
        }
    }
}
