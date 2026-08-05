using UnityEngine;

namespace _Main.Scripts.Configs.Time
{
	[CreateAssetMenu(fileName = "TimeConfig", menuName = "Configs/TimeConfig")]
	public sealed class TimeConfig : ScriptableObject, ITimeConfig
	{
		[SerializeField, Min(0f)] private long _countdownAnimationDurationInSeconds = 2;
		[SerializeField, Min(0f)] private long _gameSessionDurationInSeconds = 120;
		[SerializeField, Min(0f)] private long _saveIntervalInSeconds = 1;
		[SerializeField, Min(0)] private int _maximumPauseDurationInSeconds = 60;

		public long CountdownAnimationDurationInSeconds => _countdownAnimationDurationInSeconds;
		public long GameSessionDurationInSeconds => _gameSessionDurationInSeconds + _countdownAnimationDurationInSeconds;
		public long SaveIntervalInSeconds => _saveIntervalInSeconds;
		public int MaximumPauseDurationInSeconds => _maximumPauseDurationInSeconds;
	}
}
