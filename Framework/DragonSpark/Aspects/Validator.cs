using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[PSerializable, ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public sealed class ValidatorForAttribute : MethodInterceptionAspect
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			base.OnInvoke( args );

			var validator = new Validator( args.Instance, args.Method, args.Arguments );
			args.ReturnValue.As<bool>( validator.Mark );
		}
	}

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

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public sealed class ValidatedByAttribute : MethodInterceptionAspect, IAspectProvider
	{
		readonly string validatingMethodName;

		public ValidatedByAttribute( string validatingMethodName )
		{
			this.validatingMethodName = validatingMethodName;
		}

		class ValidatingMethodFactory : FactoryBase<MethodBase, MethodBase>
		{
			readonly string name;

			public ValidatingMethodFactory( string name )
			{
				this.name = name;
			}

			protected override MethodBase CreateItem( MethodBase parameter )
			{
				var typeInfo = parameter.DeclaringType.GetTypeInfo();
				var types = parameter.GetParameters().Select( info => info.ParameterType ).ToArray();
				var result = parameter.DeclaringType.GetRuntimeMethod( name, types ) ?? FromInterface( parameter, typeInfo, types );
				return result;
			}

			MethodInfo FromInterface( MemberInfo parameter, TypeInfo typeInfo, Type[] types ) => 
				parameter.DeclaringType.Adapt()
						 .GetAllInterfaces()
						 .Select( typeInfo.GetRuntimeInterfaceMap )
						 .SelectMany( mapping => mapping.TargetMethods.Select( info => new { info, mapping.InterfaceType } ) )
						 .Where( item => item.info.ReturnType == typeof(bool) && new[] { name, $"{item.InterfaceType.FullName}.{name}" }.Contains( item.info.Name ) && types.SequenceEqual( item.info.GetParameters().Select( p => p.ParameterType ) ) )
						 .WithFirst( item => item.info );
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var method = new ValidatingMethodFactory( validatingMethodName ).Create( args.Method );
			var validator = new Validator( args.Instance, method, args.Arguments );
			if ( validator.IsValid() )
			{
				base.OnInvoke( args );
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) =>
			targetElement.AsTo<MethodBase, AspectInstance[]>( method =>
															  {
																  var target = new ValidatingMethodFactory( validatingMethodName ).Create( method );
																  // MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "0001", $"HELLO THERE: {target}", null, null, null ));
																  var items = new AspectInstance( target, new ValidatorForAttribute() ).ToItem();
																  return items;
															  } );
	}
}
