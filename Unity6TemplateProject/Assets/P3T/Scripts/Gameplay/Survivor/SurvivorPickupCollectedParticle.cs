namespace P3T.Scripts.Gameplay.Survivor
{
	public class SurvivorPickupCollectedParticle : PooledParticleBase<SurvivorPointsPickupManager>
	{
		private SurvivorPointsPickupManager _manager;

		protected override void Release()
		{
			_manager.Release(this);
		}

		public override void SetManager(SurvivorPointsPickupManager manager)
		{
			_manager = manager;
		}
	}
}