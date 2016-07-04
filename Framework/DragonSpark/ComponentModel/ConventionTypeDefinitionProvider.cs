using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class TypeDefinitionProvider : FirstFromParameterFactory<TypeInfo, TypeInfo>, ITypeDefinitionProvider
	{
		public static ICache<TypeInfo, TypeInfo> Instance { get; } = new TypeDefinitionProvider( Items<ITypeDefinitionProvider>.Default ).Cached();

		protected TypeDefinitionProvider( params ITypeDefinitionProvider[] others ) : base( others.Concat( new IFactory<TypeInfo, TypeInfo>[] { ConventionTypeDefinitionProvider.Instance, SelfTransformer<TypeInfo>.Instance } ).Fixed() ) {}
	}

	public class ConventionTypeDefinitionProvider : FactoryBase<TypeInfo, TypeInfo>, ITypeDefinitionProvider
	{
		public static ConventionTypeDefinitionProvider Instance { get; } = new ConventionTypeDefinitionProvider();

		public override TypeInfo Create( TypeInfo parameter )
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