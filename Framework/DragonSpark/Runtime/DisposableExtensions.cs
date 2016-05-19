using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Runtime
{
	public static class DisposableExtensions
	{
		public static T AssociateForDispose<T>( this T @this, IDisposable associated ) where T : IDisposable => @this.AssociateForDispose( new[] { associated } );

		public static T AssociateForDispose<T>( this T @this, params IDisposable[] associated ) where T : IDisposable
		{
			new Associated( @this ).Value.AddRange( associated );
			return @this;
		}
	}

	public class DisposeAssociatedCommand : CommandBase<IDisposable>
	{
		public static DisposeAssociatedCommand Instance { get; } = new DisposeAssociatedCommand();

		public override void Execute( IDisposable parameter ) => new Associated( parameter ).Value.Purge().Each( disposable => disposable.Dispose() );
	}

	class Associated : AssociatedStore<IDisposable, ICollection<IDisposable>>
	{
		public Associated( IDisposable instance ) : base( instance, typeof(Associated), () => new Collection<IDisposable>() ) {}
	}

	/*public class AssociatedDisposeAttribute : AspectAttributeBase
	{
		public AssociatedDisposeAttribute( bool enabled ) : base( enabled ) {}
	}*/

	[ProvideAspectRole( "Dispose Associated Disposables" ), LinesOfCodeAvoided( 1 )]
	[PSerializable, MulticastAttributeUsage( MulticastTargets.Method, PersistMetaData = false, TargetMemberAttributes = MulticastAttributes.Instance ), AttributeUsage( AttributeTargets.Assembly ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class DisposeAssociatedAspect : OnMethodBoundaryAspect
	{
		public class Specification : SpecificationBase<MethodBase>
		{
			public static Specification Instance { get; } = new Specification();

			readonly TypeAdapter adapter = typeof(IDisposable).Adapt();

			public override bool IsSatisfiedBy( MethodBase parameter ) => 
				parameter.Name == nameof(IDisposable.Dispose) /*&& AspectSupport.IsEnabled<AssociatedDisposeAttribute>( parameter.DeclaringType )*/ && adapter.IsAssignableFrom( parameter.DeclaringType );
		}

		public override bool CompileTimeValidate( MethodBase method ) => Specification.Instance.IsSatisfiedBy( method );

		public override void OnSuccess( MethodExecutionArgs args )
		{
			// args.Instance.As<IDisposable>( DisposeAssociatedCommand.Instance.Run );
		}
	}
}