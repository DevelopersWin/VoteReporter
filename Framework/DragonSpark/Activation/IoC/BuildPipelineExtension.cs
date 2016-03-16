using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

	public class BuildPipelineExtension : UnityContainerExtension
	{
		readonly MetadataLifetimeStrategy metadataLifetimeStrategy;
		readonly ConventionStrategy conventionStrategy;
		readonly DefaultValueStrategy defaultValueStrategy;
		readonly EnumerableResolutionStrategy enumerableResolutionStrategy;

		public BuildPipelineExtension( [Required] MetadataLifetimeStrategy metadataLifetimeStrategy, [Required] ConventionStrategy conventionStrategy, [Required] DefaultValueStrategy defaultValueStrategy, [Required]EnumerableResolutionStrategy enumerableResolutionStrategy )
		{
			this.metadataLifetimeStrategy = metadataLifetimeStrategy;
			this.conventionStrategy = conventionStrategy;
			this.defaultValueStrategy = defaultValueStrategy;
			this.enumerableResolutionStrategy = enumerableResolutionStrategy;
		}

		protected override void Initialize()
		{
			Context.Strategies.Clear();
			Context.Strategies.AddNew<BuildKeyMappingStrategy>( UnityBuildStage.TypeMapping );
			Context.Strategies.Add( metadataLifetimeStrategy, UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<HierarchicalLifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.AddNew<LifetimeStrategy>( UnityBuildStage.Lifetime );
			Context.Strategies.Add( defaultValueStrategy, UnityBuildStage.Lifetime );
			Context.Strategies.Add( conventionStrategy, UnityBuildStage.PreCreation );
			Context.Strategies.AddNew<ArrayResolutionStrategy>( UnityBuildStage.Creation );
			Context.Strategies.Add( enumerableResolutionStrategy, UnityBuildStage.Creation );
			Context.Strategies.AddNew<BuildPlanStrategy>( UnityBuildStage.Creation );

			var policy = Context.Policies.Get<IBuildPlanCreatorPolicy>( null );
			var builder = new Builder<TryContext>( Context.Strategies, policy.CreatePlan );
			Context.Policies.SetDefault<IBuildPlanCreatorPolicy>( new BuildPlanCreatorPolicy( builder.Create, Policies, policy ) );
			Context.Policies.SetDefault<IConstructorSelectorPolicy>( DefaultUnityConstructorSelectorPolicy.Instance );
		}

		public class MetadataLifetimeStrategy : BuilderStrategy
		{
			readonly Func<ILogger> logger;
			readonly LifetimeManagerFactory factory;

			public MetadataLifetimeStrategy( [Required]Func<ILogger> logger, [Required]LifetimeManagerFactory factory )
			{
				this.logger = logger;
				this.factory = factory;
			}

			public override void PreBuildUp( IBuilderContext context )
			{
				var reference = new KeyReference( this, context.BuildKey ).Item;
				if ( new Checked( reference, this ).Item.Apply() )
				{
					var lifetimePolicy = context.Policies.GetNoDefault<ILifetimePolicy>( context.BuildKey, false );
					lifetimePolicy.Null( () =>
					{
						var lifetimeManager = factory.Create( reference.Type );
						lifetimeManager.With( manager =>
						{
							logger().Debug( $"'{GetType().Name}' is assigning a lifetime manager of '{manager.GetType()}' for type '{reference.Type}'." );

							context.PersistentPolicies.Set<ILifetimePolicy>( manager, reference );
						} );
					} );
				}
			}
		}

		public class Builder<T> : FactoryBase<IBuilderContext, T>
		{
			readonly NamedTypeBuildKey key = NamedTypeBuildKey.Make<T>();
			readonly IStagedStrategyChain strategies;
			readonly Func<IBuilderContext, NamedTypeBuildKey, IBuildPlanPolicy> creator;

			public Builder( [Required]IStagedStrategyChain strategies, [Required]Func<IBuilderContext, NamedTypeBuildKey, IBuildPlanPolicy> creator )
			{
				this.strategies = strategies;
				this.creator = creator;
			}

			protected override T CreateItem( IBuilderContext parameter )
			{
				var context = new BuilderContext( strategies.MakeStrategyChain(), parameter.Lifetime, parameter.PersistentPolicies, parameter.Policies, key, null );
				var plan = creator( context, key );
				plan.BuildUp( context );
				var result = context.Existing.To<T>();
				return result;
			}
		}

		public IList<IBuildPlanPolicy> Policies { get; } = new List<IBuildPlanPolicy> { new SingletonBuildPlanPolicy() };
	}

	public class BuildableTypeFromConventionLocator : DecoratedFactory<Type, Type>
	{
		public BuildableTypeFromConventionLocator( [Required]Assembly[] assemblies ) : this( assemblies, CanBuildSpecification.Instance ) {}

		public BuildableTypeFromConventionLocator( [Required]Assembly[] assemblies, [Required]CanBuildSpecification specification ) : base( new DecoratedSpecification<Type>( new InverseSpecification( specification ) ), new Context( assemblies, specification ).Create ) {}

		class Context : FactoryBase<Type, Type>
		{
			readonly Assembly[] assemblies;
			readonly CanBuildSpecification specification;

			public Context( Assembly[] assemblies, CanBuildSpecification specification )
			{
				this.assemblies = assemblies;
				this.specification = specification;
			}

			protected override Type CreateItem( Type parameter )
			{
				var adapter = parameter.Adapt();
				var name = parameter.Name.TrimStartOf( 'I' );
				var result = assemblies.Append( parameter.Assembly() ).Distinct()
					.SelectMany( assembly => assembly.DefinedTypes.AsTypes() )
					.Where( adapter.IsAssignableFrom )
					.Where( specification.IsSatisfiedBy )
					.FirstOrDefault( candidate => candidate.Name.StartsWith( name ) );
				return result;
			}
		}
	}

	public class ImplementedFromConventionTypeLocator : FactoryBase<Type, Type>
	{
		readonly Assembly[] assemblies;

		public ImplementedFromConventionTypeLocator( [Required] Assembly[] assemblies )
		{
			this.assemblies = assemblies;
		}

		[Freeze]
		protected override Type CreateItem( Type parameter )
		{
			var locators = new[] { assemblies, parameter.Append( GetType() ).Distinct().Assemblies() }.Select( candidate => new ImplementedInterfaceFromConventionLocator( candidate ) );

			var result = locators.FirstWhere( locator => locator.Create( parameter ) );
			return result;
		}
	}

	public class ImplementedInterfaceFromConventionLocator : FactoryBase<Type,Type>
	{
		readonly Assembly[] assemblies;

		public ImplementedInterfaceFromConventionLocator( [Required]Assembly[] assemblies )
		{
			this.assemblies = assemblies;
		}

		protected override Type CreateItem( Type parameter )
		{
			var result =
				parameter.GetTypeInfo().ImplementedInterfaces.ToArray().With( interfaces => 
					interfaces.FirstOrDefault( i => parameter.Name.Contains( i.Name.TrimStartOf( 'I' ) ) )
					??
					interfaces.FirstOrDefault( t => assemblies.Contains( t.Assembly() ) )
				) ?? parameter;
			return result;
		}
	}

	public class CanBuildSpecification : SpecificationBase<Type>
	{
		public static CanBuildSpecification Instance { get; } = new CanBuildSpecification();

		[Freeze]
		protected override bool Verify( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = parameter != typeof(object) && !info.IsInterface && !info.IsAbstract && info.DeclaredConstructors.Any( constructorInfo => constructorInfo.IsPublic ) && !typeof(Delegate).Adapt().IsAssignableFrom( parameter ) && ( info.IsPublic || info.Assembly.Has<RegistrationAttribute>() );
			return result;
		}
	}

	public class InvalidBuildFromContextSpecification : SpecificationBase<IBuilderContext>
	{
		public static InvalidBuildFromContextSpecification Instance { get; } = new InvalidBuildFromContextSpecification();

		readonly CanBuildSpecification specification;

		public InvalidBuildFromContextSpecification() : this( CanBuildSpecification.Instance ) {}

		public InvalidBuildFromContextSpecification( [Required]CanBuildSpecification specification )
		{
			this.specification = specification;
		}

		protected override bool Verify( IBuilderContext parameter ) => !specification.IsSatisfiedBy( parameter.BuildKey.Type ) || !CanBuildFrom( parameter );

		static bool CanBuildFrom( IBuilderContext parameter )
		{
			IPolicyList containingPolicyList;
			var constructor = parameter.Policies.Get<IConstructorSelectorPolicy>( parameter.BuildKey, out containingPolicyList ).SelectConstructor( parameter, containingPolicyList );
			var result = constructor.With( IsValidConstructor );
			return result;
		}

		static bool IsValidConstructor( SelectedConstructor selectedConstructor ) => selectedConstructor.Constructor.GetParameters().All( pi => !pi.ParameterType.IsByRef );
	}

	class KeyReference : Reference<NamedTypeBuildKey>
	{
		public KeyReference( object instance, NamedTypeBuildKey key ) : base( instance, key ) { }
	}

	public class ConventionStrategy : BuilderStrategy
	{
		readonly ConventionCandidateLocator locator;
		readonly IServiceRegistry registry;

		// [Persistent]
		public class ConventionCandidateLocator : DecoratedFactory<IBuilderContext, Type>
		{
			public ConventionCandidateLocator( [Required]BuildableTypeFromConventionLocator factory ) : this( InvalidBuildFromContextSpecification.Instance, factory ) { }

			ConventionCandidateLocator( [Required]InvalidBuildFromContextSpecification specification, [Required]BuildableTypeFromConventionLocator factory ) : base( specification, context => factory.Create( context.BuildKey.Type ) ) { }
		}

		public ConventionStrategy( [Required]ConventionCandidateLocator locator, [Required]IServiceRegistry registry )
		{
			this.locator = locator;
			this.registry = registry;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var reference = new KeyReference( this, context.BuildKey ).Item;
			if ( new Checked( reference, this ).Item.Apply() )
			{
				var convention = locator.Create( context );
				convention.With( located =>
				{
					var from = context.BuildKey.Type;
					context.BuildKey = new NamedTypeBuildKey( located, context.BuildKey.Name );
					
					registry.Register( new MappingRegistrationParameter( from, context.BuildKey.Type, context.BuildKey.Name ) );
				} );
			}
		}
	}
}