using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Specifications
{
	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceSpecification : IntroduceInterfaceAspectBase
	{
		public IntroduceSpecification( Type specificationType, Type implementationType )
			: this( specificationType, 
				  Constructors.Default
					.Get( implementationType )
					.Get( specificationType )
					.WithParameter( Activator.Default.WithParameter( specificationType ).Get )
					.Accept ) {}

		public IntroduceSpecification( Type specificationType, Func<object, object> factory ) 
			: base( DragonSpark.Specifications.Extensions.Inverse( SpecificationTypeDefinition.Default ), factory, specificationType.Adapt().GetImplementations( SpecificationTypeDefinition.Default.ReferencedType ) ) {}

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