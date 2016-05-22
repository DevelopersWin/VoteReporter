using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using Serilog;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace DragonSpark.Testing.Framework
{
	[Serializable, LinesOfCodeAvoided( 8 )]
	public sealed class OutputAttribute : MethodInterceptionAspect
	{
		public override void OnInvoke( MethodInterceptionArgs args ) => new OutputCommand().Run( new OutputCommand.Parameter( args.Instance, args.Method, args.Proceed ) );
	}

	/*public class ParameterFactory : FactoryBase<MethodInterceptionArgs, OutputCommand.Parameter>
	{
		public static ParameterFactory Instance { get; } = new ParameterFactory();

		protected override OutputCommand.Parameter CreateItem( MethodInterceptionArgs parameter ) => new OutputCommand.Parameter( parameter.Instance, parameter.Method, parameter.Proceed );
	}*/

	public class OutputCommand : CommandBase<OutputCommand.Parameter>
	{
		public class Parameter
		{
			public Parameter( object instance, MethodBase method, Action @continue )
			{
				Instance = instance;
				Method = method;
				Continue = @continue;
			}

			public object Instance { get; }
			public MethodBase Method { get; }
			public Action Continue { get; }
		}

		readonly Func<MethodBase, DisposingCommand<MethodBase>> commandSource;
		readonly Func<Parameter, IProfiler> profilerSource;

		public OutputCommand() : this( method => new AssignExecutionContextCommand() ) {}

		public OutputCommand( Func<MethodBase, DisposingCommand<MethodBase>> commandSource ) : this( commandSource, Factory.New ) {}

		OutputCommand( Func<MethodBase, DisposingCommand<MethodBase>> commandSource, Func<Parameter, IProfiler> profilerSource )
		{
			this.commandSource = commandSource;
			this.profilerSource = profilerSource;
		}

		public override void Execute( Parameter parameter )
		{
			using ( commandSource( parameter.Method ).AsExecuted( parameter.Method ) )
			{
				using ( profilerSource( parameter ) )
				{
					parameter.Continue();
				}
			}
		}

		class Factory : FactoryBase<Parameter, IProfiler>
		{
			readonly ILogger logger;
			readonly ILoggerHistory history;

			public static IProfiler New( Parameter arg ) => new Factory().Create( arg );

			Factory() : this( Services.Get<ILogger>(), Services.Get<ILoggerHistory>() ) {}

			Factory( ILogger logger, ILoggerHistory history )
			{
				this.logger = logger;
				this.history = history;
			}

			public override IProfiler Create( Parameter parameter )
			{
				var output = parameter.Instance.AsTo<ITestOutputAware, Action<string>>( value => value.Output.WriteLine ) ?? IgnoredOutputCommand.Instance.Run;
				var result = new Diagnostics.TraceAwareProfilerFactory( output, logger, history ).Create( parameter.Method );
				return result;
			}
		}
	}
}