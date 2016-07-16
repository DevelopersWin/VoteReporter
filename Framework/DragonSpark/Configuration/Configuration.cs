using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup.Commands;
using System;
using System.Windows.Input;
using System.Windows.Markup;
using Defaults = DragonSpark.Activation.Defaults;

namespace DragonSpark.Configuration
{
	/*	public class EnableMethodCaching : StructuredParameterizedConfiguration<bool>
		{
			public static EnableMethodCaching Instance { get; } = new EnableMethodCaching();
			EnableMethodCaching() : base( EnableMethodCachingConfiguration.Instance.Get ) {}
		}

		public class EnableMethodCachingConfiguration : DeclarativeParameterizedConfigurationBase<bool>
		{
			public static EnableMethodCachingConfiguration Instance { get; } = new EnableMethodCachingConfiguration();

			public EnableMethodCachingConfiguration() : base( !PostSharpEnvironment.IsPostSharpRunning ) {}
		}*/

	public interface IInitializationCommand : ICommand, IDisposable {}

	[ApplyAutoValidation]
	public abstract class InitializationCommandBase : CompositeCommand, IInitializationCommand
	{
		protected InitializationCommandBase( params ICommand[] commands ) : base( new OnlyOnceSpecification(), commands ) {}
	}

	/*public class ConfigurationValues : Dictionary<IWritableStore, IStore> {}

	public class ApplyConfiguration : ServicedCommand<ApplyConfigurationCommand, ConfigurationValues>, IInitializationCommand
	{
		public void Dispose() => Command.TryDispose();
	}

	public class ApplyConfigurationCommand : CommandBase<ConfigurationValues>
	{
		public override void Execute( ConfigurationValues parameter )
		{
			foreach ( var key in parameter.Keys )
			{
				key.Assign( parameter[key].Value );
			}
		}
	}*/

	/*public class ApplyConfiguration : DeclarativeCommandBase<>*/

	public class AssignValueCommand : CommandBase<IStore>
	{
		readonly IWritableStore writable;
		public AssignValueCommand( IWritableStore writable )
		{
			this.writable = writable;
		}

		public override void Execute( IStore parameter ) => writable.Assign( parameter.Value );
	}

	public abstract class ApplyFactoryConfigurationCommandBase<T> : ApplyFactoryConfigurationCommandBase<object, T>
	{
		protected ApplyFactoryConfigurationCommandBase( IFactory<object, T> factory ) : base( factory ) {}
	}

	[ContentProperty( nameof(Value) )]
	public abstract class ApplyFactoryConfigurationCommandBase<TKey, TValue> : ApplyConfigurationCommandBase<TKey, TValue>
	{
		protected ApplyFactoryConfigurationCommandBase( IFactory<TKey, TValue> factory )
		{
			Value = factory;
		}

		public IFactory<TKey, TValue> Value { get; set; }

		protected override TValue Get( TKey key ) => Value.Create( key );
	}

	public abstract class ApplyConfigurationCommandBase<TKey, TValue> : DeclarativeCommandBase<IWritableParameterizedConfiguration<TKey, TValue>>
	{
		readonly Func<TKey, TValue> get;

		protected ApplyConfigurationCommandBase()
		{
			get = Get;
		}

		protected abstract TValue Get( TKey key );

		public override void Execute( object parameter ) => Parameter.Assign( get );
	}

	[ContentProperty( nameof(Value) )]
	public abstract class ApplyValueConfigurationCommandBase<TKey, TValue> : ApplyConfigurationCommandBase<TKey, TValue>
	{
		protected ApplyValueConfigurationCommandBase( TValue defaultValue )
		{
			Value = defaultValue;
		}

		public TValue Value { get; set; }

		protected override TValue Get( TKey key ) => Value;
	}

	public abstract class DeclarativeParameterizedConfigurationBase<T> : ApplyValueConfigurationCommandBase<object, T>
	{
		protected DeclarativeParameterizedConfigurationBase( T defaultValue ) : base( defaultValue ) {}
	}

	public interface IParameterizedConfiguration<out T> : IParameterizedConfiguration<object, T> {}

	public interface IParameterizedConfiguration<in TKey, out TValue>
	{
		TValue Get( TKey key );
	}

	public interface IWritableParameterizedConfiguration<T> : IWritableParameterizedConfiguration<object, T> {}

	public interface IWritableParameterizedConfiguration<TKey, TValue> : IParameterizedConfiguration<TKey, TValue>
	{
		void Assign( Func<TKey, TValue> factory );
	}

	public interface IConfiguration<out T>
	{
		T Get();
	}

	public interface IWritableConfiguration<T> : IConfiguration<T>
	{
		void Assign( Func<T> factory );
	}

	public abstract class ConfigurationBase<T> : IWritableConfiguration<T>
	{
		readonly IWritableStore<Func<T>> store;

		protected ConfigurationBase( Func<T> factory ) : this( new FixedStore<Func<T>>( factory ) ) {}

		protected ConfigurationBase( IWritableStore<Func<T>> store )
		{
			this.store = store;
		}

		public void Assign( Func<T> item ) => store.Assign( item );

		public T Get() => store.Value();
	}

	public static class ConfigurationExtensions
	{
		public static T Default<T>( this IParameterizedConfiguration<object, T> @this ) => @this.Get( Defaults.ExecutionContext() );
	}

	/*public abstract class ParameterizedConfigurationBase<T> : ParameterizedConfigurationBase<object, T>, IParameterizedConfiguration<T>
	{
		protected ParameterizedConfigurationBase( Func<object, T> factory ) : base( factory ) {}
	}

	public abstract class ParameterizedConfigurationBase<TKey, TValue> : IWritableParameterizedConfiguration<TKey, TValue>
	{
		readonly IWritableStore<Func<TKey, TValue>> store = new FixedStore<Func<TKey, TValue>>();

		protected ParameterizedConfigurationBase( Func<TKey, TValue> factory )
		{
			Assign( factory );
		}

		public TValue Get( TKey key ) => store.Value( key );
		public void Assign( Func<TKey, TValue> factory ) => store.Assign( factory );
	}*/
}
