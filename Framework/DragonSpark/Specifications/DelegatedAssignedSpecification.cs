using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Specifications
{
	public class DelegatedAssignedSpecification<TParameter, TResult> : DelegatedSpecification<TParameter>
	{
		readonly static Func<TResult, bool> Specification = AssignedSpecification<TResult>.Default.ToDelegate();

		public DelegatedAssignedSpecification( Func<TParameter, TResult> source ) : this( source, Specification ) {}

		[UsedImplicitly]
		public DelegatedAssignedSpecification( Func<TParameter, TResult> source, Func<TResult, bool> specification ) : base( source.To( specification ).ToDelegate() ) {}
	}
}