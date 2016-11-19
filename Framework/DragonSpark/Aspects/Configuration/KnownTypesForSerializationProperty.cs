namespace DragonSpark.Aspects.Configuration
{
	public sealed class KnownTypesForSerializationProperty : ProjectPropertyExpression
	{
		public static KnownTypesForSerializationProperty Default { get; } = new KnownTypesForSerializationProperty();
		KnownTypesForSerializationProperty() : base( "DragonSparkDiagnosticsSerializationKnownTypes" ) {}
	}
}