using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;

namespace DragonSpark.Aspects
{
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public sealed class ApplyValuesFromSource : InstanceLevelAspect
	{
		readonly Action<object> apply;

		public ApplyValuesFromSource() : this( ApplyValuesFromSourceCommand.Default.Execute ) {}

		public ApplyValuesFromSource( Action<object> apply )
		{
			this.apply = apply;
		}

		[OnInstanceConstructedAdvice, UsedImplicitly]
		public void OnInstanceConstructed() => apply( Instance );
	}
}
