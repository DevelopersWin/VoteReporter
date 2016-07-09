using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using Delegate = System.Delegate;

namespace DragonSpark.Setup.Registration
{
	public class FactoryRegistration : IRegistration
	{
		readonly RegisterFactoryParameter parameter;

		public FactoryRegistration( [Required, OfFactoryType]Type factoryType, params Type[] registrationTypes ) : this( new RegisterFactoryParameter( factoryType, registrationTypes ) ) {}

		FactoryRegistration( [Required]RegisterFactoryParameter parameter )
		{
			this.parameter = parameter;
		}

		public void Register( IServiceRegistry registry ) => new FactoryRegistrationCommand( registry ).Execute( parameter );
	}

	class FactoryRegistrationCommand : FirstCommand<RegisterFactoryParameter>
	{
		public FactoryRegistrationCommand( IServiceRegistry registry ) : base( new RegisterFactoryWithParameterCommand( registry ), new RegisterFactoryCommand( registry ) ) {}
	}

	public static class ServiceRegistryExtensions
	{
		public static void Register<TFrom, TTo>( this IServiceRegistry @this, string name = null ) where TTo : TFrom => @this.Register( new MappingRegistrationParameter( typeof(TFrom), typeof(TTo), name ) );

		public static void Register<T>( this IServiceRegistry @this, T instance, string name = null ) => @this.Register( new InstanceRegistrationParameter( typeof(T), instance, name ) );

		public static void Register<T>( this IServiceRegistry @this, Func<T> factory, string name = null ) => @this.RegisterFactory( new FactoryRegistrationParameter( typeof(T), factory.Convert(), name ) );
	}

	public class FactoryDelegateFactory : FactoryBase<Type, Func<object>>
	{
		readonly Func<Type, IFactory> createFactory;

		public static FactoryDelegateFactory Instance { get; } = new FactoryDelegateFactory();

		FactoryDelegateFactory() : this( GlobalServiceProvider.Instance.Get<IFactory> ) {}

		public FactoryDelegateFactory( [Required]Func<Type, IFactory> createFactory )
		{
			this.createFactory = createFactory;
		}

		public override Func<object> Create( Type parameter ) => createFactory( parameter ).ToDelegate();
	}

	public class FactoryWithParameterDelegateFactory : FactoryBase<Type, Func<object, object>>
	{
		public static FactoryWithParameterDelegateFactory Instance { get; } = new FactoryWithParameterDelegateFactory();

		readonly Func<Type, IFactoryWithParameter> createFactory;

		FactoryWithParameterDelegateFactory() : this( GlobalServiceProvider.Instance.Get<IFactoryWithParameter> ) {}

		public FactoryWithParameterDelegateFactory( [Required]Func<Type, IFactoryWithParameter> createFactory )
		{
			this.createFactory = createFactory;
		}

		public override Func<object, object> Create( Type parameter ) => createFactory( parameter ).ToDelegate();
	}

	public class FactoryWithActivatedParameterDelegateFactory : FactoryBase<Type, Func<object>>
	{
		public static FactoryWithActivatedParameterDelegateFactory Instance { get; } = new FactoryWithActivatedParameterDelegateFactory();

		readonly Func<Type, Func<object, object>> factory;
		readonly Func<Type, object> createParameter;
		readonly Func<Type, Type> parameterLocator;

		FactoryWithActivatedParameterDelegateFactory() : this( FactoryWithParameterDelegateFactory.Instance.ToDelegate(), GlobalServiceProvider.Instance.Get<object> ) {}

		public FactoryWithActivatedParameterDelegateFactory( Func<Type, Func<object, object>> factory, Func<Type, object> createParameter ) : this( factory, createParameter, ParameterTypeLocator.Instance.ToDelegate() ) {}

		public FactoryWithActivatedParameterDelegateFactory( Func<Type, Func<object, object>> factory, Func<Type, object> createParameter, Func<Type, Type> parameterLocator )
		{
			this.factory = factory;
			this.createParameter = createParameter;
			this.parameterLocator = parameterLocator;
		}

		public override Func<object> Create( Type parameter )
		{
			var @delegate = factory( parameter );
			if ( @delegate != null )
			{
				var createdParameter = createParameter( parameterLocator( parameter ) );
				var result = new FixedFactory<object, object>( @delegate, createdParameter ).ToDelegate();
				return result;
			}
			return null;
		}
	}

	public class FuncFactory<T, U> : FactoryBase<Func<object, object>, Func<T, U>>
	{
		public static FuncFactory<T, U> Instance { get; } = new FuncFactory<T, U>();
		FuncFactory() {}

		public override Func<T, U> Create( Func<object, object> parameter ) => parameter.Convert<T, U>();
	}

	public class FuncFactory<T> : FactoryBase<Func<object>, Func<T>>
	{
		public static FuncFactory<T> Instance { get; } = new FuncFactory<T>();
		FuncFactory() {}

		public override Func<T> Create( Func<object> parameter ) => parameter.Convert<T>();
	}

	public struct RegisterFactoryParameter
	{
		public RegisterFactoryParameter( [Required, OfFactoryType]Type factoryType, params Type[] registrationTypes )
		{
			FactoryType = factoryType;
			RegisterTypes = registrationTypes.WhereAssigned().Append( ResultTypeLocator.Instance.Get( factoryType ) ).Distinct().ToArray();
		}
		
		public Type FactoryType { get; }

		public Type[] RegisterTypes { get; }
	}

	public abstract class RegisterFactoryCommandBase<TFactory> : CommandBase<RegisterFactoryParameter>
	{
		readonly IServiceRegistry registry;
		readonly ISingletonLocator locator;
		readonly Func<Type, Func<object>> create;
		readonly Func<Type, Delegate> determineDelegate;

		protected RegisterFactoryCommandBase( [Required]IServiceRegistry registry, [Required]ISingletonLocator locator, [Required]Func<Type, Func<object>> create ) : this( registry, locator, create, type => null ) {}

		protected RegisterFactoryCommandBase( [Required]IServiceRegistry registry, [Required]ISingletonLocator locator, [Required]Func<Type, Func<object>> create, [Required]Func<Type, Delegate> determineDelegate ) : base( Specification.Instance )
		{
			this.registry = registry;
			this.locator = locator;
			this.create = create;
			this.determineDelegate = determineDelegate;
		}

		public override bool CanExecute( RegisterFactoryParameter parameter ) => base.CanExecute( parameter ) && typeof(TFactory).Adapt().IsAssignableFrom( parameter.FactoryType );

		public override void Execute( RegisterFactoryParameter parameter )
		{
			var created = create( parameter.FactoryType );
			foreach ( var type in parameter.RegisterTypes )
			{
				registry.RegisterFactory( new FactoryRegistrationParameter( type, created ) );
				var factory = locator.Locate( MakeGenericType( parameter.FactoryType, type ) ).AsValid<IFactoryWithParameter>();
				var @delegate = determineDelegate( parameter.FactoryType ) ?? created;
				var typed = factory.Create( @delegate );
				registry.Register( new InstanceRegistrationParameter( typed.GetType(), typed ) );
			}
			
			new[] { ImplementedInterfaceFromConventionLocator.Instance.Get( parameter.FactoryType ), FactoryInterfaceLocator.Instance.Get( parameter.FactoryType ) }
				.WhereAssigned()
				.Distinct()
				.Introduce( parameter.FactoryType, tuple => new MappingRegistrationParameter( tuple.Item1, tuple.Item2 ) )
				.Each( registry.Register );
		}

		protected abstract Type MakeGenericType( Type parameter, Type itemType );

		class Specification : DelegatedSpecification<RegisterFactoryParameter>
		{
			public static Specification Instance { get; } = new Specification();

			Specification() : base( parameter => typeof(TFactory).Adapt().IsAssignableFrom( parameter.FactoryType ) ) {}
		}
	}

	public class RegisterFactoryCommand : RegisterFactoryCommandBase<IFactory>
	{
		public RegisterFactoryCommand( IServiceRegistry registry ) : base( registry, SingletonLocator.Instance, FactoryDelegateFactory.Instance.ToDelegate() ) {}

		protected override Type MakeGenericType( Type parameter, Type itemType ) => typeof(FuncFactory<>).MakeGenericType( itemType.ToItem() );
	}

	public class RegisterFactoryWithParameterCommand : RegisterFactoryCommandBase<IFactoryWithParameter>
	{
		readonly static Func<Type, Type> ParameterLocator = ParameterTypeLocator.Instance.ToDelegate();

		readonly Func<Type, Type> parameterLocator;
		public RegisterFactoryWithParameterCommand( IServiceRegistry registry ) : this( registry, SingletonLocator.Instance, FactoryWithParameterDelegateFactory.Instance, ParameterLocator ) {}

		RegisterFactoryWithParameterCommand( IServiceRegistry registry, ISingletonLocator locator, FactoryWithParameterDelegateFactory delegateFactory, Func<Type, Type> parameterLocator ) : base( registry, locator, FactoryWithActivatedParameterDelegateFactory.Instance.Create, delegateFactory.Create )
		{
			this.parameterLocator = parameterLocator;
		}

		protected override Type MakeGenericType( Type parameter, Type itemType ) => typeof(FuncFactory<,>).MakeGenericType( parameterLocator( parameter ), itemType );
	}
}