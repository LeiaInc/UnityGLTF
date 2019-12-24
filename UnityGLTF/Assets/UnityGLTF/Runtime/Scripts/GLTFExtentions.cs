using System.IO;

namespace UnityGLTF
{
    public static class GLTFExtentions
    {
        public static MemoryStream ToStream(this byte[] data, bool writable = false)
        {
            return new MemoryStream(data, 0, data.Length, writable, true);
        }
    }
}