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

namespace Overlay.Engine.DirectX.DXGI {


    #region "Delegates"
    public delegate void SwapChainCapturedHandler(SharpDX.DXGI.SwapChain swapChain);
    public delegate void FrameHandler();
    public delegate void TargetResizedHandler();
    public delegate void BuffersResizedHandler();

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int SwapChainPresentDelegate(IntPtr swapChainPtr, int syncInterval, SharpDX.DXGI.PresentFlags flags);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int SwapChainResizeTargetDelegate(IntPtr swapChainPtr, ref SharpDX.DXGI.ModeDescription targetMode);
    #endregion


    public class SwapChainInterface : BaseDeviceInterface {

        #region "Local Variables"
        private IntPtr _LastSwapChainPtr = IntPtr.Zero;
        #endregion

        #region "Events"
        public event FrameHandler OnFrame;
        public event TargetResizedHandler TargetResized;
        public event BuffersResizedHandler BuffersResized;
        public event SwapChainCapturedHandler SwapChainCaptured;
        #endregion

        public SwapChainInterface() : base() {

        }

        public override void Load() {
            try {
                Log.Write("Loading SwapChainInterface");
                LocalHook hook;

                // Hook SwapChain::Present
                hook = LocalHook.Create(
                    VTable.DXGI.Get(VTable.DXGI.AddressIndicies.Present),
                    new SwapChainPresentDelegate(Present),
                    this
                );
                InstalledHooks.Add(hook);
                Log.Write("SwapChain::Present hook loaded");

                // Hook SwapChain::TargetResized
                hook = LocalHook.Create(
                    VTable.DXGI.Get(VTable.DXGI.AddressIndicies.ResizeTarget),
                    new SwapChainResizeTargetDelegate(ResizeTarget),
                    this
                );
                InstalledHooks.Add(hook);
                Log.Write("SwapChain::ResizeTarget hook loaded");
            } catch (Exception ex) {
                Log.Write("Failed to load method hooks for SwapChainInterface: {0}", ex.Message);
                Diagnostics.Assert(false, "Failed to load method hooks for SwapChainInterface.", ex.Message);
            }
        }

        void SafeInvokeSwapChainCaptured(SharpDX.DXGI.SwapChain swapChain) {
            try {
                if (SwapChainCaptured != null)
                    SwapChainCaptured(swapChain);
            } catch (Exception ex) {
                Log.Write("Failed to propogate SwapChainCaptured Event: {0}", ex.Message);
            }
        }

        void SafeInvokeTargetResized() {
            try {
                if (TargetResized != null)
                    TargetResized();
            } catch(Exception ex) {
                Log.Write("Failed to propogate TargetResized Event: {0}", ex.Message);
            }
        }

        void SafeInvokeBuffersResized() {

        }

        void SafeInvokeOnFrame() {
            try {
                if (OnFrame != null)
                    OnFrame();
            } catch(Exception ex) {
                Log.Write("Failed to propogate OnFrame Event: {0}", ex.Message);
            }
        }


        
        int Present(IntPtr swapChainPtr, int syncInterval, SharpDX.DXGI.PresentFlags flags) {
            var swapChain = (SharpDX.DXGI.SwapChain)swapChainPtr;

            // Check if we need to notify of a new swap chain
            if (_LastSwapChainPtr != swapChain.NativePointer) {
                // We got a new device, let everything know
                SafeInvokeSwapChainCaptured(swapChain);
                // Cache the pointer
                _LastSwapChainPtr = swapChain.NativePointer;
            }

            // Finally, register that the application is ready to render a frame
            SafeInvokeOnFrame();

            // Call original present
            swapChain.Present(syncInterval, flags);
            return SharpDX.Result.Ok.Code;
        }

        int ResizeTarget(IntPtr swapChainPtr, ref SharpDX.DXGI.ModeDescription targetMode) {
            var swapChain = (SharpDX.DXGI.SwapChain)swapChainPtr;

            // Let the device adapters know 
            SafeInvokeTargetResized();

            swapChain.ResizeTarget(ref targetMode);
            return SharpDX.Result.Ok.Code;
        }
    }
}
