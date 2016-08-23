using System;
using DragonSpark.Extensions;
using DragonSpark.Specifications;

namespace DragonSpark.Commands
{
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
}