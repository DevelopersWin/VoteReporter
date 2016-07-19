using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup.Commands;
using System;
using System.Windows.Input;

namespace DragonSpark.Configuration
{
	public class EnableMethodCaching : Configuration<bool>
	{
		public static EnableMethodCaching Instance { get; } = new EnableMethodCaching();
		EnableMethodCaching() : base( () => true ) {}
	}

	public interface IInitializationCommand : ICommand, IDisposable {}

	[ApplyAutoValidation]
	public abstract class InitializationCommandBase : CompositeCommand, IInitializationCommand
	{
		protected InitializationCommandBase( params ICommand[] commands ) : base( new OnlyOnceSpecification(), commands ) {}
	}

	public class ApplyParameterizedFactoryConfigurationCommand<T> : ApplyParameterizedFactoryConfigurationCommand<object, T>
	{
		public ApplyParameterizedFactoryConfigurationCommand() {}

		public ApplyParameterizedFactoryConfigurationCommand( IFactory<object, T> factory ) : base( factory ) {}
	}

	public class ApplyFactoryConfigurationCommand<T> : ApplyConfigurationCommandBase<T>
	{
		public ApplyFactoryConfigurationCommand() {}

		public ApplyFactoryConfigurationCommand( IFactory<T> factory )
		{
			Factory = factory;
		}

		public IFactory<T> Factory { get; set; }

		protected override T Get() => Factory.Create();
	}

	public class ApplyParameterizedFactoryConfigurationCommand<TKey, TValue> : ApplyParameterizedConfigurationCommandBase<TKey, TValue>
	{
		public ApplyParameterizedFactoryConfigurationCommand() {}

		public ApplyParameterizedFactoryConfigurationCommand( IFactory<TKey, TValue> factory )
		{
			Factory = factory;
		}

		public IFactory<TKey, TValue> Factory { get; set; }

		protected override TValue Get( TKey key ) => Factory.Create( key );
	}

	public abstract class ApplyConfigurationCommandBase<T> : DeclarativeCommandBase<IConfiguration<T>>
	{
		readonly Func<T> get;
		
		protected ApplyConfigurationCommandBase()
		{
			get = Get;
		}

		protected abstract T Get();

		public override void Execute( object _ ) => Parameter.Assign( get );
	}

	public abstract class ApplyParameterizedConfigurationCommandBase<TKey, TValue> : DeclarativeCommandBase<IParameterizedConfiguration<TKey, TValue>>
	{
		readonly Func<TKey, TValue> get;
		
		protected ApplyParameterizedConfigurationCommandBase()
		{
			get = Get;
		}

		protected abstract TValue Get( TKey key );

		public override void Execute( object _ ) => Parameter.Assign( get );
	}

	public class ApplyValueConfigurationCommand<T> : ApplyConfigurationCommandBase<T>
	{
		public ApplyValueConfigurationCommand() {}

		public ApplyValueConfigurationCommand( T value )
		{
			Value = value;
		}

		public T Value { get; set; }

		protected override T Get() => Value;
	}

	public class ApplyParameterizedValueConfigurationCommand<TKey, TValue> : ApplyParameterizedConfigurationCommandBase<TKey, TValue>
	{
		public ApplyParameterizedValueConfigurationCommand() {}

		public ApplyParameterizedValueConfigurationCommand( TValue value )
		{
			Value = value;
		}

		public TValue Value { get; set; }

		protected override TValue Get( TKey key ) => Value;
	}

	public class ApplyParameterizedValueConfigurationCommand<T> : ApplyParameterizedValueConfigurationCommand<object, T>
	{
		public ApplyParameterizedValueConfigurationCommand() {}

		public ApplyParameterizedValueConfigurationCommand( T value ) : base( value ) {}
	}

	public interface IParameterizedConfiguration<T> : IParameterizedConfiguration<object, T> {}

	public interface IParameterizedConfiguration<TKey, TValue> : IAssignable<Func<TKey, TValue>>
	{
		TValue Get( TKey key );
	}

	public interface IConfiguration<T> : IAssignable<Func<T>>
	{
		T Get();
	}

	public class Configuration<T> : IConfiguration<T>
	{
		readonly IWritableStore<Func<T>> store;

		public Configuration( Func<T> factory ) : this( new CacheStore<T>( factory ) ) {}

		protected Configuration( IWritableStore<Func<T>> store )
		{
			this.store = store;
		}

		public T Get() => store.Value();

		public void Assign( Func<T> item ) => store.Assign( item );
		void IAssignable.Assign( object item ) => store.Assign( item );
	}

	class CacheStore<T> : ExecutionContextStore<Func<T>>
	{
		public CacheStore( Func<T> factory ) : base( factory ) {}

		protected override void OnAssign( Func<T> value ) => base.OnAssign( new DeferredStore<T>( value ).Get );
	}

	/*public static class ConfigurationExtensions
	{
		public static T Apply<T>( this IConfiguration<T> @this, Func<T> factory )
		{
			@this.Assign( factory );
			return @this.Get();
		}

		/*public static T Default<T>( this IParameterizedConfiguration<object, T> @this ) => @this.Get( Defaults.ExecutionContext() );#1#
	}*/
}
