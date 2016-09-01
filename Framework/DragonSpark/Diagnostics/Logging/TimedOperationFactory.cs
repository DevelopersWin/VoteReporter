using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using SerilogTimings.Extensions;
using System;
using System.Reflection;

namespace DragonSpark.Diagnostics.Logging
{
	public class TimedOperationFactory : ParameterizedSourceBase<MethodBase, IDisposable>
	{
		/*public static TimedOperationFactory Default { get; } = new TimedOperationFactory();
		TimedOperationFactory() : this( "Executed Method '{@Method}'" ) {}*/

		readonly string template;

		public TimedOperationFactory( string template )
		{
			this.template = template;
		}

		public override IDisposable Get( MethodBase parameter ) => Logger.Default.Get( parameter ).TimeOperation( template, parameter.ToItem() );
	}

	public static class Configuration
	{
		public static IParameterizedScope<string, Func<MethodBase, IDisposable>> TimedOperationFactory { get; } = new ParameterizedScope<string, Func<MethodBase, IDisposable>>( TimedOperationFactorySource.Default.ToSourceDelegate().GlobalCache() );
	}

	public sealed class TimedOperationFactorySource : ParameterizedSourceBase<string, Func<MethodBase, IDisposable>>
	{
		public static IParameterizedSource<string, Func<MethodBase, IDisposable>> Default { get; } = new TimedOperationFactorySource().ToEqualityCache();
		TimedOperationFactorySource() {}

		public override Func<MethodBase, IDisposable> Get( string parameter ) => new TimedOperationFactory( parameter ).ToSourceDelegate();
	}
}
