using DragonSpark.Activation;
using DragonSpark.Extensions;
using System;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class TypeDefinitionProvider : TransformerBase<TypeInfo>, ITypeDefinitionProvider
	{
		public static TypeDefinitionProvider Instance { get; } = new TypeDefinitionProvider();

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