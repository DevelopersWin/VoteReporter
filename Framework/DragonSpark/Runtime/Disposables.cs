using DragonSpark.Extensions;
using DragonSpark.Sources.Scopes;
using System;
using System.Collections.Generic;
using DragonSpark.Diagnostics;

namespace DragonSpark.Runtime
{
	public sealed class Disposables : SingletonScope<IDisposables>, IComposable<IDisposable>, IDisposable
	{
		public static Disposables Default { get; } = new Disposables();
		Disposables() : base( () => new Repository() ) {}

		public void Add( IDisposable instance ) => Get().Add( instance );

		sealed class Repository : RepositoryBase<IDisposable>, IDisposables
		{
			readonly ConditionMonitor monitor = new ConditionMonitor();

			public Repository() : base( new PurgingCollection<IDisposable>( new HashSet<IDisposable>() ) ) {}

			~Repository()
			{
				OnDispose();
			}

			public void Dispose()
			{
				OnDispose();
				GC.SuppressFinalize( this );
			}

			void OnDispose()
			{
				if ( monitor.Apply() )
				{
					this.Each( entry => entry.Dispose() );
				}
				Logger.Default.Get( this ).Information( "Disposed!!!" );
			}
		}

		public void Dispose() => Get().Dispose();
	}
}