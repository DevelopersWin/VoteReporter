using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Runtime
{
	public sealed class RegisterForDispose<T> : AlterationBase<T>
	{
		public static RegisterForDispose<T> Default { get; } = new RegisterForDispose<T>();
		RegisterForDispose() : this( Disposables.Default ) {}

		readonly IComposable<IDisposable> disposables;

		[UsedImplicitly]
		public RegisterForDispose( IComposable<IDisposable> disposables )
		{
			this.disposables = disposables;
		}

		public override T Get( T parameter )
		{
			parameter.AsDisposable().With( disposables.Add );
			return parameter;
		}
	}
}