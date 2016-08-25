using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using SerilogTimings.Extensions;
using System;
using System.Reflection;

namespace DragonSpark.Diagnostics.Logging
{
	public class TimedOperationFactory : ValidatedParameterizedSourceBase<MethodBase, IDisposable>
	{
		public static TimedOperationFactory Default { get; } = new TimedOperationFactory();
		TimedOperationFactory() : this( "Executed Method '{@Method}'" ) {}

		readonly string template;

		public TimedOperationFactory( string template ) : this( template, Specifications.Specifications.Assigned ) {}

		public TimedOperationFactory( string template, ISpecification<MethodBase> specification ) : base( specification )
		{
			this.template = template;
		}

		public override IDisposable Get( MethodBase parameter ) => Logger.Default.Get( parameter ).TimeOperation( template, parameter.ToItem() );
	}
}
