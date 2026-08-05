using UnityEngine;

namespace _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Factory
{
	public interface IGameplaySessionObjectFactory
	{
		GameObject Create();
	}
}
