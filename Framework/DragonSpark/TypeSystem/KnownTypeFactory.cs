using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public class KnownTypeFactory : StoreCache<Type, ImmutableArray<Type>>
	{
		public static KnownTypeFactory Instance { get; } = new KnownTypeFactory( FrameworkTypes.Instance.Create() );

		public KnownTypeFactory( Type[] types ) : this( types.ToImmutableArray() ) {}

		KnownTypeFactory( ImmutableArray<Type> types ) : base( new Factory( types ).Create ) {}

		sealed class Factory : FactoryBase<Type, ImmutableArray<Type>>
		{
			readonly ImmutableArray<Type> types;

			// public Factory( Type[] types ) : this( types.ToImmutableArray() ) {}

			public Factory( ImmutableArray<Type> types )
			{
				this.types = types;
			}

			public override ImmutableArray<Type> Create( System.Type parameter ) => types.Where( parameter.Adapt().IsAssignableFrom ).ToImmutableArray();
		}
	}
}