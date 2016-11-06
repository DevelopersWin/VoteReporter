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
	public sealed class Descriptors : ItemSource<ISupportDefinition>
	{
		public static Descriptors Default { get; } = new Descriptors();
		Descriptors() : this( /*CommandDescriptor.Default,*/ SourceDescriptor.Default, SpecificationDescriptor.Default ) {}
		Descriptors( params ISupportDefinition[] descriptors ) : this( descriptors.Select( definition => definition.DeclaringType ).AsAdapters(), descriptors ) {}
		Descriptors( ImmutableArray<TypeAdapter> adapters, ISupportDefinition[] descriptors ) 
			: this( descriptors, new TypedPairs<IAspect>( adapters.Tuple( descriptors.Select( descriptor => new Func<object, IAspect>( descriptor.Get ) ).ToArray() ) ) ) {}

		Descriptors( IEnumerable<ISupportDefinition> descriptors, ITypedPairs<IAspect> instances ) : base( descriptors )
		{
			Aspects = instances;
		}

		public ITypedPairs<IAspect> Aspects { get; }
	}
}