using System.IO;
using UnityEditor;
using UnityEngine;
namespace Aerosol {
    [ExecuteAlways]
    class Aerosol : MonoBehaviour {
        public Config Config;
        static Model model;
        static readonly int TransmittanceTexture = Shader.PropertyToID("transmittance_texture");
        static readonly int ScatteringTexture = Shader.PropertyToID("scattering_texture");
        static readonly int IrradianceTexture = Shader.PropertyToID("irradiance_texture");

        void Awake() {
            Init();
        }

        void OnValidate() {
            model?.Init();
        }

        void Init() {
            model = new Model(Config);
            model.Init();
            Config.Skybox.SetTexture(TransmittanceTexture, model.Transmittance);
            Config.Skybox.SetTexture(ScatteringTexture, model.Scattering);
            Config.Skybox.SetTexture(IrradianceTexture, model.Irradiance);
        }

#if UNITY_EDITOR
        [ContextMenu("GenHeader")]
        void GenHeader() {
            string path = Path.Combine(Application.dataPath,
                "beantowel/Aerosol/Shaders/header.hlsl");
            string header = Model.Header(Config.Params, Const.Lambdas);
            File.WriteAllText(path, header);
            AssetDatabase.Refresh();
            Init();
        }
#endif
        public (RenderTexture, RenderTexture, RenderTexture) GetTextures() {
            return (model.Transmittance, model.Irradiance, model.Scattering);
        }
    }
}