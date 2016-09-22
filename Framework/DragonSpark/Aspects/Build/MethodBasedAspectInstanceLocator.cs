using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	sealed class MethodBasedAspectInstanceLocator<T> : IAspectInstanceLocator where T : IAspect
	{
		readonly static Func<MethodInfo, AspectInstance> Factory = AspectInstanceFactory<T, MethodInfo>.Default.Get;

		readonly Func<Type, MethodInfo> methodSource;
		readonly Func<MethodInfo, AspectInstance> inner;

		public MethodBasedAspectInstanceLocator( IMethodStore store ) : this( store.Get, Factory ) {}

		public MethodBasedAspectInstanceLocator( Func<Type, MethodInfo> methodSource, Func<MethodInfo, AspectInstance> inner )
		{
			this.methodSource = methodSource;
			this.inner = inner;
		}

		public AspectInstance Get( Type parameter )
		{
			var method = methodSource( parameter );
			var result = method != null ? inner( method ) : null;
			return result;
		}
	}

	public class TypeBasedAspectInstanceLocator<T> : SpecificationParameterizedSource<Type, AspectInstance>, IAspectInstanceLocator where T : IAspect
	{
		readonly static Func<Type, AspectInstance> Factory = AspectInstanceFactory<T, Type>.Default.Get;

		public TypeBasedAspectInstanceLocator( ITypeAware typeAware ) : this( typeAware.DeclaringType ) {}
		public TypeBasedAspectInstanceLocator( Type declaringType ) : this( TypeAssignableSpecification.Defaults.Get( declaringType ), Factory ) {}

		protected TypeBasedAspectInstanceLocator( ISpecification<Type> specification ) : this( specification, Factory ) {}
		protected TypeBasedAspectInstanceLocator( ISpecification<Type> specification, Func<Type, AspectInstance> factory ) : base( specification, factory ) {}
	}
}