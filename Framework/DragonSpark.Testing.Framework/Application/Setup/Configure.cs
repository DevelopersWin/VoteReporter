using DragonSpark.Application.Setup;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Testing.Framework.Runtime;
using System;
using System.Composition;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	public sealed class Configure : TransformerBase<IServiceProvider>
	{
		[Export( typeof(ITransformer<IServiceProvider>) )]
		public static Configure Default { get; } = new Configure();
		Configure() {}

		public override IServiceProvider Get( IServiceProvider parameter ) => 
			new CompositeServiceProvider( new InstanceRepository<object>( new SourceCollection( FixtureContext.Default, MethodContext.Default ) ), new FixtureServiceProvider( FixtureContext.Default.Get() ), parameter );
	}
}