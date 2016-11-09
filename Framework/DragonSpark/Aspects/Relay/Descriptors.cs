using DragonSpark.Sources;

namespace DragonSpark.Aspects.Relay
{
	public sealed class Descriptors : ItemSource<IApplyRelayAspectBuildDefinition>
	{
		public static Descriptors Default { get; } = new Descriptors();
		Descriptors() : this( /*CommandDescriptor.Default,*/ ApplySourceRelayDefinition.Default, ApplySpecificationRelayDefinition.Default ) {}
		Descriptors( params IApplyRelayAspectBuildDefinition[] descriptors ) : base( /*descriptors.SelectTypes().AsAdapters(),*/ descriptors ) {}
		/*Descriptors( ImmutableArray<TypeAdapter> adapters, IRelayMethodAspectBuildDefinition[] descriptors ) 
			: this( descriptors, new TypedPairs<IAspect>( adapters.Tuple( descriptors.Select( descriptor => new Func<object, IAspect>( descriptor.Get ) ).ToArray() ) ) ) {}

		Descriptors( IEnumerable<IRelayMethodAspectBuildDefinition> descriptors/*, ITypedPairs<IAspect> instances#1# ) : base( descriptors )
		{
			// Aspects = instances;
		}

		/*public ITypedPairs<IAspect> Aspects { get; }#1#*/
	}
}