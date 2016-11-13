using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
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

	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceCoercer : IntroduceInterfaceAspectBase
	{
		public IntroduceCoercer( Type coercerType, Type implementationType )
			: this( coercerType, Constructors.Default.Get( implementationType ).Get( coercerType ) ) {}

		public IntroduceCoercer( Type coercerType, Func<object, object> factory ) : base( ParameterizedSourceTypeDefinition.Default, factory, coercerType.Adapt().GetImplementations( ParameterizedSourceTypeDefinition.Default.ReferencedType ) ) {}

		sealed class Constructors : Cache<Type, IParameterizedSource<Type, Func<object, ICoercerAdapter>>>
		{
			public static Constructors Default { get; } = new Constructors();
			Constructors() : base( type => new Constructor( type ).ToCache() )
			{
				Set( typeof(CoercerAdapter<,>), Constructor.Default );
			}
		}
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