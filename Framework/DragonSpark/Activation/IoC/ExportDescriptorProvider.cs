using DragonSpark.Extensions;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Activation.IoC
{
	public class ServicesIntegrationExtension : UnityContainerExtension
	{
		readonly IStrategyRepository repository;
		readonly ServicesStrategy strategy;
		// readonly CompositionContext host;
		// readonly ExportDescriptorProvider provider;
		// readonly RegisterHierarchyCommand<OnlyIfNotRegistered> command;

		public ServicesIntegrationExtension( [Required]IStrategyRepository repository, [Required]ServicesStrategy strategy/*, [Required]CompositionHost host, [Required]ExportDescriptorProvider provider, [Required]RegisterHierarchyCommand<OnlyIfNotRegistered> command*/ )
		{
			this.repository = repository;
			this.strategy = strategy;
			// this.host = host;
			// this.provider = provider;
			// this.command = command;
		}

		protected override void Initialize()
		{
			repository.Add( new StrategyEntry( strategy, UnityBuildStage.PreCreation, Priority.Highest ) );

			// command.ExecuteWith( new InstanceRegistrationParameter( host ) );

			// host.GetExport<IExportDescriptorProviderRegistry>().Register( provider );
			// Context.Strategies.Add( , UnityBuildStage.PreCreation );
			
		}

		// public IUnityContainer Refresh() => Container.With( unityContainer => Initialize() );
	}

	public class ServicesStrategy : BuilderStrategy
	{
		readonly IServiceProvider provider;
		
		public ServicesStrategy( [Required] IServiceProvider provider )
		{
			this.provider = provider;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			if ( !context.HasBuildPlan() )
			{
				var existing = provider.GetService( context.BuildKey.Type );
				/*existing.With( o =>
				{
					/*if ( new Checked( o, this ).Item.Apply() )
					{
						var instance = o.Has<SharedAttribute>() || new ExportProperties.Instance( o ).Item || new ExportProperties.Factory( o ).Item.With( promise => promise.Contract.ContractType.Has<SharedAttribute>() );
						if ( instance )
						{
							registry().Register( new InstanceRegistrationParameter( context.BuildKey.Type, o ) );
						}
					}#1#
				} );*/
				context.Complete( existing );
			}
		}
	}
}