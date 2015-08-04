using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D11;
using SwapChain = SharpDX.DXGI.SwapChain;

using Overlay.Engine.DirectX.DXGI;


namespace Overlay.Engine.Graphics {
    public class GraphicsAdapter : IDisposable {

        #region "Local Variables"
        private Game _GameContext;
        private Device _Device;
        private SwapChain _SwapChain;
        private DeviceContext _DeviceContext;
        private SwapChainInterface _DeviceInterface;
        private Texture2D _RenderTarget;
        private RenderTargetView _RenderTargetView;
        #endregion

        #region "Class Properties"
        protected Game GameContext { get { return _GameContext; } }
        protected SwapChain SwapChain { get { return _SwapChain; } }
        #endregion

        #region "Public Properties"
        public Device GraphicsDevice { get { return _Device; } }
        public DeviceContext GraphicsDeviceContext { get { return _DeviceContext; } }

        public Texture2D RenderTarget { get { return _RenderTarget; } }
        public RenderTargetView RenderTargetView { get { return _RenderTargetView; } }

        public bool IsDefferedContext {
            get { return GraphicsDeviceContext.TypeInfo == DeviceContextType.Deferred; }
        }

        public bool IsInitialized {
            get {
                return (GraphicsDevice != null) &&
                       (GraphicsDeviceContext != null) &&
                       (SwapChain != null);
            }
        }
        #endregion


        public GraphicsAdapter(Game game) {
            _GameContext = game;

            // Initialize the Device Interface and register events
            _DeviceInterface = new SwapChainInterface();
            _DeviceInterface.OnFrame += OnFrame;
            _DeviceInterface.TargetResized += TargetResized;
            _DeviceInterface.SwapChainCaptured += SwapChainCaptured;
            _DeviceInterface.Load();
        }


        public void Dispose() {

        }

        private void SafeInvokeDeviceReady() {

        }


        private void TargetResized() {
            // TODO: Figure out what to do here
        }

        private void OnFrame() {
            if (IsInitialized) {
                // Update the game context
                GameContext.Update();
                // Draw pending elements
                GameContext.Draw();
            }
        }

        private void SwapChainCaptured(SharpDX.DXGI.SwapChain swapChain) {
            // Cache the new swapChain
            _SwapChain = swapChain;

            // Load D3D11 Device and render targets
            _Device = SwapChain.GetDevice<Device>();
            _RenderTarget = SwapChain.GetBackBuffer<Texture2D>(0);
            _RenderTargetView = new RenderTargetView(GraphicsDevice, RenderTarget);
            
            try {
                _DeviceContext = new DeviceContext(GraphicsDevice);
            } catch (SharpDXException) {
                _DeviceContext = GraphicsDevice.ImmediateContext;
            }


            if (IsDefferedContext) {
                GraphicsDeviceContext.Rasterizer.SetViewport(new ViewportF(0f, 0f, RenderTarget.Description.Width, RenderTarget.Description.Height, 0, 1));
                GraphicsDeviceContext.OutputMerger.SetTargets(_RenderTargetView);
            }

            // Initialize a new SpriteBatch
            if (GameContext.SpriteBatch != null) {
                GameContext.SpriteBatch.Dispose();
                GameContext.SpriteBatch = null;
            }

            GameContext.SpriteBatch = new SpriteBatch(this);
            GameContext.Content = new ContentManager(this);
        }
    }
}
