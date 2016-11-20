using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;

namespace DragonSpark
{
	public sealed class FormattableTypes : SingletonScope<ImmutableArray<Type>>
	{
		public static FormattableTypes Default { get; } = new FormattableTypes();
		FormattableTypes() : this( Implementation.Instance.Get ) {}

		[UsedImplicitly]
		public FormattableTypes( Func<ImmutableArray<Type>> factory ) : base( factory ) {}

		public sealed class Implementation : ItemSourceBase<Type>
		{
			public static Implementation Instance { get; } = new Implementation();
			Implementation() : this( KnownTypesOf<IFormattable>.Default.Get, ConstructingParameterTypes.Default.Get ) {}

			readonly Func<ImmutableArray<Type>> source;
			readonly Func<Type, Type> locator;

			[UsedImplicitly]
			public Implementation( Func<ImmutableArray<Type>> source, Func<Type, Type> locator )
			{
				this.source = source;
				this.locator = locator;
			}

			protected override IEnumerable<Type> Yield() => source().SelectAssigned( locator );
		}
	}
}