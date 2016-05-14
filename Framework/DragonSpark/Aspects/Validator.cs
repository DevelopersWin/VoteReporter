using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using Microsoft.Practices.Unity.Utility;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.Aspects
{
	/*class Validator
	{
		readonly object instance;
		readonly MethodBase method;
		readonly object[] arguments;
		readonly ConditionMonitor monitor;

		public Validator( object instance, MethodBase method, Arguments arguments ) : this( instance, method, arguments.ToArray(), new Check( method, KeyFactory.Instance.CreateUsing( method, instance, arguments ).ToString() ).Value ) {}

		Validator( object instance, MethodBase method, object[] arguments, ConditionMonitor monitor )
		{
			this.instance = instance;
			this.method = method;
			this.arguments = arguments;
			this.monitor = monitor;
		}

		public void Mark( bool result )
		{
			monitor.Reset();
			if ( result )
			{
				monitor.Apply();
			}
		}

		public bool IsValid()
		{
			var result = monitor.IsApplied || (bool)method.Invoke( instance, arguments );
			monitor.Reset();
			return result;
		}

		class Check : Checked
		{
			public Check( MethodBase source, string key ) : base( source, key ) {}
		}
	}*/

	[AttributeUsage( AttributeTargets.Class )]
	public class ValidationAttribute : Attribute
	{
		public ValidationAttribute( bool enabled )
		{
			Enabled = enabled;
		}

		public bool Enabled { get; }
	}

	public class ParameterValidator : IParameterValidator
	{
		public static ParameterValidator Instance { get; } = new ParameterValidator();
		ParameterValidator() {}

		// readonly ConditionMonitor monitor = new ConditionMonitor();

		public bool IsValid( IsValidParameter parameter )
		{
			var deferred = new Lazy<bool>( parameter.Result );
			var monitor = new Checked( parameter.Instance, parameter.Value ).Value;
			var result = monitor.IsApplied || deferred.Value;
			monitor.Reset();
			if ( deferred.IsValueCreated )
			{
				monitor.Apply();
			}
			return result;
		}
	}

	public class IsValidParameter : ValidateParameterBase<bool>
	{
		public IsValidParameter( object instance, object value, Func<bool> result ) : base( instance, value, result ) {}
	}

	public abstract class ValidateParameterBase<T>
	{
		protected ValidateParameterBase( object instance, object value, Func<T> result )
		{
			Instance = instance;
			Value = value;
			Result = result;
		}

		public object Instance { get; }
		public object Value { get; }

		public Func<T> Result { get; }
	}

	public class ValidateParameter : ValidateParameterBase<object>
	{
		public ValidateParameter( object instance, object value, Func<object> result ) : base( instance, value, result ) {}
	}

	public class ParameterValidation : IParameterValidation
	{
		readonly MethodBase reference;
		readonly string name;
		
		public ParameterValidation( MethodBase reference, string name )
		{
			this.reference = reference;
			this.name = name;
		}

		class Enabled : AssociatedStore<Type, bool>
		{
			public Enabled( Type source ) : base( source, typeof(Enabled), () => source.GetTypeInfo().GetCustomAttribute<ValidationAttribute>( true ).With( attribute => attribute.Enabled, () => true ) ) {}
		}

		public object GetValidatedValue( ValidateParameter parameter )
		{
			return new Enabled( parameter.Instance.GetType() ).Value ? DetermineValue( parameter ) : parameter.Result();
		}

		object DetermineValue( ValidateParameter parameter )
		{
			var store = new Validated( parameter.Instance );
			var result = store.Value || Validate( parameter ) ? AsValidated( store, parameter.Result ) : null;
			return result;
		}

		bool Validate( ValidateParameter parameter )
		{
			var type = parameter.Instance.GetType();
			var mapped = type.GetMethodHierarchical( reference.Name, Factory.GetParameterType( type ).ToItem() );
			var validator = new ValidatingMethodFactory( name ).Create( mapped );
			var result = (bool)validator.Invoke( parameter.Instance, new[] { parameter.Value } );
			return result;
		}

		static object AsValidated( Validated store, Func<object> factory )
		{
			using ( new AssignValueCommand<bool>( store ).AsExecuted( true ) )
			{
				return factory();
			}
		}

		class Validated : ThreadAmbientStore<bool>
		{
			public Validated( object instance ) : base( instance.GetHashCode().ToString() ) {}
		}
	}

	public interface IParameterValidator
	{
		bool IsValid( IsValidParameter parameter );
	}

	public interface IParameterValidation
	{
		object GetValidatedValue( ValidateParameter parameter );
	}

	[Validation( false )]
	class ValidatingMethodFactory : FactoryBase<MethodBase, MethodBase>
	{
		readonly string name;

		public ValidatingMethodFactory( string name = null )
		{
			this.name = name;
		}

		protected override MethodBase CreateItem( MethodBase parameter )
		{
			var typeInfo = parameter.DeclaringType.GetTypeInfo();
			var types = parameter.GetParameters().Select( info => info.ParameterType ).ToArray();
			var target = DetermineName( parameter.Name );
			var result = parameter.DeclaringType.GetRuntimeMethod( target, types ) ?? FromInterface( target, parameter, typeInfo, types );
			return result;
		}

		string DetermineName( string parameter ) => name ?? $"Can{parameter.ToStringArray( '.' ).Last()}";

		static MethodInfo FromInterface( string methodName, MemberInfo parameter, TypeInfo typeInfo, Type[] types ) => 
			parameter.DeclaringType.Adapt()
						.GetAllInterfaces()
						.Select( typeInfo.GetRuntimeInterfaceMap )
						.SelectMany( mapping => mapping.TargetMethods.Select( info => new { info, mapping.InterfaceType } ) )
						.Where( item => item.info.ReturnType == typeof(bool) && Contains( methodName, item.InterfaceType, item.info ) && types.SequenceEqual( item.info.GetParameters().Select( p => p.ParameterType ) ) )
						.WithFirst( item => item.info );

		static bool Contains( string methodName, Type interfaceType, MemberInfo candidate ) => new[] { methodName, $"{interfaceType.FullName}.{methodName}" }.Contains( candidate.Name );
	}

	/*public class MethodLocator : FactoryBase<string, MethodInfo>
	{
		protected override MethodInfo CreateItem( string parameter )
		{
			return null;
		}
	}*/

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class ValidatorAttribute : MethodInterceptionAspect //, IInstanceScopedAspect
	{
		// public static ValidatorAttribute Instance { get; } = new ValidatorAttribute();

		readonly IParameterValidator validator;

		public ValidatorAttribute() : this( ParameterValidator.Instance ) {}
		public ValidatorAttribute( IParameterValidator validator )
		{
			this.validator = validator;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var parameter = new IsValidParameter( args.Instance, args.Arguments.Single(), args.GetReturnValue<bool> );
			args.ReturnValue = validator.IsValid( parameter );
			//args.ApplyReturnValue( result );

			/*if ( !PostSharpEnvironment.IsPostSharpRunning )
			{
				
			}
			else
			{
				base.OnInvoke( args );
			}*/
		}

		/*object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => /*!PostSharpEnvironment.IsPostSharpRunning ?  : MemberwiseClone()#1#new ValidatorAttribute();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}*/
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class ValidateAttribute : MethodInterceptionAspect//, IInstanceScopedAspect
	{
		readonly string validatingMethodName;

		public ValidateAttribute() {}

		public ValidateAttribute( [Optional]string validatingMethodName )
		{
			this.validatingMethodName = validatingMethodName;
		}

		/*ValidateAttribute( IParameterValidation validator )
		{
			Validator = validator;
		}

		MethodBase Method { get; set; }*/

		IParameterValidation Validator { get; set; }

		public override void RuntimeInitialize( MethodBase method )
		{
			// Debugger.Break();
			Validator = /*!PostSharpEnvironment.IsPostSharpRunning ?*/ new ParameterValidation( method, validatingMethodName ) /*: null*/;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {args.Instance}", null, null, null ));
			var parameter = new ValidateParameter( args.Instance, args.Arguments.Single(), args.GetReturnValue );
			var result = Validator.GetValidatedValue( parameter );
			args.ApplyReturnValue( result );
			/*
			
			*/

			/*if ( !PostSharpEnvironment.IsPostSharpRunning )
			{
				
			}
			else
			{
				base.OnInvoke( args );
			}*/
		}

		/*[OnMethodInvokeAdvice, MethodPointcut( nameof(GetValidationMethod) )]
		public void OnInvokeValidator( MethodInterceptionArgs args )
		{
			var parameter = CreateParameter( args );
			args.ReturnValue = Validator.Validate( parameter );
		}

		IEnumerable<MethodBase> GetValidationMethod( MethodInfo source )
		{
			// source.DeclaringType.GetTypeInfo().
			yield return new ValidatingMethodFactory( validatingMethodName ).Create( source );
		}*/

		/*object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs )
		{
			var parameterValidation = !PostSharpEnvironment.IsPostSharpRunning ? new ParameterValidation( Method, validatingMethodName ) : Validator;
			return new ValidateAttribute( parameterValidation );
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}*/
	}
}
