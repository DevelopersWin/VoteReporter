using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Support : AspectBuildDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : base( 
			AspectLocatorFactory<IntroducedSpecificationAspect, Aspect>.Default,
			ParameterizedSourceTypeDefinition.Default, 
			GenericCommandCoreTypeDefinition.Default ) {}

		/*public override IEnumerable<AspectInstance> Get( Type parameter )
		{
			var locator = new TypeBasedAspectInstanceLocator<IntroducedSpecificationAspect>();

			return base.Get( parameter );
		}*/
	}

	[UsedImplicitly]
	public sealed class IntroducedSpecificationAspect : IntroducedGenericInterfaceAspect
	{
		readonly static Func<object, object> Factory = new AdapterFactory( typeof(ISpecification), typeof(IntroducedSpecificationAdapter<>) ).Get;

		public IntroducedSpecificationAspect() : base( typeof(ISpecification<>), Factory ) {}
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

	public class IntroducedGenericInterfaceAspect : CompositionAspect
	{
		readonly Type interfaceType;
		readonly ISpecification<Type> specification;
		readonly Func<object, object> factory;

		public IntroducedGenericInterfaceAspect( Type interfaceType, Func<object, object> factory ) : this( interfaceType, TypeAssignableSpecification.Defaults.Get( interfaceType ).Inverse(), factory ) {}

		public IntroducedGenericInterfaceAspect( Type interfaceType, ISpecification<Type> specification, Func<object, object> factory )
		{
			this.interfaceType = interfaceType;
			this.specification = specification;
			this.factory = factory;
		}

		public override bool CompileTimeValidate( Type type ) => specification.IsSatisfiedBy( type );

		protected override Type[] GetPublicInterfaces( Type targetType ) => 
			interfaceType.MakeGenericType( ParameterTypes.Default.Get( targetType ) ).ToItem();

		public override object CreateImplementationObject( AdviceArgs args ) => factory( args.Instance );
	}


}