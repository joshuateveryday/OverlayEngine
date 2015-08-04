using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using DrawingRectangle = System.Drawing.Rectangle;
using BitmapData = System.Drawing.Imaging.BitmapData;

using Overlay.Engine.Graphics;

using SharpDX;
using SharpDX.Direct3D11;

namespace Overlay.Engine {
    public class ContentManager : IDisposable {
        #region "Local Variables"
        private Device _GraphicsDevice;
        private DeviceContext _GraphicsDeviceContext;
        #endregion

        #region "Public Properties"
        public Device GraphicsDevice { get { return _GraphicsDevice; } }
        public DeviceContext GraphicsDeviceContext { get { return _GraphicsDeviceContext; } }
        #endregion

        public ContentManager(GraphicsAdapter adapter) {
            _GraphicsDevice = adapter.GraphicsDevice;
            _GraphicsDeviceContext = adapter.GraphicsDeviceContext;
        }

        public void Dispose() {

        }



        #region "Static Members"
        static string _Root;
        public static string Root {
            get {
                if (_Root == null || _Root == string.Empty) {
                    string basePath = typeof(ContentManager).Assembly.Location;
                    _Root = Path.Combine(basePath, "Content");
                }

                return _Root;
            }
            set { _Root = value; }
        }

        public static Texture2D TextureFromBitmap(Device device, System.Drawing.Bitmap bitmap) {
            BitmapData _BitmapData;
            _BitmapData = bitmap.LockBits(
                new DrawingRectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            var _TextureDescription = new Texture2DDescription {
                Width = bitmap.Width,
                Height = bitmap.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Usage = ResourceUsage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            _TextureDescription.SampleDescription.Count = 1;
            _TextureDescription.SampleDescription.Quality = 0;


            DataBox _Data;
            _Data.DataPointer = _BitmapData.Scan0;
            _Data.RowPitch = bitmap.Width * 4;
            _Data.SlicePitch = 0;

            var _Texture = new Texture2D(device, _TextureDescription, new[] { _Data });
            bitmap.UnlockBits(_BitmapData);

            return _Texture;
        }
        #endregion
    }
}
