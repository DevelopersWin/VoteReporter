using DragonSpark.Activation;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
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
		public ApplyCoercerAttribute( Type coercerType ) : base( 
			ParameterConstructor<ICoercer, ApplyCoercerAttribute>.Default.WithParameter( Source.Default.WithParameter( coercerType ).Get ).ToDelegate().Wrap(),
			Support.Default ) {}

		[UsedImplicitly]
		public ApplyCoercerAttribute( ICoercer invocation ) : base( invocation ) {}
	}
}