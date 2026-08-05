using _Main.Scripts._ProjectAgnostic.Ecs.AppLifecycle.Cmp;
using _Main.Scripts._ProjectAgnostic.GameplaySessionLifetime.Service;
using _Main.Scripts._ProjectAgnostic.Providers.SaveLoading;
using _Main.Scripts._ProjectAgnostic.Services.EventBus;
using _Main.Scripts._ProjectAgnostic.Services.Input;
using _Main.Scripts.Ecs;
using _Main.Scripts.Services.Events;
using UnityEngine;
using VContainer;

namespace _Main.Scripts
{
	/// <summary>
	/// The scene-level entry point that drives the ECS runtime: starts/stops gameplay sessions,
	/// ticks the world every frame, and forwards OS app-lifecycle callbacks into the ECS as input
	/// events. Rename/extend this per game (see <c>MahjongGameManager</c> for a full-scale example
	/// wired to the platform layer).
	/// </summary>
	public sealed class TemplateGameManager : MonoBehaviour
	{
		[Tooltip("Start a session automatically on scene start. Disable when a meta/menu layer decides when gameplay begins.")]
		[SerializeField] private bool _autoStartSessionOnStart = true;

		[Tooltip("When auto-starting, restore the previously saved session if one exists.")]
		[SerializeField] private bool _restoreSavedSessionIfAvailable = true;

		private EcsRuntime _ecsRuntime = null!;
		private IGameplayInputService _inputService = null!;
		private IGameplaySessionLifetimeService _gameplaySessionLifetimeService = null!;
		private IGameplayEventBusService _eventBus = null!;
		private IPersistantStorageProvider _storageProvider = null!;

		private bool _stopRequested;

		[Inject]
		public void Construct(
			EcsRuntime ecsRuntime,
			IGameplayInputService inputService,
			IGameplaySessionLifetimeService gameplaySessionLifetimeService,
			IGameplayEventBusService eventBus,
			IPersistantStorageProvider storageProvider)
		{
			_ecsRuntime = ecsRuntime;
			_inputService = inputService;
			_gameplaySessionLifetimeService = gameplaySessionLifetimeService;
			_eventBus = eventBus;
			_storageProvider = storageProvider;
		}

		private void Start()
		{
			_eventBus.Subscribe<OnGameplaySceneCloseRequestedEvent>(OnGameplaySceneCloseRequested);
			_eventBus.Subscribe<OnGameSessionStartedEvent>(OnGameSessionStarted);
			_eventBus.Subscribe<OnGameSessionEndedEvent>(OnGameSessionEnded);

			if (_autoStartSessionOnStart)
				StartSession(restore: _restoreSavedSessionIfAvailable && _storageProvider.Read().IsT0);
		}

		private void OnDestroy()
		{
			_eventBus.Unsubscribe<OnGameplaySceneCloseRequestedEvent>(OnGameplaySceneCloseRequested);
			_eventBus.Unsubscribe<OnGameSessionStartedEvent>(OnGameSessionStarted);
			_eventBus.Unsubscribe<OnGameSessionEndedEvent>(OnGameSessionEnded);
		}

		private void Update()
		{
			if (_stopRequested)
			{
				_stopRequested = false;
				StopSession();
				return;
			}

#if UNITY_EDITOR
			// Editor convenience: simulate the app being backgrounded (pause + save flush).
			if (Input.GetKeyDown(KeyCode.P))
				_inputService.SendInput(new OnAppBackgroundedEvt());
#endif
			_ecsRuntime.Tick();
		}

		public void StartSession(bool restore = false)
		{
			if (_ecsRuntime.IsRunning)
			{
				Debug.LogError($"[{nameof(TemplateGameManager)}] Trying to start a session while one is already running.");
				return;
			}

			_gameplaySessionLifetimeService.ShouldGameSessionBeRestored = restore;
			_gameplaySessionLifetimeService.InstantiateGameplaySessionGameObjects();
			_ecsRuntime.Start();
			_inputService.UnlockInput();
		}

		private void StopSession()
		{
			_inputService.LockInput();
			_ecsRuntime.Stop();
			_gameplaySessionLifetimeService.DisposeGameplaySessionGameObjects();
		}

		private void OnApplicationPause(bool isPaused)
		{
			if (!_ecsRuntime.IsRunning)
				return;

			if (isPaused)
			{
				_inputService.SendInput(new OnAppBackgroundedEvt());
				// Flush synchronously: on iOS this is the last tick before the process suspends.
				_ecsRuntime.Tick();
				return;
			}

			_inputService.SendInput(new OnAppForegroundedEvt());
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (Application.isEditor)
				return;
			OnApplicationPause(!hasFocus);
		}

		private void OnGameplaySceneCloseRequested(OnGameplaySceneCloseRequestedEvent evt)
		{
			// Published mid-tick by CloseGameplaySceneSys; defer the teardown to the next Update
			// so the world is not cleared while systems are still iterating it.
			_stopRequested = true;
		}

		// Placeholder handlers: replace with real UI / meta-layer wiring in your game.
		private void OnGameSessionStarted(OnGameSessionStartedEvent evt)
		{
			Debug.Log($"[{nameof(TemplateGameManager)}] Session started. Restored from save: {evt.WasRestoredFromSave}");
		}

		private void OnGameSessionEnded(OnGameSessionEndedEvent evt)
		{
			Debug.Log($"[{nameof(TemplateGameManager)}] Session ended. Reason: {evt.Reason}, final score: {evt.FinalScore}");
		}
	}
}
