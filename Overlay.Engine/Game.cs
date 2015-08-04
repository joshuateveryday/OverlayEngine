using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Overlay.Engine.Graphics;

using SharpDX;

namespace Overlay.Engine {
    public class Game : IDisposable {
        #region "Local Variables"
        private SpriteBatch _SpriteBatch;
        private GraphicsAdapter _GraphicsAdapter;
        private ContentManager _ContentManager;
        #endregion

        #region "Public Properties"
        public ContentManager Content { get { return _ContentManager; } set { _ContentManager = value; } }
        public GraphicsAdapter GraphicsAdapter { get { return _GraphicsAdapter; } }
        public SpriteBatch SpriteBatch { get { return _SpriteBatch; } set { _SpriteBatch = value; } }
        #endregion

        public Game() {
            _GraphicsAdapter = new GraphicsAdapter(this);
        }

        /// <summary>
        /// Load assets required by the system, called 
        /// after SpriteBatch and ContentManager are initialized
        /// </summary>
        public void LoadContent() {
            
        }


        public void Update() {

        }

        public void Draw() {
            SpriteBatch.Begin();

            // Draw Stuff

            SpriteBatch.End();
        }

        public void Dispose() {

        }
    }
}
