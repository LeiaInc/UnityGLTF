using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityGLTF
{
    public class TextureCompressor : MonoBehaviour, ITextureCompressor
    {
        const int TEXTURE_SIZE = 512;
        const int CHECK_INTERVAL = 250;
        static readonly Color[,] pixels = new Color[TEXTURE_SIZE, TEXTURE_SIZE];

        string modelDirectory;
        Texture2D textureToCompress;
        Queue<string> imagePaths = new Queue<string>();

        IProgress<ImportProgress> progress;
        ImportProgress progressStatus;

        public const string COMPRESSION_SUFFIX = "_COMPRESSED";

        public void Init(string modelPath, IProgress<ImportProgress> progress)
        {
            modelDirectory = Path.GetDirectoryName(modelPath);
            this.progress = progress;
            progressStatus = default;
        }

        public async Task<string> TryCompressThreadSafe(string imageFileName)
        {
            string imagePath = Path.Combine(modelDirectory, imageFileName);

            if (File.Exists(imagePath))
            {
                imagePaths.Enqueue(imagePath);
                ++progressStatus.TextureTotal;
                progress.Report(progressStatus);

                //Wait until Copression Done on main thread
                while (imagePaths.Count > 0)
                {
                    await Task.Delay(CHECK_INTERVAL);
                }

                ++progressStatus.TextureLoaded;
                progress.Report(progressStatus);
                return ToCompressedPath(imagePath);
            }
            else
            {
                throw new Exception("Invalid Image path: " + imagePath);
            }
        }

        bool ShouldCompress(Texture2D texture, string path)
        {
            return ShouldCompress(texture) && IsNotAlreadyCompressed(path);
        }

        bool ShouldCompress(Texture2D texture)
        {
            return texture.width > TEXTURE_SIZE && texture.height > TEXTURE_SIZE;
        }

        bool IsNotAlreadyCompressed(string path)
        {
            return !File.Exists(ToCompressedPath(path));
        }

        void Update()
        {
            if (imagePaths.Count > 0)
            {
                TryCompress(imagePaths.Peek());
            }
        }

        //Main thread due to intensive usage of Unity API
        void TryCompress(string path)
        {
            byte[] imageData = File.ReadAllBytes(path);

            textureToCompress = new Texture2D(0, 0, TextureFormat.ARGB32, false, true);
            textureToCompress.LoadImage(imageData);

            if (ShouldCompress(textureToCompress, path))
            {
                Compress(textureToCompress);
            }
            else
            {
                FinishTextureScaling();
            }
        }

        void Compress(Texture2D texture)
        {
            for(int x0 = 0; x0 < TEXTURE_SIZE; ++x0)
            {
                float xNormalized = (float)x0 / TEXTURE_SIZE;
                int x1 = (int)(xNormalized * texture.width);

                for (int y0 = 0; y0 < TEXTURE_SIZE; ++y0)
                {
                    float yNormalized = (float)y0 / TEXTURE_SIZE;
                    int y1 = (int)(yNormalized * texture.height);

                    pixels[x0, y0] = texture.GetPixel(x1, y1);
                }
            }

            texture.Resize(TEXTURE_SIZE, TEXTURE_SIZE);

            for (int x = 0; x < TEXTURE_SIZE; ++x)
            {
                for (int y = 0; y < TEXTURE_SIZE; ++y)
                {
                    texture.SetPixel(x, y, pixels[x, y]);
                }
            }

            texture.Apply();
            OnCompressed();
        }

        void OnCompressed()
        {
            string path = imagePaths.Peek();

            string extention = Path.GetExtension(path).ToLower();

            byte[] compressedData;

            switch (extention)
            {
                case ".jpg":

                    compressedData = textureToCompress.EncodeToJPG();
                    break;

                case ".jpeg":

                    compressedData = textureToCompress.EncodeToJPG();
                    break;

                case ".png":

                    compressedData = textureToCompress.EncodeToPNG();
                    break;

                case ".tga":

                    compressedData = textureToCompress.EncodeToTGA();
                    break;

                case ".exr":

                    compressedData = textureToCompress.EncodeToEXR();
                    break;

                default:
                    Debug.LogError("Texture Resolution Compression is not supported.");
                    return;
            }

            string compressedPath = ToCompressedPath(path);

            File.WriteAllBytes(compressedPath, compressedData);

            FinishTextureScaling();
        }

        void FinishTextureScaling()
        {
            imagePaths.Dequeue();

            //Clean Memory
            DestroyImmediate(textureToCompress);
        }

        public static string ToCompressedPath(string path)
        {
            string extention = Path.GetExtension(path);
            int indexOfExtention = path.LastIndexOf(extention);
            return path.Insert(indexOfExtention, COMPRESSION_SUFFIX);
        }

        public static bool IsCompressed(string path)
        {
            return path.Contains(COMPRESSION_SUFFIX);
        }
    }
}