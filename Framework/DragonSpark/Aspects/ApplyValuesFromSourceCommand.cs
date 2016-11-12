using DragonSpark.Application.Setup;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using JetBrains.Annotations;

namespace DragonSpark.Aspects
{
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