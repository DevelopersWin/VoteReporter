using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xunit;

namespace DragonSpark.Testing.Runtime
{
	public class Controller
	{
		readonly IParameterAware workflow;
		readonly IAssignableParameterAware assignable;

		public Controller( object instance, IAssignableParameterAware assignable ) : this( new ParameterWorkflow( instance, assignable ), assignable ) {}

		Controller( IParameterAware workflow, IAssignableParameterAware assignable )
		{
			this.workflow = workflow;
			this.assignable = assignable;
		}

		public bool IsAllowed( Func<object, bool> assign, object parameter )
		{
			using ( new IsAllowedAssignment( assignable, assign ) )
			{
				return workflow.IsAllowed( parameter );
			}
		}

		public object Execute( Func<object, object> assign, object parameter )
		{
			using ( new ExecuteAssignment( assignable, assign ) )
			{
				return workflow.Execute( parameter );
			}
		}

		class IsAllowedAssignment : Assignment<Func<object, bool>>
		{
			public IsAllowedAssignment( IAssignableParameterAware assignable, Func<object, bool> first ) : base( assignable.Assign, new Value<Func<object, bool>>( first ) ) {}
		}

		class ExecuteAssignment : Assignment<Func<object, object>>
		{
			public ExecuteAssignment( IAssignableParameterAware assignable, Func<object, object> first ) : base( assignable.Assign, new Value<Func<object, object>>( first ) ) {}
		}
	}

	[Serializable]
	public class Profile
	{
		public Profile( Type type, string isAllowed, string execute )
		{
			Type = type;
			IsAllowed = isAllowed;
			Execute = execute;
		}

		public Type Type { get; }
		public string IsAllowed { get; }
		public string Execute { get; }
	}

	public interface IControllerFactory
	{
		Controller Create( object instance );
	}

	abstract class ControllerFactoryBase<T> : IControllerFactory
	{
		public Controller Create( object instance )
		{
			var aware = instance.AsTo<T, IParameterAware>( Create );
			var assignable = new AssignableParameterAware( aware );
			var result = new Controller( instance, assignable );
			return result;
		}

		protected abstract IParameterAware Create( T instance );
	}

	abstract class GenericFactoryControllerFactoryBase : IControllerFactory
	{
		readonly Type genericType;
		readonly string methodName;

		protected GenericFactoryControllerFactoryBase( Type genericType, string methodName = nameof(Create) )
		{
			this.genericType = genericType;
			this.methodName = methodName;
		}

		public Controller Create( object instance )
		{
			var implementation = instance.GetType().Adapt().GetImplementations( genericType ).First();
			var result = GetType().Adapt().Invoke<Controller>( methodName, implementation.GenericTypeArguments, instance );
			return result;
		}
	}

	class GenericFactoryControllerFactory : GenericFactoryControllerFactoryBase
	{
		public static GenericFactoryControllerFactory Instance { get; } = new GenericFactoryControllerFactory();

		GenericFactoryControllerFactory() : base( typeof(IFactory<,>), nameof(Create) ) {}

		static Controller Create<TParameter, TResult>( object instance ) => FactoryControllerFactory<TParameter, TResult>.Instance.Create( instance );
	}
	class GenericCommandControllerFactory : GenericFactoryControllerFactoryBase
	{
		public static GenericCommandControllerFactory Instance { get; } = new GenericCommandControllerFactory();

		GenericCommandControllerFactory() : base( typeof(ICommand<>), nameof(Create) ) {}

		static Controller Create<T>( object instance ) => CommandControllerFactory<T>.Instance.Create( instance );
	}

	class FactoryControllerFactory<TParameter, TResult> : ControllerFactoryBase<IFactory<TParameter, TResult>>
	{
		public static FactoryControllerFactory<TParameter, TResult> Instance { get; } = new FactoryControllerFactory<TParameter, TResult>();

		protected override IParameterAware Create( IFactory<TParameter, TResult> instance ) => new FactoryParameterAware<TParameter, TResult>( instance );
	}

	class CommandControllerFactory<T> : ControllerFactoryBase<ICommand<T>>
	{
		public static CommandControllerFactory<T> Instance { get; } = new CommandControllerFactory<T>();

		protected override IParameterAware Create( ICommand<T> instance ) => new CommandParameterAware<T>( instance );
	}

	class CommandControllerFactory : ControllerFactoryBase<ICommand>
	{
		public static CommandControllerFactory Instance { get; } = new CommandControllerFactory();

		protected override IParameterAware Create( ICommand instance ) => new CommandParameterAware( instance );
	}

	class FactoryControllerFactory : ControllerFactoryBase<IFactoryWithParameter>
	{
		public static FactoryControllerFactory Instance { get; } = new FactoryControllerFactory();

		protected override IParameterAware Create( IFactoryWithParameter instance ) => new FactoryWithParameterAware( instance );
	}

	class ControllerRepository : IControllerFactory
	{
		readonly IControllerFactory inner;
		readonly ConditionalWeakTable<object, Controller> controllers = new ConditionalWeakTable<object, Controller>();

		public ControllerRepository( IControllerFactory inner )
		{
			this.inner = inner;
		}

		public Controller Create( object instance ) => controllers.GetValue( instance, inner.Create );
	}

	public interface IResourceRepository<in TKey, TValue>
	{
		void Add( TKey key, TValue resource );

		TValue Get( TKey key );

		// void Attach( object key, Action<object> callback );
	}

	class AmbientResourceRepository<TKey, TValue> : IResourceRepository<TKey, TValue> where TValue : class where TKey : class
	{
		// public static AmbientResourceRepository<TKey, TValue> Instance { get; } = new AmbientResourceRepository<TKey, TValue>();

		readonly ConditionalWeakTable<TKey, TValue> resources = new ConditionalWeakTable<TKey, TValue>();

		public void Add( TKey key, TValue resource ) => resources.Add( key, resource );

		public TValue Get( TKey key )
		{
			TValue result;
			return resources.TryGetValue( key, out result ) ? result : null;
		}
	}

	public class ObservableResourceRepository<TKey, TValue> : IResourceRepository<TKey, TValue>, IObservable<TKey> where TValue : class where TKey : class
	{
		// public static ObservableResourceRepository<TKey, TValue> Instance { get; } = new ObservableResourceRepository<TKey, TValue>( AmbientResourceRepository<TKey, TValue>.Instance );

		readonly ISubject<TKey> subject = new ReplaySubject<TKey>();

		readonly IResourceRepository<TKey, TValue> inner;
		public ObservableResourceRepository( IResourceRepository<TKey, TValue> inner )
		{
			this.inner = inner;
		}

		public void Add( TKey key, TValue resource )
		{
			inner.Add( key, resource );
			subject.OnNext( key );
		}

		public TValue Get( TKey key ) => inner.Get( key );

		public IDisposable Subscribe( IObserver<TKey> observer ) => subject.Subscribe( observer );
	}

	class ResourceRepository : ObservableResourceRepository<ResourceKey, IControllerFactory>
	{
		public static ResourceRepository Instance { get; } = new ResourceRepository();

		public ResourceRepository() : base( new AmbientResourceRepository<ResourceKey, IControllerFactory>() ) {}
	}

	/*public static class ResourceRepositoryExtensions
	{
		public static T Get<T>( this IResourceRepository @this, object key ) => (T)@this.Get( key );
	}*/

	public class ResourceKey : IEquatable<ResourceKey>
	{
		readonly static ReflectionTypeComparer Comparer = ReflectionTypeComparer.GetInstance();

		public ResourceKey( Type targetType, Type implementationType )
		{
			TargetType = targetType;
			ImplementationType = implementationType;
		}

		public Type TargetType { get; }
		public Type ImplementationType { get; }

		public bool Equals( ResourceKey other ) => !ReferenceEquals( null, other ) && ( ReferenceEquals( this, other ) || Comparer.Equals( TargetType, other.TargetType ) && Comparer.Equals( ImplementationType, other.ImplementationType ) );

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( ReferenceEquals( this, obj ) || obj.GetType() == GetType() && Equals( (ResourceKey)obj ) );

		public override int GetHashCode()
		{
			unchecked
			{
				return ( ( TargetType?.GetHashCode() ?? 0 ) * 397 ) ^ ( ImplementationType?.GetHashCode() ?? 0 );
			}
		}

		public static bool operator ==( ResourceKey left, ResourceKey right ) => Equals( left, right );

		public static bool operator !=( ResourceKey left, ResourceKey right ) => !Equals( left, right );
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	public abstract class ParameterValidatorBase : TypeLevelAspect, IAspectProvider
	{
		readonly Profile profile;
		readonly IControllerFactory factory;
		readonly IResourceRepository<ResourceKey, IControllerFactory> repository;

		protected ParameterValidatorBase( Profile profile, IControllerFactory factory ) : this( profile, factory, ResourceRepository.Instance ) {}

		protected ParameterValidatorBase( Profile profile, IControllerFactory factory, IResourceRepository<ResourceKey, IControllerFactory> repository )
		{
			this.profile = profile;
			this.factory = factory;
			this.repository = repository;
		}

		public override void RuntimeInitialize( Type type ) => repository.Add( new ResourceKey( type, profile.Type ), factory );

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var result = targetElement.AsTo<Type, IEnumerable<AspectInstance>>( type =>
			{
				var implementation = type.Adapt().DetermineImplementation( profile.Type );
				var targets = type.GetInterfaceMap( implementation ).TargetMethods;

				var items = new[] { profile.IsAllowed, profile.Execute }
							.Select( s => targets.Single( info => info.Name == s ) )
							.TupleWith( new IAspect[] { new IsAllowedAspect( profile.Type ), new ExecuteAspect( profile.Type ) } )
							.Select( info => new AspectInstance( info.Item1, info.Item2 ) );
				return items;
			} );
			return result;
		}
	}

	public sealed class FactoryParameterValidator : ParameterValidatorBase
	{
		public FactoryParameterValidator() : 
			base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), FactoryControllerFactory.Instance ) {}
	}

	public sealed class GenericFactoryParameterValidator : ParameterValidatorBase
	{
		public GenericFactoryParameterValidator() : 
			base( new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), GenericFactoryControllerFactory.Instance ) {}
	}

	public sealed class CommandParameterValidator : ParameterValidatorBase
	{
		public CommandParameterValidator() : 
			base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.CanExecute) ), CommandControllerFactory.Instance ) {}
	}

	// [Serializable]
	public sealed class GenericCommandParameterValidator : ParameterValidatorBase
	{
		public GenericCommandParameterValidator() : 
			base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.CanExecute) ), GenericCommandControllerFactory.Instance ) {}
	}

	[Serializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public abstract class ParameterValidationMethodBase : MethodInterceptionAspect
	{
		protected ParameterValidationMethodBase( Type implementationType )
		{
			ImplementationType = implementationType;
		}

		Type ImplementationType { get; set; }

		IControllerFactory Factory { get; set; }

		public override void RuntimeInitialize( MethodBase method )
		{
			var key = new ResourceKey( method.DeclaringType, ImplementationType );
			ResourceRepository.Instance
				//.OfType<Type>()
				.Where( o => key == o )
				.Take( 1 )
				.Subscribe( resourceKey =>
				{
					Factory = ResourceRepository.Instance.Get( resourceKey );
				} );
		}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = Factory.Create( args.Instance );
			args.ReturnValue = GetValue( controller, args.GetReturnValue, args.Arguments.Single() );
		}

		protected abstract object GetValue( Controller controller, Func<object> factory, object parameter );
	}

	[Serializable]
	public sealed class IsAllowedAspect : ParameterValidationMethodBase
	{
		public IsAllowedAspect( Type implementationType ) : base( implementationType ) {}

		// public IsAllowedAspect( IControllerFactory factory ) : base( factory ) {}
		protected override object GetValue( Controller controller, Func<object> factory, object parameter ) => 
			controller.IsAllowed( o => (bool)factory(), parameter );
	}

	[Serializable]
	public sealed class ExecuteAspect : ParameterValidationMethodBase
	{
		public ExecuteAspect( Type implementationType ) : base( implementationType ) {}

		protected override object GetValue( Controller controller, Func<object> factory, object parameter ) => controller.Execute( o => factory(), parameter );
	}

	class AssignableParameterAware : IAssignableParameterAware
	{
		readonly IParameterAware inner;

		public AssignableParameterAware( IParameterAware inner )
		{
			this.inner = inner;
		}

		public void Assign( Func<object, bool> condition ) => Condition = condition;

		public void Assign( Func<object, object> execute ) => Factory = execute;

		Func<object, bool> Condition { get; set; }

		Func<object, object> Factory { get; set; }

		public bool IsAllowed( object parameter ) => Condition?.Invoke( parameter ) ?? inner.IsAllowed( parameter );

		public object Execute( object parameter ) => Factory?.Invoke( parameter ) ?? inner.Execute( parameter );
	}

	public interface IAssignableParameterAware : IParameterAware
	{
		void Assign( Func<object, bool> condition );

		void Assign( Func<object, object> execute );
	}

	public class ParameterWorkflowTests
	{
		[Fact]
		public void BasicCondition()
		{
			var sut = new Factory();
			var cannot = sut.CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );

			var can = sut.CanCreate( 123 );
			Assert.True( can );
			Assert.Equal( 2, sut.CanCreateCalled );

			Assert.Equal( 0, sut.CreateCalled );

			var created = sut.Create( 123 );
			Assert.Equal( 2, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 6776, created );
		}

		[Fact]
		public void ExtendedCheck()
		{
			for ( int i = 0; i < 10000; i++ )
			{
				var sut = new ExtendedFactory();
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );
			var cannot = sut.CanCreate( (object)456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );

			var can = sut.CanCreate( 6776 );
			Assert.True( can );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = sut.Create( (object)6776 );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			}
		}

		[FactoryParameterValidator]
		public class Factory : IFactoryWithParameter
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return (int)parameter == 123;
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return 6776;
			}
		}

		[FactoryParameterValidator, GenericFactoryParameterValidator]
		class ExtendedFactory : IFactory<int, float>
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && CanCreate( (int)parameter );
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return Create( (int)parameter );
			}

			public bool CanCreate( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public float Create( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}
	}
}
