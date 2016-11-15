using DragonSpark.Extensions;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;


namespace DragonSpark.TypeSystem
{
	class MethodMapper
	{
		readonly Type type;
		readonly TypeInfo info;

		public MethodMapper( Type type ) : this( type, type.GetTypeInfo() ) {}

		public MethodMapper( Type type, TypeInfo info )
		{
			this.type = type;
			this.info = info;
		}

		public ImmutableArray<MethodMapping> Get( Type parameter )
		{
			var generic = parameter.GetTypeInfo().IsGenericTypeDefinition ? type.GetImplementations( parameter ).FirstOrDefault() : null;
			var implementation = generic ?? ( parameter.IsAssignableFrom( type ) ? parameter : null );
			if ( implementation != null )
			{
				var map = info.GetRuntimeInterfaceMap( implementation );
				var result = map.InterfaceMethods.Tuple( map.TargetMethods ).Select( tuple => new MethodMapping( tuple.Item1, tuple.Item2 ) ).ToImmutableArray();
				return result;
			}
			return Items<MethodMapping>.Immutable;
		}
	}
}