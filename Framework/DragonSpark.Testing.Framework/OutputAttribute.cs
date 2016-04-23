using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using Serilog;
using System;
using System.Reflection;

namespace DragonSpark.Testing.Framework
{
	[Serializable, LinesOfCodeAvoided( 8 )]
	public sealed class OutputAttribute : MethodInterceptionAspect
	{
		public override void OnInvoke( MethodInterceptionArgs args ) => new OutputCommand().Run( args );
	}

	public class OutputCommand : Command<MethodInterceptionArgs>
	{
		readonly Func<MethodBase, DisposingCommand<MethodBase>> commandSource;
		readonly Func<MethodInterceptionArgs, IProfiler> profilerSource;

		public OutputCommand() : this( method => new AssignExecutionContextCommand() ) {}

		public OutputCommand( Func<MethodBase, DisposingCommand<MethodBase>> commandSource ) : this( commandSource, new Factory().Create ) {}

		OutputCommand( Func<MethodBase, DisposingCommand<MethodBase>> commandSource, Func<MethodInterceptionArgs, IProfiler> profilerSource )
		{
			this.commandSource = commandSource;
			this.profilerSource = profilerSource;
		}

		protected override void OnExecute( MethodInterceptionArgs parameter )
		{
			using ( commandSource( parameter.Method ).ExecuteWith( parameter.Method ) )
			{
				using ( profilerSource( parameter ) )
				{
					parameter.Proceed();
				}
			}
		}

		class Factory : FactoryBase<MethodInterceptionArgs, IProfiler>
		{
			readonly ILogger logger;
			readonly ILoggerHistory history;

			public Factory() : this( Services.Get<ILogger>(), Services.Get<ILoggerHistory>() ) {}

			Factory( ILogger logger, ILoggerHistory history )
			{
				this.logger = logger;
				this.history = history;
			}

			protected override IProfiler CreateItem( MethodInterceptionArgs parameter )
			{
				var output = parameter.Instance.AsTo<ITestOutputAware, Action<string>>( value => value.Output.WriteLine ) ?? IgnoredOutputCommand.Instance.Run;
				var result = new Diagnostics.ProfilerFactory( output, logger, history ).Create( parameter.Method );
				return result;
			}
		}
	}
}