﻿using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Sources;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using JetBrains.Annotations;
using System.Composition;
using System.Reflection;
using Xunit;
using ExecutionContext = DragonSpark.Testing.Framework.ExecutionContext;

namespace DragonSpark.Testing.Activation
{
	[ContainingTypeAndNested]
	public class ApplicationContextTests
	{
		[Fact]
		public void Assignment()
		{
			var before = Execution.Current();
			var current = Assert.IsType<TaskContext>( before );
			Assert.Same( ExecutionContext.Default.Get(), current );
			Assert.Equal( Identification.Default.Get(), current.Origin );
			Assert.Null( MethodContext.Default.Get() );
			Assert.True( EnableMethodCaching.Default.Get() );

			var method = MethodBase.GetCurrentMethod();
			object inner;
			using ( ApplicationFactory.Default.Get( method ).Run( new AutoData( FixtureContext.Default.WithFactory( FixtureFactory<AutoDataCustomization>.Default.Get ), method ) ) )
			{
				Assert.NotNull( MethodContext.Default.Get() );
				Assert.Same( method, MethodContext.Default.Get() );
				inner = Execution.Current();
				Assert.Same( current, inner );
				Assert.False( EnableMethodCaching.Default.Get() );
			}

			var after = Execution.Current();
			Assert.NotNull( after );
			Assert.NotSame( inner, after );
			Assert.NotSame( current, after );

			Assert.Null( MethodContext.Default.Get() );

			Assert.True( EnableMethodCaching.Default.Get() );
		}

		[Export( typeof(ISetup) ), UsedImplicitly]
		class Setup : DragonSpark.Setup.Setup
		{
			public Setup() : base( EnableMethodCaching.Default.Configured( false ) ) {}
		}
	}
}