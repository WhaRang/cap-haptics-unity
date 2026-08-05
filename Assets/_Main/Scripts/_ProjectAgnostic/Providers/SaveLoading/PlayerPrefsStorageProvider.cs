using JetBrains.Annotations;
using OneOf;
using OneOf.Types;
using UnityEngine;

namespace _Main.Scripts._ProjectAgnostic.Providers.SaveLoading
{
	/// <summary>
	/// Default, dependency-free persistent storage backed by <see cref="PlayerPrefs"/>.
	/// Swap the registration for a platform/backend-specific provider when the game
	/// gets a real persistence layer (server state, cloud save, files, ...).
	/// </summary>
	[UsedImplicitly]
	public sealed class PlayerPrefsStorageProvider : IPersistantStorageProvider
	{
		private const string StorageKey = "GameplayState";

		public OneOf<string, None> Read()
		{
			string data = PlayerPrefs.GetString(StorageKey, string.Empty);

			if (string.IsNullOrEmpty(data))
				return new None();

			return data;
		}

		public void Write(string data)
		{
			PlayerPrefs.SetString(StorageKey, data);
			PlayerPrefs.Save();
		}

		public void Clear()
		{
			PlayerPrefs.DeleteKey(StorageKey);
			PlayerPrefs.Save();
		}
	}
}
