using System;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Windows.Markup
{
	public class StringDesignerValueFactory : ValidatedParameterizedSourceBase<Type, object>
	{
		public static StringDesignerValueFactory Default { get; } = new StringDesignerValueFactory();

		public StringDesignerValueFactory() : base( TypeAssignableSpecification<string>.Default ) {}

		public override object Get( Type parameter ) => parameter.AssemblyQualifiedName;
	}
}