using System;

namespace DragonSpark.Runtime
{
	/*public sealed class AssociatedDisposables : ListCache<IDisposable, IDisposable>
	{
		public static AssociatedDisposables Default { get; } = new AssociatedDisposables();
		AssociatedDisposables() {}
	}

	public sealed class ConfigureAssociatedDisposables : DecoratedSourceCache<IDisposable, bool>
	{
		public static ConfigureAssociatedDisposables Default { get; } = new ConfigureAssociatedDisposables();
		ConfigureAssociatedDisposables() {}
	}*/

	/*public static class DisposableExtensions
	{
		public static T Configured<T>( this T @this, bool on ) where T : IDisposable
		{
			ConfigureAssociatedDisposables.Default.Set( @this, on );
			return @this;
		}

		public static T AssociateForDispose<T>( this T @this, IDisposable associated ) where T : IDisposable => @this.AssociateForDispose( associated.ToItem() );

		public static T AssociateForDispose<T>( this T @this, params IDisposable[] associated ) where T : IDisposable
		{
			@this.Configured( true );
			AssociatedDisposables.Default.Get( @this ).AddRange( associated );
			return @this;
		}
	}*/

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

	/*public sealed class DisposeAssociatedCommand : CommandBase<IDisposable>
	{
		public static DisposeAssociatedCommand Default { get; } = new DisposeAssociatedCommand( AssociatedDisposables.Default );

		readonly AssociatedDisposables cache;

		DisposeAssociatedCommand( AssociatedDisposables cache ) : base( new CacheContains<IDisposable, IList<IDisposable>>( cache ) )
		{
			this.cache = cache;
		}

		public override void Execute( IDisposable parameter ) => cache.Get( parameter ).Purge().Each( disposable => disposable.Dispose() );
	}

	
	[OnMethodBoundaryAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( "Dispose Associated Disposables" ), LinesOfCodeAvoided( 1 )]
	[MulticastAttributeUsage( MulticastTargets.Method, TargetMemberAttributes = MulticastAttributes.Default | MulticastAttributes.NonAbstract ), AttributeUsage( AttributeTargets.Assembly ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class DisposeAssociatedAspect : OnMethodBoundaryAspect
	{
		readonly static Func<IDisposable, bool> Configure = ConfigureAssociatedDisposables.Default.Get;
		readonly static Action<IDisposable> Command = DisposeAssociatedCommand.Default.Execute;
		readonly static Func<MethodBase, bool> Compile = Specification.Default.IsSatisfiedBy;

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
			public static Specification Default { get; } = new Specification();

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
	}*/
}