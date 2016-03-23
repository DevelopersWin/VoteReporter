using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using System.Diagnostics;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Setup.Registration
{
	public class MetadataRegistrationCommandTests : Tests
	{
		public MetadataRegistrationCommandTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void First()
		{
			var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				ApplicationFactory.Instance.Create( methodUnderTest );
			} );
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );
		}

		[Fact]
		public void Second() 
		{
			var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				ApplicationFactory.Instance.Create( methodUnderTest );
			} );
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );
		}
	}
}