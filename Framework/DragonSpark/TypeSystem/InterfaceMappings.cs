using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class InterfaceMappings : ParameterizedSourceCache<Type, Type, ImmutableArray<MethodMapping>>
	{
		public static InterfaceMappings Default { get; } = new InterfaceMappings();
		InterfaceMappings() : base( type => new Implementation( type ) ) {}

		[UsedImplicitly]
		public InterfaceMappings( Func<Type, IParameterizedSource<Type, ImmutableArray<MethodMapping>>> create ) : base( create ) {}

		public sealed class Implementation : ParameterizedItemSourceBase<Type, MethodMapping>
		{
			readonly Type type;
			readonly TypeInfo info;

			public Implementation( Type type ) : this( type, type.GetTypeInfo() ) {}

			[UsedImplicitly]
			public Implementation( Type type, TypeInfo info )
			{
				this.type = type;
				this.info = info;
			}

			public override IEnumerable<MethodMapping> Yield( Type parameter )
			{
				var generic = parameter.GetTypeInfo().IsGenericTypeDefinition ? type.GetImplementations( parameter ).FirstOrDefault() : null;
				var implementation = generic ?? ( parameter.IsAssignableFrom( type ) ? parameter : null );
				if ( implementation != null )
				{
					var map = info.GetRuntimeInterfaceMap( implementation );
					var result = map.InterfaceMethods.Tuple( map.TargetMethods ).Select( tuple => new MethodMapping( tuple.Item1, tuple.Item2 ) );
					return result;
				}
				return Items<MethodMapping>.Default;
			}
		}
	}
}