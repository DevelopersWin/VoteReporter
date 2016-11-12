using DragonSpark.Aspects.Adapters;
using JetBrains.Annotations;
using System;
using Aspect = DragonSpark.Aspects.Coercion.Aspect;

namespace DragonSpark.Aspects.Alteration
{
	public sealed class ApplyAlteration : ApplyAlterationBase
	{
		public ApplyAlteration( Type alterationType ) : base( Constructors<ApplyAlteration>.Default.Get( alterationType ), Definition<Aspect>.Default ) {}

		[UsedImplicitly]
		public ApplyAlteration( IAlterationAdapter alteration ) : base( alteration ) {}
	}
}