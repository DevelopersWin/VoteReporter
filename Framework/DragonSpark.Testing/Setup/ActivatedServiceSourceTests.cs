﻿using DragonSpark.Activation.Location;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using DragonSpark.Application.Setup;
using Xunit;

namespace DragonSpark.Testing.Setup
{
	public class ActivatedServiceSourceTests
	{
		[Fact]
		public void RecursionCheck()
		{
			var count = 0;
			IValidatedParameterizedSource<Type, object> sut = null;
			var provider = new DecoratedServiceProvider( type =>
														 {
															 ++count;
															 if ( type == typeof(int) )
																 return count;

															 if ( count > 3 )
															 {
																 throw new InvalidOperationException( "Recursion detected" );
															 }

															 // ReSharper disable once AccessToModifiedClosure
															 return sut.Get( type );
														 } );

			sut = new ActivatedServiceSource( provider );
			var first = sut.Get( typeof(int) );
			Assert.Null( first );
			ServicesEnabled.Default.Assign( true );
			Assert.Equal( 0, count );
			Assert.Equal( 1,  sut.Get( typeof(int) ) );
			Assert.Equal( 1, count );

			Assert.Null( sut.Get( GetType() ) );
			Assert.Equal( 3, count );
		}
	}
}