using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Composition
{
	public sealed class ConventionTypeSelector : DelegatedTransformer<Type>
	{
		public static ConventionTypeSelector Default { get; } = new ConventionTypeSelector();
		ConventionTypeSelector() : this( ConventionImplementations.Default.Get ) {}

		public ConventionTypeSelector( Transform<Type> source ) : base( source ) {}

		public override Type Get( Type parameter ) => base.Get( parameter ) ?? parameter;
	}
}