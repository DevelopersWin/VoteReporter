using System;
using DragonSpark.Extensions;
using DragonSpark.Sources;

namespace DragonSpark.Runtime
{
	public sealed class Disposables : RepositoryBase<IDisposable>, IDisposable
	{
		readonly IDisposable disposable;

		public static ISource<Disposables> Default { get; } = new Scope<Disposables>( Factory.Global( () => new Disposables() ) );
		Disposables() : base( new PurgingCollection<IDisposable>() )
		{
			disposable = new DelegatedDisposable( OnDispose );
		}

		void OnDispose() => List().Each( entry => entry.Dispose() );
		public void Dispose() => disposable.Dispose();
	}
}