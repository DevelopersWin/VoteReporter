using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Sources;
using DragonSpark.Setup;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public class KnownTypes : ParameterizedScope<Type, ImmutableArray<Type>>
	{
		public static KnownTypes Instance { get; } = new KnownTypes();
		KnownTypes() : base( Factory.CachedPerScope<Type, ImmutableArray<Type>>( type => ApplicationTypes.Instance.Get().Where( type.Adapt().IsAssignableFrom ).ToImmutableArray() ) ) {}

		public ImmutableArray<Type> Get<T>() => Get( typeof(T) );
	}
}