using DragonSpark.Specifications;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class IntroducedTypeAspectDefinition<T> : TypeAspectDefinition<T> where T : CompositionAspect, IAspect
	{
		public IntroducedTypeAspectDefinition( ISpecification<TypeInfo> specification ) : base( specification.Inverse() ) {}
	}

	public class TypeAspectDefinition<T> : AspectDefinitionBase where T : IAspect
	{
		readonly static Func<TypeInfo, AspectInstance> Factory = TypeAspectFactory<T>.Default.Get;

		public TypeAspectDefinition( ISpecification<TypeInfo> specification ) : base( specification.And( TypeAspectFactory<T>.Default ).ToDelegate(), Factory ) {}
	}
}