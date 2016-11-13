using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( 
			IntroducedAspectSelector<IntroduceCoercer, Aspect>.Default,

			CommandTypeDefinition.Default, 
			GeneralizedSpecificationTypeDefinition.Default, 
			GeneralizedParameterizedSourceTypeDefinition.Default/*,
			ParameterizedSourceTypeDefinition.Default,
			GenericCommandCoreTypeDefinition.Default,
			SpecificationTypeDefinition.Default*/
		) {}
	}

	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceCoercer : IntroduceGenericInterfaceAspectBase
	{
		readonly static Func<Type, Func<object, object>> Factory = new ImplementationCache( typeof(ICoercerAdapter) ).ToCache().ToDelegate();

		public IntroduceCoercer() : this( typeof(DefaultCoercerImplementation<,>) ) {}
		public IntroduceCoercer( Type implementationType ) : this( implementationType, SourceCoercer<ICoercerAdapter>.Default.To( Factory( implementationType ) ).Get ) {}
		public IntroduceCoercer( Type implementationType, Func<object, object> factory ) : base( ParameterizedSourceTypeDefinition.Default, implementationType, factory ) {}
	}

	public sealed class DefaultCoercerImplementation<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly ICoercerAdapter coercer;

		public DefaultCoercerImplementation( ICoercerAdapter coercer )
		{
			this.coercer = coercer;
		}

		public override TResult Get( TParameter parameter ) => coercer.Get( parameter ).As<TResult>();
	}
}