using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Overlay.Engine.Debug;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Overlay.Engine.Graphics {
    public enum SpriteSortMode {
        Deferred,
        Immediate,
        Texture,
        BackToFront,
        FrontToBack
    };

    public enum SpriteEffects {
        None,
        FlipHorizontally,
        FlipVertically
    }


    public class SpriteBatch : IDisposable {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SpriteVertex {
            public Vector3 Position;
            public Vector2 TexCoord;
            public Color4 Color;
        }

        #region "Local Variables"
        private Vector2 _ScreenBounds;
        private Buffer _VertexBuffer;
        private Buffer _IndexBuffer;
        private GraphicsAdapter _GrahpicsAdapter;
        private SpriteSortMode _SortMode;

        private Shader _Shader;
        private Dictionary<Texture2D, List<Sprite>> _SpriteCacheByResource;
        private Dictionary<Texture2D, ShaderResourceView> _ResourceCache;

        private Color4 _BlendFactor;
        private BlendState _BlendState;
        private int _PreviousBlendMask;
        private Color4 _PreviousBlendFactor;
        private BlendState _PreviousBlendState;
        #endregion

        #region "Class Properties"
        protected Shader Shader { get { return _Shader; } }
        protected GraphicsAdapter GraphicsAdapter { get { return _GrahpicsAdapter; } }
        #endregion


        public SpriteBatch(GraphicsAdapter graphics) {
            _GrahpicsAdapter = graphics;
            _SpriteCacheByResource = new Dictionary<Texture2D, List<Sprite>>();
            _ResourceCache = new Dictionary<Texture2D, ShaderResourceView>();
            _Shader = new Shader(graphics.GraphicsDevice, @"Shaders\VertexShader.shader");

            InitBuffers();
            InitBlendState();
        }


        private void InitBuffers() {
            BufferDescription _VertexBufferDescription = new BufferDescription() {
                SizeInBytes = (2048 * Marshal.SizeOf(typeof(SpriteVertex))),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };
            _VertexBuffer = new Buffer(GraphicsAdapter.GraphicsDevice, _VertexBufferDescription);


            short[] _Indicies = new short[3072];
            for(ushort i = 0; i < 512; ++i) {
                _Indicies[i * 6] = (short)(i * 4);
                _Indicies[i * 6 + 1] = (short)(i * 4 + 1);
                _Indicies[i * 6 + 2] = (short)(i * 4 + 2);
                _Indicies[i * 6 + 3] = (short)(i * 4);
                _Indicies[i * 6 + 4] = (short)(i * 4 + 2);
                _Indicies[i * 6 + 5] = (short)(i * 4 + 3);
            }

            IntPtr _IndexBufferPtr = Marshal.AllocHGlobal(_Indicies.Length * Marshal.SizeOf(_Indicies[0]));
            Marshal.Copy(_Indicies, 0, _IndexBufferPtr, _Indicies.Length);

            BufferDescription _IndexBufferDescription = new BufferDescription() {
                SizeInBytes = 3072 * Marshal.SizeOf(typeof(short)),
                Usage = ResourceUsage.Immutable,
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };
            _IndexBuffer = new Buffer(GraphicsAdapter.GraphicsDevice, _IndexBufferPtr, _IndexBufferDescription);
        }

        private void InitBlendState() {
            BlendStateDescription _BlendStateDescription = new BlendStateDescription {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };

            _BlendStateDescription.RenderTarget[0].IsBlendEnabled = true;
            _BlendStateDescription.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            _BlendStateDescription.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            _BlendStateDescription.RenderTarget[0].BlendOperation = BlendOperation.Add;
            _BlendStateDescription.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            _BlendStateDescription.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            _BlendStateDescription.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            _BlendStateDescription.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            _BlendState = new BlendState(GraphicsAdapter.GraphicsDevice, _BlendStateDescription);
        }

        public void DrawString(int x, int y, string text, Color color, SpriteFont font) {
            var vector = new Vector4(
                (color.R > 0 ? (float)(color.R / 255f) : 0f),
                (color.G > 0 ? (float)(color.G / 255f) : 0f),
                (color.B > 0 ? (float)(color.B / 255f) : 0f),
                (color.A > 0 ? (float)(color.A / 255f) : 0f)
            );

            int _X = x;
            int _Y = y;
            var _Texture = font.Texture;
            var _Color = new Color4(vector);
            
            for(int i = 0; i < text.Length; ++i) {
                var character = text[i];

                if (character == ' ') {
                    _X += font.Space;
                } else if (character == '\n') {
                    _X = x;
                    _Y += font.Height;
                } else {
                    var source = font.Size(character);
                    var width = source.Right - source.Left;
                    var height = source.Bottom - source.Top;

                    var dest = new Rectangle(_X, _Y, (_X + width), (_Y + height));

                    Draw(_Color, _Texture, source, dest, 0);

                    _X += (width + 1);
                }
            }
        }

        public void Draw(Color4 color, Texture2D texture, Rectangle source, Rectangle dest, float depth) {
            DrawSprite(new Sprite {
                Color = color,
                Texture = texture,
                Source = source,
                Destination = dest,
                Depth = depth
            });
        }

        public void DrawSprite(Sprite sprite) {
            if (!_SpriteCacheByResource.ContainsKey(sprite.Texture))
                _SpriteCacheByResource[sprite.Texture] = new List<Sprite>();

            _SpriteCacheByResource[sprite.Texture].Add(sprite);
        }

        private void DrawSpriteBatch(List<Sprite> batch, int index, int count) {
            DataBox map = GraphicsAdapter.GraphicsDeviceContext.MapSubresource(_VertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);

            unsafe
            {
                SpriteVertex* verts = (SpriteVertex*)map.DataPointer.ToPointer();
                for(int i = 0; i < count; ++i) {
                    var sprite = batch[index + i];
                    var quad = QuadFromSprite(sprite);

                    verts[i * 4] = quad[0];
                    verts[i * 4 + 1] = quad[1];
                    verts[i * 4 + 2] = quad[2];
                    verts[i * 4 + 3] = quad[3];
                }
            }

            GraphicsAdapter.GraphicsDeviceContext.UnmapSubresource(_VertexBuffer, 0);
            GraphicsAdapter.GraphicsDeviceContext.DrawIndexed(count * 6, 0, 0);
        }

        public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null) {
            _SortMode = sortMode;
            _BlendFactor = new Color4(1f);
            if (blendState != null)
                _BlendState = blendState;

            _SpriteCacheByResource.Clear();
            _ResourceCache.Clear();


            var context = GraphicsAdapter.GraphicsDeviceContext;

            // Save current blend state
            _PreviousBlendState = context.OutputMerger.GetBlendState(out _PreviousBlendFactor, out _PreviousBlendMask);

            // Apply our blend state
            context.OutputMerger.SetBlendState(_BlendState, _BlendFactor);
        }

        public void End() {
            var context = GraphicsAdapter.GraphicsDeviceContext;
            var viewports = context.Rasterizer.GetViewports();

            _ScreenBounds = new Vector2 {
                X = viewports[0].Width,
                Y = viewports[0].Height
            };

            var length = Marshal.SizeOf(typeof(SpriteVertex));
            var offset = 0;

            context.InputAssembler.InputLayout = Shader.Layout;
            context.InputAssembler.SetIndexBuffer(_IndexBuffer, SharpDX.DXGI.Format.R16_UInt, offset);
            context.InputAssembler.SetVertexBuffers(0, new[] { _VertexBuffer }, new[] { length }, new[] { offset });
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            foreach(var texture in _SpriteCacheByResource.Keys) {
                // Load texture resource from cache
                var _ShaderResourceView = ResourceFromTexture(texture);

                // Set the texture resource
                Shader.Resources.SetResource(_ShaderResourceView);

                // Retrieve the list of sprites using this texture
                var _SpriteCacheList = _SpriteCacheByResource[texture];

                // TODO: Is it correct to run an effect pass for each texture?
                using (EffectPass _EffectPass = Shader.Technique.GetPassByIndex(0)) {
                    int index = 0;
                    int count = _SpriteCacheList.Count;

                    while(_SpriteCacheList.Count > 0) {
                        if (count <= 512) {
                            DrawSpriteBatch(_SpriteCacheList, index, count);
                            count = 0;
                        } else {
                            DrawSpriteBatch(_SpriteCacheList, index, count);
                            index += 512;
                            count -= 512;
                        }
                    }
                }
            }

            // Restore device blend state
            context.OutputMerger.SetBlendState(_PreviousBlendState, _PreviousBlendFactor, _PreviousBlendMask);
        }
        
        private SpriteVertex[] QuadFromSprite(Sprite sprite) {
            SpriteVertex[] verts = new SpriteVertex[4];

            Rectangle src = sprite.Source;
            Rectangle dest = sprite.Destination;
            var width = sprite.Texture.Description.Width;
            var height = sprite.Texture.Description.Height;

            verts[0].Position = PointToNdc(dest.Left, dest.Bottom, 0f);
            verts[1].Position = PointToNdc(dest.Left, dest.Top, 0f);
            verts[2].Position = PointToNdc(dest.Right, dest.Top, 0f);
            verts[3].Position = PointToNdc(dest.Right, dest.Bottom, 0f);

            verts[0].TexCoord = new Vector2((float)src.Left / width, (float)src.Bottom / height);
            verts[1].TexCoord = new Vector2((float)src.Left / width, (float)src.Top / height);
            verts[2].TexCoord = new Vector2((float)src.Right / width, (float)src.Top / height);
            verts[3].TexCoord = new Vector2((float)src.Right / width, (float)src.Bottom / height);

            for(int i = 0; i < verts.Length; i++) {
                verts[i].Color = sprite.Color;
            }

            var tx = 0.5f * (verts[0].Position.X + verts[3].Position.X);
            var ty = 0.5f * (verts[0].Position.Y + verts[1].Position.Y);
            var origin = new Vector2(tx, ty);
            var translate = new Vector2(0f, 0f);
            var transform = Matrix.AffineTransformation2D(1f, 0f, translate);

            for(int i = 0; i < verts.Length; ++i) {
                Vector3 position = verts[i].Position;
                position = Vector3.TransformCoordinate(position, transform);
                verts[i].Position = position;
            }

            return verts;
        }

        private ShaderResourceView ResourceFromTexture(Texture2D texture) {
            ShaderResourceView _Resource;

            if (!_ResourceCache.TryGetValue(texture, out _Resource)) {
                ShaderResourceViewDescription _ResourceViewDescription = new ShaderResourceViewDescription {
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D
                };
                _ResourceViewDescription.Texture2D.MipLevels = 1;
                _ResourceViewDescription.Texture2D.MostDetailedMip = 0;

                _Resource = new ShaderResourceView(GraphicsAdapter.GraphicsDevice, texture, _ResourceViewDescription);
            }

            return _Resource;
        }
        
        private Vector3 PointToNdc(int x, int y, float z) {
            Vector3 result;

            result.X = (2.0f * (float)x / _ScreenBounds.X - 1.0f);
            result.Y = (1.0f - 2.0f * (float)y / _ScreenBounds.Y);
            result.Z = z;

            return result;
        }


        public void Dispose() {
        }

    }
}
