using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using PostSharp;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;

namespace DragonSpark.Runtime
{
	public sealed class Disposables : SingletonScope<IDisposables>, IComposable<IDisposable>
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
					Message.Write( MessageLocation.Of( this ), SeverityType.Warning, "6776", "DISPOSE CALLED!!!" );
					this.Each( entry => entry.Dispose() );
				}
			}
		}
	}

	public sealed class RegisterForDispose<T> : AlterationBase<T>
	{
		public static RegisterForDispose<T> Default { get; } = new RegisterForDispose<T>();
		RegisterForDispose() : this( Disposables.Default ) {}

		readonly IComposable<IDisposable> disposables;

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