using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	class AspectProfile : ParameterizedSourceBase<Type, MethodInfo>, IAspectProfile
	{
		readonly static MethodDescriptor DefaultValidation = new MethodDescriptor( typeof(ISpecification<>), nameof(ISpecification<object>.IsSatisfiedBy) );

		readonly Func<MethodLocator.Parameter, MethodInfo> source;

		public AspectProfile( MethodDescriptor method ) : this( method, DefaultValidation ) {}

		public AspectProfile( MethodDescriptor method, MethodDescriptor validation ) : this( method, validation, Defaults.Locator ) {}

		public AspectProfile( MethodDescriptor method, MethodDescriptor validation, Func<MethodLocator.Parameter, MethodInfo> source )
		{
			Method = method;
			Validation = validation;
			this.source = source;
		}

		public MethodDescriptor Method { get; }
		public MethodDescriptor Validation { get; }

		public override MethodInfo Get( Type parameter ) => source( new MethodLocator.Parameter( Method.DeclaringType, Method.MethodName, parameter ) );
	}
}