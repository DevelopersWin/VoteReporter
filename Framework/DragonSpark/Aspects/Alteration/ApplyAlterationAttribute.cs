﻿using DragonSpark.Aspects.Adapters;
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
	public abstract class ApplyAlterationBase : InvocationAspectBase, ISource<IAlterationAdapter>
	{
		readonly IAlterationAdapter alteration;

		protected ApplyAlterationBase( IAlterationAdapter alteration )
		{
			this.alteration = alteration;
		}
		protected ApplyAlterationBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( factory, definition ) {}

		protected sealed class Factory<T> : TypedParameterAspectFactory<IAlterationAdapter, T> where T :  ApplyAlterationBase
		{
			public static Factory<T> Default { get; } = new Factory<T>();
			Factory() : base( Source.Default.Get ) {}
		}

		public IAlterationAdapter Get() => alteration;
		object ISource.Get() => Get();
	}

	public sealed class ApplyAlterationAttribute : ApplyAlterationBase
	{
		public ApplyAlterationAttribute( Type alterationType ) : base( Factory<ApplyAlterationAttribute>.Default.Get( alterationType ), Definition<Aspect>.Default ) {}

		[UsedImplicitly]
		public ApplyAlterationAttribute( IAlterationAdapter alteration ) : base( alteration ) {}
	}

	public sealed class ApplyResultAlterationAttribute : ApplyAlterationBase
	{
		public ApplyResultAlterationAttribute( Type alterationType ) : base( Factory<ApplyResultAlterationAttribute>.Default.Get( alterationType ), Definition<ResultAspect>.Default ) {}

		[UsedImplicitly]
		public ApplyResultAlterationAttribute( IAlterationAdapter alteration ) : base( alteration ) {}
	}
}