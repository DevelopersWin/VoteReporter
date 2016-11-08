using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Support : AspectBuildDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : this( 
			AspectLocatorFactory<IntroduceSpecification, Aspect>.Default,
			GenericSpecificationTypeDefinition.Default ) {}

		Support( 
			IParameterizedSource<ImmutableArray<ITypeDefinition>, ImmutableArray<IAspectInstanceLocator>> locatorSource,
			params ITypeDefinition[] definitions
		) : base( type => true, locatorSource.GetFixed( definitions ) ) {}
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

		protected IntroduceGenericInterfaceAspectBase( Type interfaceType, ISpecification<Type> specification, Func<object, object> factory )
		{
			this.interfaceType = interfaceType;
			this.specification = specification;
			this.factory = factory;
		}

		public override bool CompileTimeValidate( Type type ) => specification.IsSatisfiedBy( type );

		protected override Type[] GetPublicInterfaces( Type targetType )
		{
			return interfaceType.MakeGenericType( ParameterTypes.Default.Get( targetType ) ).ToItem();
		}

		public override object CreateImplementationObject( AdviceArgs args ) => factory( args.Instance );
	}


}