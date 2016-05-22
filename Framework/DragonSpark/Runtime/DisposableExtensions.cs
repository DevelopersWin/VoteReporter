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
	public class AssociatedDisposables : AttachedProperty<IDisposable, ICollection<IDisposable>>
	{
		public static AssociatedDisposables Instance { get; } = new AssociatedDisposables();

		AssociatedDisposables() : base( key => new System.Collections.ObjectModel.Collection<IDisposable>() ) {}
	}

	public class ConfigureAssociatedDisposables : AttachedValue<IDisposable, bool>
	{
		public static ConfigureAssociatedDisposables Instance { get; } = new ConfigureAssociatedDisposables();

		ConfigureAssociatedDisposables() : base( key => true ) {}
	}

	public static class DisposableExtensions
	{
		public static T Configured<T>( this T @this, bool on ) where T : IDisposable
		{
			ConfigureAssociatedDisposables.Instance.Set( @this, new Tuple<bool>( on ) );
			return @this;
		}

		public static T AssociateForDispose<T>( this T @this, IDisposable associated ) where T : IDisposable => @this.AssociateForDispose( new[] { associated } );

		public static T AssociateForDispose<T>( this T @this, params IDisposable[] associated ) where T : IDisposable
		{
			AssociatedDisposables.Instance.Get( @this ).AddRange( associated );
			return @this;
		}
	}

	public class DisposeAssociatedCommand : CommandBase<IDisposable>
	{
		public static DisposeAssociatedCommand Instance { get; } = new DisposeAssociatedCommand( AssociatedDisposables.Instance );

		readonly AssociatedDisposables property;

		DisposeAssociatedCommand( AssociatedDisposables property ) : base( new IsAttachedSpecification<IDisposable, ICollection<IDisposable>>( property ) )
		{
			this.property = property;
		}

		public override void Execute( IDisposable parameter ) => property.Get( parameter ).Purge().Each( disposable => disposable.Dispose() );
	}

	/*class Associated : AssociatedStore<IDisposable, ICollection<IDisposable>>
	{
		public Associated( IDisposable instance ) : base( instance, typeof(Associated), () =>  ) {}
	}*/

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

		public override void OnSuccess( MethodExecutionArgs args ) =>
			args.Instance.As<IDisposable>( disposable =>
										   {
											   if ( ConfigureAssociatedDisposables.Instance.Get( disposable ).Item1 )
											   {
												   DisposeAssociatedCommand.Instance.Run( disposable );
											   }
										   } );
	}
}