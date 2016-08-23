using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Sources.Parameterized
{
	public class ConfiguringFactory<T> : DelegatedSource<T>
	{
		readonly Action initialize;
		readonly Action<T> configure;

		public ConfiguringFactory( Func<T> inner, Action<T> configure ) : this( inner, TypeSystem.Delegates.Empty, configure ) {}

		public ConfiguringFactory( Func<T> inner, Action initialize ) : this( inner, initialize, Delegates<T>.Empty ) {}

		public ConfiguringFactory( Func<T> inner, Action initialize, Action<T> configure ) : base( inner )
		{
			this.initialize = initialize;
			this.configure = configure;
		}

		public override T Get()
		{
			initialize();
			var result = base.Get();
			configure( result );
			return result;
		}
	}
	
	public abstract class ValidatedParameterizedSourceBase<TParameter, TResult> : IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly Coerce<TParameter> coercer;
		readonly ISpecification<TParameter> specification;

		protected ValidatedParameterizedSourceBase() : this( Specifications<TParameter>.Assigned ) {}
		protected ValidatedParameterizedSourceBase( ISpecification<TParameter> specification ) : this( Defaults<TParameter>.Coercer, specification ) {}

		protected ValidatedParameterizedSourceBase( Coerce<TParameter> coercer, ISpecification<TParameter> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public virtual bool IsSatisfiedBy( TParameter parameter ) => specification.IsSatisfiedBy( parameter );
		
		bool ISpecification.IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( coercer( parameter ) );

		public abstract TResult Get( TParameter parameter );

		object IParameterizedSource.Get( object parameter )
		{
			var coerced = coercer( parameter );
			var result = coerced.IsAssignedOrValue() ? Get( coerced ) : default(TResult);
			return result;
		}
	}

	public static class Defaults<T>
	{
		public static Coerce<T> Coercer { get; } = Coercer<T>.Default.Coerce;
	}
}