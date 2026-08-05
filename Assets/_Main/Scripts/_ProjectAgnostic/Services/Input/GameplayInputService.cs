using Arch.Core;
using OneOf;
using OneOf.Types;
using UnityEngine;

namespace _Main.Scripts._ProjectAgnostic.Services.Input
{
	public class GameplayInputService : IGameplayInputService
	{
		private readonly World _ecsWorld;
		private bool _isInputLocked;
		
		public GameplayInputService(World ecsWorld)
		{
			_ecsWorld = ecsWorld;
		}

		public void LockInput()
		{
			_isInputLocked = true;
		}
		
		public void UnlockInput()
		{
			_isInputLocked = false;
		}

		public OneOf<Vector2, None> GetFingerPosition()
		{
#if UNITY_EDITOR
			if(UnityEngine.Input.GetMouseButton(0))
				return new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y);
#else
			if(UnityEngine.Input.touchCount > 0)
			{
				var touch = UnityEngine.Input.GetTouch(0);
				if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
					return new Vector2(touch.position.x, touch.position.y);
			}
#endif
			return new None();
		}

		public OneOf<Vector2, None> GetTapPosition()
		{
#if UNITY_EDITOR
			if(UnityEngine.Input.GetMouseButtonDown(0))
				return new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y);
#else
			if(UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began)
				return new Vector2(UnityEngine.Input.GetTouch(0).position.x, UnityEngine.Input.GetTouch(0).position.y);
#endif
			return new None();
		}

		public OneOf<Vector2, None> GetFingerReleasePosition()
		{
#if UNITY_EDITOR
			if(UnityEngine.Input.GetMouseButtonUp(0))
				return new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y);
#else
			if(UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Ended)
				return new Vector2(UnityEngine.Input.GetTouch(0).position.x, UnityEngine.Input.GetTouch(0).position.y);
#endif
			return new None();
		}

		public void SendInput<T>(T inputEvent) where T : struct
		{
			if(!_isInputLocked)
				_ecsWorld.Create(inputEvent);
		}
	}
}
