using DragonSpark.Commands;
using Polly;
using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public sealed class PolicyDecoratedCommand<T> : DecoratedCommand<T>
	{
		readonly Func<Policy> source;

		public PolicyDecoratedCommand( Func<Policy> source, ICommand<T> inner ) : base( inner )
		{
			this.source = source;
		}

		public override void Execute( T parameter = default(T) ) => source().Execute( () => base.Execute( parameter ) );
	}
}