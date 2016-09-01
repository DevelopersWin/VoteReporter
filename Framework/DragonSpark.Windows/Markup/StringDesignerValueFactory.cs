using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Windows.Markup
{
	public class StringDesignerValueFactory : SpecificationParameterizedSource<Type, object>
	{
		public static StringDesignerValueFactory Default { get; } = new StringDesignerValueFactory();
		StringDesignerValueFactory() : base( TypeAssignableSpecification<string>.Default.ToSpecificationDelegate(), type => type.AssemblyQualifiedName ) {}
	}
}