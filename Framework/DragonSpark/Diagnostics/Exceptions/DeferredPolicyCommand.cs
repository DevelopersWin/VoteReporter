using DragonSpark.Commands;
using Polly;
using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public sealed class DeferredPolicyCommand<T> : DecoratedCommand<T>
	{
		readonly Func<Policy> source;

		public DeferredPolicyCommand( Func<Policy> source, ICommand<T> inner ) : base( inner )
		{
			this.source = source;
		}

		public override void Execute( T parameter ) => source().Execute( () => base.Execute( parameter ) );
	}
}