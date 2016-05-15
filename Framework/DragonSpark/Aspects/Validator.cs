using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
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
	public abstract class AspectAttributeBase : Attribute
	{
		protected AspectAttributeBase( bool enabled )
		{
			Enabled = enabled;
		}

		public bool Enabled { get; }
	}

	public class AutoValidationAttribute : AspectAttributeBase
	{
		public AutoValidationAttribute( bool enabled ) : base( enabled ) {}
	}

	public class ParameterValidator : IParameterValidator
	{
		public static ParameterValidator Instance { get; } = new ParameterValidator();
		ParameterValidator() {}

		// readonly ConditionMonitor monitor = new ConditionMonitor();

		public bool IsValid( IsValidParameter parameter )
		{
			var deferred = new Lazy<bool>( parameter.Result );
			var monitor = new Checked( parameter.Instance, parameter.Arguments ).Value;
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
		public IsValidParameter( object instance, object[] arguments, Func<bool> result ) : base( instance, arguments, result ) {}
	}

	public abstract class ValidateParameterBase<T>
	{
		protected ValidateParameterBase( object instance, object[] arguments, Func<T> result )
		{
			Instance = instance;
			Arguments = arguments;
			Result = result;
		}

		public object Instance { get; }
		public object[] Arguments { get; }

		public Func<T> Result { get; }
	}

	public class ValidateParameter : ValidateParameterBase<object>
	{
		public ValidateParameter( object instance, object[] arguments, Func<object> result ) : base( instance, arguments, result ) {}
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

		public object GetValidatedValue( ValidateParameter parameter )
		{
			var store = new Validated( parameter.Instance );
			var validated = store.Value;
			var result = validated || Validate( parameter ) ? AsValidated( store, parameter.Result ) : null;
			return result;
		}

		bool Validate( ValidateParameter parameter )
		{
			var type = parameter.Instance.GetType();
			var mapped = new MethodLocator( reference ).Create( type );
			var itemName = name ?? $"Can{mapped.Name.ToStringArray( '.' ).Last()}";
			var validator = new MethodLocator( mapped, itemName ).Create( type );
			var result = (bool)validator.Invoke( parameter.Instance, parameter.Arguments );
			return result;
		}

		static object AsValidated( Validated store, Func<object> factory )
		{
			using ( new AssignValidation( store ).AsExecuted( true ) )
			{
				return factory();
			}
		}

		[AutoValidation( false ), AssociatedDispose( false )]
		class AssignValidation : AssignValueCommand<bool>
		{
			public AssignValidation( IWritableStore<bool> store ) : base( store ) {}
		}

		[AssociatedDispose( false )]
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

	[AutoValidation( false )]
	class MethodLocator : FactoryBase<Type, MethodInfo>
	{
		readonly Type[] types;
		readonly MethodBase reference;
		readonly string name;

		public MethodLocator( MethodBase reference ) : this( reference, reference.Name ) {}

		public MethodLocator( MethodBase reference, string name ) : this( reference, name, GetParameterTypes( reference ) ) {}

		public MethodLocator( MethodBase reference, string name, Type[] types )
		{
			this.reference = reference;
			this.name = name;
			this.types = types;
		}

		public override MethodInfo Create( Type parameter )
		{
			var parameters = reference.ContainsGenericParameters ? ParameterTypeLocator.Instance.Create( parameter ).With( type => type.ToItem(), () => types ) : types;
			var result = parameter.GetRuntimeMethods().FirstOrDefault( info => info.Name == name && GetParameterTypes( info ).SequenceEqual( parameters ) )
						 ?? FromInterface( parameter, types );
			return result;
		}

		static Type[] GetParameterTypes( MethodBase @this ) => @this.GetParameters().Select( parameterInfo => parameterInfo.ParameterType ).ToArray();

		MethodInfo FromInterface( Type parameter, Type[] parameterTypes ) => 
			parameter.Adapt()
					 .GetAllInterfaces()
					 .Select( parameter.GetTypeInfo().GetRuntimeInterfaceMap )
					 .SelectMany( mapping => mapping.TargetMethods.Select( info => new { info, mapping.InterfaceType } ) )
					 .Where( item => /*item.info.ReturnType == reference &&*/ Contains( item.InterfaceType, item.info ) && GetParameterTypes( item.info ).SequenceEqual( parameterTypes ) )
					 .WithFirst( item => item.info );

		bool Contains( Type interfaceType, MemberInfo candidate ) => new[] { name, $"{interfaceType.FullName}.{name}" }.Contains( candidate.Name );
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class ValidatorAttribute : MethodInterceptionAspect
	{
		readonly IParameterValidator validator;

		public ValidatorAttribute() : this( ParameterValidator.Instance ) {}
		public ValidatorAttribute( IParameterValidator validator )
		{
			this.validator = validator;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var parameter = new IsValidParameter( args.Instance, args.Arguments.ToArray(), args.GetReturnValue<bool> );
			args.ReturnValue = validator.IsValid( parameter );
		}
	}

	public class AspectSupport
	{
		public static AspectSupport Instance { get; } = new AspectSupport();

		public bool IsEnabled<T>( Type type, bool defaultValue = true ) where T : AspectAttributeBase => type.GetTypeInfo().GetCustomAttribute<T>( true ).With( attribute => attribute.Enabled, () => defaultValue );
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class AutoValidateAttribute : MethodInterceptionAspect
	{
		readonly string validatingMethodName;

		public AutoValidateAttribute() {}

		public AutoValidateAttribute( [Optional]string validatingMethodName )
		{
			this.validatingMethodName = validatingMethodName;
		}

		public override bool CompileTimeValidate( MethodBase method ) => AspectSupport.Instance.IsEnabled<AutoValidationAttribute>( method.DeclaringType );

		IParameterValidation Validator { get; set; }

		public override void RuntimeInitialize( MethodBase method ) => Validator = new ParameterValidation( method, validatingMethodName );

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var enabled = AspectSupport.Instance.IsEnabled<AutoValidationAttribute>( args.Instance.GetType() );
			if ( enabled )
			{
				var parameter = new ValidateParameter( args.Instance, args.Arguments.ToArray(), args.GetReturnValue );
				var result = Validator.GetValidatedValue( parameter );
				args.ApplyReturnValue( result );
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}
}
