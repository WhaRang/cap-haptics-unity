using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;

namespace _Main.Scripts._ProjectAgnostic.Providers.Serialization
{
	[UsedImplicitly]
	public sealed class GameplaySerializationProvider : IGameplaySerializationProvider
	{
		public OneOf<Success<string>, Error<string>> Serialize(object obj)
		{
			try
			{
				string serializedData = JsonConvert.SerializeObject(obj);
				
				if(string.IsNullOrEmpty(serializedData))
					return new Error<string>($"Serialization returned null or empty string.\n Object: {obj}");
				
				return new Success<string>(serializedData);
			}
			catch (Exception e)
			{
				return new Error<string>($"Serialization of object failed with exception.\nException: {e.Message}\n Object: {obj}");
			}
		}

		public OneOf<Success<T>, Error<string>> Deserialize<T>(string serializedData)
		{
			if (string.IsNullOrEmpty(serializedData))
				return new Error<string>("Deserialization failed: serialized data is null or empty.");

			try
			{
				var deserializedObject = JsonConvert.DeserializeObject<T>(serializedData);
				
				if (deserializedObject == null)
					return new Error<string>($"Deserialization returned null for type {typeof(T)} with data: {serializedData}");
				
				return new Success<T>(deserializedObject);
			}
			catch (Exception e)
			{
				return new Error<string>($"Deserialization of object failed with exception.\nException: {e.Message}\n Data: {serializedData}");
			}
		}

		public OneOf<Success<object>, Error<string>> Deserialize(string serializedData, Type type)
		{
			if (string.IsNullOrEmpty(serializedData))
				return new Error<string>("Deserialization failed: serialized data is null or empty.");

			try
			{
				var deserializedObject = JsonConvert.DeserializeObject(serializedData, type);
				
				if (deserializedObject == null)
					return new Error<string>($"Deserialization returned null for type {type} with data: {serializedData}");
				
				return new Success<object>(deserializedObject);
			}
			catch (Exception e)
			{
				return new Error<string>($"Deserialization of object failed with exception.\nException: {e.Message}\n Data: {serializedData}");
			}
		}
	}
}
