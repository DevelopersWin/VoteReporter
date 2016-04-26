using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Setup.Registration;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	[Persistent]
	public class TypeDefinitionProvider : FirstFromParameterFactory<TypeInfo, TypeInfo>, ITypeDefinitionProvider
	{
		public static TypeDefinitionProvider Instance { get; } = new TypeDefinitionProvider();

		TypeDefinitionProvider() : base( MetadataTypeDefinitionProvider.Instance, ComponentModel.TypeDefinitionProvider.Instance/*, SelfTransformer<TypeInfo>.Instance*/ ) {}

		[Freeze]
		protected override TypeInfo CreateItem( TypeInfo parameter ) => base.CreateItem( parameter );
	}

	/*public class MetadataClassProvider : TransformerBase<MemberInfo>
	{
		readonly ITypeDefinitionProvider converter;
		public MetadataClassProvider( ITypeDefinitionProvider converter )
		{
			this.converter = converter;
		}

		protected override MemberInfo CreateItem( MemberInfo parameter )
		{
			// var typeInfo = DetermineTypeInfo( parameter );

			return null;
		}
	}*/

	
}