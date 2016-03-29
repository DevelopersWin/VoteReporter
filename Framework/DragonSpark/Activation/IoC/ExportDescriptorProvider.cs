using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition;

namespace DragonSpark.Activation.IoC
{
	public class ServicesIntegrationExtension : UnityContainerExtension
	{
		readonly ServicesStrategy strategy;
		// readonly CompositionContext host;
		// readonly ExportDescriptorProvider provider;
		// readonly RegisterHierarchyCommand<OnlyIfNotRegistered> command;

		public ServicesIntegrationExtension( [Required]ServicesStrategy strategy/*, [Required]CompositionHost host, [Required]ExportDescriptorProvider provider, [Required]RegisterHierarchyCommand<OnlyIfNotRegistered> command*/ )
		{
			this.strategy = strategy;
			// this.host = host;
			// this.provider = provider;
			// this.command = command;
		}

		protected override void Initialize()
		{
			// command.ExecuteWith( new InstanceRegistrationParameter( host ) );

			// host.GetExport<IExportDescriptorProviderRegistry>().Register( provider );

			Context.Strategies.Add( strategy, UnityBuildStage.PreCreation );
		}

		public IUnityContainer Refresh() => Container.With( unityContainer => Initialize() );
	}

	public class ServicesStrategy : BuilderStrategy
	{
		readonly Func<IServiceProvider> coordinator;
		readonly Func<IServiceRegistry> registry;

		public ServicesStrategy( [Required] Func<IServiceProvider> coordinator, [Required]Func<IServiceRegistry> registry )
		{
			this.coordinator = coordinator;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			if ( !context.HasBuildPlan() )
			{
				var existing = coordinator().GetService( context.BuildKey.Type );
				existing.With( o =>
				{
					if ( new Checked( o, this ).Item.Apply() )
					{
						var instance = o.Has<SharedAttribute>() || new ExportProperties.Instance( o ).Item || new ExportProperties.Factory( o ).Item.With( promise => promise.Contract.ContractType.Has<SharedAttribute>() );
						if ( instance )
						{
							registry().Register( new InstanceRegistrationParameter( context.BuildKey.Type, o ) );
						}
					}
				} );
				context.Complete( existing );
			}
		}
	}
}