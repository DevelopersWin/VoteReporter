using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public class KnownTypes : CachedParameterizedScope<Type, ImmutableArray<Type>>
	{
		public static KnownTypes Instance { get; } = new KnownTypes();
		KnownTypes() : base( type => ApplicationTypes.Instance.Get().Where( type.Adapt().IsAssignableFrom ).ToImmutableArray() ) {}

		public ImmutableArray<Type> Get<T>() => Get( typeof(T) );
	}
}