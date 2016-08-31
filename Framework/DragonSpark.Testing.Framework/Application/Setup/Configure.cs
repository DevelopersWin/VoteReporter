using DragonSpark.Application.Setup;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Testing.Framework.Runtime;
using System;
using System.Composition;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	public sealed class Configure : AlterationBase<IServiceProvider>
	{
		[Export( typeof(IAlteration<IServiceProvider>) )]
		public static Configure Default { get; } = new Configure();
		Configure() {}

		public override IServiceProvider Get( IServiceProvider parameter ) => 
			new CompositeServiceProvider( new InstanceRepository( FixtureContext.Default, MethodContext.Default ), new FixtureServiceProvider( FixtureContext.Default.Get() ), parameter );
	}
}