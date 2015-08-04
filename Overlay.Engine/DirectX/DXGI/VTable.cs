using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Diagnostics = System.Diagnostics.Debug;

using Swap = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;


namespace Overlay.Engine.DirectX.DXGI {
    public class VTable {

        public static IntPtr[] LookupAddresses(IntPtr start, int count) {
            return LookupAddresses(start, 0, count);
        }

        public static IntPtr[] LookupAddresses(IntPtr start, int index, int count) {
            List<IntPtr> results = new List<IntPtr>();

            IntPtr tablePtr = Marshal.ReadIntPtr(start);

            for (int i = index; i < (index + count); i++) {
                results.Add(Marshal.ReadIntPtr(tablePtr, i * IntPtr.Size));
            }

            return results.ToArray();
        }


        public class DXGI {
            public enum AddressIndicies : short {
                // IUnknown
                QueryInterface = 0,
                AddRef = 1,
                Release = 2,

                // IDXGIObject
                SetPrivateData = 3,
                SetPrivateDataInterface = 4,
                GetPrivateData = 5,
                GetParent = 6,

                // IDXGIDeviceSubObject
                GetDevice = 7,

                // IDXGISwapChain
                Present = 8,
                GetBuffer = 9,
                SetFullscreenState = 10,
                GetFullscreenState = 11,
                GetDesc = 12,
                ResizeBuffers = 13,
                ResizeTarget = 14,
                GetContainingOutput = 15,
                GetFrameStatistics = 16,
                GetLastPresentCount = 17
            }

            static DXGI() {
                LoadCache();
            }

            static int _MethodCount = 18;
            static List<IntPtr> _Cache = new List<IntPtr>();
            static void LoadCache() {
                _Cache.Clear();

                try {
                    Swap.SwapChain swapChain;
                    D3D11.Device d3dDevice;

                    using (var form = new SharpDX.Windows.RenderForm()) {
                        var _Description = new Swap.SwapChainDescription {
                            BufferCount = 1,
                            Flags = Swap.SwapChainFlags.None,
                            IsWindowed = true,
                            ModeDescription = new Swap.ModeDescription(100, 100, new Swap.Rational(60, 1), Swap.Format.R8G8B8A8_UNorm),
                            OutputHandle = form.Handle,
                            SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                            SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                            Usage = SharpDX.DXGI.Usage.RenderTargetOutput
                        };

                        D3D11.Device.CreateWithSwapChain(
                            SharpDX.Direct3D.DriverType.Hardware,
                            SharpDX.Direct3D11.DeviceCreationFlags.None,
                            _Description,
                            out d3dDevice,
                            out swapChain
                        );

                        _Cache.AddRange(VTable.LookupAddresses(swapChain.NativePointer, _MethodCount));

                        d3dDevice.Dispose();
                        swapChain.Dispose();
                        d3dDevice = null;
                        swapChain = null;
                        Diagnostics.Assert(_Cache.Count == _MethodCount, "Failed to generate address list for DXGI SwapChain, not sure why");
                    }
                } catch (Exception ex) {
                    Diagnostics.Assert(_Cache.Count == _MethodCount, "Failed to generate address list for DXGI SwapChain due to an exception.", ex.Message);
                }
            }

            public static IntPtr[] Addresses {
                get {
                    if (_Cache.Count < _MethodCount)
                        LoadCache();

                    return _Cache.ToArray();
                }
            }

            public static IntPtr Get(AddressIndicies index) {
                if (_Cache.Count < _MethodCount)
                    LoadCache();

                return Addresses[(int)index];
            }
        }

    }
}
