using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Relay
{
	public sealed class Descriptors : ItemSource<IRelayMethodAspectBuildDefinition>
	{
		public static Descriptors Default { get; } = new Descriptors();
		Descriptors() : this( /*CommandDescriptor.Default,*/ SourceDescriptor.Default, SpecificationDescriptor.Default ) {}
		Descriptors( params IRelayMethodAspectBuildDefinition[] descriptors ) : this( descriptors.SelectTypes().AsAdapters(), descriptors ) {}
		Descriptors( ImmutableArray<TypeAdapter> adapters, IRelayMethodAspectBuildDefinition[] descriptors ) 
			: this( descriptors, new TypedPairs<IAspect>( adapters.Tuple( descriptors.Select( descriptor => new Func<object, IAspect>( descriptor.Get ) ).ToArray() ) ) ) {}

		Descriptors( IEnumerable<IRelayMethodAspectBuildDefinition> descriptors, ITypedPairs<IAspect> instances ) : base( descriptors )
		{
			Aspects = instances;
		}

		public ITypedPairs<IAspect> Aspects { get; }
	}
}