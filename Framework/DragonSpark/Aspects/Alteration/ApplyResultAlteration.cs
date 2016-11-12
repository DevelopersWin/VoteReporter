using System;
using DragonSpark.Aspects.Adapters;
using JetBrains.Annotations;

namespace DragonSpark.Aspects.Alteration
{
	public sealed class ApplyResultAlteration : ApplyAlterationBase
	{
		public ApplyResultAlteration( Type alterationType ) : base( Constructors<ApplyResultAlteration>.Default.Get( alterationType ), Definition<ResultAspect>.Default ) {}

		[UsedImplicitly]
		public ApplyResultAlteration( IAlterationAdapter alteration ) : base( alteration ) {}
	}
}