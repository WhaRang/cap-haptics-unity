using Arch.Core;
using UnityEngine;

namespace _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.EntityReference
{
	public sealed class EcsMonoEntityReference : MonoBehaviour
	{
		public Entity Entity { get; set; }
	}
}
