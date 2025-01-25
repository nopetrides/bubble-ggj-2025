namespace P3T.Scripts.Gameplay.Survivor
{
	/// <summary>
	/// Generic Game Power
	/// See <see cref="MonoTriggeredGamePower"/> for game usage
	/// </summary>
	public interface IGamePower
	{
		public bool IsNegativePower => false;
		public bool ActivateImmediately => true;
	}
}