namespace DragonSpark.Testing.Framework
{
	/*public class OutputCommand : CommandBase<OutputCommand.Parameter>
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
			public Parameter( Action @continue ) : this( @continue.Target, @continue ) {}
			public Parameter( object instance, Action @continue ) : this( instance, @continue.Method, @continue ) {}

			public Parameter( object instance, MethodBase method, Action @continue )
			{
				Instance = instance;
				Method = method;
				Continue = @continue;
			}

			public object Default { get; }
			public MethodBase Method { get; }
			public Action Continue { get; }
		}

		class Factory : FactoryBase<Parameter, IProfiler>
		{
			readonly ILoggerHistory history;

			public static IProfiler New( Parameter arg ) => new Factory().Create( arg );

			Factory() : this( GlobalServiceProvider.GetService<ILoggerHistory>() ) {}

			Factory( ILoggerHistory history )
			{
				this.history = history;
			}

			public override IProfiler Create( Parameter parameter )
			{
				var output = parameter.Default.AsTo<ITestOutputAware, Action<string>>( value => value.Output.WriteLine ) ?? IgnoredOutputCommand.Default.Execute;
				var result = new Diagnostics.ProfilerFactory( output, history ).Create( parameter.Method );
				return result;
			}
		}
	}*/
}