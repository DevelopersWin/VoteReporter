using DragonSpark.Commands;
using DragonSpark.Runtime;
using PostSharp.Extensibility;
using Serilog.Debugging;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class ConfigureSelfLog : RunCommandBase
	{
		public static ConfigureSelfLog Default { get; } = new ConfigureSelfLog();
		ConfigureSelfLog() {}

		public override void Execute()
		{
			SelfLog.Enable( Emit.Instance.Execute );
			Disposables.Default.Add( new DisposableAction( SelfLog.Disable ) );
		}

		public sealed class Emit : CommandBase<string>
		{
			public static Emit Instance { get; } = new Emit();
			Emit() {}

			public override void Execute( string parameter ) => LoggingSink.Default.Execute( this, SeverityType.Error, "The PostSharp SelfLog encountered a problem: {0}", parameter );
		}
	}
}