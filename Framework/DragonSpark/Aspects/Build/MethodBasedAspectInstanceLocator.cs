using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class MethodBasedAspectInstanceLocator<T> : DelegatedParameterizedSource<Type, AspectInstance>, IAspectInstanceLocator where T : IAspect
	{
		readonly static Func<MethodInfo, AspectInstance> Factory = AspectInstanceFactory<T, MethodInfo>.Default.Get;

		public MethodBasedAspectInstanceLocator( IMethodStore store ) : this( store.To( Factory ).Get ) {}

		[UsedImplicitly]
		public MethodBasedAspectInstanceLocator( Func<Type, AspectInstance> inner ) : base( inner ) {}

		public Type ReferencedType => typeof(T);
	}
}