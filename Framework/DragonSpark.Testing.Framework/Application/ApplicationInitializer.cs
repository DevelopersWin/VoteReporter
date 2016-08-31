using System.Reflection;
using DragonSpark.Commands;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Testing.Framework.Runtime;

namespace DragonSpark.Testing.Framework.Application
{
	public sealed class ApplicationInitializer : CommandBase<MethodBase>
	{
		public static IScope<ApplicationInitializer> Default { get; } = new Scope<ApplicationInitializer>( Factory.GlobalCache( () => new ApplicationInitializer() ) );
		ApplicationInitializer() {}

		public override void Execute( MethodBase parameter )
		{
			MethodContext.Default.Assign( parameter );
			Disposables.Default.Get().Add( ExecutionContext.Default.Get() );
		}
	}
}