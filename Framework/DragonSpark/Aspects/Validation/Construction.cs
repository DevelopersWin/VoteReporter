using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp;
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
using Message = PostSharp.Extensibility.Message;

namespace DragonSpark.Aspects.Validation
{
	[LinesOfCodeAvoided( 10 )]
	public class ApplyAutoValidationAttribute : Attribute, IAspectProvider
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

	class AspectFactory<T> : FactoryBase<object, T> where T : IAspect
	{
		public static AspectFactory<T> Instance { get; } = new AspectFactory<T>();

		readonly Func<object, IParameterValidationAdapter> adapterSource;
		readonly Func<IAutoValidationController, T> resultSource;

		public AspectFactory() : this( AutoValidation.DefaultAdapterSource, Delegates.From<IAutoValidationController, T>() ) {}

		public AspectFactory( Func<object, IParameterValidationAdapter> adapterSource, Func<IAutoValidationController, T> resultSource )
		{
			this.adapterSource = adapterSource;
			this.resultSource = resultSource;
		}

		public override T Create( object parameter )
		{
			var adapter = adapterSource( parameter );
			var controller = new AutoValidationController( adapter );
			var result = resultSource( controller );
			return result;
		}
	}


	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class AutoValidationAspectBase : MethodInterceptionAspect, IInstanceScopedAspect
	{
		// TODO: http://support.sharpcrafters.com/discussions/questions/1561-iaspectprovider-and-explicit-interface-methods
		readonly static Func<Type, Attribute> Applies = new Cache<Type, Attribute>( type => type.GetTypeInfo().GetCustomAttribute( typeof(ApplyAutoValidationAttribute), true ) ).ToDelegate();

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
		readonly static Func<object, Implementation> Factory = AspectFactory<Implementation>.Instance.ToDelegate();
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
				var valid = controller.IsValid( parameter );
				if ( !valid.HasValue )
				{
					controller.MarkValid( parameter, args.GetReturnValue<bool>() );
				}
				else
				{
					args.ReturnValue = valid.Value;
				}
			}
		}
	}

	public class AutoValidationExecuteAspect : AutoValidationAspectBase
	{
		readonly static Func<object, Implementation> Factory = AspectFactory<Implementation>.Instance.ToDelegate();
		public AutoValidationExecuteAspect() : base( Factory ) {}

		sealed class Implementation : AutoValidationExecuteAspect
		{
			readonly IAutoValidationController controller;
			public Implementation( IAutoValidationController controller )
			{
				this.controller = controller;
			}

			public override void OnInvoke( MethodInterceptionArgs args )
			{
				object result;
				switch ( controller.Execute( args.Arguments[0], out result ) )
				{
					case AutoValidationControllerResult.ResultFound:
						args.ReturnValue = result;
						break;
					case AutoValidationControllerResult.Proceed:
						args.Proceed();
						break;
				}
			}
		}
	}

/*public sealed class AutoValidationValidationAspect : AutoValidationAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = AutoValidation.Controller( args.Instance );
			if ( controller != null )
			{
				var parameter = args.Arguments[0];
				var valid = controller.IsValid( parameter );
				if ( !valid.HasValue )
				{
					controller.MarkValid( parameter, args.GetReturnValue<bool>() );
				}
				else
				{
					args.ReturnValue = valid.Value;
				}
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	public sealed class AutoValidationExecuteAspect : AutoValidationAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = AutoValidation.Controller( args.Instance );
			if ( controller != null )
			{
				args.ReturnValue = controller.Execute( args.Arguments[0] ) ? args.GetReturnValue() : args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}
*/

	public interface IProfile : IEnumerable<Func<Type, AspectInstance>>
	{
		TypeAdapter InterfaceType { get; }

		Func<object, IParameterValidationAdapter> CreateAdapter { get; }
	}

	class Profile : IProfile
	{
		readonly Func<Type, AspectInstance> validate;
		readonly Func<Type, AspectInstance> execute;
		protected Profile( Type interfaceType, string valid, string execute, IFactory<object, IParameterValidationAdapter> factory ) : this( interfaceType.Adapt(), 
			new AspectInstanceMethodFactory<AutoValidationValidationAspect>( interfaceType, valid ).ToDelegate(), 
			new AspectInstanceMethodFactory<AutoValidationExecuteAspect>( interfaceType, execute ).ToDelegate(), factory.ToDelegate() )
		{}

		protected Profile( TypeAdapter interfaceType, Func<Type, AspectInstance> validate, Func<Type, AspectInstance> execute, Func<object, IParameterValidationAdapter> createAdapter )
		{
			InterfaceType = interfaceType;
			CreateAdapter = createAdapter;
			this.validate = validate;
			this.execute = execute;
		}

		public TypeAdapter InterfaceType { get; }
		public Func<object, IParameterValidationAdapter> CreateAdapter { get; }

		public IEnumerator<Func<Type, AspectInstance>> GetEnumerator()
		{
			yield return validate;
			yield return execute;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class AspectInstanceMethodFactory<T> : AspectInstanceFactoryBase where T : IAspect
	{
		public AspectInstanceMethodFactory( Type implementingType, string methodName ) : this( implementingType, methodName, Items<object>.Default ) {}
		public AspectInstanceMethodFactory( Type implementingType, string methodName, params object[] arguments ) : base( implementingType, methodName, Construct.New<T>( arguments ) ) {}
	}

	static class Construct
	{
		public static Func<MethodInfo, AspectInstance> New<T>( params object[] arguments ) => new ConstructAspectInstanceFactory( new ObjectConstruction( typeof(T), arguments ) ).ToDelegate();
	}

	class ConstructAspectInstanceFactory : FactoryBase<MethodInfo, AspectInstance>
	{
		readonly ObjectConstruction construction;

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
			var mapping = mappings.ToArray().Introduce( methodName, pair => pair.Item1.InterfaceMethod.Name == pair.Item2 && ( pair.Item1.MappedMethod.IsFinal || pair.Item1.MappedMethod.IsVirtual ) && !pair.Item1.MappedMethod.IsAbstract ).SingleOrDefault();
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

	public class AspectInstanceFactory : FactoryBase<Type, IEnumerable<AspectInstance>>
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

		public override IEnumerable<AspectInstance> Create( Type parameter )
		{
			foreach ( var factory in factories )
			{
				var instance = factory( parameter );
				if ( instance != null )
				{
					MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"YO: {FormatterFactory.Instance.From(instance.TargetElement)}: {instance.AspectTypeName}", null, null, null ));
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
