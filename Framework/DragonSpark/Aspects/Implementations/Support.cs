using DragonSpark.Aspects.Build;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Implementations
{
	sealed class Support : DelegatedSpecification<Type>, ISupportDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : this( Descriptors.Default.ToArray() ) {}

		readonly ImmutableArray<IDescriptor> descriptors;

		[UsedImplicitly]
		public Support( params IDescriptor[] descriptors ) : base( SpecificationFactory.Default.Get( descriptors ) )
		{
			this.descriptors = descriptors.ToImmutableArray();
		}

		public IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var descriptor in descriptors )
			{
				var instance = descriptor.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}
}