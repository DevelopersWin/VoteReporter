using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Runtime
{
	public class AssociatedDisposables : CollectionCache<IDisposable, IDisposable>
	{
		public static AssociatedDisposables Instance { get; } = new AssociatedDisposables();

		AssociatedDisposables() {}
	}

	public class ConfigureAssociatedDisposables : StoreCache<IDisposable, bool>
	{
		public static ConfigureAssociatedDisposables Instance { get; } = new ConfigureAssociatedDisposables();

		ConfigureAssociatedDisposables() : base( new Cache<IDisposable, IWritableStore<bool>>( disposable => new FixedStore<bool>( true ) ) ) {}
	}

	public static class DisposableExtensions
	{
		public static T Configured<T>( this T @this, bool on ) where T : IDisposable
		{
			ConfigureAssociatedDisposables.Instance.Set( @this, on );
			return @this;
		}

		public static T AssociateForDispose<T>( this T @this, IDisposable associated ) where T : IDisposable => @this.AssociateForDispose( associated.ToItem() );

		public static T AssociateForDispose<T>( this T @this, params IDisposable[] associated ) where T : IDisposable
		{
			AssociatedDisposables.Instance.Get( @this ).AddRange( associated );
			return @this;
		}
	}

	public class DisposeAssociatedCommand : CommandBase<IDisposable>
	{
		public static DisposeAssociatedCommand Instance { get; } = new DisposeAssociatedCommand( AssociatedDisposables.Instance );

		readonly AssociatedDisposables cache;

		DisposeAssociatedCommand( AssociatedDisposables cache ) : base( new IsAttachedSpecification<IDisposable, ICollection<IDisposable>>( cache ) )
		{
			this.cache = cache;
		}

		public override void Execute( IDisposable parameter ) => cache.Get( parameter ).Purge().Each( disposable => disposable.Dispose() );
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
	[OnMethodBoundaryAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[MulticastAttributeUsage( MulticastTargets.Method, PersistMetaData = false, TargetMemberAttributes = MulticastAttributes.Instance ), AttributeUsage( AttributeTargets.Assembly ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class DisposeAssociatedAspect : OnMethodBoundaryAspect
	{
		readonly Func<IDisposable, bool> property;
		readonly Action<IDisposable> command;

		public DisposeAssociatedAspect() : this( ConfigureAssociatedDisposables.Instance.Get, DisposeAssociatedCommand.Instance.Execute ) {}

		public DisposeAssociatedAspect( Func<IDisposable, bool> property, Action<IDisposable> command )
		{
			this.property = property;
			this.command = command;
		}

		public class Specification : SpecificationBase<MethodBase>
		{
			public static Specification Instance { get; } = new Specification();

			public override bool IsSatisfiedBy( MethodBase parameter ) => parameter.DeclaringType.GetTypeInfo().IsClass && !parameter.IsSpecialName &&
				GetMappedMethods( parameter ).Any( tuple => tuple.Item1.Name == nameof(IDisposable.Dispose) && Equals( tuple.Item2, parameter ) );

			[Freeze]
			static IEnumerable<Tuple<MethodInfo, MethodInfo>> GetMappedMethods( MemberInfo parameter ) => parameter.DeclaringType.Adapt().GetMappedMethods( typeof(IDisposable) );
		}

		public override bool CompileTimeValidate( MethodBase method ) => Specification.Instance.IsSatisfiedBy( method );

		public override void OnSuccess( MethodExecutionArgs args )
		{
			var disposable = args.Instance as IDisposable;
			if ( disposable != null )
			{
				if ( property( disposable ) )
				{
					command( disposable );
				}
			}
		}
	}
}