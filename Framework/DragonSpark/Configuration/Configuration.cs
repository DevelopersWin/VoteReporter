using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows.Input;

namespace DragonSpark.Configuration
{
	public class EnableMethodCaching : Scope<bool>
	{
		public static EnableMethodCaching Default { get; } = new EnableMethodCaching();
		EnableMethodCaching() : base( () => true ) {}
	}

	/*public class AssignConfigurationsCommand<T> : ConfigureGlobalScopeCommand<ImmutableArray<ITransformer<T>>>
	{
		public AssignConfigurationsCommand( ImmutableArray<ITransformer<T>> value, IConfigurationScope<T> assignable = null ) : base( value, assignable ) {}
	}*/

	public static class Extensions
	{
		public static ICommand From<T>( this IConfigurationScope<T> @this, IEnumerable<ITransformer<T>> configurations ) => @this.Configured( configurations.ToImmutableArray() );
		public static ICommand From<T>( this IConfigurationScope<T> @this, params ITransformer<T>[] configurations ) => @this.Configured( configurations.ToImmutableArray() );
		// public static ICommand Configure<T>( this IConfigurationScope<T> @this, ImmutableArray<ITransformer<T>> ConfigurationScope ) => @this.Configured( ConfigurationScope );
	}

	/*public class GlobalAssignmentCommand<T> : AssignCommand<Func<object, T>>
	{
		readonly Func<object, T> get;
		
		protected GlobalAssignmentCommand( IAssignable<Func<object, T>> parameter ) : base( parameter )
		{
			get = Get;
		}

		protected abstract T Get( object parameter );

		public override void Execute( object _ ) => Parameter.Assign( get );
	}*/

	/*public class ApplySourceConfigurationCommand<T> : GlobalAssignmentCommand<T>
	{
		public ApplySourceConfigurationCommand() {}

		public ApplySourceConfigurationCommand( ISource<T> source, IAssignable<Func<object, T>> parameter = null ) : base( parameter )
		{
			Source = source;
		}

		public ISource<T> Source { get; set; }

		protected override T Get( object parameter ) => Source.Get();
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

	public abstract class AssignmentCommandBase<T> : DeclaredCommandBase<IAssignable<T>>
	{
		protected AssignmentCommandBase( IAssignable<T> parameter = null ) : base( parameter ) {}
	}

	/*public abstract class GlobalAssignmentCommandBase<T> : DeclaredCommandBase<Func<object, T>>
	{
		
	}#1#

	public abstract class GlobalAssignmentCommand<T> : AssignmentCommandBase<Func<object, T>>
	{
		readonly Func<object, T> get;
		
		protected GlobalAssignmentCommand( IAssignable<Func<object, T>> parameter = null ) : base( parameter )
		{
			get = Get;
		}

		protected abstract T Get( object parameter );

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

	public class ApplyDelegateConfigurationCommand<T> : GlobalAssignmentCommand<T>
	{
		readonly Func<T> factory;

		public ApplyDelegateConfigurationCommand( Func<T> factory, IAssignable<Func<object, T>> parameter = null ) : base( parameter )
		{
			this.factory = factory;
		}

		protected override T Get( object parameter ) => factory();
	}

	public class ConfigureGlobalScopeCommand<T> : GlobalAssignmentCommand<T>
	{
		public ConfigureGlobalScopeCommand() {}

		public ConfigureGlobalScopeCommand( T value, IAssignable<Func<object, T>> assignable = null )
		{
			Parameter = assignable;
			Value = value;
		}

		public T Value { get; set; }

		protected override T Get( object parameter ) => Value;
	}

	public class ConfigureParameterizedScopeCommand<T> : ApplyConfigurationCommandBase<object, T>
	{
		public ConfigureParameterizedScopeCommand() {}

		public ConfigureParameterizedScopeCommand( T value, IAssignable<Func<object, T>> assignable = null )
		{
			Parameter = assignable;
			Value = value;
		}

		public T Value { get; set; }

		protected override T Get( object key ) => Value;
	}*/

	
}
