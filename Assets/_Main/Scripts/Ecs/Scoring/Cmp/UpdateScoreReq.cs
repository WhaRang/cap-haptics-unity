namespace _Main.Scripts.Ecs.Scoring.Cmp
{
	public struct UpdateScoreReq
	{
		public ScoreUpdateRequestKind Kind;

		/// <summary>Base points for <see cref="ScoreUpdateRequestKind.Gain"/> / <see cref="ScoreUpdateRequestKind.Penalty"/>. Ignored for Restore.</summary>
		public int Points;
	}
}
