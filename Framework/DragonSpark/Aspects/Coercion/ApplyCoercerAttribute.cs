using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Coercion
{
	[IntroduceInterface( typeof(ICoercer) )]
	[ProvideAspectRole( KnownRoles.ValueConversion ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public sealed class ApplyCoercerAttribute : InvocationAspectBase, ICoercer
	{
		public ApplyCoercerAttribute( Type coercerType ) : base( Factory.Default.Get( coercerType ), Definition.Default ) {}

		[UsedImplicitly]
		public ApplyCoercerAttribute( ICoercer coercer ) : base( coercer.Get ) {}

		sealed class Factory : TypedParameterAspectFactory<ICoercer, ApplyCoercerAttribute>
		{
			public static Factory Default { get; } = new Factory();
			Factory() : base( Source.Default.Get ) {}
		}
	}
}