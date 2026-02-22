using UnityEngine;
using UnityEngine.Rendering;

namespace Aerosol {
    static class Util {
        static readonly CommandBuffer buffer = new();
        static readonly Matrix4x4 view = Matrix4x4.identity;
        static readonly Matrix4x4 ortho = Matrix4x4.Ortho(-1, 1, -1, 1, -1, 1);
        static Mesh fullTri;
        static readonly int Layer = Shader.PropertyToID("layer");

        static void InitBuffer() {
            buffer.Clear();
            buffer.SetViewProjectionMatrices(view, ortho);
        }


        public static void DrawRect(Material mat, params RenderTargetIdentifier[] rts) {
            Mesh mesh = FullSceneTri();
            InitBuffer();
            buffer.SetRenderTarget(rts, rts[0]);
            buffer.DrawMesh(mesh, Matrix4x4.identity, mat);
            Graphics.ExecuteCommandBuffer(buffer);
        }

        public static void DrawCube(Material mat, params RenderTargetIdentifier[] rts) {
            Mesh mesh = FullSceneTri();
            for (int i = 0; i < Const.ScatteringTextureSize.Depth; i++) {
                InitBuffer();
                buffer.SetRenderTarget(rts, rts[0], 0, CubemapFace.Unknown, i);
                mat.SetInteger(Layer, i);
                buffer.DrawMesh(mesh, Matrix4x4.identity, mat);
                Graphics.ExecuteCommandBuffer(buffer);
            }
        }

        public static RenderTextureDescriptor Tex2Desc(int width, int height) {
            RenderTextureDescriptor desc = new(
                width, height, RenderTextureFormat.ARGBHalf, 0)
            {
                sRGB = false
            };
            return desc;
        }

        public static RenderTextureDescriptor Tex3Desc() {
            RenderTextureDescriptor desc = new(
                Const.ScatteringTextureSize.Width,
                Const.ScatteringTextureSize.Height,
                RenderTextureFormat.ARGBHalf, 0)
            {
                dimension = TextureDimension.Tex3D,
                volumeDepth = Const.ScatteringTextureSize.Depth,
                sRGB = false
            };
            return desc;
        }

        static Mesh FullSceneTri(float scale = 1) {
            if (fullTri != null) {
                return fullTri;
            }
            Vector3[] vertices = new Vector3[] {
                new Vector3(-1, -1, 0) * scale,
                new Vector3(3, -1, 0) * scale,
                new Vector3(-1, 3, 0) * scale,
            };
            Vector2[] uv = new Vector2[] {
                new(0, 0),
                new(2, 0),
                new(0, 2),
            };
            int[] triangles = new int[]{
                0, 1, 2
            };
            fullTri = new Mesh
            {
                vertices = vertices,
                uv = uv,
                triangles = triangles
            };
            return fullTri;
        }
    }
}