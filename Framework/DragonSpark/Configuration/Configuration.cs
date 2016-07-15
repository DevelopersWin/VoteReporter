using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup.Commands;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Markup;
using Defaults = DragonSpark.Activation.Defaults;

namespace DragonSpark.Configuration
{
	public class EnableMethodCaching : WritableStructureConfiguration<bool>
	{
		public static EnableMethodCaching Instance { get; } = new EnableMethodCaching();
		EnableMethodCaching() : base( EnableMethodCachingConfiguration.Instance.Get ) {}
	}

	public class EnableMethodCachingConfiguration : DeclarativeConfigurationBase<bool>
	{
		public static EnableMethodCachingConfiguration Instance { get; } = new EnableMethodCachingConfiguration();

		public EnableMethodCachingConfiguration() : base( !PostSharpEnvironment.IsPostSharpRunning ) {}
	}

	public interface IInitializationCommand : ICommand, IDisposable {}

	[ApplyAutoValidation]
	abstract class InitializationCommandBase : CompositeCommand, IInitializationCommand
	{
		protected InitializationCommandBase( params ICommand[] commands ) : base( new OnlyOnceSpecification(), commands ) {}
	}

	public class ConfigurationValues : Dictionary<IWritableStore, IStore> {}

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
	}

	class AssignValueCommand : CommandBase<IStore>
	{
		readonly IWritableStore writable;
		public AssignValueCommand( IWritableStore writable )
		{
			this.writable = writable;
		}

		public override void Execute( IStore parameter ) => writable.Assign( parameter.Value );
	}

	public abstract class DeclarativeFactoryConfigurationBase<T> : DeclarativeFactoryConfigurationBase<object, T>
	{
		protected DeclarativeFactoryConfigurationBase( IFactory<object, T> factory ) : base( factory ) {}
	}

	[ContentProperty( nameof(Value) )]
	public abstract class DeclarativeFactoryConfigurationBase<TKey, TValue> : ConfigurationBase<TKey, TValue>
	{
		protected DeclarativeFactoryConfigurationBase( IFactory<TKey, TValue> factory )
		{
			Value = factory;
		}

		public IFactory<TKey, TValue> Value { get; set; }

		public override TValue Get( TKey key ) => Value.Create( key );
	}


	[ContentProperty( nameof(Value) )]
	public abstract class DeclarativeConfigurationBase<TKey, TValue> : ConfigurationBase<TKey, TValue>
	{
		protected DeclarativeConfigurationBase( TValue defaultValue )
		{
			Value = defaultValue;
		}

		public TValue Value { get; set; }

		public override TValue Get( TKey key ) => Value;
	}

	public abstract class DeclarativeConfigurationBase<T> : DeclarativeConfigurationBase<object, T>, IConfiguration<T>
	{
		protected DeclarativeConfigurationBase( T defaultValue ) : base( defaultValue ) {}
	}

	public interface IConfiguration<out T> : IConfiguration<object, T> {}

	public interface IConfiguration<in TKey, out TValue>
	{
		TValue Get( TKey key );
	}

	public static class ConfigurationExtensions
	{
		public static T Default<T>( this IConfiguration<object, T> @this ) => @this.Get( Defaults.ExecutionContext() );
	}

	public abstract class ConfigurationBase<T> : ConfigurationBase<object, T>, IConfiguration<T> {}

	public abstract class ConfigurationBase<TKey, TValue> : IConfiguration<TKey, TValue>, IStore<Func<TKey, TValue>>
	{
		readonly Func<TKey, TValue> value;

		protected ConfigurationBase()
		{
			value = Get;
		}

		public abstract TValue Get( TKey key );
		object IStore.Value => value;
		Func<TKey, TValue> IStore<Func<TKey, TValue>>.Value => value;
	}

	/*public class Configuration<T> : Configuration<object, T>
	{
		public Configuration( Func<object, T> value ) : base( value ) {}
	}

	public class Configuration<TKey, TValue> : ExecutionContextConfigurationBase<TKey, TValue>
	{
		readonly Func<TKey, TValue> value;
		public Configuration( Func<TKey, TValue> value )
		{
			this.value = value;
		}

		protected override Func<TKey, TValue> Get() => value;
	}

	public abstract class ExecutionContextConfigurationBase<TKey, TValue> : StoreBase<Func<TKey, TValue>>, IConfiguration<TKey, TValue> {}*/

	/*public abstract class ExecutionContextConfigurationBase<T> : PropertyStore<T>
	{
		protected ExecutionContextConfigurationBase( T value )
		{
			Value = value;
		}
	}*/

	/*[ContentProperty( nameof(Configurations) )]
	[ApplyAutoValidation]
	public class InitializeConfigurationCommand : ServicedCommand<ConfigureCommand, ImmutableArray<IWritableStore>>
	{
		public InitializeConfigurationCommand() : base( new OnlyOnceSpecification() ) {}

		public Collection<IWritableStore> Configurations { get; } = new Collection<IWritableStore>();

		public override ImmutableArray<IWritableStore> GetParameter() => Configurations.ToImmutableArray();
	}*/
}
