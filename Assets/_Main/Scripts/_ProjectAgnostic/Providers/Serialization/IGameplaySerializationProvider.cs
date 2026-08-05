using System;
using OneOf;
using OneOf.Types;

namespace _Main.Scripts._ProjectAgnostic.Providers.Serialization
{
	public interface IGameplaySerializationProvider
	{
		OneOf<Success<string>, Error<string>> Serialize(object obj);
		OneOf<Success<T>, Error<string>> Deserialize<T>(string serializedData);
		OneOf<Success<object>, Error<string>> Deserialize(string serializedData, Type type);
	}
}
