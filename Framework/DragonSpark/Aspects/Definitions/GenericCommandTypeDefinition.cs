namespace DragonSpark.Aspects.Definitions
{
	public sealed class GenericCommandTypeDefinition : ValidatedTypeDefinition
	{
		public static GenericCommandTypeDefinition Default { get; } = new GenericCommandTypeDefinition();
		GenericCommandTypeDefinition() : base( GenericCommandCoreTypeDefinition.Execute ) {}
	}
}