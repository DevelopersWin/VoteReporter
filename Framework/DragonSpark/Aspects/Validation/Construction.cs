﻿using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict, PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : TypeLevelAspect, IAspectProvider
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = AspectInstances.Instance.ToSourceDelegate();

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

	class AutoValidationControllerFactory : ParameterizedSourceBase<IAutoValidationController>
	{
		public static IParameterizedSource<IAutoValidationController> Instance { get; } = new AutoValidationControllerFactory().ToCache();
		AutoValidationControllerFactory() : this( AdapterLocator.Instance.Get ) {}

		readonly Func<object, IParameterValidationAdapter> adapterSource;
		readonly Action<object, IAspectHub> set;

		protected AutoValidationControllerFactory( Func<object, IParameterValidationAdapter> adapterSource ) : this( adapterSource, AspectHub.Instance.Set ) {}

		protected AutoValidationControllerFactory( Func<object, IParameterValidationAdapter> adapterSource, Action<object, IAspectHub> set )
		{
			this.adapterSource = adapterSource;
			this.set = set;
		}

		public override IAutoValidationController Get( object parameter )
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

		public static ImmutableArray<IAspectProfile> AspectProfiles { get; } = 
			new IAspectProfile[]
			{
				new AspectProfile( typeof(IValidatedParameterizedSource<,>), nameof(IParameterizedSource.Get) ),
				new AspectProfile( typeof(IValidatedParameterizedSource), nameof(IParameterizedSource.Get) ),
				new AspectProfile( typeof(ICommand<>), nameof(ICommand.Execute) ),
				new AspectProfile( typeof(ICommand), nameof(ICommand.Execute) )
			}.ToImmutableArray();
	}

	class AspectFactory<T> : ParameterizedSourceBase<T> where T : class, IAspect
	{
		readonly Func<object, IAutoValidationController> controllerSource;
		readonly Func<IAutoValidationController, T> resultSource;

		public AspectFactory( Func<IAutoValidationController, T> resultSource ) : this( Defaults.ControllerSource, resultSource ) {}

		public AspectFactory( Func<object, IAutoValidationController> controllerSource, Func<IAutoValidationController, T> resultSource )
		{
			this.controllerSource = controllerSource;
			this.resultSource = resultSource;
		}

		public override T Get( object parameter )
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
		readonly static Func<object, Implementation> Factory = new AspectFactory<Implementation>( controller => new Implementation( controller ) ).Get;
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
		readonly static Func<object, Implementation> Factory = new AspectFactory<Implementation>( controller => new Implementation( controller ) ).Get;

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

	public interface IAdapterSource : IParameterizedSource<IParameterValidationAdapter>, ISpecification<Type> {}

	class AdapterSource : DelegatedParameterizedSource<object, IParameterValidationAdapter>, IAdapterSource
	{
		readonly TypeAdapter adapter;

		public AdapterSource( Type declaringType, Func<object, IParameterValidationAdapter> source ) : this( declaringType.Adapt(), source ) {}

		public AdapterSource( TypeAdapter adapter, Func<object, IParameterValidationAdapter> source ) : base( source )
		{
			this.adapter = adapter;
		}

		public bool IsSatisfiedBy( Type parameter ) => adapter.IsAssignableFrom( parameter );

		bool ISpecification.IsSatisfiedBy( object parameter ) => parameter is Type && IsSatisfiedBy( (Type)parameter );
	}

	public interface IAspectProfile : IParameterizedSource<Type, MethodInfo>
	{
		Type DeclaringType { get; }
		/*string MethodName { get; }*/
	}

	class AspectProfile : ParameterizedSourceBase<Type, MethodInfo>, IAspectProfile
	{
		readonly string methodName;

		public AspectProfile( Type declaringType, string methodName )
		{
			DeclaringType = declaringType;
			this.methodName = methodName;
		}

		public Type DeclaringType { get; }

		/*readonly Func<Type, AspectInstance> execute;

		// protected ProfileBase( Type interfaceType, string valid, string execute, Func<object, IParameterValidationAdapter> factory ) : this( interfaceType, valid, interfaceType, execute, factory ) {}

		protected ProfileBase( Type interfaceType, Type executionType, string execute, Func<object, IParameterValidationAdapter> adapterSource ) : this( interfaceType.Adapt(), 
			// new AspectInstanceMethodFactory<AutoValidationValidationAspect>( validationType, valid ).Get, 
			new AspectInstanceMethodFactory<AutoValidationExecuteAspect>( executionType, execute ).Get, adapterSource )
		{}

		protected ProfileBase( TypeAdapter interfaceType, Func<Type, AspectInstance> execute, Func<object, IParameterValidationAdapter> adapterSource )
		{
			InterfaceType = interfaceType;
			AdapterSource = adapterSource;
			this.execute = execute;
		}

		public TypeAdapter InterfaceType { get; }
		public Func<object, IParameterValidationAdapter> AdapterSource { get; }*/

		public override MethodInfo Get( Type parameter )
		{
			var mappings = parameter.Adapt().GetMappedMethods( DeclaringType );
			var mapping = mappings.Introduce( methodName, pair => pair.Item1.InterfaceMethod.Name == pair.Item2 && ( pair.Item1.MappedMethod.IsFinal || pair.Item1.MappedMethod.IsVirtual ) && !pair.Item1.MappedMethod.IsAbstract ).SingleOrDefault();
			var result = mapping.IsAssigned() ? mapping.MappedMethod : null;
			return result;
		}
	}

	sealed class AspectInstanceConstructor<T> : ParameterizedSourceBase<MethodInfo, AspectInstance>
	{
		public static AspectInstanceConstructor<T> Instance { get; } = new AspectInstanceConstructor<T>();
		AspectInstanceConstructor() : this( new ObjectConstruction( typeof(T), Items<object>.Default ) ) {}

		readonly ObjectConstruction construction;

		public AspectInstanceConstructor( ObjectConstruction construction )
		{
			this.construction = construction;
		}

		public override AspectInstance Get( MethodInfo parameter ) => new AspectInstance( parameter, construction, null );
	}

	/*sealed class AspectInstanceMethodFactory<T> : AspectInstanceFactoryBase where T : IAspect
	{
		public AspectInstanceMethodFactory( Type implementingType, string methodName ) : this( implementingType, methodName, Items<object>.Default ) {}
		public AspectInstanceMethodFactory( Type implementingType, string methodName, params object[] arguments ) : base( implementingType, methodName, Construct.New<T>( arguments ) ) {}
	}*/

	public sealed class AspectInstanceFactory<T> : ParameterizedSourceBase<MethodInfo, AspectInstance>
	{
		public static AspectInstanceFactory<T> Instance { get; } = new AspectInstanceFactory<T>();
		AspectInstanceFactory() : this( AspectInstanceConstructor<T>.Instance.Get ) {}

		readonly Func<MethodInfo, AspectInstance> constructorSource;

		public AspectInstanceFactory( Func<MethodInfo, AspectInstance> constructorSource )
		{
			this.constructorSource = constructorSource;
		}

		public override AspectInstance Get( MethodInfo parameter )
		{
			var method = parameter.AccountForGenericDefinition();
			var repository = PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>();
			var instance = constructorSource( method );
			var type = instance.Aspect?.GetType() ?? Type.GetType( instance.AspectConstruction.TypeName );
			var result = !repository.HasAspect( method, type ) ? instance : null;
			return result;
		}
	}

	public sealed class ValidationMethodLocator : TransformerBase<MethodInfo>
	{
		public static ValidationMethodLocator Instance { get; } = new ValidationMethodLocator();
		ValidationMethodLocator() {}

		public override MethodInfo Get( MethodInfo parameter )
		{
			return null;
		}
	}

	/*public struct AspectInstanceParameter
	{
		public AspectInstanceParameter( IAspectProfile profile, Type candidate )
		{
			Profile = profile;
			Candidate = candidate;
		}

		public IAspectProfile Profile { get; }
		public Type Candidate { get; }
	}*/

	public sealed class AspectInstances : ValidatedParameterizedSourceBase<Type, IEnumerable<AspectInstance>>
	{
		public static AspectInstances Instance { get; } = new AspectInstances();
		AspectInstances() : this( Defaults.AspectProfiles ) {}

		readonly ImmutableArray<IAspectProfile> profiles;
		readonly Func<MethodInfo, MethodInfo> specificationSource;
		readonly Func<MethodInfo, AspectInstance> validatorSource;
		readonly Func<MethodInfo, AspectInstance> executionSource;

		public AspectInstances( ImmutableArray<IAspectProfile> profiles ) : this( profiles, ValidationMethodLocator.Instance.Get, AspectInstanceFactory<AutoValidationValidationAspect>.Instance.Get, AspectInstanceFactory<AutoValidationExecuteAspect>.Instance.Get ) {}

		public AspectInstances( ImmutableArray<IAspectProfile> profiles, Func<MethodInfo, MethodInfo> specificationSource, Func<MethodInfo, AspectInstance> validatorSource, Func<MethodInfo, AspectInstance> executionSource ) : base( new Specification( profiles.Select( profile => profile.DeclaringType.Adapt() ).ToImmutableArray() ) )
		{
			this.profiles = profiles;
			this.specificationSource = specificationSource;
			this.validatorSource = validatorSource;
			this.executionSource = executionSource;
		}

		public override IEnumerable<AspectInstance> Get( Type parameter )
		{
			// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {method.DeclaringType} {FormatterFactory.Instance.From(instance.TargetElement)}: {instance.AspectTypeName}", null, null, null ));
			foreach ( var profile in profiles )
			{
				var method = profile.Get( parameter );
				if ( method != null )
				{
					var validator = specificationSource( method );
					if ( validator != null )
					{
						yield return validatorSource( validator );
						yield return executionSource( method );
					}
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
