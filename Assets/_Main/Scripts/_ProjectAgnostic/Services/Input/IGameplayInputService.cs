using OneOf;
using OneOf.Types;
using UnityEngine;

namespace _Main.Scripts._ProjectAgnostic.Services.Input
{
	public interface IGameplayInputService
	{
		void LockInput();
		void UnlockInput();
		OneOf<Vector2, None> GetFingerPosition();
		OneOf<Vector2, None> GetTapPosition();
		OneOf<Vector2, None> GetFingerReleasePosition();
		void SendInput<T>(T inputEvent) where T : struct;
	}
}
