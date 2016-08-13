using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict, PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : TypeLevelAspect, IAspectProvider
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = AspectInstanceFactory.Instance.ToDelegate();

		readonly Func<Type, IEnumerable<AspectInstance>> source;

		public ApplyAutoValidationAttribute() : this( DefaultSource ) {}

		protected ApplyAutoValidationAttribute( Func<Type, IEnumerable<AspectInstance>> source )
		{
			this.source = source;
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			var result = type != null ? source( type ) : Items<AspectInstance>.Default;
			return result;
		}
	}

	class AutoValidationControllerFactory // : FactoryBase<object, IAutoValidationController>
	{
		public static IParameterizedSource<IAutoValidationController> Instance { get; } = new Cache<IAutoValidationController>( new AutoValidationControllerFactory().Create );
		AutoValidationControllerFactory() : this( AdapterLocator.Instance.Create ) {}

		readonly Func<object, IParameterValidationAdapter> adapterSource;
		readonly Action<object, IAspectHub> set;

		protected AutoValidationControllerFactory( Func<object, IParameterValidationAdapter> adapterSource ) : this( adapterSource, AspectHub.Instance.Set ) {}

		protected AutoValidationControllerFactory( Func<object, IParameterValidationAdapter> adapterSource, Action<object, IAspectHub> set )
		{
			this.adapterSource = adapterSource;
			this.set = set;
		}

		IAutoValidationController Create( object parameter )
		{
			var adapter = adapterSource( parameter );
			var result = new AutoValidationController( adapter );
			set( parameter, result );
			return result;
		}
	}

	static class Defaults
	{
		public static Func<object, IAutoValidationController> ControllerSource { get; } = AutoValidationControllerFactory.Instance.Get;
	}

	class AspectFactory<T> /*: FactoryBase<object, T>*/ where T : class, IAspect
	{
		/*public static AspectFactory<T> Instance { get; } = new AspectFactory<T>();
		AspectFactory() : this( Defaults.ControllerSource, ParameterConstructor<IAutoValidationController, T>.Default ) {}*/

		readonly Func<object, IAutoValidationController> controllerSource;
		readonly Func<IAutoValidationController, T> resultSource;

		public AspectFactory( Func<IAutoValidationController, T> resultSource ) : this( Defaults.ControllerSource, resultSource ) {}

		public AspectFactory( Func<object, IAutoValidationController> controllerSource, Func<IAutoValidationController, T> resultSource )
		{
			this.controllerSource = controllerSource;
			this.resultSource = resultSource;
		}

		public T Create( object parameter )
		{
			var controller = controllerSource( parameter );
			var result = resultSource( controller );
			return result;
		}
	}


	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class AutoValidationAspectBase : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly static Func<Type, ApplyAutoValidationAttribute> Applies = AttributeSupport<ApplyAutoValidationAttribute>.All.Get;

		readonly Func<object, IAspect> factory;
		protected AutoValidationAspectBase( Func<object, IAspect> factory )
		{
			this.factory = factory;
		}

		public object CreateInstance( AdviceArgs adviceArgs ) => Applies( adviceArgs.Instance.GetType() ) != null ? factory( adviceArgs.Instance ) : this;
		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}

	public class AutoValidationValidationAspect : AutoValidationAspectBase
	{
		readonly static Func<object, Implementation> Factory = new AspectFactory<Implementation>( controller => new Implementation( controller ) ).Create;
		public AutoValidationValidationAspect() : base( Factory ) {}

		sealed class Implementation : AutoValidationValidationAspect
		{
			readonly IAutoValidationController controller;
			public Implementation( IAutoValidationController controller )
			{
				this.controller = controller;
			}

			public override void OnInvoke( MethodInterceptionArgs args )
			{
				var parameter = args.Arguments[0];
				args.ReturnValue = controller.IsSatisfiedBy( parameter ) ||  controller.Marked( parameter, args.GetReturnValue<bool>() );
			}
		}
	}

	public class AutoValidationExecuteAspect : AutoValidationAspectBase
	{
		readonly static Func<object, Implementation> Factory = new AspectFactory<Implementation>( controller => new Implementation( controller ) ).Create;

		public AutoValidationExecuteAspect() : base( Factory ) {}

		sealed class Implementation : AutoValidationExecuteAspect
		{
			readonly IAutoValidationController controller;
			public Implementation( IAutoValidationController controller )
			{
				this.controller = controller;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = controller.Execute( args.Arguments[0], args.GetReturnValue );
		}
	}

	public interface IProfile : IEnumerable<Func<Type, AspectInstance>>
	{
		TypeAdapter InterfaceType { get; }

		Func<object, IParameterValidationAdapter> ProfileSource { get; }
	}

	abstract class ProfileBase : IProfile
	{
		readonly Func<Type, AspectInstance> validate;
		readonly Func<Type, AspectInstance> execute;

		protected ProfileBase( Type interfaceType, string valid, string execute, Func<object, IParameterValidationAdapter> factory ) : this( interfaceType, valid, interfaceType, execute, factory ) {}

		protected ProfileBase( Type interfaceType, string valid, Type executionType, string execute, Func<object, IParameterValidationAdapter> factory ) : this( interfaceType.Adapt(), 
			new AspectInstanceMethodFactory<AutoValidationValidationAspect>( interfaceType, valid ).Create, 
			new AspectInstanceMethodFactory<AutoValidationExecuteAspect>( executionType, execute ).Create, factory )
		{}

		protected ProfileBase( TypeAdapter interfaceType, Func<Type, AspectInstance> validate, Func<Type, AspectInstance> execute, Func<object, IParameterValidationAdapter> profileSource )
		{
			InterfaceType = interfaceType;
			ProfileSource = profileSource;
			this.validate = validate;
			this.execute = execute;
		}

		public TypeAdapter InterfaceType { get; }
		public Func<object, IParameterValidationAdapter> ProfileSource { get; }

		public IEnumerator<Func<Type, AspectInstance>> GetEnumerator()
		{
			yield return validate;
			yield return execute;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	sealed class AspectInstanceMethodFactory<T> : AspectInstanceFactoryBase where T : IAspect
	{
		public AspectInstanceMethodFactory( Type implementingType, string methodName ) : this( implementingType, methodName, Items<object>.Default ) {}
		public AspectInstanceMethodFactory( Type implementingType, string methodName, params object[] arguments ) : base( implementingType, methodName, Construct.New<T>( arguments ) ) {}
	}

	static class Construct
	{
		public static Func<MethodInfo, AspectInstance> New<T>( params object[] arguments ) => new ConstructAspectInstanceFactory( new ObjectConstruction( typeof(T), arguments ) ).Create;
	}

	sealed class ConstructAspectInstanceFactory// : FactoryBase<MethodInfo, AspectInstance>
	{
		readonly ObjectConstruction construction;

		public ConstructAspectInstanceFactory( ObjectConstruction construction )
		{
			this.construction = construction;
		}

		public AspectInstance Create( MethodInfo parameter ) => new AspectInstance( parameter, construction, null );
	}

	abstract class AspectInstanceFactoryBase// : FactoryBase<Type, AspectInstance>
	{
		readonly Type implementingType;
		readonly string methodName;
		readonly Func<MethodInfo, AspectInstance> factory;

		protected AspectInstanceFactoryBase( Type implementingType, string methodName, Func<MethodInfo, AspectInstance> factory )
		{
			this.implementingType = implementingType;
			this.methodName = methodName;
			this.factory = factory;
		}

		public AspectInstance Create( Type parameter )
		{
			var mappings = parameter.Adapt().GetMappedMethods( implementingType );

			/*if ( implementingType == typeof(IFactory<,>) )
			{
				foreach ( var methodMapping in mappings )
				{
					MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {methodMapping.InterfaceMethod} ({methodMapping.InterfaceMethod.DeclaringType.IsConstructedGenericType}) => {methodMapping.MappedMethod} ({methodMapping.MappedMethod.DeclaringType.IsConstructedGenericType})", null, null, null ));
				}	
				throw new InvalidOperationException( "SUUUUUUUUUUUUP!!!" );
			}*/

			var mapping = mappings.Introduce( methodName, pair => pair.Item1.InterfaceMethod.Name == pair.Item2 && ( pair.Item1.MappedMethod.IsFinal || pair.Item1.MappedMethod.IsVirtual ) && !pair.Item1.MappedMethod.IsAbstract ).SingleOrDefault();
			if ( mapping.IsAssigned() )
			{
				var method = mapping.MappedMethod.AccountForGenericDefinition();
				var result = FromMethod( method );
				return result;
			}
			return null;
		}

		AspectInstance FromMethod( MethodInfo method )
		{
			var repository = PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>();
			var instance = factory( method );
			var type = instance.Aspect != null ? instance.Aspect.GetType() : Type.GetType( instance.AspectConstruction.TypeName );
			var result = !repository.HasAspect( method, type ) ? instance : null;
			return result;
		}
	}

	public sealed class AspectInstanceFactory : ValidatedParameterizedSourceBase<Type, IEnumerable<AspectInstance>>
	{
		public static AspectInstanceFactory Instance { get; } = new AspectInstanceFactory();

		readonly ImmutableArray<Func<Type, AspectInstance>> factories;

		AspectInstanceFactory() : this( AutoValidation.DefaultProfiles ) {}

		public AspectInstanceFactory( ImmutableArray<IProfile> profiles ) : this( profiles.Select( profile => profile.InterfaceType ).ToImmutableArray(), profiles.ToArray().Concat().ToImmutableArray() ) {}

		AspectInstanceFactory( ImmutableArray<TypeAdapter> knownTypes, ImmutableArray<Func<Type, AspectInstance>> factories ) : this( new Specification( knownTypes ), factories ) {}

		AspectInstanceFactory( ISpecification<Type> specification, ImmutableArray<Func<Type, AspectInstance>> factories ) : base( specification )
		{
			this.factories = factories;
		}

		public override IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var factory in factories )
			{
				var instance = factory( parameter );
				if ( instance != null )
				{
					/*var method = instance.TargetElement as MethodBase;
					MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {method.DeclaringType} {FormatterFactory.Instance.From(instance.TargetElement)}: {instance.AspectTypeName}", null, null, null ));*/
					yield return instance;
				}
			}
		}

		sealed class Specification : SpecificationWithContextBase<Type, ImmutableArray<TypeAdapter>>
		{
			public Specification( ImmutableArray<TypeAdapter> context ) : base( context ) {}

			public override bool IsSatisfiedBy( Type parameter )
			{
				if ( !Context.IsAssignableFrom( parameter ) )
				{
					throw new InvalidOperationException( $"{parameter} does not implement any of the types defined in {GetType()}, which are: {string.Join( ",", Context.Select( t => t.Type.FullName ) )}" );
				}
				return true;
			}
		}
	}	
}
