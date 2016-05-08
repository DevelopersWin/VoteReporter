using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	class Validator
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
			public Check( MethodBase instance, string key ) : base( instance, key ) {}
		}
	}

	[AttributeUsage( AttributeTargets.Class )]
	public class ValidationAttribute : Attribute
	{
		public ValidationAttribute( bool enabled )
		{
			Enabled = enabled;
		}

		public bool Enabled { get; }
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public sealed class ValidateAttribute : MethodInterceptionAspect
	{
		readonly string validatingMethodName;

		public ValidateAttribute() {}

		public ValidateAttribute( string validatingMethodName )
		{
			this.validatingMethodName = validatingMethodName;
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

		class Enabled : AssociatedStore<object, bool>
		{
			public Enabled( object instance ) : this( instance.GetType().GetTypeInfo() ) {}

			Enabled( MemberInfo type ) : base( type, () => type.GetCustomAttribute<ValidationAttribute>().With( attribute => attribute.Enabled, () => true ) ) {}
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var applied = new Enabled( args.Instance ).Value;
			
			if ( !applied || Validate( args ) )
			{
				base.OnInvoke( args );
			}
			else
			{
				args.ApplyReturnValue();
			}
		}

		class ValidatingMethod : AssociatedStore<MethodBase, MethodBase>
		{
			public ValidatingMethod( MethodBase instance, string name = null ) : base( instance, () => new ValidatingMethodFactory( name ).Create( instance )  ) {}
		}

		bool Validate( MethodInterceptionArgs args )
		{
			var method = new ValidatingMethod( args.Method, validatingMethodName ).Value;
			var validator = new Validator( args.Instance, method, args.Arguments );
			var result = validator.IsValid();
			return result;
		}

		[OnMethodInvokeAdvice, MethodPointcut( nameof(GetValidationMethod) )]
		public void OnInvokeValidator( MethodInterceptionArgs args )
		{
			args.Proceed();

			var validator = new Validator( args.Instance, args.Method, args.Arguments );
			args.ReturnValue.As<bool>( validator.Mark );
		}

		IEnumerable<MethodBase> GetValidationMethod( MethodBase source )
		{
			yield return new ValidatingMethod( source, validatingMethodName ).Value;
		}
	}
}
