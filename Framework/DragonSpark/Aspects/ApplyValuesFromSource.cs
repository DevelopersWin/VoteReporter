using DragonSpark.Application.Setup;
using DragonSpark.Commands;
using DragonSpark.Extensions;
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

	public class ApplyValuesFromSourceCommand : CommandBase<object>
	{
		public static ApplyValuesFromSourceCommand Default { get; } = new ApplyValuesFromSourceCommand();
		ApplyValuesFromSourceCommand() : this( Instances.Default ) {}

		readonly IServiceRepository repository;

		[UsedImplicitly]
		public ApplyValuesFromSourceCommand( IServiceRepository repository )
		{
			this.repository = repository;
		}

		public override void Execute( object parameter )
		{
			var type = parameter.GetType();
			if ( repository.IsSatisfiedBy( type ) )
			{
				var source = repository.GetService( type );
				source.MapInto( parameter );
			}
		}
	}
}
