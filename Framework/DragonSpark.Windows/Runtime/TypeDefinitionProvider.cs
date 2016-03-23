using DragonSpark.ComponentModel;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Windows.Runtime
{
	[Persistent]
	public class TypeDefinitionProvider : CompositeTypeDefinitionProvider
	{
		public TypeDefinitionProvider( [Required]ComponentModel.TypeDefinitionProvider convention ) : base( new ITypeDefinitionProvider[] { convention, MetadataTypeDefinitionProvider.Instance } ) {}
	}
}