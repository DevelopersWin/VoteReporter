using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

	public class AssignConfigurationsCommand<T> : ApplyConfigurationCommand<ImmutableArray<ITransformer<T>>>
	{
		public AssignConfigurationsCommand( ImmutableArray<ITransformer<T>> assignable ) : base( assignable ) {}

		public AssignConfigurationsCommand( ImmutableArray<ITransformer<T>> value, IAssignable<Func<ImmutableArray<ITransformer<T>>>> assignable = null ) : base( value, assignable ) {}
	}

	public static class Extensions
	{
		public static ICommand From<T>( this IAssignable<Func<ImmutableArray<T>>> @this, params T[] items ) => new ApplyDelegateConfigurationCommand<ImmutableArray<T>>( items.ToImmutableArray, @this );
		public static ICommand From<T>( this IAssignable<Func<ImmutableArray<T>>> @this, IEnumerable<T> items ) => @this.From( items.Fixed() );

		public static ICommand From<T>( this IAssignable<Func<T>> @this, T value ) => new ApplyConfigurationCommand<T>( value, @this );
		public static ICommand From<T>( this IAssignable<Func<T>> @this, ISource<T> value ) => new ApplySourceConfigurationCommand<T>( value, @this );

		public static ICommand From<T>( this IAssignable<Func<ImmutableArray<ITransformer<T>>>> @this, IEnumerable<ITransformer<T>> configurations ) => @this.From( configurations.ToImmutableArray() );
		public static ICommand From<T>( this IAssignable<Func<ImmutableArray<ITransformer<T>>>> @this, params ITransformer<T>[] configurations ) => @this.From( configurations.ToImmutableArray() );
		public static ICommand From<T>( this IAssignable<Func<ImmutableArray<ITransformer<T>>>> @this, ImmutableArray<ITransformer<T>> configurations ) => new AssignConfigurationsCommand<T>( configurations, @this );
	}

	public class ApplySourceConfigurationCommand<T> : ApplyConfigurationCommandBase<T>
	{
		public ApplySourceConfigurationCommand() {}

		public ApplySourceConfigurationCommand( ISource<T> source, IAssignable<Func<T>> parameter = null ) : base( parameter )
		{
			Source = source;
		}

		public ISource<T> Source { get; set; }

		protected override T Get() => Source.Get();
	}

	public class ApplyParameterizedSourceConfigurationCommand<T> : ApplyParameterizedSourceConfigurationCommand<object, T>
	{
		public ApplyParameterizedSourceConfigurationCommand() {}
		public ApplyParameterizedSourceConfigurationCommand( IParameterizedSource<object, T> source ) : base( source ) {}
	}

	public class ApplyParameterizedSourceConfigurationCommand<TKey, TValue> : ApplyConfigurationCommandBase<TKey, TValue>
	{
		public ApplyParameterizedSourceConfigurationCommand() {}

		public ApplyParameterizedSourceConfigurationCommand( IParameterizedSource<TKey, TValue> source )
		{
			Source = source;
		}

		public IParameterizedSource<TKey, TValue> Source { get; set; }

		protected override TValue Get( TKey key ) => Source.Get( key );
	}

	public abstract class ApplyConfigurationCommandBase<T> : DeclaredCommandBase<IAssignable<Func<T>>>
	{
		readonly Func<T> get;
		
		protected ApplyConfigurationCommandBase( IAssignable<Func<T>> parameter = null ) : base( parameter )
		{
			get = Get;
		}

		protected abstract T Get();

		public override void Execute( object _ ) => Parameter.Assign( get );
	}

	public abstract class ApplyConfigurationCommandBase<TKey, TValue> : DeclaredCommandBase<IAssignable<Func<TKey, TValue>>>
	{
		readonly Func<TKey, TValue> get;
		
		protected ApplyConfigurationCommandBase( IAssignable<Func<TKey, TValue>> parameter = null ) : base( parameter )
		{
			get = Get;
		}

		protected abstract TValue Get( TKey key );

		public override void Execute( object _ ) => Parameter.Assign( get );
	}

	public class ApplyDelegateConfigurationCommand<T> : ApplyConfigurationCommandBase<T>
	{
		readonly Func<T> factory;

		public ApplyDelegateConfigurationCommand( Func<T> factory, IAssignable<Func<T>> parameter = null ) : base( parameter )
		{
			this.factory = factory;
		}

		protected override T Get() => factory();
	}

	public class ApplyConfigurationCommand<T> : ApplyConfigurationCommandBase<T>
	{
		public ApplyConfigurationCommand() {}

		public ApplyConfigurationCommand( T value, IAssignable<Func<T>> assignable = null )
		{
			Parameter = assignable;
			Value = value;
		}

		public T Value { get; set; }

		protected override T Get() => Value;
	}

	public interface IConfiguration<T> : ISource<T>, IAssignable<Func<T>> { }

	public interface IParameterizedConfiguration<T> : IParameterizedConfiguration<object, T> {}

	public interface IParameterizedConfiguration<TKey, TValue> : IParameterizedSource<TKey, TValue>, IAssignable<Func<TKey, TValue>> {}

	public interface IConfigurations<T> : IStore<ImmutableArray<ITransformer<T>>>, IPriorityAware {}

	/*public abstract class ConfigurationsStoreBase<T> : StoreBase<ImmutableArray<ITransformer<T>>>
	{
		protected override ImmutableArray<ITransformer<T>> Get() => From().ToImmutableArray();

		protected abstract IEnumerable<ITransformer<T>> From();

		public virtual Priority Priority => Priority.Normal;
	}*/

	public class Configurations<T> : ItemsStoreBase<ITransformer<T>>, IConfigurations<T>
	{
		public Configurations() {}
		public Configurations( IEnumerable<ITransformer<T>> items ) : base( items ) {}
		public Configurations( params ITransformer<T>[] items ) : base( items ) {}

		public virtual Priority Priority => Priority.Normal;
	}

	public abstract class ConfigurationsFactoryBase<TParameter, TConfiguration> : FactoryBase<TParameter, IConfigurations<TConfiguration>>
	{
		public override IConfigurations<TConfiguration> Create( TParameter parameter ) => new Configurations<TConfiguration>( From( parameter ) );

		protected abstract IEnumerable<ITransformer<TConfiguration>> From( TParameter parameter );
	}

	/*public abstract class ConfigurationsFactoryBase<T> : FactoryBase<ImmutableArray<ITransformer<T>>>
	{
		
	}

	public class ConfigurationsFactory<T> : ConfigurationsFactoryBase<T>
	{
		public static ConfigurationsFactory<T> Instance { get; } = new ConfigurationsFactory<T>( Items<ITransformer<T>>.Default );

		readonly ImmutableArray<ITransformer<T>> instances;

		public ConfigurationsFactory( params ITransformer<T>[] instances )
		{
			this.instances = instances.ToImmutableArray();
		}

		protected override IEnumerable<ITransformer<T>> From() => instances.ToArray();
	}

	*/

	public static class ConfigurationExtensions
	{
		/*public static T Apply<T>( this IConfiguration<T> @this, Func<T> factory )
		{
			@this.Assign( factory );
			return @this.Get();
		}*/

		public static T Default<T>( this IParameterizedConfiguration<object, T> @this ) => @this.Get( Execution.Current() );

		/*public static IStore<T> ToStore<T>( this IConfiguration<object, T> @this ) => StoreCache<T>.Default.Get( @this );
		class StoreCache<T> : Cache<IConfiguration<object, T>, IStore<T>>
		{
			public static StoreCache<T> Default { get; } = new StoreCache<T>();
			StoreCache() : base( configuration => new DefaultExecutionContextFactoryStore<T>( configuration.Get ) ) {}
		}*/
	}
}
