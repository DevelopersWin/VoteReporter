using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;
using Aspect = DragonSpark.Aspects.Coercion.Aspect;

namespace DragonSpark.Aspects.Alteration
{
	[IntroduceInterface( typeof(ISource<IAlterationAdapter>) )]
	[ProvideAspectRole( KnownRoles.ValueConversion ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public abstract class ApplyAlterationBase : InstanceAspectBase, ISource<IAlterationAdapter>
	{
		readonly IAlterationAdapter alteration;

		protected ApplyAlterationBase( IAlterationAdapter alteration )
		{
			this.alteration = alteration;
		}
		protected ApplyAlterationBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( factory, definition ) {}

		protected sealed class Constructors<T> : TypedAspectConstructors<IAlterationAdapter, T> where T :  ApplyAlterationBase
		{
			public static Constructors<T> Default { get; } = new Constructors<T>();
			Constructors() : base( Source.Default.Get ) {}
		}

		public IAlterationAdapter Get() => alteration;
		// object ISource.Get() => Get();
	}

	public sealed class ApplyAlteration : ApplyAlterationBase
	{
		public ApplyAlteration( Type alterationType ) : base( Constructors<ApplyAlteration>.Default.Get( alterationType ), Definition<Aspect>.Default ) {}

		[UsedImplicitly]
		public ApplyAlteration( IAlterationAdapter alteration ) : base( alteration ) {}
	}

	public sealed class ApplyResultAlteration : ApplyAlterationBase
	{
		public ApplyResultAlteration( Type alterationType ) : base( Constructors<ApplyResultAlteration>.Default.Get( alterationType ), Definition<ResultAspect>.Default ) {}

		[UsedImplicitly]
		public ApplyResultAlteration( IAlterationAdapter alteration ) : base( alteration ) {}
	}
}