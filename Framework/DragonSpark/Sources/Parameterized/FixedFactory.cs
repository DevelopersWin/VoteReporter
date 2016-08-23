using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources.Parameterized
{
	public class FixedFactory<TParameter, TResult> : SourceBase<TResult>
	{
		readonly Func<TParameter, TResult> inner;
		readonly Func<TParameter> parameter;

		public FixedFactory( Func<TParameter, TResult> inner, [Optional]TParameter parameter ) : this( inner, Factory.For( parameter ) ) {}

		public FixedFactory( Func<TParameter, TResult> inner, Func<TParameter> parameter )
		{
			this.inner = inner;
			this.parameter = parameter;
		}

		public override TResult Get() => inner( parameter() );
	}
}