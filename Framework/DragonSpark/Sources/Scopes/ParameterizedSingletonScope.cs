using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Sources.Scopes
{
	public class ParameterizedSingletonScope<TParameter, TResult> : ParameterizedScope<TParameter, TResult>
	{
		public ParameterizedSingletonScope() : this( parameter => default(TResult) ) {}
		public ParameterizedSingletonScope( Func<TParameter, TResult> factory ) : this( new Func<object, Func<TParameter, TResult>>( factory.Accept ) ) {}
		public ParameterizedSingletonScope( Func<object, Func<TParameter, TResult>> global ) : base( new GlobalSource( global ).ToSingleton() ) {}

		public override void Assign( Func<Func<TParameter, TResult>> item ) => base.Assign( new LocalSource( item ).ToSingleton() );
		public override void Assign( Func<object, Func<TParameter, TResult>> item ) => base.Assign( new GlobalSource( item ).ToSingleton() );

		sealed class LocalSource : SourceBase<Func<TParameter, TResult>>
		{
			readonly Func<Func<TParameter, TResult>> source;
			public LocalSource( Func<Func<TParameter, TResult>> source )
			{
				this.source = source;
			}

			public override Func<TParameter, TResult> Get() => source().ToSingleton();
		}

		sealed class GlobalSource : ParameterizedSourceBase<Func<TParameter, TResult>>
		{
			readonly Func<object, Func<TParameter, TResult>> global;
			public GlobalSource( Func<object, Func<TParameter, TResult>> global )
			{
				this.global = global;
			}

			public override Func<TParameter, TResult> Get( object parameter ) => global( parameter ).ToSingleton();
		}
	}
}