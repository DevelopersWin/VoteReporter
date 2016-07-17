using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup.Commands;
using System;
using System.Windows.Input;
using DragonSpark.Extensions;
using Defaults = DragonSpark.Activation.Defaults;

namespace DragonSpark.Configuration
{
	public class EnableMethodCaching : StructuredParameterizedConfiguration<bool>
	{
		public static EnableMethodCaching Instance { get; } = new EnableMethodCaching();
		EnableMethodCaching() : base( o => true ) {}
	}

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

	/*public class AssignValueCommand : CommandBase<IStore>
	{
		readonly IWritableStore writable;
		public AssignValueCommand( IWritableStore writable )
		{
			this.writable = writable;
		}

		public override void Execute( IStore parameter ) => writable.Assign( parameter.Value );
	}*/

	/*public class ApplyConfigurationCommand : DeclarativeCommandBase<object>
	{
		public static GenericInvocationFactory<object, ICommand> Factory { get; } = new GenericInvocationFactory<object, ICommand>( typeof(IWritableParameterizedConfiguration<,>), typeof(ApplyConfigurationCommand), nameof(Create) );

		public object Value { get; set; }

		public override void Execute( object parameter ) => Factory.Create( Value ).Execute( parameter );

		static ICommand Create<TKey, TValue>( object value ) => new ApplyConfigurationCommand<TKey, TValue>( value );
	}

	public class ApplyConfigurationCommand<TKey, TValue> : CommandBase<IWritableParameterizedConfiguration<TKey, TValue>>
	{
		readonly static ImmutableArray<TypeAdapter> DefaultTypes = new[] { typeof(ApplyFactoryConfigurationCommand<TKey, TValue>), typeof(ApplyValueConfigurationCommand<TKey, TValue>) }.Select( type => type.Adapt() ).ToImmutableArray();

		readonly object value;
		readonly ImmutableArray<TypeAdapter> commandTypes;

		public ApplyConfigurationCommand( object value ) : this( value, DefaultTypes ) {}

		public ApplyConfigurationCommand( object value, ImmutableArray<TypeAdapter> commandTypes )
		{
			this.value = value;
			this.commandTypes = commandTypes;
		}

		public override void Execute( IWritableParameterizedConfiguration<TKey, TValue> parameter )
		{
			var constructor = commandTypes.Introduce( value.GetType().ToItem(), tuple => tuple.Item1.FindConstructor( tuple.Item2 ) ).FirstAssigned();
			var activator = ParameterConstructorDelegateFactory<object, ICommand<IWritableParameterizedConfiguration<TKey, TValue>>>.Make( constructor );
			var command = activator( value );
			command.Execute( parameter );
		}
	}*/

	public class ApplyFactoryConfigurationCommand<T> : ApplyFactoryConfigurationCommand<object, T>
	{
		public ApplyFactoryConfigurationCommand() {}

		public ApplyFactoryConfigurationCommand( IFactory<object, T> factory ) : base( factory ) {}
	}

	public class ApplyFactoryConfigurationCommand<TKey, TValue> : ApplyConfigurationCommandBase<TKey, TValue>
	{
		public ApplyFactoryConfigurationCommand() {}

		public ApplyFactoryConfigurationCommand( IFactory<TKey, TValue> factory )
		{
			Factory = factory;
		}

		public IFactory<TKey, TValue> Factory { get; set; }

		protected override TValue Get( TKey key ) => Factory.Create( key );
	}

	public abstract class ApplyConfigurationCommandBase<TKey, TValue> : DeclarativeCommandBase<IWritableParameterizedConfiguration<TKey, TValue>>
	{
		readonly Func<TKey, TValue> get;
		
		readonly FixedStore<Func<TKey, TValue>> store = new FixedStore<Func<TKey, TValue>>();

		protected ApplyConfigurationCommandBase()
		{
			get = Get;
		}

		protected abstract TValue Get( TKey key );

		public override void Execute( object _ )
		{
			var aware = Parameter as IStoreAware<Func<TKey, TValue>>;
			if ( aware != null )
			{
				store.Assign( aware.Value );
			}

			Parameter.Assign( get );
		}

		protected override void OnDispose()
		{
			if ( store.Value != null )
			{
				Parameter.Assign( store.Value );
				store.Dispose();
			}
		}
	}

	public class ApplyValueConfigurationCommand<TKey, TValue> : ApplyConfigurationCommandBase<TKey, TValue>
	{
		public ApplyValueConfigurationCommand() {}

		public ApplyValueConfigurationCommand( TValue value )
		{
			Value = value;
		}

		public TValue Value { get; set; }

		protected override TValue Get( TKey key ) => Value;
	}

	public class ApplyValueConfigurationCommand<T> : ApplyValueConfigurationCommand<object, T>
	{
		public ApplyValueConfigurationCommand() {}

		public ApplyValueConfigurationCommand( T value ) : base( value ) {}
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

	public abstract class ConfigurationBase<T> : IWritableConfiguration<T>, IStoreAware<Func<T>>
	{
		readonly IWritableStore<Func<T>> store;

		protected ConfigurationBase( Func<T> factory ) : this( new FixedStore<Func<T>>( factory ) ) {}

		protected ConfigurationBase( IWritableStore<Func<T>> store )
		{
			this.store = store;
		}

		public void Assign( Func<T> item ) => store.Assign( item );

		public T Get() => store.Value();

		Func<T> IStoreAware<Func<T>>.Value => store.Value;
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
