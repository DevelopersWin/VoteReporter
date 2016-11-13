using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using System;

namespace DragonSpark.Aspects.Alteration
{
	[IntroduceInterface( typeof(ISource<IAlterationAdapter>) )]
	[LinesOfCodeAvoided( 1 )]
	public abstract class ApplyAlterationBase : InstanceAspectBase, ISource<IAlterationAdapter>
	{
		readonly IAlterationAdapter alteration;

		protected ApplyAlterationBase( IAlterationAdapter alteration )
		{
			this.alteration = alteration;
		}
		protected ApplyAlterationBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( factory, definition ) {}

		protected sealed class Constructors<T> : TypedParameterConstructors<IAlterationAdapter, T> where T :  ApplyAlterationBase
		{
			public static Constructors<T> Default { get; } = new Constructors<T>();
			Constructors() : base( Source.Default.Get ) {}
		}

		public IAlterationAdapter Get() => alteration;
		// object ISource.Get() => Get();
	}
}