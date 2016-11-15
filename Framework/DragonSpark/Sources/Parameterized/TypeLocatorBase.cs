using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Parameterized
{
	public abstract class SourceTypeLocatorBase : Cache<Type, Type>
	{
		protected SourceTypeLocatorBase( Func<Type[], Type> locate, params Type[] types ) 
			: base( new TypeLocator( info => info.Append( info.Adapt().GetAllInterfaces() ).ToImmutableArray(), new CompositeAssignableSpecification( types ).IsSatisfiedBy, locate ).Get ) {}
	}
}