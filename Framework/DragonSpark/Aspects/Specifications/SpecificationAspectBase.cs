using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Specifications
{
	public abstract class SpecificationAspectBase : InstanceAspectBase
	{
		readonly ISpecificationAdapter specification;
		protected SpecificationAspectBase( Func<object, IAspect> factory ) : base( factory, Definition.Default ) {}
		protected SpecificationAspectBase( ISpecificationAdapter specification )
		{
			this.specification = specification;
		}

		protected sealed class Factory<T> : TypedParameterAspectFactory<ISpecificationAdapter, T> where T : SpecificationAspectBase
		{
			public static Factory<T> Default { get; } = new Factory<T>();
			Factory() : base( Source.Default.Get ) {}
		}

		public ISpecificationAdapter Get() => specification;
		// object ISource.Get() => Get();
	}
}