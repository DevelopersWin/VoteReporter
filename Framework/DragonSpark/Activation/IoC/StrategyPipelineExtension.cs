﻿using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using Defaults = DragonSpark.Sources.Parameterized.Defaults;
using Delegate = System.Delegate;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	public abstract class MonitorExtensionBase : UnityContainerExtension, IDisposable
	{
		protected override void Initialize() => Context.RegisteringInstance += OnRegisteringInstance;

		protected abstract void OnRegisteringInstance( object sender, RegisterInstanceEventArgs args );

		public override void Remove()
		{
			base.Remove();

			Context.RegisteringInstance -= OnRegisteringInstance;
		}

		public void Dispose() => Remove();
	}

	public class InstanceTypeRegistrationMonitorExtension : MonitorExtensionBase
	{
		protected override void OnRegisteringInstance( object sender, RegisterInstanceEventArgs args )
		{
			var type = args.Instance.GetType();

			if ( args.RegisteredType != type && !Container.IsRegistered( type, args.Name ) )
			{
				var registry = new ServiceRegistry( Container, args.LifetimeManager.GetType() );
				registry.Register( new InstanceRegistrationParameter( type, args.Instance, args.Name ) );
			}
		}
	}

	public class DefaultConstructorPolicyExtension : UnityContainerExtension
	{
		readonly ConstructorLocator store;

		public DefaultConstructorPolicyExtension( ConstructorLocator store )
		{
			this.store = store;
		}

		protected override void Initialize() => Context.Policies.SetDefault<IConstructorSelectorPolicy>( new ConstructorSelectorPolicy( store ) );
	}

	public class CachingBuildPlanExtension : UnityContainerExtension
	{
		readonly static IAtomicCache<object, IBuildPlanCreatorPolicy> Policies = new Cache<IBuildPlanCreatorPolicy>();

		readonly IBuildPlanRepository repository;
		
		public CachingBuildPlanExtension( IBuildPlanRepository repository )
		{
			this.repository = repository;
		}

		protected override void Initialize()
		{
			var creator = (object)Origin.Default.Get( Container )?.GetType() ?? Execution.Current();
			var policies = repository.List();
			var policy = new BuildPlanCreatorPolicy( Policies.GetOrSet( creator, Create ), policies );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( policy );
		}

		IBuildPlanCreatorPolicy Create( object instance )
		{
			var buildPlanCreatorPolicy = Context.Policies.Get<IBuildPlanCreatorPolicy>( null );
			return new CachedCreatorPolicy( buildPlanCreatorPolicy );
		}

		class CachedCreatorPolicy : IBuildPlanCreatorPolicy
		{
			readonly IBuildPlanCreatorPolicy inner;

			readonly ICache<NamedTypeBuildKey, IBuildPlanPolicy> cache = new Cache<NamedTypeBuildKey, IBuildPlanPolicy>();

			public CachedCreatorPolicy( IBuildPlanCreatorPolicy inner )
			{
				this.inner = inner;
			}

			public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey )
			{
				var key = References.Keys.Get( buildKey );
				var result = cache.Contains( key ) ? cache.Get( key ) : cache.SetValue( key, inner.CreatePlan( context, buildKey ) );
				return result;
			}
		}
	}

	public static class References
	{
		public static EqualityReference<NamedTypeBuildKey> Keys { get; } = EqualityReference<NamedTypeBuildKey>.Instance;
	}

	public class StrategyPipelineExtension : UnityContainerExtension
	{
		readonly IStrategyRepository strategyRepository;
		readonly DefaultStrategyEntries factory;

		public StrategyPipelineExtension( IStrategyRepository strategyRepository, DefaultStrategyEntries factory )
		{
			this.strategyRepository = strategyRepository;
			this.factory = factory;
		}

		protected override void Initialize()
		{
			factory.Get().Each( strategyRepository.Add );

			Context.Strategies.Clear();

			foreach ( var entry in strategyRepository.List().CastArray<StrategyEntry>() )
			{
				Context.Strategies.Add( entry.Get(), entry.Stage );
			}
		}

		public class DefaultStrategyEntries : SourceBase<IEnumerable<StrategyEntry>>
		{
			readonly MetadataLifetimeStrategy metadataLifetimeStrategy;
			readonly ConventionStrategy conventionStrategy;

			public DefaultStrategyEntries( MetadataLifetimeStrategy metadataLifetimeStrategy, ConventionStrategy conventionStrategy )
			{
				this.metadataLifetimeStrategy = metadataLifetimeStrategy;
				this.conventionStrategy = conventionStrategy;
			}

			public override IEnumerable<StrategyEntry> Get() => new[]
			{
				new StrategyEntry( metadataLifetimeStrategy, UnityBuildStage.Lifetime, Priority.Higher ),
				new StrategyEntry( conventionStrategy, UnityBuildStage.PreCreation )
			};
		}

		public class MetadataLifetimeStrategy : BuilderStrategy
		{
			readonly ILogger logger;
			readonly LifetimeManagerFactory factory;
			readonly Condition condition = new Condition();

			public MetadataLifetimeStrategy( ILogger logger, LifetimeManagerFactory factory )
			{
				this.logger = logger;
				this.factory = factory;
			}

			public override void PreBuildUp( IBuilderContext context )
			{
				var reference = References.Keys.Get( context.BuildKey );
				if ( condition.Get( reference ).Apply() )
				{
					var lifetimePolicy = context.Policies.GetNoDefault<ILifetimePolicy>( context.BuildKey, false );
					if ( lifetimePolicy == null )
					{
						var manager = factory.Create( reference.Type );
						if ( manager != null )
						{
							logger.Debug( "'{TypeName}' is assigning a lifetime manager of '{LifetimeManager}' for type '{Reference}'.", GetType().Name, manager.GetType(), reference.Type );

							context.PersistentPolicies.Set<ILifetimePolicy>( manager, reference );
						}
					}
				}
			}
		}
	}

	public sealed class ConventionTypes : ParameterizedScope<Type, Type>
	{
		readonly static Func<Type, ITypeCandidateWeightProvider> Weight = ParameterConstructor<Type, TypeCandidateWeightProvider>.Default;
		readonly static Func<Type, bool> Specification = Defaults.ActivateSpecification.IsSatisfiedBy;

		public static IParameterizedSource<Type, Type> Instance { get; } = new ConventionTypes();
		ConventionTypes() : this( ApplicationTypes.Instance ) {}

		public ConventionTypes( ITypeSource source ) : base( new Locator( source ).ToDelegate().CachedPerScope() ) {}

		[ApplyAutoValidation]
		sealed class Locator : FactoryBase<Type, Type>
		{
			readonly ITypeSource source;

			public Locator( ITypeSource source ) : base( CanInstantiateSpecification.Instance.Inverse() )
			{
				this.source = source;
			}

			static Type Map( Type parameter )
			{
				var name = $"{parameter.Namespace}.{ConventionCandidateNameFactory.Instance.Create( parameter )}";
				var result = name != parameter.FullName ? parameter.Assembly().GetType( name ) : null;
				return result;
			}

			Type Search( Type parameter )
			{
				var adapter = parameter.Adapt();
				var order = Weight( parameter );
				var convention = IsConventionCandidateSpecification.Default.Get( parameter );
				var result =
						source.Get()
						.Where( adapter.IsAssignableFrom )
						.Where( Specification )
						.OrderByDescending( order.GetWeight )
						.FirstOrDefault( convention );
				return result;
			}

			public override Type Create( Type parameter ) => Map( parameter ) ?? Search( parameter );
		}
	}

	class IsConventionCandidateSpecification : GuardedSpecificationBase<Type>
	{
		readonly static Func<Type, string> Sanitizer = ConventionCandidateNameFactory.Instance.ToDelegate();

		public static IParameterizedSource<Type, Func<Type, bool>> Default { get; } = new Cache<Type, Func<Type, bool>>( t => new IsConventionCandidateSpecification( t ).IsSatisfiedBy );
		IsConventionCandidateSpecification( Type type ) : this( type, Sanitizer ) {}

		readonly string type;

		public IsConventionCandidateSpecification( Type type, Func<Type, string> sanitizer ) : this( sanitizer( type ) ) {}
		IsConventionCandidateSpecification( string type )
		{
			this.type = type;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter.Name.Equals( type );
	}

	class ConventionCandidateNameFactory : FactoryBase<Type, string>
	{
		public static ConventionCandidateNameFactory Instance { get; } = new ConventionCandidateNameFactory();
		ConventionCandidateNameFactory() {}

		public override string Create( Type parameter ) => parameter.Name.TrimStartOf( 'I' );
	}

	public interface ITypeCandidateWeightProvider
	{
		int GetWeight( Type candidate );
	}

	public class TypeCandidateWeightProvider : FactoryBase<Type, int>, ITypeCandidateWeightProvider
	{
		readonly Type subject;

		public TypeCandidateWeightProvider( Type subject )
		{
			this.subject = subject;
		}

		public override int Create( Type parameter ) => parameter.IsNested ? subject.GetTypeInfo().DeclaredNestedTypes.Contains( parameter.GetTypeInfo() ) ? 2 : -1 : 0;

		public int GetWeight( Type candidate ) => Create( candidate );
	}

	public sealed class SelfAndNestedTypes : Cache<Type, IEnumerable<Type>>
	{
		public static SelfAndNestedTypes Instance { get; } = new SelfAndNestedTypes();
		SelfAndNestedTypes() : base( type => type.Adapt().WithNested() ) {}
	}

	public class ConventionImplementedInterfaces : FactoryCache<Type, Type>
	{
		public static ConventionImplementedInterfaces Instance { get; } = new ConventionImplementedInterfaces( typeof(ISource), typeof(IParameterizedSource), typeof(IValidatedParameterizedSource) );
		ConventionImplementedInterfaces( params Type[] ignore ) : this( ignore.ToImmutableArray() ) {}

		readonly ImmutableArray<Type> ignore;

		public ConventionImplementedInterfaces( ImmutableArray<Type> ignore )
		{
			this.ignore = ignore;
		}

		protected override Type Create( Type parameter )
		{
			var types = parameter.GetTypeInfo().ImplementedInterfaces.Except( ignore.ToArray() ).ToArray();
			foreach ( var type in types )
			{
				if ( parameter.Name.Contains( type.Name.TrimStartOf( 'I' ) ) )
				{
					return type;
				}
			}
			return null;
		}
	}

	public class CanInstantiateSpecification : GuardedSpecificationBase<Type>
	{
		public static ISpecification<Type> Instance { get; } = new CanInstantiateSpecification().Cached();
		CanInstantiateSpecification() {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = !info.IsGenericTypeDefinition && !info.ContainsGenericParameters && !info.IsInterface && !info.IsAbstract && info.DeclaredConstructors.Any( constructorInfo => constructorInfo.IsPublic ) && ( info.IsPublic || info.Assembly.Has<RegistrationAttribute>() );
			return result;
		}
	}

	public class InstantiableTypeSpecification : GuardedSpecificationBase<Type>
	{
		public static ISpecification<Type> Instance { get; } = new InstantiableTypeSpecification().Cached();
		InstantiableTypeSpecification() : this( new[] { typeof(Delegate), typeof(Array) }.Select( type => type.Adapt() ).ToImmutableArray() ) {}

		readonly ImmutableArray<TypeAdapter> exempt;

		public InstantiableTypeSpecification( ImmutableArray<TypeAdapter> exempt )
		{
			this.exempt = exempt;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter != typeof(object) && !exempt.IsAssignableFrom( parameter );
	}

	public class ConventionStrategy : BuilderStrategy
	{
		readonly Condition condition = new Condition();

		readonly ConventionCandidates locator;
		readonly IServiceRegistry registry;

		public ConventionStrategy( ConventionCandidates locator, IServiceRegistry registry )
		{
			this.locator = locator;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = References.Keys.Get( context.BuildKey );
			if ( condition.Get( reference ).Apply() )
			{
				var from = context.BuildKey.Type;
				var convention = locator.Get( from );
				if ( convention != null )
				{
					context.BuildKey = new NamedTypeBuildKey( convention, context.BuildKey.Name );
					
					registry.Register( new MappingRegistrationParameter( from, context.BuildKey.Type, context.BuildKey.Name ) );
				}
			}
		}

		public class ConventionCandidates : FactoryCache<Type, Type>
		{
			readonly static ISpecification<Type> Specification = InstantiableTypeSpecification.Instance.And( CanInstantiateSpecification.Instance.Inverse() );

			public ConventionCandidates() : base( Specification ) {}
			protected override Type Create( Type parameter ) => ConventionTypes.Instance.Get( parameter );
		}
	}
}