using System;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Application;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Composition
{
	sealed class ConventionLocator : TransformerBase<Type>
	{
		readonly Func<ImmutableArray<Type>> source;
		readonly Func<Type, Func<Type, bool>> @where;

		public static ConventionLocator Default { get; } = new ConventionLocator();
		ConventionLocator() : this( ApplicationTypes.Default.ToDelegate(), IsConventionCandidateSpecification.Defaults.ToSourceDelegate() ) {}

		public ConventionLocator( Func<ImmutableArray<Type>> source, Func<Type, Func<Type, bool>> @where )
		{
			this.source = source;
			this.@where = @where;
		}

		public override Type Get( Type parameter ) => source().FirstOrDefault( @where( parameter ) );
	}
}