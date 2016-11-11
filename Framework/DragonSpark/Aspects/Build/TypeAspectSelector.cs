using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class TypeAspectSelector<T> : AspectSelectorBase where T : IAspect
	{
		readonly static Func<TypeInfo, AspectInstance> Factory = TypeAspectFactory<T>.Default.Get;
		public TypeAspectSelector( ITypeAware typeAware ) : this( typeAware.ReferencedType ) {}
		public TypeAspectSelector( Type declaringType ) : this( TypeAssignableSpecification.Defaults.Get( declaringType ).Coerce( AsTypeCoercer.Default ) ) {}

		protected TypeAspectSelector( ISpecification<TypeInfo> specification ) : base( specification.And( TypeAspectFactory<T>.Default ).ToDelegate(), Factory ) {}
	}
}