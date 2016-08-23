using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Commands
{
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
}