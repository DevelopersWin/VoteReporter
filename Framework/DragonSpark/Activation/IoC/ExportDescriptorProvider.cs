using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;

namespace DragonSpark.Activation.IoC
{
	public class CompositionExtension : UnityContainerExtension
	{
		readonly ComposeStrategy strategy;
		readonly CompositionHost host;
		readonly ExportDescriptorProvider provider;
		readonly RegisterHierarchyCommand<OnlyIfNotRegistered> command;

		public CompositionExtension( [Required]ComposeStrategy strategy, [Required]CompositionHost host, [Required]ExportDescriptorProvider provider, [Required]RegisterHierarchyCommand<OnlyIfNotRegistered> command )
		{
			this.strategy = strategy;
			this.host = host;
			this.provider = provider;
			this.command = command;
		}

		protected override void Initialize()
		{
			command.ExecuteWith( new InstanceRegistrationParameter( host ) );

			host.GetExport<IExportDescriptorProviderRegistry>().Register( provider );

			Context.Strategies.Add( strategy, UnityBuildStage.PreCreation );
		}
	}

	public class ExportDescriptorProvider : System.Composition.Hosting.Core.ExportDescriptorProvider
	{
		readonly CompositionCoordinator coordinator;

		public ExportDescriptorProvider( [Required]CompositionCoordinator coordinator )
		{
			this.coordinator = coordinator;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Context( coordinator, contract ).Create );
			}
		}

		 class Context
		{
			readonly CompositionCoordinator coordinator;
			readonly CompositionContract contract;

			public Context( [Required]CompositionCoordinator coordinator, [Required]CompositionContract contract )
			{
				this.coordinator = coordinator;
				this.contract = contract;
			}

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( Activate, NoMetadata );

			object Activate( LifetimeContext context, CompositionOperation operation ) => coordinator.Create( contract );
		}
	}

	[Persistent]
	public class CompositionCoordinator
	{
		readonly IActivator activator;
		readonly CompositionHost host;

		public CompositionCoordinator( [Required]IActivator activator, [Required]CompositionHost host )
		{
			this.activator = activator;
			this.host = host;
		}

		class Operation
		{
			readonly NamedTypeBuildKey key;

			public Operation( [Required]NamedTypeBuildKey key )
			{
				this.key = key;
			}

			public bool InProgress( [Required] Type type, string name ) => key.Type == type && key.Name == name;
		}

		public object Create( CompositionContract contract )
		{
			var current = Ambient.GetCurrent<Operation>();
			var result = current.With( operation => !operation.InProgress( contract.ContractType, contract.ContractName ), () => true ) ? activator.Create( new LocateTypeRequest( contract.ContractType, contract.ContractName ) ) : null;
			return result;
		}

		public object Create( NamedTypeBuildKey key )
		{
			var chain = new ThreadAmbientChain<Operation>();
			if ( !chain.Item.Any( operation => operation.InProgress( key.Type, key.Name ) ) )
			{
				using ( new AmbientContextCommand<Operation>( chain ).ExecuteWith( new Operation( key ) )  )
				{
					object existing;
					if ( host.TryGetExport( key.Type, key.Name, out existing ) )
					{
						return existing;
					}
				}
			}
			return null;
		}
	}

	public class ComposeStrategy : BuilderStrategy
	{
		readonly CompositionCoordinator coordinator;
		readonly IServiceRegistry registry;

		public ComposeStrategy( [Required] CompositionCoordinator coordinator, [Required]IServiceRegistry registry )
		{
			this.coordinator = coordinator;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			if ( !context.HasRegisteredBuildPlan() )
			{
				var existing = coordinator.Create( context.BuildKey );
				existing.With( o =>
				{
					if ( new Checked( o, this ).Item.Apply() )
					{
						var instance = o.Has<SharedAttribute>() || new ExportProperties.Instance( o ).Item || new ExportProperties.Factory( o ).Item.With( promise => promise.Contract.ContractType.Has<SharedAttribute>() );
						if ( instance )
						{
							registry.Register( new InstanceRegistrationParameter( context.BuildKey.Type, o ) );
						}
					}
				} );
				context.Complete( existing );
			}
		}
	}
}