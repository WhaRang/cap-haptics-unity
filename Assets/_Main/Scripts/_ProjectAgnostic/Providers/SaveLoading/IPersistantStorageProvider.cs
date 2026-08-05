using OneOf;
using OneOf.Types;

namespace _Main.Scripts._ProjectAgnostic.Providers.SaveLoading
{
	public interface IPersistantStorageProvider
	{
		OneOf<string, None> Read();
		void Write(string data);
		void Clear();
	}
}
