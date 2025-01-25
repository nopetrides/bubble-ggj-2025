namespace P3T.Scripts.Gameplay.Survivor
{
	public static class NumUtil
	{
		/// <summary>
		/// Format a score value with commas
		/// Exact format is based on system's Culture Info
		/// </summary>
		/// <param name="score">(long) score to display</param>
		/// <returns>Formatted display string</returns>
		public static string FormatScoreForDisplay(long score)
		{
			return $"{score:n0}";
		}
	}
}