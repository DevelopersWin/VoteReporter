using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Runtime
{
	public sealed class AssociatedDisposables : ListCache<IDisposable, IDisposable>
	{
		public static AssociatedDisposables Instance { get; } = new AssociatedDisposables();
		AssociatedDisposables() {}
	}

	public sealed class ConfigureAssociatedDisposables : StoreCache<IDisposable, bool>
	{
		public static ConfigureAssociatedDisposables Instance { get; } = new ConfigureAssociatedDisposables();
		ConfigureAssociatedDisposables() {}
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
			@this.Configured( true );
			AssociatedDisposables.Instance.Get( @this ).AddRange( associated );
			return @this;
		}
	}

	public sealed class DisposeDisposableCommand : DisposingCommand<object>
	{
		readonly IDisposable disposable;
		public DisposeDisposableCommand( IDisposable disposable )
		{
			this.disposable = disposable;
		}

		public override void Execute( object parameter ) {}

		protected override void OnDispose() => disposable.Dispose();
	}

	public sealed class DisposeAssociatedCommand : CommandBase<IDisposable>
	{
		public static DisposeAssociatedCommand Instance { get; } = new DisposeAssociatedCommand( AssociatedDisposables.Instance );

		readonly AssociatedDisposables cache;

		DisposeAssociatedCommand( AssociatedDisposables cache ) : base( new CacheContains<IDisposable, IList<IDisposable>>( cache ) )
		{
			this.cache = cache;
		}

		public override void Execute( IDisposable parameter ) => cache.Get( parameter ).Purge().Each( disposable => disposable.Dispose() );
	}

	
	[OnMethodBoundaryAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( "Dispose Associated Disposables" ), LinesOfCodeAvoided( 1 )]
	[MulticastAttributeUsage( MulticastTargets.Method, TargetMemberAttributes = MulticastAttributes.Instance | MulticastAttributes.NonAbstract ), AttributeUsage( AttributeTargets.Assembly ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class DisposeAssociatedAspect : OnMethodBoundaryAspect
	{
		readonly static Func<IDisposable, bool> Configure = ConfigureAssociatedDisposables.Instance.Get;
		readonly static Action<IDisposable> Command = DisposeAssociatedCommand.Instance.Execute;
		readonly static Func<MethodBase, bool> Compile = Specification.Instance.IsSatisfiedBy;

		readonly Func<IDisposable, bool> configured;
		readonly Action<IDisposable> command;

		public DisposeAssociatedAspect() : this( Configure, Command ) {}

		DisposeAssociatedAspect( Func<IDisposable, bool> configured, Action<IDisposable> command )
		{
			this.configured = configured;
			this.command = command;
		}

		public class Specification : SpecificationBase<MethodBase>
		{
			public static Specification Instance { get; } = new Specification();

			public override bool IsSatisfiedBy( MethodBase parameter )
			{
				var adapter = parameter.DeclaringType.Adapt();
				return !parameter.IsSpecialName && adapter.Info.IsClass &&
					   adapter.GetMappedMethods<IDisposable>().Introduce( parameter ).Any( tuple => tuple.Item1.InterfaceMethod.Name == nameof(IDisposable.Dispose) && Equals( tuple.Item1.MappedMethod, tuple.Item2 ) );
			}
		}

		public override bool CompileTimeValidate( MethodBase method ) => Compile( method );

		public override void OnSuccess( MethodExecutionArgs args )
		{
			var disposable = args.Instance as IDisposable;
			if ( disposable != null && configured( disposable ) )
			{
				command( disposable );
			}
		}
	}
}