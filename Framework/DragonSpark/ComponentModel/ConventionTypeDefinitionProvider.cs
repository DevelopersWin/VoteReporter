using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System;
using System.Reflection;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.ComponentModel
{
	public sealed class TypeDefinitions : ParameterizedScope<TypeInfo, TypeInfo>
	{
		public static IParameterizedSource<TypeInfo, TypeInfo> Default { get; } = new TypeDefinitions();
		TypeDefinitions() : base( Factory.Global<TypeInfo, TypeInfo>( Create ) ) {}

		static TypeInfo Create( TypeInfo parameter )
		{
			foreach ( var provider in TypeSystem.Configuration.TypeDefinitionProviders.Get() )
			{
				var info = provider.Get( parameter );
				if ( info != null )
				{
					return info;
				}
			}
			return null;
		}
	}

	public sealed class ConventionTypeDefinitionProvider : TransformerBase<TypeInfo>, ITypeDefinitionProvider
	{
		public static ConventionTypeDefinitionProvider Default { get; } = new ConventionTypeDefinitionProvider();
		ConventionTypeDefinitionProvider() {}

		public override TypeInfo Get( TypeInfo parameter )
		{
			var context = new Context( parameter );
			var result = context.Loop( 
				item => item.CreateFromBaseType(), 
				item => item.Metadata != null,
				item => item.Metadata
				);
			return result;
		}

		struct Context
		{
			readonly static ICache<TypeInfo, TypeInfo> Cache = new Cache<TypeInfo, TypeInfo>( info => Type.GetType( $"{info.FullName}Metadata, {info.Assembly.FullName}", false )?.GetTypeInfo() );

			readonly TypeInfo current;

			public Context( TypeInfo current ) : this( current, Cache.Get( current ) ) {}

			Context( TypeInfo current, TypeInfo metadata )
			{
				this.current = current;
				Metadata = metadata;
			}

			public Context CreateFromBaseType() => current.BaseType.With( x => new Context( x.GetTypeInfo() ) );

			public TypeInfo Metadata { get; }
		}
	}
}