using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using DragonSpark.Testing.Framework.Setup.Location;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Testing.Framework
{
	[AttributeUsage( AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true )]
	public abstract class RegistrationBaseAttribute : HostingAttribute
	{
		protected RegistrationBaseAttribute( Func<object, ICustomization> factory ) : base( x => x.AsTo( factory ) ) {}
	}

	public class RegistrationCustomization : ICustomization
	{
		readonly IRegistration registration;

		public RegistrationCustomization( [Required]IRegistration registration )
		{
			this.registration = registration;
		}

		public void Customize( IFixture fixture ) => registration.Register( AssociatedRegistry.Default.Get( fixture ) );
	}

	public class AssociatedRegistry : Cache<IFixture, IServiceRegistry>
	{
		public static AssociatedRegistry Default { get; } = new AssociatedRegistry();

		AssociatedRegistry() : base( instance => new FixtureRegistry( instance ) ) {}
	}

	/*public class RegisterFactoryAttribute : RegistrationBaseAttribute
	{
		public RegisterFactoryAttribute( [Required, OfSourceType]Type factoryType ) : base( t => new RegistrationCustomization( new FactoryRegistration( factoryType ) ) ) {}
	}

	public class FactoryRegistration : IRegistration
	{
		readonly static ICache<IServiceRegistry, ICommand<RegisterFactoryParameter>> Cache = new Cache<IServiceRegistry, ICommand<RegisterFactoryParameter>>( registry => new FactoryRegistrationCommand( registry ) );

		readonly RegisterFactoryParameter parameter;

		public FactoryRegistration( [OfSourceType]Type factoryType, params Type[] registrationTypes ) : this( new RegisterFactoryParameter( factoryType, registrationTypes ) ) {}

		FactoryRegistration( RegisterFactoryParameter parameter )
		{
			this.parameter = parameter;
		}

		public void Register( IServiceRegistry registry ) => Cache.Get( registry ).Execute( parameter );
	}

	class FactoryRegistrationCommand : FirstCommand<RegisterFactoryParameter>
	{
		public FactoryRegistrationCommand( IServiceRegistry registry ) : base( new RegisterParameterizedSourceCommand( registry ), new RegisterFactoryCommand( registry ) ) {}
	}

	public struct RegisterFactoryParameter
	{
		public RegisterFactoryParameter( [Required, OfSourceType]Type factoryType, params Type[] registrationTypes ) : this( factoryType, registrationTypes.WhereAssigned().Append( ResultTypes.Instance.Get( factoryType ) ).Distinct().ToImmutableArray() ) {}

		public RegisterFactoryParameter( [Required, OfSourceType]Type factoryType, ImmutableArray<Type> registerTypes )
		{
			SourceType = factoryType;
			RegisterTypes = registerTypes;
		}
		
		public Type SourceType { get; }

		public ImmutableArray<Type> RegisterTypes { get; }
	}

	public abstract class RegisterFactoryCommandBase<TFactory> : CommandBase<RegisterFactoryParameter>
	{
		readonly static ISpecification<RegisterFactoryParameter> Specification = new DelegatedSpecification<RegisterFactoryParameter>( parameter => typeof(TFactory).Adapt().IsAssignableFrom( parameter.SourceType ) );

		readonly IServiceRegistry registry;
		readonly ISingletonLocator locator;
		readonly Func<Type, Func<object>> create;
		readonly Func<Type, Delegate> determineDelegate;

		protected RegisterFactoryCommandBase( IServiceRegistry registry, ISingletonLocator locator, Func<Type, Func<object>> create ) : this( registry, locator, create, type => null ) {}

		protected RegisterFactoryCommandBase( IServiceRegistry registry, ISingletonLocator locator, Func<Type, Func<object>> create, Func<Type, Delegate> determineDelegate ) : base( Specification )
		{
			this.registry = registry;
			this.locator = locator;
			this.create = create;
			this.determineDelegate = determineDelegate;
		}

		public override bool CanExecute( RegisterFactoryParameter parameter ) => base.CanExecute( parameter ) && typeof(TFactory).Adapt().IsAssignableFrom( parameter.SourceType );

		public override void Execute( RegisterFactoryParameter parameter )
		{
			var created = create( parameter.SourceType );
			foreach ( var type in parameter.RegisterTypes )
			{
				registry.RegisterFactory( new FactoryRegistrationParameter( type, created ) );
				var factory = locator.Get( MakeGenericType( parameter.SourceType, type ) ).AsValid<IValidatedParameterizedSource>();
				var @delegate = determineDelegate( parameter.SourceType ) ?? created;
				var typed = factory.Create( @delegate );
				registry.Register( new InstanceRegistrationParameter( typed.GetType(), typed ) );
			}
			
			new[] { ConventionImplementedInterfaces.Instance.Get( parameter.SourceType ), SourceInterfaces.Instance.Get( parameter.SourceType ) }
				.WhereAssigned()
				.Distinct()
				.Introduce( parameter.SourceType, tuple => new MappingRegistrationParameter( tuple.Item1, tuple.Item2 ) )
				.Each( registry.Register );
		}

		protected abstract Type MakeGenericType( Type parameter, Type itemType );
	}

	public class RegisterFactoryCommand : RegisterFactoryCommandBase<ISource>
	{
		public RegisterFactoryCommand( IServiceRegistry registry ) : base( registry, SingletonLocator.Instance, SourceDelegates.Instance.ToDelegate() ) {}

		protected override Type MakeGenericType( Type parameter, Type itemType ) => typeof(SourceDelegates<>).MakeGenericType( itemType.ToItem() );
	}

	public class RegisterParameterizedSourceCommand : RegisterFactoryCommandBase<IParameterizedSource>
	{
		readonly static Func<Type, Type> ParameterLocator = ParameterTypes.Instance.ToDelegate();

		readonly Func<Type, Type> parameterLocator;
		public RegisterParameterizedSourceCommand( IServiceRegistry registry ) : this( registry, SingletonLocator.Instance, ParameterizedSourceDelegates.Instance, ParameterLocator ) {}

		RegisterParameterizedSourceCommand( IServiceRegistry registry, ISingletonLocator locator, ParameterizedSourceDelegates delegates, Func<Type, Type> parameterLocator ) 
			: base( registry, locator, ServiceProvidedParameterizedSourceDelegates.Instance.Get, delegates.Get )
		{
			this.parameterLocator = parameterLocator;
		}

		protected override Type MakeGenericType( Type parameter, Type itemType ) => typeof(SourceDelegates<,>).MakeGenericType( parameterLocator( parameter ), itemType );
	}*/

	/*public class RegisterServiceAttribute : RegistrationBaseAttribute
	{
		public RegisterServiceAttribute( [Required] Type serviceType ) : base( t => new RegistrationCustomization( new ServiceRegistration( serviceType ) ) ) {}
	}*/

	public class ServiceRegistration : IRegistration, ICustomization
	{
		readonly Type serviceType;

		public ServiceRegistration( [Required] Type serviceType )
		{
			this.serviceType = serviceType;
		}

		public void Register( IServiceRegistry registry )
		{
			var instance = GlobalServiceProvider.GetService<object>( serviceType );
			if ( instance.IsAssigned() )
			{
				var parameter = new InstanceRegistrationParameter( serviceType, instance );
				registry.Register( parameter );
			}
		}

		public void Customize( IFixture fixture ) => Register( AssociatedRegistry.Default.Get( fixture ) );
	}
}