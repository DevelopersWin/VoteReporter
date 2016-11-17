using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using System;

namespace DragonSpark.Runtime
{
	public sealed class Disposables : SingletonScope<IDisposables>, IComposable<IDisposable>, IDisposable
	{
		public static Disposables Default { get; } = new Disposables();
		Disposables() : base( () => new Repository() ) {}

		public void Add( IDisposable instance ) => Get().Add( instance );

		sealed class Repository : RepositoryBase<IDisposable>, IDisposables
		{
			public Repository() : base( new PurgingCollection<IDisposable>() ) {}

			public void Dispose() => this.Each( entry => entry.Dispose() );
		}

		public void Dispose() => Get().Dispose();
	}

	public sealed class RegisteredDisposable<T> : AlterationBase<T>
	{
		public static RegisteredDisposable<T> Default { get; } = new RegisteredDisposable<T>();
		RegisteredDisposable() : this( Disposables.Default ) {}

		readonly IComposable<IDisposable> disposables;

		public RegisteredDisposable( IComposable<IDisposable> disposables )
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