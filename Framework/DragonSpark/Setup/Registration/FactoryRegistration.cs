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
		readonly RegisterFactoryParameter parameter;

		public FactoryRegistration( [Required, OfFactoryType]Type factoryType ) : this( new RegisterFactoryParameter( factoryType ) ) {}

		public FactoryRegistration( [Required, OfFactoryType]Type factoryType, params Type[] registrationTypes ) : this( new RegisterFactoryParameter( factoryType, registrationTypes ) ) {}

		FactoryRegistration( [Required]RegisterFactoryParameter parameter )
		{
			this.parameter = parameter;
		}

		public void Register( IServiceRegistry registry )
		{
			new ICommand<RegisterFactoryParameter>[] { new RegisterFactoryWithParameterCommand( registry ), new RegisterFactoryCommand( registry ) }
				.FirstOrDefault( command => command.CanExecute( parameter ) )
				.With( command => command.Run( parameter ) );
		}
	}

	public static class ServiceRegistryExtensions
	{
		public static void Register<TFrom, TTo>( this IServiceRegistry @this, string name = null ) where TTo : TFrom => @this.Register( new MappingRegistrationParameter( typeof(TFrom), typeof(TTo), name ) );

		public static void Register<TService>( this IServiceRegistry @this, TService instance, string name = null ) => @this.Register( new InstanceRegistrationParameter( typeof(TService), instance, name ) );

		public static void Register<TService>( this IServiceRegistry @this, Func<TService> factory, string name = null ) => @this.RegisterFactory( new FactoryRegistrationParameter( typeof(TService), () => factory(), name ) );

		/*public static IServiceRegistry RegisterFactory( [Required] this IServiceRegistry @this, [Required]IFactory factory )
		{
			new RegisterFactoryCommand( @this, new FactoryDelegateFactory( factory ).Create )
				.ExecuteWith( new RegisterFactoryParameter( factory.GetType() ) );
			return @this;
		}

		public static IServiceRegistry RegisterFactory( [Required] this IServiceRegistry @this, [Required]IFactoryWithParameter factory )
		{
			new RegisterFactoryWithParameterCommand( @this, new FactoryWithActivatedParameterDelegateFactory( type => factory.Create ).Create )
				.ExecuteWith( factory.GetType() );
			return @this;
		}*/
	}

	public class FactoryDelegateFactory : FactoryBase<Type, Func<object>>
	{
		readonly Func<Type, IFactory> createFactory;

		public static FactoryDelegateFactory Instance { get; } = new FactoryDelegateFactory();

		FactoryDelegateFactory() : this( ActivateFactory<IFactory>.Instance.CreateUsing ) {}

		public FactoryDelegateFactory( [Required]Func<Type, IFactory> createFactory )
		{
			this.createFactory = createFactory;
		}

		protected override Func<object> CreateItem( Type parameter ) => createFactory( parameter ).With( f => new Func<object>( f.Create ) );
	}

	public class FactoryWithParameterDelegateFactory : FactoryBase<Type, Func<object, object>>
	{
		public static FactoryWithParameterDelegateFactory Instance { get; } = new FactoryWithParameterDelegateFactory();

		readonly Func<Type, IFactoryWithParameter> createFactory;

		FactoryWithParameterDelegateFactory() : this( ActivateFactory<IFactoryWithParameter>.Instance.CreateUsing ) {}

		public FactoryWithParameterDelegateFactory( [Required]Func<Type, IFactoryWithParameter> createFactory )
		{
			this.createFactory = createFactory;
		}

		protected override Func<object, object> CreateItem( Type parameter ) => createFactory( parameter ).With( f => new Func<object, object>( f.Create ) );
	}

	public class FactoryWithActivatedParameterDelegateFactory : FactoryBase<Type, Func<object>>
	{
		public static FactoryWithActivatedParameterDelegateFactory Instance { get; } = new FactoryWithActivatedParameterDelegateFactory();

		readonly Func<Type, Func<object, object>> factory;
		readonly Func<Type, object> createParameter;

		FactoryWithActivatedParameterDelegateFactory() : this( FactoryWithParameterDelegateFactory.Instance.Create, ActivateFactory<object>.Instance.CreateUsing ) {}

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

		public FuncFactory() : base( new FactoryParameterCoercer<Func<object, object>>() ) { }

		protected override Func<T, U> CreateItem( Func<object, object> parameter ) => t => (U)parameter( t );
	}

	public class FuncFactory<T> : FactoryBase<Func<object>, Func<T>>
	{
		public static FuncFactory<T> Instance { get; } = new FuncFactory<T>();

		public FuncFactory() : base( new FactoryParameterCoercer<Func<object>>() ) {}

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

		protected RegisterFactoryCommandBase( [Required]IServiceRegistry registry, [Required]Func<Type, Func<object>> create ) : this( registry, new SingletonLocator(), create ) {}

		RegisterFactoryCommandBase( [Required]IServiceRegistry registry, [Required]ISingletonLocator locator, [Required]Func<Type, Func<object>> create )
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
		public RegisterFactoryCommand( IServiceRegistry registry ) : base( registry, FactoryDelegateFactory.Instance.Create ) {}

		protected override Type MakeGenericType( Type parameter, Type itemType ) => typeof(FuncFactory<>).MakeGenericType( itemType );
	}

	public class RegisterFactoryWithParameterCommand : RegisterFactoryCommandBase<IFactoryWithParameter>
	{
		public RegisterFactoryWithParameterCommand( IServiceRegistry registry ) : base( registry, FactoryWithActivatedParameterDelegateFactory.Instance.Create ) {}

		protected override Type MakeGenericType( Type parameter, Type itemType ) => typeof(FuncFactory<,>).MakeGenericType( Factory.GetParameterType( parameter ), itemType );

		protected override object Create( IFactoryWithParameter factory, Type type, Func<object> func ) => FactoryWithParameterDelegateFactory.Instance.Create( type ); // TODO: Fix this.
	}
}