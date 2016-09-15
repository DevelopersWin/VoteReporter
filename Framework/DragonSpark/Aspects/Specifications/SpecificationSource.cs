using System;
using DragonSpark.Sources.Parameterized;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class SpecificationSource : ParameterizedSourceBase<Type, ISpecification>
	{
		readonly Func<Type, Func<object, ISpecification>> constructorSource;
		readonly Func<Type, object> specificationSource;
		public static SpecificationSource Default { get; } = new SpecificationSource();
		SpecificationSource() : this( SpecificationConstructor.Default.Get, Activator.Default.GetService ) {}

		SpecificationSource( Func<Type, Func<object, ISpecification>> constructorSource, Func<Type, object> specificationSource )
		{
			this.constructorSource = constructorSource;
			this.specificationSource = specificationSource;
		}

		public override ISpecification Get( Type parameter )
		{
			var constructor = constructorSource( parameter );
			var specification = specificationSource( parameter );
			var result = constructor( specification );
			return result;
		}
	}
}