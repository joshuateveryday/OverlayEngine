using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Overlay.Engine.Debug;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace Overlay.Engine.Graphics {
    public class Shader {
        #region "Local Variables"
        private string _File;
        private Effect _Effect;
        private EffectTechnique _EffectTechnique;
        private EffectShaderResourceVariable _SpriteMap;
        private CompilationResult _CompiledEffect;
        private InputLayout _InputLayout;
        #endregion

        #region "Public Properties"
        public bool IsReady { get { return (_CompiledEffect != null && !_CompiledEffect.HasErrors); } }
        public EffectTechnique Technique { get { return _EffectTechnique; } }
        public EffectShaderResourceVariable Resources { get { return _SpriteMap; } }
        public InputLayout Layout { get { return _InputLayout; } } 
        #endregion


        public Shader(Device device, string file) {
            string path = Path.Combine(ContentManager.Root, @"Shaders\VertexShader.shader");
            CompileShader(device, path);
        }

        private void CompileShader(Device device, string fullPath) {
            _File = fullPath;

            _CompiledEffect = ShaderBytecode.CompileFromFile(_File, "SpriteTech", "fx_5_0");

            if (_CompiledEffect.HasErrors) {
                Log.Write("Shader compilation failed with status code: {0} - {1} | Path: {2}", _CompiledEffect.ResultCode.Code, _CompiledEffect.Message, _File);
                return;
            }

            _Effect = new Effect(device, _CompiledEffect);
            _EffectTechnique = _Effect.GetTechniqueByName("SpriteTech");
            _SpriteMap = _Effect.GetVariableByName("SpriteTex").AsShaderResource();
            var _EffectPass = _EffectTechnique.GetPassByIndex(0).Description.Signature;

            InputElement[] _LayoutDescription = {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 20, 0, InputClassification.PerVertexData, 0)
            };

            _InputLayout = new InputLayout(device, _EffectPass, _LayoutDescription);
        }
    }
}

