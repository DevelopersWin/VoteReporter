using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;
using ICommand = System.Windows.Input.ICommand;

namespace DragonSpark.Extensions
{
	public static class CommandExtensions
	{
		public static ICommand<T> Cast<T>( this ICommand @this ) => Cast<T>( @this, arg => arg );

		public static ICommand<T> Cast<T>( this ICommand @this, Func<T, object> projection ) => new ProjectedCommand<T>( @this, projection );

		public static void Run( this ICommand @this ) => @this.Execute( default(object) );

		public static void Run<T>( this ICommand<T> @this ) => @this.Execute( default(T) );

		public static TCommand Run<TCommand, TParameter>( this TCommand @this, TParameter parameter ) where TCommand : ICommand<TParameter>
		{
			var result = @this.CanExecute( parameter ) ? @this : default(TCommand);
			if ( result != null )
			{
				result.Execute( parameter );
			}
			return result;
		}

		// public static FixedCommand<Func<T>> Fixed<T>( this ICommand<Func<T>> @this, T parameter ) => new FixedCommand<Func<T>>( @this, new FixedFactory<T>( parameter ).Create );
		public static FixedCommand<T> Fixed<T>( this ICommand<T> @this, T parameter ) => new FixedCommand<T>( @this, parameter );
		
		public static Action<T> ToDelegate<T>( this ICommand<T> @this ) => DelegateCache<T>.Default.Get( @this );
		class DelegateCache<T> : Cache<ICommand<T>, Action<T>>
		{
			public static DelegateCache<T> Default { get; } = new DelegateCache<T>();

			DelegateCache() : base( command => command.Execute ) {}
		}

		/*public static ICommand<T> WithAutoValidation<T>( this ICommand<T> @this ) => AutoValidationCache<T>.Default.Get( @this );
		class AutoValidationCache<T> : Cache<ICommand<T>, ICommand<T>>
		{
			public static AutoValidationCache<T> Default { get; } = new AutoValidationCache<T>();

			AutoValidationCache() : base( command => new AutoValidatingCommand<T>( command ) ) {}
		}*/

		public static Action<object> ToDelegate( this ICommand @this ) => DelegateCache.Default.Get( @this );
		class DelegateCache : Cache<ICommand, Action<object>>
		{
			public static DelegateCache Default { get; } = new DelegateCache();

			DelegateCache() : base( command => command.Execute ) {}
		}

		public static ISpecification<object> ToSpecification( this ICommand @this ) => SpecificationCache.Default.Get( @this );
		class SpecificationCache : Cache<ICommand, ISpecification<object>>
		{
			public static SpecificationCache Default { get; } = new SpecificationCache();

			SpecificationCache() : base( command => new DelegatedSpecification<object>( command.CanExecute ) ) {}
		}

		public static ISpecification<T> ToSpecification<T>( this ICommand<T> @this ) => SpecificationCache<T>.Default.Get( @this );
		class SpecificationCache<T> : Cache<ICommand<T>, ISpecification<T>>
		{
			public static SpecificationCache<T> Default { get; } = new SpecificationCache<T>();

			SpecificationCache() : base( command => new DelegatedSpecification<T>( command.CanExecute ) ) {}
		}
	}
}
