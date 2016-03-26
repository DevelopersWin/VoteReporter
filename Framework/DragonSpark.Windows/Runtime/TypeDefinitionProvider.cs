using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	[Persistent]
	public class TypeDefinitionProvider : CompositeTypeDefinitionProvider
	{
		public static TypeDefinitionProvider Instance { get; } = new TypeDefinitionProvider( ComponentModel.TypeDefinitionProvider.Instance );

		// TypeDefinitionProvider() : this(  ) {}

		TypeDefinitionProvider( [Required]ComponentModel.TypeDefinitionProvider convention ) : base( new ITypeDefinitionProvider[] { MetadataTypeDefinitionProvider.Instance, convention } ) {}

		[Freeze]
		public override TypeInfo GetDefinition( TypeInfo info ) => base.GetDefinition( info );
	}
}