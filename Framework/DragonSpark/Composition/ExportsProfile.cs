namespace DragonSpark.Composition
{
	public struct ExportsProfile 
	{
		public ExportsProfile( ConstructedExports constructed, ConventionExports convention, SingletonExports singletons )
		{
			Constructed = constructed;
			Convention = convention;
			Singletons = singletons;
		}

		public ConstructedExports Constructed { get; }
		public ConventionExports Convention { get; }
		public SingletonExports Singletons { get; }
	}
}