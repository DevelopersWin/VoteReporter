using DragonSpark.Activation.FactoryModel;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
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
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	public class CompositionExtension : UnityContainerExtension
	{
		readonly ComposeStrategy strategy;
		readonly CompositionHost host;
		readonly ExportDescriptorProvider provider;
		readonly RegisterHierarchyCommand command;

		public CompositionExtension( [Required]Factory factory, [Required]IActivator activator, RegisterHierarchyCommand command, IServiceRegistry registry ) 
			: this( factory.Create(), activator, command, registry ) {}

		CompositionExtension( CompositionHost host, IActivator activator, RegisterHierarchyCommand command, IServiceRegistry registry )
			: this( host, new CompositionManager( activator, host ), command, registry ) {}

		CompositionExtension( CompositionHost host, CompositionManager manager, RegisterHierarchyCommand command, [Required]IServiceRegistry registry )
			: this( new ComposeStrategy( manager, registry ), host, new ExportDescriptorProvider( manager ), command ) {}

		CompositionExtension( [Required]ComposeStrategy strategy, [Required]CompositionHost host, [Required]ExportDescriptorProvider provider, [Required]RegisterHierarchyCommand command )
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

		public class Factory : FirstFactory<CompositionHost>
		{
			public Factory( [Required]IActivator activator, [Required]Assembly[] assemblies ) : base( activator.Activate<CompositionHost>, () => CompositionHostFactory.Instance.Create( assemblies ) ) {}
		}
	}

	public class ExportDescriptorProvider : System.Composition.Hosting.Core.ExportDescriptorProvider
	{
		readonly CompositionManager activator;

		public ExportDescriptorProvider( [Required]CompositionManager activator )
		{
			this.activator = activator;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Context( activator, contract ).Create );
			}
		}

		 class Context
		{
			readonly CompositionManager manager;
			readonly CompositionContract contract;

			public Context( [Required]CompositionManager manager, [Required]CompositionContract contract )
			{
				this.manager = manager;
				this.contract = contract;
			}

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( Activate, NoMetadata );

			object Activate( LifetimeContext context, CompositionOperation operation ) => manager.Create( contract );
		}
	}

	public class CompositionManager
	{
		readonly IActivator activator;
		readonly CompositionHost host;

		public CompositionManager( [Required]IActivator activator, [Required]CompositionHost host )
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
			var result = current.With( operation => !operation.InProgress( contract.ContractType, contract.ContractName ), () => true ) ? activator.Activate<object>( contract.ContractType, contract.ContractName ) : null;
			return result;
		}

		public object Create( NamedTypeBuildKey key )
		{
			var chain = new ThreadAmbientChain<Operation>();
			if ( chain.Item.All( operation => !operation.InProgress( key.Type, key.Name ) ) )
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
		readonly CompositionManager manager;
		readonly IServiceRegistry registry;

		public ComposeStrategy( [Required] CompositionManager manager, [Required]IServiceRegistry registry )
		{
			this.manager = manager;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			if ( !context.HasRegisteredBuildPlan() )
			{
				var existing = manager.Create( context.BuildKey );
				existing.With( o =>
				{
					if ( new Checked( o, this ).Item.Apply() )
					{
						var instance = new ExportProperties.Instance( o ).Item || new ExportProperties.Factory( o ).Item.With( promise => promise.Contract.ContractType.Has<SharedAttribute>() );
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