using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace _Main.Scripts._ProjectAgnostic.Services.Raycast
{
	[UsedImplicitly]
	public sealed class RaycastService : IRaycastService
	{
		public void RaycastAllUI(GraphicRaycaster raycaster, Vector2 screenPosition, List<GameObject> result)
		{
			var raycastResults = ListPool<RaycastResult>.Get();
			var eventData = new PointerEventData(EventSystem.current)
			{
				position = screenPosition
			};

			raycaster.Raycast(eventData, raycastResults);

			raycastResults.ForEach(r => result.Add(r.gameObject));

			ListPool<RaycastResult>.Release(raycastResults);
		}
	}
}
