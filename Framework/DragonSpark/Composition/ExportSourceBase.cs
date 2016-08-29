using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Composition
{
	public abstract class ExportSourceBase<T> : ValidatedParameterizedSourceBase<Type, T>
	{
		readonly ImmutableArray<Type> types;

		protected ExportSourceBase( IEnumerable<Type> types )
		{
			this.types = types.ToImmutableArray();
		}

		public override bool IsSatisfiedBy( Type parameter ) => types.Contains( parameter );
	}
}