using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
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
		public Associated( IDisposable source ) : base( source, typeof(Associated), () => new Collection<IDisposable>() ) {}
	}

	[ProvideAspectRole( "Dispose Associated Disposables" ), LinesOfCodeAvoided( 1 )]
	[PSerializable, MulticastAttributeUsage( MulticastTargets.Method, PersistMetaData = false ), AttributeUsage( AttributeTargets.Assembly ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class DisposeAssociatedAspect : OnMethodBoundaryAspect
	{
		public override bool CompileTimeValidate( MethodBase method ) => typeof(IDisposable).Adapt().IsAssignableFrom( method.DeclaringType ) && method.Name == nameof(IDisposable.Dispose);

		public override void OnSuccess( MethodExecutionArgs args ) => args.Instance.As<IDisposable>( DisposeAssociatedCommand.Instance.Run );
	}
}