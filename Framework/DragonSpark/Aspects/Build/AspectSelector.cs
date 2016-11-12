using System;
using System.Collections.Generic;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using JetBrains.Annotations;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public class AspectSelector<TType, TMethod> : CompositeAspectSelector
		where TType : IAspect 
		where TMethod : IAspect
	{
		public static AspectSelector<TType, TMethod> Default { get; } = new AspectSelector<TType, TMethod>();
		AspectSelector() : this( definition => new TypeAspectDefinition<TType>( definition ).Yield() ) {}

		public AspectSelector( Func<ITypeDefinition, IEnumerable<IAspectDefinition>> typeSource ) : this( typeSource, MethodAspectSelector<TMethod>.Default.Yield ) {}

		[UsedImplicitly]
		public AspectSelector( Func<ITypeDefinition, IEnumerable<IAspectDefinition>> typeSource, Func<ITypeDefinition, IEnumerable<IAspectDefinition>> methodSource ) 
			: base( typeSource, methodSource ) {}
	}
}