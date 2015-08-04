using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D11;

namespace Overlay.Engine.Graphics {
    public class Sprite : IDisposable {
        public float Depth { get; set; }
        public Color4 Color { get; set; }
        public Rectangle Source { get; set; }
        public Rectangle Destination { get; set; }
        public Texture2D Texture { get; set; }


        public void Dispose() {
            if (Texture != null) {
                Texture.Dispose();
                Texture = null;
            }
        }
    }
}
