using System.Threading.Tasks;

namespace UnityGLTF
{
    /// <summary>
    /// TextureCompressor intended to prevent OutOfMemory Exception.
    /// </summary>
    public interface ITextureCompressor
    {
        Task<string> TryCompressThreadSafe(string imageFileName);
    }
}