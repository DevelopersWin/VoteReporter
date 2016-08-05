namespace DragonSpark.TypeSystem
{
	public sealed class PublicParts : PartsBase
	{
		public static PublicParts Instance { get; } = new PublicParts();
		PublicParts() : base( AssemblyTypes.Public.Get ) {}
	}
}