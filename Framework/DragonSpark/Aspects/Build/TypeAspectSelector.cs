using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Build
{
	public class TypeAspectSelector<T> : AspectSelectorBase where T : IAspect
	{
		readonly static Func<Type, AspectInstance> Factory = TypeAspectFactory<T>.Default.Get;
		public TypeAspectSelector( ITypeAware typeAware ) : this( typeAware.ReferencedType ) {}
		public TypeAspectSelector( Type declaringType ) : base( declaringType, Factory ) {}

		protected TypeAspectSelector( Func<Type, bool> specification ) : base( specification, Factory ) {}

		/*protected TypeAspectSelector( ISpecification<Type> specification ) : this( specification, Factory ) {}
		protected TypeAspectSelector( ISpecification<Type> specification, Func<Type, AspectInstance> factory ) : base( specification, factory ) {}

		public Type ReferencedType => typeof(T);*/
	}
}