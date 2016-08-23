using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict, PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : TypeLevelAspect, IAspectProvider
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = ToSourceDelegate();

		static Func<Type, IEnumerable<AspectInstance>> ToSourceDelegate()
		{
			try
{
				return AspectInstances.Default.ToSourceDelegate();
}
catch ( Exception e )
{
	MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {e}", null, null, null ));
	throw;
}
			
		}

		readonly Func<Type, IEnumerable<AspectInstance>> source;

		public ApplyAutoValidationAttribute() : this( DefaultSource ) {}

		protected ApplyAutoValidationAttribute( Func<Type, IEnumerable<AspectInstance>> source )
		{
			this.source = source;
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			var result = type != null ? source( type )/*.Fixed()*/ : Items<AspectInstance>.Default;
			/*foreach ( var aspectInstance in result )
			{
				var method = aspectInstance.TargetElement.To<MethodInfo>();
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {new MethodFormatter( method ).ToString()}: {aspectInstance.AspectTypeName}", null, null, null ) );
			}*/
			return result;
		}
	}

	/*sealed class AspectInstanceMethodFactory<T> : AspectInstanceFactoryBase where T : IAspect
	{
		public AspectInstanceMethodFactory( Type implementingType, string methodName ) : this( implementingType, methodName, Items<object>.Default ) {}
		public AspectInstanceMethodFactory( Type implementingType, string methodName, params object[] arguments ) : base( implementingType, methodName, Construct.New<T>( arguments ) ) {}
	}*/
}
