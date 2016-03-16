using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;

namespace DragonSpark.Setup.Registration
{
	public class FactoryRegistration : IRegistration
	{
		readonly IActivator activator;
		readonly ISingletonLocator locator;
		readonly RegisterFactoryParameter parameter;

		public FactoryRegistration( [Required]IActivator activator, [Required]ISingletonLocator locator, [Required, OfFactoryType]Type factoryType ) : this( activator, locator, new RegisterFactoryParameter( factoryType ) ) {}

		public FactoryRegistration( [Required]IActivator activator, [Required]ISingletonLocator locator, [Required, OfFactoryType]Type factoryType, params Type[] registrationTypes ) : this( activator, locator, new RegisterFactoryParameter( factoryType, registrationTypes ) ) {}

		FactoryRegistration( [Required]IActivator activator, [Required]ISingletonLocator locator, [Required]RegisterFactoryParameter parameter )
		{
			this.activator = activator;
			this.locator = locator;
			this.parameter = parameter;
		}

		public void Register( IServiceRegistry registry )
		{
			new ICommand<RegisterFactoryParameter>[] { new RegisterFactoryWithParameterCommand( activator, registry, locator ), new RegisterFactoryCommand( activator, registry, locator ) }
				.FirstOrDefault( command => command.CanExecute( parameter ) )
				.With( command => command.Run( parameter ) );
		}
	}

	public static class ServiceRegistryExtensions
	{
		public static void Register<TFrom, TTo>( this IServiceRegistry @this, string name = null ) where TTo : TFrom => @this.Register( new MappingRegistrationParameter( typeof(TFrom), typeof(TTo), name ) );

		public static void Register<TService>( this IServiceRegistry @this, TService instance, string name = null ) => @this.Register( new InstanceRegistrationParameter( typeof(TService), instance, name ) );

		public static void Register<TService>( this IServiceRegistry @this, Func<TService> factory, string name = null ) => @this.RegisterFactory( new FactoryRegistrationParameter( typeof(TService), () => factory(), name ) );
	}

	public class FactoryDelegateFactory : FactoryBase<Type, Func<object>>
	{
		readonly Func<Type, IFactory> createFactory;

		// public static FactoryDelegateFactory Instance { get; } = new FactoryDelegateFactory();

		public FactoryDelegateFactory( [Required]IActivator activator ) : this( new ActivateFactory<IFactory>( activator ).CreateUsing ) {}

		public FactoryDelegateFactory( [Required]Func<Type, IFactory> createFactory )
		{
			this.createFactory = createFactory;
		}

		protected override Func<object> CreateItem( Type parameter ) => createFactory( parameter ).With( f => new Func<object>( f.Create ) );
	}

	public class FactoryWithParameterDelegateFactory : FactoryBase<Type, Func<object, object>>
	{
		// public static FactoryWithParameterDelegateFactory Instance { get; } = new FactoryWithParameterDelegateFactory();

		readonly Func<Type, IFactoryWithParameter> createFactory;

		public FactoryWithParameterDelegateFactory( IActivator activator ) : this( new ActivateFactory<IFactoryWithParameter>( activator ).CreateUsing ) {}

		public FactoryWithParameterDelegateFactory( [Required]Func<Type, IFactoryWithParameter> createFactory )
		{
			this.createFactory = createFactory;
		}

		protected override Func<object, object> CreateItem( Type parameter ) => createFactory( parameter ).With( f => new Func<object, object>( f.Create ) );
	}

	public class FactoryWithActivatedParameterDelegateFactory : FactoryBase<Type, Func<object>>
	{
		// public static FactoryWithActivatedParameterDelegateFactory Instance { get; } = new FactoryWithActivatedParameterDelegateFactory();

		readonly Func<Type, Func<object, object>> factory;
		readonly Func<Type, object> createParameter;

		public FactoryWithActivatedParameterDelegateFactory( [Required]IActivator activator ) : this( new FactoryWithParameterDelegateFactory( activator ).Create, new ActivateFactory<object>( activator ).CreateUsing ) {}

		public FactoryWithActivatedParameterDelegateFactory( [Required]Func<Type, Func<object, object>> factory, [Required]Func<Type, object> createParameter )
		{
			this.factory = factory;
			this.createParameter = createParameter;
		}

		protected override Func<object> CreateItem( Type parameter ) => factory( parameter ).With( func => new Func<object>( () =>
		{
			var o = createParameter( Factory.GetParameterType( parameter ) );
			return func( o );
		} ) );
	}

	public class FuncFactory<T, U> : FactoryBase<Func<object, object>, Func<T, U>>
	{
		public static FuncFactory<T, U> Instance { get; } = new FuncFactory<T, U>();

		FuncFactory() {}

		protected override Func<T, U> CreateItem( Func<object, object> parameter ) => t => (U)parameter( t );
	}

	public class FuncFactory<T> : FactoryBase<Func<object>, Func<T>>
	{
		public static FuncFactory<T> Instance { get; } = new FuncFactory<T>();

		FuncFactory() {}

		protected override Func<T> CreateItem( Func<object> parameter ) => () => (T)parameter();
	}

	public class RegisterFactoryParameter
	{
		public RegisterFactoryParameter( [Required, OfFactoryType]Type factoryType ) : this( factoryType, Factory.GetResultType( factoryType ) ) { }

		public RegisterFactoryParameter( [Required, OfFactoryType]Type factoryType, params Type[] registrationTypes )
		{
			FactoryType = factoryType;
			RegisterTypes = registrationTypes.NotNull().Distinct().ToArray();
		}
		
		public Type FactoryType { get; }

		public Type[] RegisterTypes { get; }
	}

	public abstract class RegisterFactoryCommandBase<TFactory> : Command<RegisterFactoryParameter>
	{
		readonly IServiceRegistry registry;
		readonly ISingletonLocator locator;
		readonly Func<Type, Func<object>> create;

		protected RegisterFactoryCommandBase( [Required]IServiceRegistry registry, [Required]ISingletonLocator locator, [Required]Func<Type, Func<object>> create )
		{
			this.registry = registry;
			this.locator = locator;
			this.create = create;
		}

		public override bool CanExecute( RegisterFactoryParameter parameter ) => base.CanExecute( parameter ) && typeof(TFactory).Adapt().IsAssignableFrom( parameter.FactoryType );

		protected override void OnExecute( RegisterFactoryParameter parameter )
		{
			new[] { Factory.GetInterface( parameter.FactoryType ), parameter.FactoryType }.Each( type => registry.Register( new MappingRegistrationParameter( type, parameter.FactoryType ) ) );

			var func = create( parameter.FactoryType );
			parameter.RegisterTypes.Each( type =>
			{
				registry.RegisterFactory( new FactoryRegistrationParameter( type, func ) );
				locator.Locate( MakeGenericType( parameter.FactoryType, type ) ).AsValid<IFactoryWithParameter>( factory =>
				{
					var typed = Create( factory, parameter.FactoryType, func );
					registry.Register( new InstanceRegistrationParameter( typed.GetType(), typed ) );
				} );
			} );
		}

		protected virtual object Create( IFactoryWithParameter factory, Type type, Func<object> func ) => factory.Create( func );

		protected abstract Type MakeGenericType( Type parameter, Type itemType );
	}

	public class RegisterFactoryCommand : RegisterFactoryCommandBase<IFactory>
	{
		public RegisterFactoryCommand( IActivator activator, IServiceRegistry registry, ISingletonLocator locator ) : base( registry, locator, new FactoryDelegateFactory( activator ).Create ) {}

		protected override Type MakeGenericType( Type parameter, Type itemType ) => typeof(FuncFactory<>).MakeGenericType( itemType );
	}

	public class RegisterFactoryWithParameterCommand : RegisterFactoryCommandBase<IFactoryWithParameter>
	{
		readonly FactoryWithParameterDelegateFactory delegateFactory;

		public RegisterFactoryWithParameterCommand( IActivator activator, IServiceRegistry registry, ISingletonLocator locator ) : this( activator, registry, locator, new FactoryWithParameterDelegateFactory( activator ) ) {}

		public RegisterFactoryWithParameterCommand( IActivator activator, IServiceRegistry registry, ISingletonLocator locator, [Required]FactoryWithParameterDelegateFactory delegateFactory ) : base( registry, locator, new FactoryWithActivatedParameterDelegateFactory( activator ).Create )
		{
			this.delegateFactory = delegateFactory;
		}

		protected override Type MakeGenericType( Type parameter, Type itemType ) => typeof(FuncFactory<,>).MakeGenericType( Factory.GetParameterType( parameter ), itemType );

		protected override object Create( IFactoryWithParameter factory, Type type, Func<object> func ) => delegateFactory.Create( type ); // TODO: Fix this.
	}
}