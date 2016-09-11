using System;
using System.Linq;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public sealed class GenericSourceAdapterFactory : ParameterizedSourceBase<object, IParameterValidationAdapter>
	{
		public static GenericSourceAdapterFactory Default { get; } = new GenericSourceAdapterFactory();
		GenericSourceAdapterFactory() /*: base( typeof(IParameterizedSource<,>), typeof(GenericSourceAdapterFactory), nameof(Create) )*/ {}

		public override IParameterValidationAdapter Get( object parameter ) => 
			Cache.DefaultNested.Get( parameter.GetType() ).Invoke( parameter );

		sealed class Cache : Cache<Type, Func<object, IParameterValidationAdapter>>
		{
			public static Cache DefaultNested { get; } = new Cache();
			Cache() : base( Create ) {}

			static Func<object, IParameterValidationAdapter> Create( Type parameter )
			{
				var types = parameter.Adapt().GetTypeArgumentsFor( typeof(IParameterizedSource<,>) );
				var parameterType = typeof(ISpecification<>).MakeGenericType( types.First() );
				var resultType = typeof(SourceAdapter<,>).MakeGenericType( types );
				var result = ParameterConstructor<object, IParameterValidationAdapter>.Make(
					parameterType,
					resultType );
				return result;
			}
		}

		// static IParameterValidationAdapter Create<TParameter, TResult>( ISpecification<TParameter> instance ) => new SourceAdapter<TParameter, TResult>( instance );
	}
}