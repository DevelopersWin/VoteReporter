using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.Activation.Sources.Caching;

namespace DragonSpark.Aspects.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IProfile> DefaultProfiles { get; } = new IProfile[] { GenericFactoryProfile.Instance, GenericCommandProfile.Instance, FactoryProfile.Instance, CommandProfile.Instance }.ToImmutableArray();

		class GenericFactoryProfile : ProfileBase
		{
			public static GenericFactoryProfile Instance { get; } = new GenericFactoryProfile();
			GenericFactoryProfile() : base( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), GenericFactoryProfileFactory.Instance.Create ) {}
		}

		class FactoryProfile : ProfileBase
		{
			public static FactoryProfile Instance { get; } = new FactoryProfile();
			FactoryProfile() : base( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), FactoryProfileFactory.Instance.Create ) {}
		}

		class GenericCommandProfile : ProfileBase
		{
			public static GenericCommandProfile Instance { get; } = new GenericCommandProfile();
			GenericCommandProfile() : base( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute), GenericCommandProfileFactory.Instance.Create ) {}
		}

		class CommandProfile : ProfileBase
		{
			public static CommandProfile Instance { get; } = new CommandProfile();
			CommandProfile() : base( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute), CommandProfileFactory.Instance.Create ) {}
		}
	}
	
	class AdapterLocator // : FactoryBase<object, IParameterValidationAdapter>
	{
		public static AdapterLocator Instance { get; } = new AdapterLocator();
		AdapterLocator() : this( AutoValidation.DefaultProfiles ) {}

		readonly Func<Type, Func<object, IParameterValidationAdapter>> factorySource;

		public AdapterLocator( ImmutableArray<IProfile> profiles ) : this( new Cache<Type, Func<object, IParameterValidationAdapter>>( new Factory( profiles ).Create ).Get ) {}

		AdapterLocator( Func<Type, Func<object, IParameterValidationAdapter>> factorySource )
		{
			this.factorySource = factorySource;
		}

		class Factory// : FactoryBase<Type, Func<object, IParameterValidationAdapter>>
		{
			readonly ImmutableArray<IProfile> profiles;
			public Factory( ImmutableArray<IProfile> profiles )
			{
				this.profiles = profiles;
			}

			public Func<object, IParameterValidationAdapter> Create( Type parameter )
			{
				foreach ( var profile in profiles )
				{
					if ( profile.InterfaceType.IsAssignableFrom( parameter ) )
					{
						return profile.ProfileSource;
					}
				}
				return null;
			}
		}

		public IParameterValidationAdapter Create( object parameter )
		{
			var other = parameter.GetType();
			var factory = factorySource( other );
			if ( factory != null )
			{
				return factory( parameter );
			}

			throw new InvalidOperationException( $"Profile not found for {other}." );
		}
	}

	public interface IAutoValidationController
	{
		bool IsValid( object parameter );

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

	class LinkedParameterAwareHandler : IParameterAwareHandler
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

	class AutoValidationController : ConcurrentDictionary<int, object>, IAutoValidationController, IAspectHub
	{
		readonly static object Executing = new object();

		readonly IParameterValidationAdapter validator;
		readonly Func<MethodInfo, bool> specification;
		
		public AutoValidationController( IParameterValidationAdapter validator ) : this( validator, MethodEqualitySpecification.For( validator.Method ) ) {}

		public AutoValidationController( IParameterValidationAdapter validator, Func<MethodInfo, bool> specification )
		{
			this.validator = validator;
			this.specification = specification;
		}

		IParameterAwareHandler Handler { get; set; }

		public bool IsValid( object parameter )
		{
			return Handler?.Handles( parameter ) ?? false;
			/*if ( Handler != null && Handler.Handles( parameter ) )
			{
				return true;
			}*/

			// return Current( parameter );
		}

		bool? Current( object parameter )
		{
			object current;
			var contains = TryGetValue( Environment.CurrentManagedThreadId, out current );
			var result = contains && current != Executing ? (bool?)Equals( current, parameter ) : null;
			return result;
		}

		public void MarkValid( object parameter, bool valid )
		{
			if ( valid )
			{
				this[Environment.CurrentManagedThreadId] = parameter;
			}
			else
			{
				object removed;
				TryRemove( Environment.CurrentManagedThreadId, out removed );
			}
		}

		public object Execute( object parameter, Func<object> proceed )
		{
			object handled;
			if ( Handler != null && Handler.Handle( parameter, out handled ) )
			{
				MarkValid( parameter, false );
				return handled;
			}

			var valid = Current( parameter ).GetValueOrDefault() || CheckAndMark( parameter );
			if ( valid )
			{
				var result = proceed();
				MarkValid( parameter, false );
				return result;
			}
			return null;
		}

		bool CheckAndMark( object parameter )
		{
			MarkValid( Executing, true );
			var result = validator.IsValid( parameter );
			MarkValid( parameter, result );
			return result;
		}

		public void Register( IAspect aspect )
		{
			var methodAware = aspect as IMethodAware;
			if ( methodAware != null && specification( methodAware.Method ) )
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
