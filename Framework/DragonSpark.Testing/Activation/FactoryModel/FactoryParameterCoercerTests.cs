﻿using Ploeh.AutoFixture.Xunit2;
using System;
using DragonSpark.Activation;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryParameterCoercerTests
	{
		[Theory, AutoData]
		void Parameter( ParameterCoercer<IntegerParameter> sut, int item )
		{
			var parameter = sut.Coerce( item );
			Assert.NotNull( parameter );
			Assert.Equal( parameter.SomeInteger, item );
			
		}

		[Theory, AutoData]
		void ConstructParameter( ConstructorBase.Coercer sut, Type item )
		{
			var parameter = sut.Coerce( item );
			Assert.NotNull( parameter );
			Assert.Equal( parameter.RequestedType, item );
		}

		[Theory, AutoData]
		public void Fixed( [Frozen]Guid guid, [Greedy]FixedParameterCoercer<Guid> sut, object parameter )
		{
			var result = sut.Coerce( parameter );
			Assert.Equal( guid, result );
		}

		class IntegerParameter
		{
			public IntegerParameter( int someInteger )
			{
				SomeInteger = someInteger;
			}

			public int SomeInteger { get; }
		}
	}
}