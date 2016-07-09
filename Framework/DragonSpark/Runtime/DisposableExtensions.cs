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
	public class AssociatedDisposables : ListCache<IDisposable, IDisposable>
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

		DisposeAssociatedCommand( AssociatedDisposables cache ) : base( new CacheContains<IDisposable, IList<IDisposable>>( cache ) )
		{
			this.cache = cache;
		}

		public override void Execute( IDisposable parameter ) => cache.Get( parameter ).Purge().Each( disposable => disposable.Dispose() );
	}

	
	[ProvideAspectRole( "Dispose Associated Disposables" ), LinesOfCodeAvoided( 1 )]
	[OnMethodBoundaryAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[MulticastAttributeUsage( MulticastTargets.Method, PersistMetaData = false, TargetMemberAttributes = MulticastAttributes.Instance ), AttributeUsage( AttributeTargets.Assembly ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class DisposeAssociatedAspect : OnMethodBoundaryAspect
	{
		readonly static Func<IDisposable, bool> Configure = ConfigureAssociatedDisposables.Instance.Get;
		readonly static Action<IDisposable> Command = DisposeAssociatedCommand.Instance.Execute;

		readonly Func<IDisposable, bool> property;
		readonly Action<IDisposable> command;

		public DisposeAssociatedAspect() : this( Configure, Command ) {}

		public DisposeAssociatedAspect( Func<IDisposable, bool> property, Action<IDisposable> command )
		{
			this.property = property;
			this.command = command;
		}

		public class Specification : SpecificationBase<MethodBase>
		{
			public static Specification Instance { get; } = new Specification();

			public override bool IsSatisfiedBy( MethodBase parameter ) => parameter.DeclaringType.GetTypeInfo().IsClass && !parameter.IsSpecialName &&
				parameter.DeclaringType.Adapt().GetMappedMethods<IDisposable>().Introduce( parameter ).Any( tuple => tuple.Item1.InterfaceMethod.Name == nameof(IDisposable.Dispose) && Equals( tuple.Item1.MappedMethod, tuple.Item2 ) );
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
		// public override bool CompileTimeValidate( MethodBase method ) => false;
	}
}