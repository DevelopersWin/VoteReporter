using DragonSpark.Aspects.Build;
using DragonSpark.Commands;
using DragonSpark.Runtime;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class MonitorProjectCommand : CommandBase<IProject>
	{
		public static MonitorProjectCommand Default { get; } = new MonitorProjectCommand();
		MonitorProjectCommand() : this( AspectRepositoryService.Default, Disposables.Default ) {}

		readonly IAspectRepositoryService service;
		readonly IDisposable disposable;
		readonly EventHandler onAction;

		[UsedImplicitly]
		public MonitorProjectCommand( IAspectRepositoryService service, IDisposable disposable )
		{
			this.service = service;
			this.disposable = disposable;
			onAction = DefaultOnAspectDiscoveryCompleted;
		}
		
		public override void Execute( IProject parameter )
		{
			service.AspectDiscoveryCompleted += onAction;
		}

		void DefaultOnAspectDiscoveryCompleted( object sender, EventArgs eventArgs )
		{
			service.AspectDiscoveryCompleted -= onAction;
			disposable.Dispose();
		}
	}
}