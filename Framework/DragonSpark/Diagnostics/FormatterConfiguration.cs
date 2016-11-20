using DragonSpark.Activation;
using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources;
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

	public sealed class FormattableSpecifications : ItemSourceBase<Func<Type, bool>>
	{
		public static FormattableSpecifications Default { get; } = new FormattableSpecifications();
		FormattableSpecifications() : this( KnownTypesOf<IFormattable>.Default.Get, ConstructingParameterTypeLocator.Default.Get, TypeAssignableSpecification.Delegates.Get ) {}

		readonly Func<ImmutableArray<Type>> source;
		readonly Func<Type, Type> locator;
		readonly Func<Type, Func<Type, bool>> selector;

		[UsedImplicitly]
		public FormattableSpecifications( Func<ImmutableArray<Type>> source, Func<Type, Type> locator, Func<Type, Func<Type, bool>> selector )
		{
			this.source = source;
			this.locator = locator;
			this.selector = selector;
		}

		protected override IEnumerable<Func<Type, bool>> Yield() => source().SelectAssigned( locator ).Select( selector );
	}
}