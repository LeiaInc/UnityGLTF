using System.Threading.Tasks;
namespace UnityGLTF.Loader
{
    public interface ILoader
	{
		Task LoadStream(string relativeFilePath);

		byte[] Data { get; }

		bool HasSyncLoadMethod { get; }
	}
}
