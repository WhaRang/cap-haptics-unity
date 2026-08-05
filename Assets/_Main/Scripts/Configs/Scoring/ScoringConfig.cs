using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Configs.Scoring
{
	[CreateAssetMenu(fileName = "ScoringConfig", menuName = "Configs/ScoringConfig")]
	public sealed class ScoringConfig : ScriptableObject, IScoringConfig
	{
		[Header("Combo")]
		[Tooltip("Set Min == Max == 1 to disable the combo multiplier entirely.")]
		[field: SerializeField] public int MinComboMultiplier { get; private set; } = 1;
		[field: SerializeField] public int MaxComboMultiplier { get; private set; } = 8;

		[Tooltip("Combo decay window keyed by the highest multiplier the entry applies to (inclusive). Ordered ascending by UpToMultiplier.")]
		[SerializeField] private List<ComboDurationTier> _comboDurationTiers = new()
		{
			new ComboDurationTier(3, 10),
			new ComboDurationTier(5, 8),
			new ComboDurationTier(7, 7),
			new ComboDurationTier(9, 6),
			new ComboDurationTier(10, 5),
		};

		public int GetComboDurationInSeconds(int multiplier)
		{
			if (_comboDurationTiers.Count == 0)
				return 0;

			foreach (var tier in _comboDurationTiers)
			{
				if (multiplier <= tier.UpToMultiplier)
					return tier.DurationInSeconds;
			}

			return _comboDurationTiers[_comboDurationTiers.Count - 1].DurationInSeconds;
		}

		[Serializable]
		private struct ComboDurationTier
		{
			[SerializeField] private int _upToMultiplier;
			[SerializeField] private int _durationInSeconds;

			public ComboDurationTier(int upToMultiplier, int durationInSeconds)
			{
				_upToMultiplier = upToMultiplier;
				_durationInSeconds = durationInSeconds;
			}

			public int UpToMultiplier => _upToMultiplier;
			public int DurationInSeconds => _durationInSeconds;
		}
	}
}
