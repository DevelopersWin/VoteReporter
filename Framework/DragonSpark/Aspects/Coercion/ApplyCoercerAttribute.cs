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
	public sealed class ApplyCoercerAttribute : InstanceAspectBase, ISource<ICoercerAdapter>
	{
		readonly ICoercerAdapter coercer;

		public ApplyCoercerAttribute( Type coercerType ) : base( Constructors.Default.Get( coercerType ), Definition.Default ) {}

		[UsedImplicitly]
		public ApplyCoercerAttribute( ICoercerAdapter coercer )
		{
			this.coercer = coercer;
		}

		sealed class Constructors : TypedAspectConstructors<ICoercerAdapter, ApplyCoercerAttribute>
		{
			public static Constructors Default { get; } = new Constructors();
			Constructors() : base( Source.Default.Get ) {}
		}

		public ICoercerAdapter Get() => coercer;
		// object ISource.Get() => Get();
	}
}