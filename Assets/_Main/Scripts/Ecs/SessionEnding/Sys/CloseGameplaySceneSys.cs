using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.UnityIntegration.Systems;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts.Ecs.SessionEnding.Cmp;
using _Main.Scripts.Services.Events;
using Arch.Core;

namespace _Main.Scripts.Ecs.SessionEnding.Sys
{
	public sealed class CloseGameplaySceneSys : IEcsTickSystem
	{
		private readonly World _world;
		private readonly Query _closeGameplaySceneReqQuery;
		private readonly IGameplayEventBusService _eventBus;
		
		public CloseGameplaySceneSys(World world, IGameplayEventBusService eventBus)
		{
			_world = world;
			_eventBus = eventBus;
			_closeGameplaySceneReqQuery = _world.Query(new QueryDescription().WithAll<OnCloseGameplaySceneRequestedEvt>());
		}
		
		public void OnTick()
		{
			if(_closeGameplaySceneReqQuery.IsEmpty())
				return;

			_world.DestroyAllEntitiesInQuery(_closeGameplaySceneReqQuery);
			_eventBus.Publish(new OnGameplaySceneCloseRequestedEvent());
		}
	}
}
