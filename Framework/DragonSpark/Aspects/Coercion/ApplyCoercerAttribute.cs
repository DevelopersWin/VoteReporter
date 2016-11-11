using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Coercion
{
	[IntroduceInterface( typeof(ISource<ICoercerAdapter>) )]
	[ProvideAspectRole( KnownRoles.ValueConversion ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public sealed class ApplyCoercerAttribute : InvocationAspectBase, ISource<ICoercerAdapter>
	{
		readonly ICoercerAdapter coercer;

		public ApplyCoercerAttribute( Type coercerType ) : base( Factory.Default.Get( coercerType ), Definition.Default ) {}

		[UsedImplicitly]
		public ApplyCoercerAttribute( ICoercerAdapter coercer )
		{
			this.coercer = coercer;
		}

		sealed class Factory : TypedParameterAspectFactory<ICoercerAdapter, ApplyCoercerAttribute>
		{
			public static Factory Default { get; } = new Factory();
			Factory() : base( Source.Default.Get ) {}
		}

		public ICoercerAdapter Get() => coercer;
		// object ISource.Get() => Get();
	}
}