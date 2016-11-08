using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Build
{
	public class TypeBasedAspectInstanceLocator<T> : SpecificationParameterizedSource<Type, AspectInstance>, IAspectInstanceLocator where T : IAspect
	{
		readonly static Func<Type, AspectInstance> Factory = AspectInstanceFactory<T, Type>.Default.Get;

		public TypeBasedAspectInstanceLocator( ITypeAware typeAware ) : this( typeAware.ReferencedType ) {}
		public TypeBasedAspectInstanceLocator( Type declaringType ) : this( TypeAssignableSpecification.Defaults.Get( declaringType ), Factory ) {}

		protected TypeBasedAspectInstanceLocator( ISpecification<Type> specification ) : this( specification, Factory ) {}
		protected TypeBasedAspectInstanceLocator( ISpecification<Type> specification, Func<Type, AspectInstance> factory ) : base( specification, factory ) {}

		public Type ReferencedType => typeof(T);
	}
}