using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	[Persistent]
	public class TypeDefinitionProvider : FirstFromParameterFactory<TypeInfo, TypeInfo>, ITypeDefinitionProvider
	{
		public static TypeDefinitionProvider Instance { get; } = new TypeDefinitionProvider();

		protected TypeDefinitionProvider( params ITypeDefinitionProvider[] others ) : base( others.Concat( new IFactory<TypeInfo, TypeInfo>[] { ConventionTypeDefinitionProvider.Instance, SelfTransformer<TypeInfo>.Instance } ).Fixed() ) {}

		[Freeze]
		protected override TypeInfo CreateItem( TypeInfo parameter ) => base.CreateItem( parameter );
	}

	public class ConventionTypeDefinitionProvider : FactoryBase<TypeInfo, TypeInfo>, ITypeDefinitionProvider
	{
		public static ConventionTypeDefinitionProvider Instance { get; } = new ConventionTypeDefinitionProvider();

		protected override TypeInfo CreateItem( TypeInfo parameter )
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
			
			public Context( TypeInfo current ) : this( current, Type.GetType( $"{current.FullName}Metadata, {current.Assembly.FullName}", false )?.GetTypeInfo() ) {}

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