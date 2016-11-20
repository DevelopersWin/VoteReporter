using DragonSpark.Diagnostics.Configurations;
using JetBrains.Annotations;
using Serilog;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Diagnostics
{
	public sealed class FormatterConfiguration : LoggingConfigurationBase
	{
		public static FormatterConfiguration Default { get; } = new FormatterConfiguration();
		FormatterConfiguration() : this( FormattableSpecifications.Default.Get, Formatter.Default.Get ) {}

		readonly Func<ImmutableArray<Func<Type, bool>>> specifications;
		readonly Func<object, object> formatter;

		[UsedImplicitly]
		public FormatterConfiguration( Func<ImmutableArray<Func<Type, bool>>> specifications, Func<object, object> formatter )
		{
			this.specifications = specifications;
			this.formatter = formatter;
		}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			foreach ( var specification in specifications() )
			{
				parameter.Destructure.ByTransformingWhere( specification, formatter );
			}
			return parameter;
		}
	}
}