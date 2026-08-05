using System.Linq;
using System.Collections.Generic;
using Arch.Core;
using OneOf;
using OneOf.Types;

namespace _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Baking
{
	public sealed class EcsBakingService : IEcsBakingService
	{
		private readonly HashSet<BaseEcsMonoBaker> _bakers = new();
		
		public OneOf<Success, Error<string>> AddBaker(BaseEcsMonoBaker baker)
		{
			if (_bakers.Add(baker))
				return new Success();

			return new Error<string>($"Baker {baker.gameObject.name} was already added.");
		}

		public void RemoveBaker(BaseEcsMonoBaker baker)
		{
			if (baker != null)
				_bakers.Remove(baker);
		}
		
		public void Bake(World world)
		{
			foreach (var baker in _bakers.ToArray())
			{
				world.Bake(baker);
			}
		}
	}
}
