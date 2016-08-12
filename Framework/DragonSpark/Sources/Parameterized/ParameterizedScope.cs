using System;

namespace DragonSpark.Sources.Parameterized
{
	public class ParameterizedScope<T> : ParameterizedScope<object, T>, IParameterizedScope<T>
	{
		public ParameterizedScope( Func<object, T> source ) : base( source ) {}
		public ParameterizedScope( Func<object, Func<object, T>> source ) : base( source ) {}
		protected ParameterizedScope( IScope<Func<object, T>> scope ) : base( scope ) {}
	}
}