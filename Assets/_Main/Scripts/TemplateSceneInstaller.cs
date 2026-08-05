using _Main.Scripts._ProjectAgnostic.Ecs.BetterArchApi;
using _Main.Scripts._ProjectAgnostic.Ecs.CommonSystems;
using _Main.Scripts._ProjectAgnostic.Ecs.AppLifecycle.Sys;
using _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Factory;
using _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Service;
using _Main.Scripts._ProjectAgnostic.Providers.SaveLoading;
using _Main.Scripts._ProjectAgnostic.Providers.Serialization;
using _Main.Scripts._ProjectAgnostic.Providers.Time;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts._ProjectAgnostic.Services.Input;
using _Main.Scripts._ProjectAgnostic.Services.Raycast;
using _Main.Scripts.Configs.Scoring;
using _Main.Scripts.Configs.Time;
using _Main.Scripts.Ecs;
using _Main.Scripts.Ecs.GameSaving.Sys;
using _Main.Scripts.Ecs.Scoring.Sys;
using _Main.Scripts.Ecs.SessionEnding.Sys;
using _Main.Scripts.Ecs.SessionStarting.Sys;
using _Main.Scripts.Ecs.Time.Sys;
using _Main.Scripts.Services.Time;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Main.Scripts
{
	/// <summary>
	/// The scene composition root. Registers configs, providers, services, and the ECS pipeline.
	/// The registration order of ECS systems IS their execution order within a tick — keep the
	/// ordering comments honest when inserting game-specific systems.
	/// </summary>
	public class TemplateSceneInstaller : LifetimeScope
	{
		[Header("Components")]
		[SerializeField] private TemplateGameManager _gameManager = null!;
		[SerializeField] private GameplaySessionObjectFactory _gameplaySessionObjectFactory = null!;

		[Header("Configs")]
		[SerializeField] private TimeConfig _timeConfig = null!;
		[SerializeField] private ScoringConfig _scoringConfig = null!;

		protected override void Configure(IContainerBuilder builder)
		{
			InstallComponents(builder);
			InstallConfigs(builder);
			InstallProviders(builder);
			InstallServices(builder);
			InstallEcs(builder);
		}

		private void InstallComponents(IContainerBuilder builder)
		{
			builder.RegisterComponent(_gameManager);
			builder.RegisterComponent(_gameplaySessionObjectFactory).AsImplementedInterfaces();
		}

		private void InstallConfigs(IContainerBuilder builder)
		{
			builder.RegisterInstance<ITimeConfig>(_timeConfig);
			builder.RegisterInstance<IScoringConfig>(_scoringConfig);
		}

		private void InstallProviders(IContainerBuilder builder)
		{
			builder.Register<LocalServerTimeProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ConfigGameSessionPauseDurationProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PlayerPrefsStorageProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<GameplaySerializationProvider>(Lifetime.Singleton).AsImplementedInterfaces();
		}

		private void InstallServices(IContainerBuilder builder)
		{
			builder.Register<RaycastService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<GameplayInputService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<GameplayEventBusService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<GameplayTimeService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<GameplaySessionLifetimeService>(Lifetime.Singleton).AsImplementedInterfaces();
		}

		private void InstallEcs(IContainerBuilder builder)
		{
			builder.Register<EcsRuntime>(Lifetime.Singleton).AsSelf();

			builder.RegisterEcsWorld();
			builder.RegisterEcsBaking();

			// Session lifecycle (initialize singletons first, then load save data / start a new session)
			builder.RegisterEcsSystem<InitializeSessionSys>();
			builder.RegisterEcsSystem<StartSessionSys>();

			// <-- Insert game-specific board/content generation systems here -->

			// App lifecycle (must precede Time + GameSaving so fan-out events flush in one tick)
			builder.RegisterEcsSystem<ProcessAppLifecycleEventsSys>();

			// Time
			builder.RegisterEcsSystem<UpdateGameplayTimeSys>();

			// <-- Insert game-specific gameplay systems here (input processing, matching, win condition, ...) -->

			// Scoring (consumes gain/penalty requests produced above)
			builder.RegisterEcsSystem<UpdateScoreSys>();

			// Session timeout (runs after gameplay/win systems so a same-tick win beats the clock)
			builder.RegisterEcsSystem<CheckSessionTimeoutSys>();

			// Game Saving
			builder.RegisterEcsSystem<PeriodicSaveSys>();
			builder.RegisterEcsSystem<SaveGameStateSys>();

			// Session ending
			builder.RegisterEcsSystem<EndGameSessionSys>();
			builder.RegisterEcsSystem<CloseGameplaySceneSys>();

			// Cleanup
			builder.RegisterEcsSystem<DestroyEmptyEntitiesSys>();
		}
	}
}
