using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using SerilogTimings.Extensions;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DragonSpark.Runtime
{
	public interface ICommand<in TParameter> : ISpecification<TParameter>, ICommand
	{
		void Execute( TParameter parameter );

		void Update();
	}

	public class AssignCommand<T> : DisposingCommand<T>
	{
		readonly IAssignable<T> assignable;
		readonly T current;

		public AssignCommand( IAssignableSource<T> store ) : this( store, store ) {}

		public AssignCommand( IAssignable<T> assignable, ISource<T> store ) : this( assignable, store.Get() ) {}

		public AssignCommand( IAssignable<T> assignable, [Optional]T current )
		{
			this.assignable = assignable;
			this.current = current;
		}

		public override void Execute( T parameter ) => assignable.Assign( parameter );

		protected override void OnDispose()
		{
			assignable.TryDispose();
			assignable.Assign( current );
			base.OnDispose();
		}
	}

	/*public class FixedCommand : FixedCommand<object>
	{
		public FixedCommand( ICommand<object> command, object parameter ) : base( command, parameter ) {}
	}*/


	public class FixedCommand<T> : DisposingCommand<object>
	{
		readonly ICommand<T> command;
		readonly T parameter;

		public FixedCommand( ICommand<T> command, [Optional]T parameter ) : this( command, Specifications.Specifications.Always, parameter ) {}

		public FixedCommand( ICommand<T> command, ISpecification<object> specification, [Optional]T parameter ) : base( specification )
		{
			this.command = command;
			this.parameter = parameter;
		}

		public override void Execute( [Optional]object _ ) => command.Execute( parameter );

		protected override void OnDispose() => command.TryDispose();
	}

	/*public class AddItemCommand<T> : CommandBase<T>
	{
		readonly IList<T> list;

		public AddItemCommand( IList<T> list )
		{
			this.list = list;
		}

		public override void Execute( T parameter ) => list.Add( parameter );
	}

	public class AddItemCommand : CommandBase<object>
	{
		readonly IList list;

		public AddItemCommand( IList list )
		{
			this.list = list;
		}

		public override void Execute( object parameter ) => list.Add( parameter );
	}

	public class RemoveItemCommand : CommandBase<object>
	{
		readonly IList list;

		public RemoveItemCommand( IList list )
		{
			this.list = list;
		}

		public override void Execute( object parameter ) => list.Remove( parameter );
	}*/

	public abstract class DisposingCommand<T> : CommandBase<T>, IDisposable
	{
		readonly Action onDispose;

		protected DisposingCommand() : this( Specifications<T>.Assigned ) {}

		protected DisposingCommand( ISpecification<T> specification ) : base( specification )
		{
			onDispose = OnDispose;
		}

		~DisposingCommand()
		{
			Dispose( false );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( onDispose );

		protected virtual void OnDispose() {}
	}

	
	public sealed class TimedDelegatedCommand<T> : DelegatedCommand<T>
	{
		readonly MethodBase method;
		readonly string template;

		public TimedDelegatedCommand( Action<T> action, string template ) : this( action, action.GetMethodInfo(), template ) {}

		public TimedDelegatedCommand( Action<T> action, MethodBase method, string template ) : base( action )
		{
			this.method = method;
			this.template = template;
		}

		public override void Execute( [Optional]T parameter )
		{
			using ( Logger.Default.Get( method ).TimeOperation( template, method, parameter ) )
			{
				base.Execute( parameter );
			}
		}

		/*public override TResult Get( TParameter parameter )
		{
			using ( Logger.Default.Get( method ).TimeOperation( template, method, parameter ) )
			{
				return base.Get( parameter );
			}
		}*/
	}


	public class DelegatedCommand<T> : CommandBase<T>
	{
		readonly Action<T> command;

		public DelegatedCommand( Action<T> command ) : this( command, Specifications<T>.Always ) {}

		public DelegatedCommand( Action<T> command, ISpecification<T> specification ) : this( command, Defaults<T>.Coercer, specification ) {}

		public DelegatedCommand( Action<T> command, Coerce<T> coercer ) : this( command, coercer, Specifications<T>.Always ) {}

		public DelegatedCommand( Action<T> command, Coerce<T> coercer, ISpecification<T> specification ) : base( coercer, specification )
		{
			this.command = command;
		}

		public override void Execute( [Optional]T parameter ) => command( parameter );
	}

	public class ProjectedCommand<T> : CommandBase<T>
	{
		readonly ICommand command;
		readonly Func<T, object> projection;

		public ProjectedCommand( ICommand command ) : this( command, Delegates<T>.Object ) {}

		public ProjectedCommand( ICommand command, Func<T, object> projection )
		{
			this.command = command;
			this.projection = projection;
		}

		public override void Execute( [Optional]T parameter ) => command.Execute( projection( parameter ) );
	}

	public class DecoratedCommand : DecoratedCommand<object>
	{
		public DecoratedCommand( ICommand<object> inner ) : base( inner ) {}
		public DecoratedCommand( ICommand<object> inner, Coerce<object> coercer ) : base( inner, coercer ) {}
		public DecoratedCommand( ICommand<object> inner, ISpecification<object> specification ) : base( inner, specification ) {}
		public DecoratedCommand( ICommand<object> inner, Coerce<object> coercer, ISpecification<object> specification ) : base( inner, coercer, specification ) {}
	}

	public class DecoratedCommand<T> : DelegatedCommand<T>
	{
		public DecoratedCommand( ICommand<T> inner ) : this( inner, Defaults<T>.Coercer ) {}
		public DecoratedCommand( ICommand<T> inner, Coerce<T> coercer ) : this( inner, coercer, inner.ToSpecification() ) {}
		public DecoratedCommand( ICommand<T> inner, ISpecification<T> specification ) : this( inner, Defaults<T>.Coercer, specification ) {}
		public DecoratedCommand( ICommand<T> inner, Coerce<T> coercer, ISpecification<T> specification ) : base( inner.ToDelegate(), coercer, specification ) {}
	}

	public abstract class CommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate { };
		readonly Coerce<T> coercer;
		readonly ISpecification<T> specification;

		protected CommandBase() : this( Defaults<T>.Coercer ) {}

		protected CommandBase( Coerce<T> coercer ) : this( coercer, Specifications<T>.Assigned ) {}

		protected CommandBase( ISpecification<T> specification ) : this( Defaults<T>.Coercer, specification ) {}

		protected CommandBase( Coerce<T> coercer, ISpecification<T> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		bool ICommand.CanExecute( [Optional]object parameter ) => Coerce( parameter );
		bool ISpecification.IsSatisfiedBy( [Optional]object parameter ) => Coerce( parameter );
		protected virtual bool Coerce( [Optional]object parameter ) => specification.IsSatisfiedBy( coercer( parameter ) );
		public virtual bool IsSatisfiedBy( T parameter ) => specification.IsSatisfiedBy( parameter );

		void ICommand.Execute( [Optional]object parameter ) => Execute( coercer( parameter ) );

		public abstract void Execute( T parameter );
	}
}