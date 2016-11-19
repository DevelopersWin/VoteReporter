using DragonSpark.Aspects.Diagnostics;
using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Data;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Application;
using DragonSpark.Testing.Framework.Application.Setup;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Serilog.Events;
using System;
using Xunit;
using Xunit.Abstractions;
using AddSeqSinkConfiguration = DragonSpark.Aspects.Diagnostics.AddSeqSinkConfiguration;

namespace DragonSpark.Testing.Runtime.Data
{
	public class SerializerTests : TestCollectionBase
	{
		const string Property = "Property2a258824-6489-4cd5-92e9-d6dd98d76002";
		readonly static string Expected = $@"<ClassWithProperty xmlns=""http://schemas.datacontract.org/2004/07/DragonSpark.Testing.Objects"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Property>{Property}</Property></ClassWithProperty>";
		public SerializerTests( ITestOutputHelper output ) : base( output ) {}


		[Theory, AutoData, AdditionalTypes( typeof(Serializer) )]
		public void Save( [Service]ISerializer sut, ClassWithProperty item )
		{
			item.Property = Property;
			Assert.IsType<Serializer>( sut );
			Assert.Equal( Expected, sut.Save( item ) );
		}

		[Theory, AutoData, AdditionalTypes( typeof(Serializer) )]
		public void Load( [Service]ISerializer sut )
		{
			Assert.IsType<Serializer>( sut );

			var loaded = Assert.IsType<ClassWithProperty>( sut.Load<ClassWithProperty>( Expected ) );
			Assert.Equal( "Property2a258824-6489-4cd5-92e9-d6dd98d76002", loaded.Property );
		}

		[Fact]
		public void VerifySerialization()
		{
			var source = new AssemblyBasedTypeSource( typeof(InitializeDiagnosticsCommand) );

			Assert.Contains( typeof(AddSeqSinkConfiguration), source );

			var serializer = new Serializer( new DataContractSerializers( source ).Get );

			var endpoint = new Uri( "http://localhost:12345" );
			var data = serializer.Save( new DiagnosticsConfiguration
										{
											MinimumLevel = LogEventLevel.Warning,
											KnownApplicationTypes = new TypeCollection( GetType().AssemblyQualifiedName ),
											Configurations = new DtoCollection<ILoggingConfiguration>(
																 new AddSeqSinkConfiguration { Endpoint = endpoint }
															 )
										} );
			WriteLine( data );

			var item = serializer.Load<DiagnosticsConfiguration>( data );
			Assert.Equal( LogEventLevel.Warning, item.MinimumLevel );
			Assert.Contains( GetType(), item.KnownApplicationTypes );
			var configuration = Assert.Single( item.Configurations.Get().OfType<DragonSpark.Diagnostics.Configurations.AddSeqSinkConfiguration>() );
			Assert.Equal( endpoint, configuration.Endpoint );
		}
	}
}