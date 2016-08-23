using DragonSpark.Activation;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using Serilog;
using System;

namespace DragonSpark.Diagnostics.Logging
{
	sealed class FormatterConfiguration : TransformerBase<LoggerConfiguration>
	{
		readonly static Func<object, object> Formatter = Runtime.Formatter.Default.Format;

		public static FormatterConfiguration Default { get; } = new FormatterConfiguration();
		FormatterConfiguration() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			foreach ( var type in KnownTypes.Default.Get<IFormattable>() )
			{
				var located = ConstructingParameterLocator.Default.Get( type );
				if ( located != null )
				{
					parameter.Destructure.ByTransformingWhere( new TypeAssignableSpecification( located ).ToCachedSpecification().ToSpecificationDelegate(), Formatter );
				}
			}

			return parameter;
		}
	}
}