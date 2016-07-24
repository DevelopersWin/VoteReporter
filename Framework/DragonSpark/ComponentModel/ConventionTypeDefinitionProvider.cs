using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public sealed class TypeDefinitionProvider : Cache<TypeInfo, TypeInfo>, ITypeDefinitionProvider
	{
		public static ISource<ITypeDefinitionProvider> Instance { get; } = new ExecutionScope<ITypeDefinitionProvider>( () => new TypeDefinitionProvider( AttributeConfigurations.TypeDefinitionProviders.Get() ) );
		TypeDefinitionProvider( ImmutableArray<ITypeDefinitionProvider> providers ) : base( new CompositeFactory<TypeInfo, TypeInfo>( providers.Select( provider => new Func<TypeInfo, TypeInfo>( provider.Get ) ).ToArray() ).Create ) {}
	}

	public sealed class ConventionTypeDefinitionProvider : TransformerBase<TypeInfo>, ITypeDefinitionProvider
	{
		public static ConventionTypeDefinitionProvider Instance { get; } = new ConventionTypeDefinitionProvider();
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

		class Context
		{
			readonly TypeInfo current;

			readonly static ICache<TypeInfo, TypeInfo> Cache = new Cache<TypeInfo, TypeInfo>( info => Type.GetType( $"{info.FullName}Metadata, {info.Assembly.FullName}", false )?.GetTypeInfo() );
			
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