using Arch.Core;
using OneOf;
using OneOf.Types;

namespace _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Baking
{
	public interface IEcsBakingService
	{
		OneOf<Success, Error<string>> AddBaker(BaseEcsMonoBaker baker);
		void RemoveBaker(BaseEcsMonoBaker baker);
		void Bake(World world);
	}
}
