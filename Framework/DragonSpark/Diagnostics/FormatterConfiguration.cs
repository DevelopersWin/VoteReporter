using DragonSpark.Activation;
using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Diagnostics
{
	sealed class FormatterConfiguration : LoggingConfigurationBase
	{
		public static FormatterConfiguration Default { get; } = new FormatterConfiguration();
		FormatterConfiguration() : this( FormattableSpecifications.Default.Get, Formatter.Default.Get ) {}

		readonly Func<ImmutableArray<Func<Type, bool>>> source;
		readonly Func<object, object> formatter;

		[UsedImplicitly]
		public FormatterConfiguration( Func<ImmutableArray<Func<Type, bool>>> source, Func<object, object> formatter )
		{
			this.source = source;
			this.formatter = formatter;
		}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			foreach ( var specification in source() )
			{
				parameter.Destructure.ByTransformingWhere( specification, formatter );
			}
			return parameter;
		}
	}

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