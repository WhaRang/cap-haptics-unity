using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Main.Scripts._ProjectAgnostic.Services.Raycast
{
	public interface IRaycastService
	{
		void RaycastAllUI(GraphicRaycaster raycaster, Vector2 screenPosition, List<GameObject> result);
	}
}
