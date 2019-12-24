using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityGLTF.Loader
{
    public class FileLoader : ILoader
	{
		private string _rootDirectoryPath;

        public byte[] Data { get; private set; }

		public bool HasSyncLoadMethod { get; private set; }

		public FileLoader(string rootDirectoryPath)
		{
			_rootDirectoryPath = rootDirectoryPath;
			HasSyncLoadMethod = true;
		}

		public async Task LoadStream(string gltfFilePath)
		{
			if (gltfFilePath == null)
			{
				throw new ArgumentNullException("gltfFilePath");
			}

			await LoadFileStream(_rootDirectoryPath, gltfFilePath);
		}

		private async Task LoadFileStream(string rootPath, string fileToLoad)
		{
			string pathToLoad = Path.Combine(rootPath, fileToLoad);

			if (!File.Exists(pathToLoad))
			{
				throw new FileNotFoundException("Buffer file not found", pathToLoad);
			}

            using (FileStream fileStream = File.Open(pathToLoad, FileMode.Open))
            {
                int size = (int)fileStream.Length;
                Data = new byte[size];
                int loadedBytes = await fileStream.ReadAsync(Data, 0, size);
                Debug.Log($"Loaded {loadedBytes} bytes from {pathToLoad}");
            }
		}
	}
}
