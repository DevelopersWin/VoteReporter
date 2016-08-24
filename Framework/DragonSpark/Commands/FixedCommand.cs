using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Specifications;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Commands
{
	public class FixedCommand<T> : DisposingCommand<object>
	{
		readonly Func<ICommand<T>> command;
		readonly Func<T> parameter;

		public FixedCommand( ICommand<T> command, T parameter ) : this( command, Factory.For( parameter ) ) {}

		public FixedCommand( ICommand<T> command, Func<T> parameter ) : this( command.Self, parameter ) {}

		public FixedCommand( Func<ICommand<T>> command, Func<T> parameter ) : this( command, parameter, Specifications.Specifications.Always ) {}

		public FixedCommand( Func<ICommand<T>> command, Func<T> parameter, ISpecification<object> specification ) : base( specification )
		{
			this.command = command;
			this.parameter = parameter;
		}

		public override void Execute( [Optional]object _ ) => command().Execute( parameter() );

		protected override void OnDispose() => command.TryDispose();
	}
}