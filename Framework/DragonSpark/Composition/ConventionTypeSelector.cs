using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Composition
{
	public sealed class ConventionTypeSelector : DelegatedTransformer<Type>
	{
		public static ConventionTypeSelector Default { get; } = new ConventionTypeSelector();
		ConventionTypeSelector() : this( ConventionTypes.Default.Get ) {}

		public ConventionTypeSelector( Transform<Type> source ) : base( source ) {}

		public override Type Get( Type parameter ) => base.Get( parameter ) ?? parameter;
	}
}