using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Testing.Framework;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Runtime
{
	public class DelegateTests : TestCollectionBase
	{
		public DelegateTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void BasicInvocation()
		{
			Action instance = null;
			var called = false;
			instance = () =>
					   {
						   var current = Invocation.GetCurrent();
						   Assert.NotNull( current );
						   Assert.Same( instance, current );
						   called = true;
					   };
			var action = Invocation.Create( instance );
			Assert.Null( Invocation.GetCurrent() );
			Assert.False( called );
			action();
			Assert.True( called );
			Assert.Null( Invocation.GetCurrent() );
		}

		[Theory, AutoData]
		void PropertyContext( Factory factory, Parameter parameter, string name )
		{
			var @delegate = factory.Create( parameter );
			Assert.Null( factory.Current() );
			var result = @delegate( name );
			Assert.Null( factory.Current() );
			Assert.Equal( name, result.Name );
			Assert.Equal( parameter, result.Context );
		}

		class Factory : FactoryBase<Parameter, Get>
		{
			readonly IAttachedProperty<Get, Parameter> property = new AttachedProperty<Get, Parameter>();

			public override Get Create( Parameter parameter ) => property.Apply( Get, parameter );

			Result Get( string name ) => new Result( name, property.Context() );

			public Parameter Current() => property.Context();
		}

		delegate Result Get( string name );

		class Parameter {}

		struct Result
		{
			public Result( string name, Parameter context )
			{
				Name = name;
				Context = context;
			}

			public string Name { get; }
			public Parameter Context { get; }
		}
		
	}

	public static class Temp1
	{
		public static IEnumerable<TSource> Append<TSource>( this IEnumerable<TSource> collection, TSource element )
		{
			foreach ( var element1 in collection )
				yield return element1;
			yield return element;
		}
	}
}