using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D11;

namespace Overlay.Engine.Graphics {
    public class SpriteFont : IDisposable {
        #region "Local Variables"
        private int _CharacterHeight;
        private int _SpaceWidth;
        private Texture2D _Texture;

        const char START_CHAR = (char)33;
        const char END_CHAR = (char)127;
        const uint CHAR_COUNT = END_CHAR - START_CHAR;
        #endregion

        #region "Public properties"
        public int Height { get { return _CharacterHeight; } } 
        public int Space { get { return _SpaceWidth; } }
        public Texture2D Texture { get { return _Texture; } }
        #endregion

        public SpriteFont(Device device, System.Drawing.Font font) {

        }
        
      


        public Rectangle Size(char c) {
            throw new NotImplementedException();
        }

        public void Dispose() {

        }
    }
}
