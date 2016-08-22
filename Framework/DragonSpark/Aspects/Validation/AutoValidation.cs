using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IAdapterSource> DefaultSources { get; } =
			new IAdapterSource[]
			{
				new AdapterSource( typeof(IValidatedParameterizedSource<,>), GenericSourceAdapterFactory.Default.Get ),
				new AdapterSource( typeof(IValidatedParameterizedSource), SourceAdapterFactory.Default.Get ),
				new AdapterSource( typeof(ICommand<>), GenericCommandAdapterFactory.Default.Get ),
				new AdapterSource( typeof(ICommand), CommandAdapterFactory.Default.Get ),
			}.ToImmutableArray();
	}
	
	sealed class AdapterLocator : ParameterizedSourceBase<IParameterValidationAdapter>
	{
		public static AdapterLocator Default { get; } = new AdapterLocator();
		AdapterLocator() : this( AdapterSources.DefaultNested.Get ) {}

		readonly Func<Type, IAdapterSource> factorySource;

		AdapterLocator( Func<Type, IAdapterSource> factorySource )
		{
			this.factorySource = factorySource;
		}

		sealed class AdapterSources : Cache<Type, IAdapterSource>
		{
			public static AdapterSources DefaultNested { get; } = new AdapterSources();
			AdapterSources() : this( AutoValidation.DefaultSources ) {}

			readonly ImmutableArray<IAdapterSource> sources;
			
			AdapterSources( ImmutableArray<IAdapterSource> sources )
			{
				this.sources = sources;
			}

			public override IAdapterSource Get( Type parameter )
			{
				foreach ( var source in sources )
				{
					if ( source.IsSatisfiedBy( parameter ) )
					{
						return source;
					}
				}
				return null;
			}
		}

		public override IParameterValidationAdapter Get( object parameter )
		{
			var other = parameter.GetType();
			var adapter = factorySource( other )?.Get( parameter );
			if ( adapter != null )
			{
				return adapter;
			}

			throw new InvalidOperationException( $"Adapter not found for {other}." );
		}
	}

	public static class Extensions
	{
		public static bool Marked( this IAutoValidationController @this, [Optional]object parameter, bool valid )
		{
			@this.MarkValid( parameter, valid );
			return valid;
		}
	}

	public interface IAutoValidationController : ISpecification
	{
		void MarkValid( object parameter, bool valid );

		object Execute( object parameter, Func<object> proceed );
	}

	public interface IMethodAware
	{
		MethodInfo Method { get; }
	}

	public interface IAspectHub
	{
		void Register( IAspect aspect );
	}

	sealed class LinkedParameterAwareHandler : IParameterAwareHandler
	{
		readonly IParameterAwareHandler current;
		readonly IParameterAwareHandler next;

		public LinkedParameterAwareHandler( IParameterAwareHandler current, IParameterAwareHandler next )
		{
			this.current = current;
			this.next = next;
		}

		public bool Handles( object parameter ) => current.Handles( parameter ) || next.Handles( parameter );

		public bool Handle( object parameter, out object handled ) => current.Handle( parameter, out handled ) || next.Handle( parameter, out handled );
	}

	sealed class AutoValidationController : ConcurrentDictionary<int, object>, IAutoValidationController, IAspectHub
	{
		// readonly static object Executing = new object();

		readonly IParameterValidationAdapter validator;
		
		public AutoValidationController( IParameterValidationAdapter validator )
		{
			this.validator = validator;
		}

		public bool IsSatisfiedBy( [Optional]object parameter ) => Handler?.Handles( parameter ) ?? false;

		bool IsMarked( [Optional]object parameter )
		{
			object current;
			var result = TryGetValue( Environment.CurrentManagedThreadId, out current ) && Equals( current, parameter ) && Clear();
			return result;
		}

		public void MarkValid( [Optional]object parameter, bool valid )
		{
			if ( valid )
			{
				this[Environment.CurrentManagedThreadId] = parameter;
			}
			else
			{
				Clear();
			}
		}

		new bool Clear()
		{
			object removed;
			TryRemove( Environment.CurrentManagedThreadId, out removed );
			return true;
		}

		public object Execute( [Optional]object parameter, Func<object> proceed )
		{
			object handled = null;
			if ( Handler?.Handle( parameter, out handled ) ?? false )
			{
				Clear();
				return handled;
			}

			var result = IsMarked( parameter ) || validator.IsSatisfiedBy( parameter ) ? proceed() : null;
			Clear();
			return result;
		}

		IParameterAwareHandler Handler { get; set; }
		
		public void Register( IAspect aspect )
		{
			var methodAware = aspect as IMethodAware;
			if ( methodAware != null && validator.IsSatisfiedBy( methodAware.Method ) )
			{
				var handler = aspect as IParameterAwareHandler;
				if ( handler != null )
				{
					Handler = Handler != null ? new LinkedParameterAwareHandler( handler, Handler ) : handler;
				}
			}
		}
	}
}
