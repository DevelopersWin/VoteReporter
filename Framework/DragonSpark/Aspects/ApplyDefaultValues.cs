using DragonSpark.ComponentModel;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Serialization;
using System;

namespace DragonSpark.Aspects
{
	[MulticastAttributeUsage( MulticastTargets.Property, PersistMetaData = false ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	[PSerializable, ProvideAspectRole( "Default Object Values" ), LinesOfCodeAvoided( 6 )]
	[AttributeUsage( AttributeTargets.Assembly )]
	public sealed class ApplyDefaultValues : LocationInterceptionAspect, IInstanceScopedAspect
	{
		public override bool CompileTimeValidate( LocationInfo locationInfo ) => DefaultValuePropertySpecification.Instance.IsSatisfiedBy( locationInfo.PropertyInfo );

		public override void OnGetValue( LocationInterceptionArgs args )
		{
			lock ( this )
			{
				var apply = new Checked( this ).Item.Apply();
				if ( apply )
				{
					var parameter = new DefaultValueParameter( args.Instance ?? args.Location.DeclaringType, args.Location.PropertyInfo );
					var value = DefaultPropertyValueFactory.Instance.Create( parameter );
					args.SetNewValue( args.Value = value );
				}
				else
				{
					base.OnGetValue( args );
				}
			}
		}

		public override void OnSetValue( LocationInterceptionArgs args )
		{
			new Checked( this ).Item.Apply();
			base.OnSetValue( args );
		}

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => MemberwiseClone();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}

	/*[Serializable, LinesOfCodeAvoided( 8 )]
	public class AssignExecutionContextAspect : MethodInterceptionAspect
	{
		public static AssignExecutionContextAspect Instance { get; } = new AssignExecutionContextAspect();

		AssignExecutionContextAspect() {}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			using ( new ProfilerFactory( output, command.Provider.Get<ILoggerHistory>(), args.Method.Name )).Create() )
			{
				args.Proceed();
			}
		}
	}*/
}