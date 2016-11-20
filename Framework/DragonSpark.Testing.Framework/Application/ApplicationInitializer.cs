using DragonSpark.Commands;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using DragonSpark.Testing.Framework.Runtime;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Application
{
	public sealed class ApplicationInitializer : DelegatedCommand<MethodBase>
	{
		public static ISource<ApplicationInitializer> Default { get; } = new SingletonScope<ApplicationInitializer>( () => new ApplicationInitializer() );
		ApplicationInitializer() : this( Disposables.Default, ExecutionContext.Default.Get ) {}

		readonly IComposable<IDisposable> disposables;
		readonly Func<IDisposable> source;

		[UsedImplicitly]
		public ApplicationInitializer( IComposable<IDisposable> disposables, Func<IDisposable> source ) : base( CurrentMethod.Default.Assign )
		{
			this.disposables = disposables;
			this.source = source;
		}

		public override void Execute( MethodBase parameter )
		{
			base.Execute( parameter );
			disposables.Add( source() );
		}
	}

	/*public sealed class ApplicationInitializer : CommandBase<MethodBase>
	{
		public static IScope<ApplicationInitializer> Default { get; } = new SingletonScope<ApplicationInitializer>( () => new ApplicationInitializer() );
		ApplicationInitializer() : this( Disposables.Default ) {}

		readonly IComposable<IDisposable> disposables;

		[UsedImplicitly]
		public ApplicationInitializer( IComposable<IDisposable> disposables )
		{
			this.disposables = disposables;
		}

		public override void Execute( MethodBase parameter )
		{
			CurrentMethod.Default.Assign( parameter );
			disposables.Add( ExecutionContext.Default.Get() );
		}
	}*/
}