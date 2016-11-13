using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Definition : IntroducedAspectBuildDefinition<IntroduceCoercer, Aspect>
	{
		public Definition( params object[] parameters ) : 
			base( 
				parameters.ToImmutableArray(),

				CommandTypeDefinition.Default, 
				GeneralizedSpecificationTypeDefinition.Default, 
				GeneralizedParameterizedSourceTypeDefinition.Default 
			) {}
	}

	/*public sealed class DefaultCoercerImplementation<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly ICoercerAdapter coercer;

		public DefaultCoercerImplementation( ICoercerAdapter coercer )
		{
			this.coercer = coercer;
		}

		public override TResult Get( TParameter parameter ) => coercer.Get( parameter ).As<TResult>();
	}*/
}