namespace P3T.Scripts.Gameplay.Survivor
{
	public class PowerCollectedParticle : PooledParticleBase<SurvivorPowerUpManager>
	{
		private SurvivorPowerUpManager _manager;

		protected override void Release()
		{
			_manager.Release(this);
		}

		public override void SetManager(SurvivorPowerUpManager manager)
		{
			_manager = manager;
		}
	}
}