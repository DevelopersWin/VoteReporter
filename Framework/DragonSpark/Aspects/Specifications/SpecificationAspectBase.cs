using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Specifications
{
	public abstract class SpecificationAspectBase : InstanceAspectBase
	{
		readonly ISpecificationAdapter specification;
		protected SpecificationAspectBase( Func<object, IAspect> factory, params object[] parameters ) : base( factory, new Definition( parameters ) ) {}
		protected SpecificationAspectBase( ISpecificationAdapter specification )
		{
			this.specification = specification;
		}

		protected sealed class Constructors<T> : TypedParameterConstructors<ISpecificationAdapter, T> where T : SpecificationAspectBase
		{
			public static Constructors<T> Default { get; } = new Constructors<T>();
			Constructors() : base( Source.Default.Get ) {}
		}

		public ISpecificationAdapter Get() => specification;
		// object ISource.Get() => Get();
	}
}