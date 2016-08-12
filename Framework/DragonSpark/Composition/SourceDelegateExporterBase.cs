using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using DragonSpark.Sources;
using DragonSpark.Sources.Caching;
using DragonSpark.Sources.Parameterized;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public abstract class SourceDelegateExporterBase : ExportDescriptorProvider
	{
		readonly static IStackSource<CompositeActivatorParameters> Stack = new AmbientStack<CompositeActivatorParameters>();
		readonly static Func<LocateTypeRequest, Type> Locator = SourceTypes.Instance.Delegate();
		readonly static Func<Type, Type> Parameters = ParameterTypes.Instance.ToDelegate();

		readonly ICache<LifetimeContext, object> cache = new Cache<LifetimeContext, object>();
		readonly IDictionary<CompositionContract, CompositeActivator> registry = new ConcurrentDictionary<CompositionContract, CompositeActivator>();
		readonly Func<ActivatorParameter, object> resultSource;
		readonly Func<CompositionContract, CompositionContract> resolver;

		protected SourceDelegateExporterBase( Func<ActivatorParameter, object> resultSource ) : this( resultSource, Delegates<CompositionContract>.Self ) {}

		protected SourceDelegateExporterBase( Func<ActivatorParameter, object> resultSource, Func<CompositionContract, CompositionContract> resolver )
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
				var factoryType = resolved
					.With( compositionContract => new LocateTypeRequest( compositionContract.ContractType, compositionContract.ContractName ) )
					.With( Locator );
				var success = factoryType != null && descriptorAccessor.TryResolveOptionalDependency( "Factory Request", contract.ChangeType( factoryType ), true, out dependency );
				if ( success )
				{
					var types = new[] { factoryType, Parameters( factoryType ) };
					var compositionContracts = types.WhereAssigned().Select( resolved.ChangeType ).ToArray();
					Register( descriptorAccessor, compositionContracts );
					var provider = new ServiceProvider( Stack, registry.TryGet, resolved );
					
					var activator = new ActivatorFactory( Stack, resultSource, new ActivatorParameter( provider, factoryType ) );
					var factory = dependency.Target.IsShared ? new SharedFactory( activator.Create, cache, Contracts.Default.Get( dependency.Contract ) ) : new Factory( activator.Create );
					yield return new ExportDescriptorPromise( dependency.Contract, GetType().Name, dependency.Target.IsShared, NoDependencies, factory.Create );
				}
			}
		}

		void Register( DependencyAccessor accessor, params CompositionContract[] contracts )
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

		class Factory : FactoryBase<IEnumerable<CompositionDependency>, ExportDescriptor>
		{
			readonly CompositeActivator activator, create;

			public Factory( CompositeActivator activator )
			{
				this.activator = activator;
				create = Create;
			}

			public override ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( create, NoMetadata );

			protected virtual object Create( LifetimeContext context, CompositionOperation operation ) => context.Registered( activator( context, operation ) );
		}

		class SharedFactory : Factory
		{
			readonly ICache<LifetimeContext, object> cache;
			readonly string boundary;

			public SharedFactory( CompositeActivator activator, ICache<LifetimeContext, object> cache, string boundary ) : base( activator )
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

	sealed class ActivatorFactory : FixedFactory<ActivatorParameter, object>
	{
		readonly IStackSource<CompositeActivatorParameters> stack;
		
		public ActivatorFactory( IStackSource<CompositeActivatorParameters> stack, Func<ActivatorParameter, object> factory, ActivatorParameter parameter ) : base( factory, parameter )
		{
			this.stack = stack;
		}

		public object Create( LifetimeContext context, CompositionOperation operation )
		{
			using ( stack.Assignment( new CompositeActivatorParameters( context, operation ) ) )
			{
				return Get();
			}
		}
	}

	public class DelegateResultFactory : FactoryBase<ActivatorParameter, object>
	{
		public static DelegateResultFactory Instance { get; } = new DelegateResultFactory();
		DelegateResultFactory() {}

		public override object Create( ActivatorParameter parameter ) => new SourceFactory( parameter.Services.Self ).Create( parameter.FactoryType );
	}

	public sealed class DelegateFactory : FactoryBase<ActivatorParameter, Func<object>>
	{
		public static DelegateFactory Instance { get; } = new DelegateFactory();
		DelegateFactory() {}

		public override Func<object> Create( ActivatorParameter parameter ) => new SourceDelegates( parameter.Services.Self ).Get( parameter.FactoryType );
	}

	public struct CompositeActivatorParameters
	{
		public CompositeActivatorParameters( LifetimeContext context, CompositionOperation operation )
		{
			Context = context;
			Operation = operation;
		}

		public LifetimeContext Context { get; }
		public CompositionOperation Operation { get; }
	}

	public struct ActivatorParameter
	{
		public ActivatorParameter( IServiceProvider provider, Type factoryType )
		{
			Services = provider;
			FactoryType = factoryType;
		}

		public IServiceProvider Services { get; }
		public Type FactoryType { get; }
	}

	public sealed class Contracts : FactoryCache<CompositionContract, string>
	{
		public static Contracts Default { get; } = new Contracts();
		Contracts() {}

		protected override string Create( CompositionContract parameter )
		{
			object sharingBoundaryMetadata = null;
			return parameter.MetadataConstraints?.ToDictionary( pair => pair.Key, pair => pair.Value ).TryGetValue( "SharingBoundary", out sharingBoundaryMetadata ) ?? false ? (string)sharingBoundaryMetadata : null;
		}
	}
}