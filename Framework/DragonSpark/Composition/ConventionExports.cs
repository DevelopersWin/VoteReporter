using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Composition
{
	public sealed class ConventionExports : ExportSourceBase<bool>
	{
		readonly ImmutableArray<Type> interfaces;

		public ConventionExports( IEnumerable<Type> interfaces, IEnumerable<Type> types ) : base( types )
		{
			this.interfaces = interfaces.ToImmutableArray();
		}

		public override bool Get( Type parameter ) => interfaces.Contains( parameter );
	}
}