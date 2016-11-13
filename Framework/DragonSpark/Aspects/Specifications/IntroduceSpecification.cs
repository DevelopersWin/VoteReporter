using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceSpecification : IntroduceInterfaceAspectBase
	{
		public IntroduceSpecification( Type specificationType, Type implementationType )
			: this( specificationType, Constructors.Default.Get( implementationType ).Get( specificationType ) ) {}

		public IntroduceSpecification( Type specificationType, Func<object, object> factory ) : base( SpecificationTypeDefinition.Default, factory, specificationType.Adapt().GetImplementations( SpecificationTypeDefinition.Default.ReferencedType ) ) {}

		sealed class Constructors : Cache<Type, IParameterizedSource<Type, Func<object, ISpecificationAdapter>>>
		{
			public static Constructors Default { get; } = new Constructors();
			Constructors() : base( type => new Constructor( type ).ToCache() )
			{
				Set( typeof(SpecificationAdapter<>), Constructor.Default );
			}
		}
	}
}