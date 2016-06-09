using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System;
using System.Reflection;

namespace DragonSpark.Testing.Framework
{
	/*[Serializable, LinesOfCodeAvoided( 8 )]
	public sealed class OutputAttribute : MethodInterceptionAspect
	{
		public override void OnInvoke( MethodInterceptionArgs args ) => new OutputCommand().Run( new OutputCommand.Parameter( args.Instance, args.Method, args.Proceed ) );
	}*/

	/*public class ParameterFactory : FactoryBase<MethodInterceptionArgs, OutputCommand.Parameter>
	{
		public static ParameterFactory Instance { get; } = new ParameterFactory();

		protected override OutputCommand.Parameter CreateItem( MethodInterceptionArgs parameter ) => new OutputCommand.Parameter( parameter.Instance, parameter.Method, parameter.Proceed );
	}*/

	public class OutputCommand : CommandBase<OutputCommand.Parameter>
	{
		readonly Func<MethodBase, DisposingCommand<MethodBase>> commandSource;
		readonly Func<Parameter, IProfiler> profilerSource;

		public OutputCommand() : this( method => new InitializeMethodCommand() ) {}

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

		public struct Parameter
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

		class Factory : FactoryBase<Parameter, IProfiler>
		{
			readonly ILoggerHistory history;

			public static IProfiler New( Parameter arg ) => new Factory().Create( arg );

			Factory() : this( GlobalServiceProvider.Instance.Get<ILoggerHistory>() ) {}

			Factory( ILoggerHistory history )
			{
				this.history = history;
			}

			public override IProfiler Create( Parameter parameter )
			{
				var output = parameter.Instance.AsTo<ITestOutputAware, Action<string>>( value => value.Output.WriteLine ) ?? IgnoredOutputCommand.Instance.Execute;
				var result = new Diagnostics.ProfilerFactory( output, history ).Create( parameter.Method );
				return result;
			}
		}
	}
}