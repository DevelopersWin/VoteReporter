using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( 
			AspectLocatorFactory<IntroduceSpecification, Aspect>.Default.GetFixed( GenericSpecificationTypeDefinition.Default )
		) {}

		/*Support( 
			IParameterizedSource<ImmutableArray<ITypeDefinition>, ImmutableArray<IAspectSelector>> locatorSource,
			params ITypeDefinition[] definitions
		) : base( type => true, locatorSource.GetFixed( definitions ) ) {}*/
	}

	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceSpecification : IntroduceGenericInterfaceAspectBase
	{
		readonly static Func<object, object> Factory = new AdapterFactory( typeof(ISpecification), typeof(IntroducedSpecificationAdapter<>) ).Get;

		public IntroduceSpecification() : base( GenericSpecificationTypeDefinition.Default.ReferencedType, Factory ) {}
	}

	public sealed class IntroducedSpecificationAdapter<T> : SpecificationBase<T>
	{
		readonly ISpecification specification;
		public IntroducedSpecificationAdapter( ISpecification specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( T parameter ) => (bool)specification.Get( parameter );
	}

	[AttributeUsage( AttributeTargets.Class ), CompositionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class IntroduceGenericInterfaceAspectBase : CompositionAspect
	{
		readonly Type interfaceType;
		readonly ISpecification<Type> specification;
		readonly Func<object, object> factory;

		protected IntroduceGenericInterfaceAspectBase( Type interfaceType, Func<object, object> factory ) : this( interfaceType, TypeAssignableSpecification.Defaults.Get( interfaceType ).Inverse(), factory ) {}

		[UsedImplicitly]
		protected IntroduceGenericInterfaceAspectBase( Type interfaceType, ISpecification<Type> specification, Func<object, object> factory )
		{
			this.interfaceType = interfaceType;
			this.specification = specification;
			this.factory = factory;
		}

		public override bool CompileTimeValidate( Type type ) => specification.IsSatisfiedBy( type );

		protected override Type[] GetPublicInterfaces( Type targetType ) => interfaceType.MakeGenericType( ParameterTypes.Default.Get( targetType ) ).ToItem();

		public override object CreateImplementationObject( AdviceArgs args ) => factory( args.Instance );
	}


}