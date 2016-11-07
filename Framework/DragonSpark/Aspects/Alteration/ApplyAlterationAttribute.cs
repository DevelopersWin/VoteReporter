using DragonSpark.Aspects.Build;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Alteration
{
	[IntroduceInterface( typeof(IAlteration) )]
	[ProvideAspectRole( KnownRoles.ValueConversion ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public abstract class ApplyAlterationBase : InvocationAspectBase, IAlteration
	{
		protected ApplyAlterationBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( factory, definition ) {}
		protected ApplyAlterationBase( IAlteration alteration ) : base( alteration ) {}

		protected sealed class Factory<T> : TypedAspectFactory<IAlteration, T> where T :  ApplyAlterationBase
		{
			public static Factory<T> Default { get; } = new Factory<T>();
			Factory() : base( Source.Default.Get ) {}
		}
	}

	public sealed class ApplyAlterationAttribute : ApplyAlterationBase
	{
		public ApplyAlterationAttribute( Type alterationType ) : base( Factory<ApplyAlterationAttribute>.Default.Get( alterationType ), Support<Aspect>.Default ) {}

		[UsedImplicitly]
		public ApplyAlterationAttribute( IAlteration alteration ) : base( alteration ) {}
	}

	public sealed class ApplyResultAlterationAttribute : ApplyAlterationBase
	{
		public ApplyResultAlterationAttribute( Type alterationType ) : base( Factory<ApplyResultAlterationAttribute>.Default.Get( alterationType ), Support<ResultAspect>.Default ) {}

		[UsedImplicitly]
		public ApplyResultAlterationAttribute( IAlteration alteration ) : base( alteration ) {}
	}
}