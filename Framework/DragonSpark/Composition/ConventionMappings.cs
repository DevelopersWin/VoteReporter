using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Composition
{
	public sealed class ConventionMappings : ParameterizedSourceBase<Type, ConventionMapping>
	{
		public static IParameterizedSource<Type, ConventionMapping> Default { get; } = new ConventionMappings().ToCache();
		ConventionMappings() {}

		public override ConventionMapping Get( Type parameter )
		{
			var @interface = ConventionImplementedInterfaces.Default.Get( parameter );
			var result = @interface != null ? new ConventionMapping( @interface, parameter ) : default(ConventionMapping);
			return result;
		}
	}
}