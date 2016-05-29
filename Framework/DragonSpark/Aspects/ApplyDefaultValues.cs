using DragonSpark.ComponentModel;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Serialization;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using DragonSpark.Runtime.Properties;

namespace DragonSpark.Aspects
{
	[LocationInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[MulticastAttributeUsage( MulticastTargets.Property, PersistMetaData = false )]
	[PSerializable, ProvideAspectRole( "Data" ), LinesOfCodeAvoided( 6 )]
	[AttributeUsage( AttributeTargets.Assembly )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading )]
	public sealed class ApplyDefaultValues : LocationInterceptionAspect
	{
		readonly static IAttachedProperty<object, ConditionalWeakTable<object, ConditionMonitor>> Property = new AttachedProperty<ConditionalWeakTable<object, ConditionMonitor>>( o => new ConditionalWeakTable<object, ConditionMonitor>() );

		readonly Func<PropertyInfo, bool> specification;
		readonly Func<DefaultValueParameter, object> source;
		
		public ApplyDefaultValues() : this( DefaultValuePropertySpecification.Instance.IsSatisfiedBy, DefaultPropertyValueFactory.Instance.Create ) {}

		ApplyDefaultValues( Func<PropertyInfo, bool> specification, Func<DefaultValueParameter, object> source )
		{
			this.specification = specification;
			this.source = source;
		}

		static bool Apply( object instance, PropertyInfo info ) => Property.Get( instance ).GetValue( info, key => new ConditionMonitor() ).Apply();

		public override bool CompileTimeValidate( LocationInfo locationInfo ) => specification( locationInfo.PropertyInfo );

		public override void OnGetValue( LocationInterceptionArgs args )
		{
			var instance = args.Instance ?? args.Location.PropertyInfo.DeclaringType;
			var apply = Apply( instance, args.Location.PropertyInfo );
			if ( apply )
			{
				var parameter = new DefaultValueParameter( instance, args.Location.PropertyInfo );
				args.SetNewValue( args.Value = source( parameter ) );
			}
			else
			{
				base.OnGetValue( args );
			}
		}

		public override void OnSetValue( LocationInterceptionArgs args )
		{
			Apply( args.Instance ?? args.Location.PropertyInfo.DeclaringType, args.Location.PropertyInfo );
			base.OnSetValue( args );
		}
	}
}