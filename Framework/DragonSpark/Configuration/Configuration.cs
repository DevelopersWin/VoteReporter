using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.Setup.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Configuration
{
	public class EnableMethodCaching : Configuration<bool>
	{
		public static EnableMethodCaching Instance { get; } = new EnableMethodCaching();
		EnableMethodCaching() : base( () => true ) {}
	}

	public class AssignConfigurationsCommand<T> : ApplyConfigurationCommand<ImmutableArray<ITransformer<T>>>
	{
		public AssignConfigurationsCommand( ImmutableArray<ITransformer<T>> value, IConfigurations<T> assignable = null ) : base( value, assignable ) {}
	}

	public static class Extensions
	{
		public static ICommand From<T>( this IParameterizedConfiguration<object, T> @this, T value ) => new ApplyParameterizedConfigurationCommand<T>( value, @this );

		public static ICommand From<T>( this IAssignable<Func<T>> @this, T value ) => new ApplyConfigurationCommand<T>( value, @this );
		public static ICommand From<T>( this IAssignable<Func<T>> @this, ISource<T> value ) => new ApplySourceConfigurationCommand<T>( value, @this );

		public static ICommand From<T>( this IConfigurations<T> @this, IEnumerable<ITransformer<T>> configurations ) => @this.From( configurations.ToImmutableArray() );
		public static ICommand From<T>( this IConfigurations<T> @this, params ITransformer<T>[] configurations ) => @this.From( configurations.ToImmutableArray() );
		public static ICommand From<T>( this IConfigurations<T> @this, ImmutableArray<ITransformer<T>> configurations ) => new AssignConfigurationsCommand<T>( configurations, @this );
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

	public class ApplyParameterizedConfigurationCommand<T> : ApplyConfigurationCommandBase<object, T>
	{
		public ApplyParameterizedConfigurationCommand() {}

		public ApplyParameterizedConfigurationCommand( T value, IAssignable<Func<object, T>> assignable = null )
		{
			Parameter = assignable;
			Value = value;
		}

		public T Value { get; set; }

		protected override T Get( object key ) => Value;
	}

	public interface IConfiguration<T> : ISource<T>, IAssignable<T>, IAssignable<Func<T>> { }

	public interface IParameterizedConfiguration<T> : IParameterizedConfiguration<object, T> {}

	public interface IParameterizedConfiguration<TKey, TValue> : IParameterizedSource<TKey, TValue>, IAssignable<Func<TKey, TValue>> {}

	public interface IConfigurations<T> : IConfiguration<ImmutableArray<ITransformer<T>>> {}

	public class Configurations<T> : Configuration<ImmutableArray<ITransformer<T>>>, IConfigurations<T>
	{
		public Configurations() {}
		public Configurations( params ITransformer<T>[] configurators ) : this( new Configurator<T>( configurators ).Get ) {}
		public Configurations( Func<ImmutableArray<ITransformer<T>>> defaultFactory ) : base( defaultFactory ) {}
	}

	public class Configurator<T> : ItemsStoreBase<ITransformer<T>>
	{
		readonly string name;
		public Configurator() {}
		public Configurator( IEnumerable<ITransformer<T>> items ) : base( items ) {}

		public Configurator( string name = null, params ITransformer<T>[] items ) : base( items )
		{
			this.name = name;
		}

		protected override IEnumerable<ITransformer<T>> Yield() => base.Yield().Concat( Exports.Instance.Value.GetExports<ITransformer<T>>( name ).AsEnumerable() );
	}

	public abstract class ConfiguratorsBase<TParameter, TConfiguration> : FactoryBase<TParameter, ImmutableArray<ITransformer<TConfiguration>>>
	{
		public override ImmutableArray<ITransformer<TConfiguration>> Create( TParameter parameter ) => From( parameter ).ToImmutableArray();

		protected abstract IEnumerable<ITransformer<TConfiguration>> From( TParameter parameter );
	}

	/*public static class ConfigurationExtensions
	{
		/*public static T Apply<T>( this IConfiguration<T> @this, Func<T> factory )
		{
			@this.Assign( factory );
			return @this.Get();
		}#1#

		public static T Default<T>( this IParameterizedConfiguration<object, T> @this ) => @this.Get( Execution.Current() );

		/*public static IStore<T> ToStore<T>( this IConfiguration<object, T> @this ) => StoreCache<T>.Default.Get( @this );
		class StoreCache<T> : Cache<IConfiguration<object, T>, IStore<T>>
		{
			public static StoreCache<T> Default { get; } = new StoreCache<T>();
			StoreCache() : base( configuration => new DefaultExecutionContextFactoryStore<T>( configuration.Get ) ) {}
		}#1#
	}*/
}
