using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public abstract class SourceDelegateExporterBase : ExportDescriptorProvider
	{
		readonly static IStackSource<CompositeActivatorParameters> Stack = new AmbientStack<CompositeActivatorParameters>();
		readonly static Func<LocateTypeRequest, Type> Locator = SourceTypes.Default.Delegate();
		readonly static Func<Type, Type> Parameters = ParameterTypes.Default.ToDelegate();

		readonly ICache<LifetimeContext, object> cache = new Cache<LifetimeContext, object>();
		// readonly IDictionary<CompositionContract, CompositeActivator> registry = new ConcurrentDictionary<CompositionContract, CompositeActivator>();
		readonly Func<ActivationParameter, object> resultSource;
		readonly Func<CompositionContract, CompositionContract> resolver;

		protected SourceDelegateExporterBase( Func<ActivationParameter, object> resultSource, Func<CompositionContract, CompositionContract> resolver )
		{
			this.resultSource = resultSource;
			this.resolver = resolver;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			var exists = descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency );
			if ( !exists )
			{
				var resolved = resolver( contract );
				var sourceType = resolved
					.With( compositionContract => new LocateTypeRequest( compositionContract.ContractType, compositionContract.ContractName ) )
					.With( Locator );
				var success = sourceType != null && descriptorAccessor.TryResolveOptionalDependency( "Source Request", contract.ChangeType( sourceType ), true, out dependency );
				if ( success )
				{
					IDictionary<CompositionContract, CompositeActivator> registry = new ConcurrentDictionary<CompositionContract, CompositeActivator>();
					Register( registry, descriptorAccessor, new[] { sourceType, Parameters( sourceType ) }.WhereAssigned().Select( resolved.ChangeType ).ToArray() );
					var provider = new ServiceProvider( Stack, registry.TryGet, resolved );
					
					var activator = new ActivatorSource( Stack, resultSource, new ActivationParameter( provider, sourceType ) );
					var factory = dependency.Target.IsShared ? new SharedFactory( activator.Create, cache, Contracts.Default.Get( dependency.Contract ) ) : new Factory( activator.Create );
					yield return new ExportDescriptorPromise( dependency.Contract, GetType().Name, dependency.Target.IsShared, NoDependencies, factory.Get );
				}
			}
		}

		void Register( IDictionary<CompositionContract, CompositeActivator> registry, DependencyAccessor accessor, params CompositionContract[] contracts )
		{
			foreach ( var contract in contracts )
			{
				CompositionDependency dependency;
				if ( accessor.TryResolveOptionalDependency( $"Activator Request for '{GetType().FullName}'", contract, true, out dependency ) )
				{
					registry.Add( contract, dependency.Target.GetDescriptor().Activator );
				}
			}
		}

		class Factory : ParameterizedSourceBase<IEnumerable<CompositionDependency>, ExportDescriptor>
		{
			readonly CompositeActivator activator, create;

			public Factory( CompositeActivator activator )
			{
				this.activator = activator;
				create = Create;
			}

			public override ExportDescriptor Get( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( create, NoMetadata );

			protected virtual object Create( LifetimeContext context, CompositionOperation operation ) => context.Registered( activator( context, operation ) );
		}

		sealed class SharedFactory : Factory
		{
			readonly ICache<LifetimeContext, object> cache;
			readonly string boundary;

			public SharedFactory( CompositeActivator activator, ICache<LifetimeContext, object> cache, string boundary = null ) : base( activator )
			{
				this.cache = cache;
				this.boundary = boundary;
			}

			protected override object Create( LifetimeContext context, CompositionOperation operation )
			{
				var key = context.FindContextWithin( boundary );
				var result = cache.Get( key ) ?? cache.SetValue( key, base.Create( key, operation ) );
				return result;
			}
		}

		sealed class ServiceProvider : IServiceProvider
		{
			readonly IStackSource<CompositeActivatorParameters> stack;
			readonly Func<CompositionContract, CompositeActivator> provider;
			readonly CompositionContract source;

			public ServiceProvider( IStackSource<CompositeActivatorParameters> stack, Func<CompositionContract, CompositeActivator> provider, CompositionContract source )
			{
				this.stack = stack;
				this.provider = provider;
				this.source = source;
			}

			public object GetService( Type serviceType )
			{
				var activator = provider( source.ChangeType( serviceType ) );
				if ( activator != null )
				{
					var current = stack.GetCurrentItem();
					var result = activator( current.Context, current.Operation );
					return result;
				}
				return null;
			}
		}
	}
}