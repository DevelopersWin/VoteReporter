using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public sealed class IntroducedAspectSource<T> : ParameterizedItemSourceBase<ITypeDefinition, IAspects> where T : ITypeLevelAspect
	{
		public static IntroducedAspectSource<T> Default { get; } = new IntroducedAspectSource<T>();
		IntroducedAspectSource() : this( TypeAspectFactory<T>.Default ) {}

		readonly ISpecificationParameterizedSource<TypeInfo, AspectInstance> source;
		
		public IntroducedAspectSource( ImmutableArray<object> parameters ) : this( new TypeAspectFactory<T>( parameters ) ) {}

		[UsedImplicitly]
		public IntroducedAspectSource( ISpecificationParameterizedSource<TypeInfo, AspectInstance> source  )
		{
			this.source = source;
		}

		public override IEnumerable<IAspects> Yield( ITypeDefinition parameter )
		{
			yield return new TypeAspects<T>( parameter.Inverse(), source );
		}
	}
}