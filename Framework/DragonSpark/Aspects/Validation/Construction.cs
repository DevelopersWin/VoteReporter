using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
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
using Delegates = DragonSpark.Runtime.Delegates;

namespace DragonSpark.Aspects.Validation
{
	[LinesOfCodeAvoided( 10 )]
	public class ApplyAutoValidationAttribute : Attribute, IAspectProvider
	{
		readonly Func<Type, IEnumerable<AspectInstance>> provider;

		public ApplyAutoValidationAttribute() : this( DefaultAspectInstanceFactory.Instance ) {}

		protected ApplyAutoValidationAttribute( Func<Type, IEnumerable<AspectInstance>> provider )
		{
			this.provider = provider;
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			var result = type != null ? provider( type ) : Items<AspectInstance>.Default;
			return result;
		}
	}

	class DefaultAspectInstanceFactory : AspectInstanceFactory
	{
		public static Func<Type, IEnumerable<AspectInstance>> Instance { get; } = new DefaultAspectInstanceFactory().WithAutoValidation().ToDelegate();
		DefaultAspectInstanceFactory() : base( AutoValidation.DefaultProfiles ) {}
	}


	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class AutoValidationAspectBase : MethodInterceptionAspect, IInstanceScopedAspect
	{
		protected MethodInfo Method { get; private set; }

		public override void RuntimeInitialize( MethodBase method ) => Method = (MethodInfo)method;

		/*readonly IAspectCommand command;

		protected AutoValidationAspectBase() : this( null ) {}

		protected AutoValidationAspectBase( IAspectCommand command = null )
		{
			this.command = command;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( command != null )
			{
				args.ReturnValue = command.Validate( args.Arguments[0], args.GetReturnValue<bool> );
			}
			else
			{
				base.OnInvoke( args );
			}
		}*/

		public abstract object CreateInstance( AdviceArgs adviceArgs );

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}

	public sealed class AutoValidationValidationAspect : AutoValidationAspectBase
	{
		readonly IAspectCommand<bool> command;
		public AutoValidationValidationAspect() {}
		AutoValidationValidationAspect( IAspectCommand<bool> command )
		{
			this.command = command;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( command != null )
			{
				args.ReturnValue = command.Execute( args.Arguments[0], args.GetReturnValue<bool> );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		public override object CreateInstance( AdviceArgs adviceArgs )
		{
			var @delegate = Delegates.Default.Get( adviceArgs.Instance ).Get( Method );
			var result = new AutoValidationValidationAspect( AutoValidationCommandFactory.Instance( @delegate ) );
			return result;
		}
	}

	public sealed class AutoValidationExecuteAspect : AutoValidationAspectBase
	{
		readonly IAspectCommand<object> command;
		public AutoValidationExecuteAspect() {}
		AutoValidationExecuteAspect( IAspectCommand<object> command )
		{
			this.command = command;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( command != null )
			{
				args.ReturnValue = command.Execute( args.Arguments[0], args.GetReturnValue<object> );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		public override object CreateInstance( AdviceArgs adviceArgs )
		{
			var @delegate = Delegates.Default.Get( adviceArgs.Instance ).Get( Method );
			var result = new AutoValidationExecuteAspect( AutoValidationExecutionCommandFactory.Instance( @delegate ) );
			return result;
		}
	}

	public interface IProfile : IEnumerable<Func<Type, AspectInstance>>
	{
		TypeAdapter InterfaceType { get; }

		Func<object, IParameterValidationAdapter> AdapterFactory { get; }
	}

	class Profile : IProfile
	{
		readonly Func<Type, AspectInstance> validate;
		readonly Func<Type, AspectInstance> execute;
		protected Profile( Type interfaceType, string valid, string execute, IFactory<object, IParameterValidationAdapter> factory ) : this( interfaceType.Adapt(), 
			new AspectInstanceMethodFactory<AutoValidationValidationAspect>( interfaceType, valid ).ToDelegate(), 
			new AspectInstanceMethodFactory<AutoValidationExecuteAspect>( interfaceType, execute ).ToDelegate(), factory.ToDelegate() )
		{}

		protected Profile( TypeAdapter interfaceType, Func<Type, AspectInstance> validate, Func<Type, AspectInstance> execute, Func<object, IParameterValidationAdapter> adapterFactory )
		{
			InterfaceType = interfaceType;
			AdapterFactory = adapterFactory;
			this.validate = validate;
			this.execute = execute;
		}

		public TypeAdapter InterfaceType { get; }
		public Func<object, IParameterValidationAdapter> AdapterFactory { get; }

		public IEnumerator<Func<Type, AspectInstance>> GetEnumerator()
		{
			yield return validate;
			yield return execute;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/*class GenericProfile<T> : Profile
	{
		protected GenericProfile( Type interfaceType, string valid, string execute, IFactory<object, IParameterValidationAdapter> adapterFactory )
			: base( interfaceType.Adapt(),
					new GenericAspectInstanceMethodFactory<GenericAutoValidationValidationAspect, ValidationRegisterProvider<T>>( interfaceType, valid ).ToDelegate(),
					new GenericAspectInstanceMethodFactory<GenericAutoValidationExecuteAspect, ExecutionRegisterProvider<T>>( interfaceType, execute ).ToDelegate(),
					adapterFactory.ToDelegate()
				) {}
	}*/

	class AspectInstanceMethodFactory<T> : AspectInstanceFactoryBase where T : IAspect
	{
		public AspectInstanceMethodFactory( Type implementingType, string methodName ) : this( implementingType, methodName, Items<object>.Default ) {}
		public AspectInstanceMethodFactory( Type implementingType, string methodName, params object[] arguments ) : base( implementingType, methodName, Construct.New<T>( arguments ) ) {}
	}

	/*class AspectInstanceMethodFactory : AspectInstanceFactoryBase
	{
		public AspectInstanceMethodFactory( Type implementingType, string methodName, IAspect aspect ) : base( implementingType, methodName, new AspectInstanceFromAspectFactory( aspect ).Create ) {}
	}*/

	/*class GenericAspectInstanceMethodFactory<TAspect, TRegistry> : AspectInstanceFactoryBase where TAspect : IAspect where TRegistry : RegisterProvider
	{
		public GenericAspectInstanceMethodFactory( Type implementingType, string methodName ) : base( implementingType, methodName, Construct.Instance<TAspect>( implementingType, typeof(TRegistry) ) ) {}
	}*/

	static class Construct
	{
		public static Func<MethodInfo, AspectInstance> New<T>( params object[] arguments ) => new ConstructAspectInstanceFactory( new ObjectConstruction( typeof(T), arguments ) ).ToDelegate();
	}

/*	class AspectInstanceFromAspectFactory : FactoryBase<MethodInfo, AspectInstance>
	{
		readonly IAspect aspect;

		public AspectInstanceFromAspectFactory( IAspect aspect )
		{
			this.aspect = aspect;
		}

		public override AspectInstance Create( MethodInfo parameter ) => new AspectInstance( parameter, aspect, null );
	}*/

	class ConstructAspectInstanceFactory : FactoryBase<MethodInfo, AspectInstance>
	{
		readonly ObjectConstruction construction;

		// public ConstructAspectInstanceFactory( params object[] arguments ) : this( Construct.New<T>( arguments ) ) {}

		public ConstructAspectInstanceFactory( ObjectConstruction construction )
		{
			this.construction = construction;
		}

		public override AspectInstance Create( MethodInfo parameter ) => new AspectInstance( parameter, construction, null );
	}

	abstract class AspectInstanceFactoryBase : FactoryBase<Type, AspectInstance>
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

		public override AspectInstance Create( Type parameter )
		{
			var mappings = parameter.Adapt().GetMappedMethods( implementingType );
			var mapping = mappings.Introduce( methodName, pair => pair.Item1.Item1.Name == pair.Item2 && ( pair.Item1.Item2.IsFinal || pair.Item1.Item2.IsVirtual ) && !pair.Item1.Item2.IsAbstract ).SingleOrDefault();
			if ( mapping.IsAssigned() )
			{
				var method = mapping.Item2.AccountForGenericDefinition();
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

	public class AspectInstanceFactory : FactoryBase<Type, IEnumerable<AspectInstance>>
	{
		readonly ImmutableArray<Func<Type, AspectInstance>> factories;

		public AspectInstanceFactory( ImmutableArray<IProfile> profiles ) : this( profiles.Select( profile => profile.InterfaceType ).ToImmutableArray(), profiles.ToArray().Concat().ToImmutableArray() ) {}

		AspectInstanceFactory( ImmutableArray<TypeAdapter> knownTypes, ImmutableArray<Func<Type, AspectInstance>> factories ) : this( new Specification( knownTypes ), factories ) {}

		AspectInstanceFactory( ISpecification<Type> specification, ImmutableArray<Func<Type, AspectInstance>> factories ) : base( specification )
		{
			this.factories = factories;
		}

		public override IEnumerable<AspectInstance> Create( Type parameter )
		{
			foreach ( var factory in factories )
			{
				var instance = factory( parameter );
				if ( instance != null )
				{
					// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"YO: {FormatterFactory.Instance.From(instance.TargetElement)}: {instance.AspectTypeName}", null, null, null ));
					yield return instance;
				}
			}
		}

		class Specification : SpecificationWithContextBase<Type, ImmutableArray<TypeAdapter>>
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
