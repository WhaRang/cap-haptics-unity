using System;

namespace _Main.Scripts.Ecs.Scoring.Cmp
{
	public struct ScoreSinglCmp
	{
		public int CurrentScore;
		public int CurrentMultiplier;

		// Combo state. The combo window is authoritative on the time-provider timestamp; the UI meter is a visual mirror.
		public bool ComboActivated;
		public DateTime? LastComboTriggerTimestamp;
	}
}
