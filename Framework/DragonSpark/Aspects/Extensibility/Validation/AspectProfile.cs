using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	class AspectProfile : ParameterizedSourceBase<Type, MethodInfo>, IAspectProfile
	{
		readonly static Aspects.Extensions.MethodDefinition DefaultValidation = new Aspects.Extensions.MethodDefinition( typeof(ISpecification<>), nameof(ISpecification<object>.IsSatisfiedBy) );

		readonly Func<MethodLocator.Parameter, MethodInfo> source;

		public AspectProfile( Aspects.Extensions.MethodDefinition method ) : this( method, DefaultValidation ) {}

		public AspectProfile( Aspects.Extensions.MethodDefinition method, Aspects.Extensions.MethodDefinition validation ) : this( method, validation, Defaults.Locator ) {}

		public AspectProfile( Aspects.Extensions.MethodDefinition method, Aspects.Extensions.MethodDefinition validation, Func<MethodLocator.Parameter, MethodInfo> source )
		{
			Method = method;
			Validation = validation;
			this.source = source;
		}

		public Aspects.Extensions.MethodDefinition Method { get; }
		public Aspects.Extensions.MethodDefinition Validation { get; }

		public override MethodInfo Get( Type parameter ) => source( new MethodLocator.Parameter( Method.DeclaringType, Method.MethodName, parameter ) );
	}
}