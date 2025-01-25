namespace P3T.Scripts.Gameplay.Survivor
{
	public sealed class SurvivorHazardDestroyParticle : PooledParticleBase<SurvivorHazardManager>
	{
		private SurvivorHazardManager _manager;

		protected override void Release()
		{
			_manager.Release(this);
		}

		public override void SetManager(SurvivorHazardManager manager)
		{
			_manager = manager;
		}
	}
}