using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using SharpDX;

namespace Overlay.Engine.DirectX {

    public interface IDeviceInterface : IDisposable {

        /// <summary>
        /// Installs method hooks for interface
        /// </summary>
        void Load();

        /// <summary>
        /// Unloads hooked methods
        /// </summary>
        void Unload();

    }
}
