using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Sources;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;

namespace DragonSpark
{
	public sealed class FormattableSpecifications : ItemSourceBase<Func<Type, bool>>
	{
		public static FormattableSpecifications Default { get; } = new FormattableSpecifications();
		FormattableSpecifications() : this( FormattableTypes.Default.Get, TypeAssignableSpecification.Delegates.Get ) {}

		readonly Func<ImmutableArray<Type>> types;
		readonly Func<Type, Func<Type, bool>> selector;

		[UsedImplicitly]
		public FormattableSpecifications( Func<ImmutableArray<Type>> types, Func<Type, Func<Type, bool>> selector )
		{
			this.types = types;
			this.selector = selector;
		}

		protected override IEnumerable<Func<Type, bool>> Yield() => types().Select( selector );
	}
}