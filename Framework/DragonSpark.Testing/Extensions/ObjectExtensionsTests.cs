﻿using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using ApplicationServiceProviderFactory = DragonSpark.Setup.ApplicationServiceProviderFactory;
using AssemblyProvider = DragonSpark.Testing.Framework.Setup.AssemblyProvider;

namespace DragonSpark.Testing.Extensions
{
	public class ObjectExtensionsTests
	{
		[Theory, Objects.IoC.AutoData]
		void ProvidedValues( ClassWithProperties sut )
		{
			sut.PropertyOne = null;
			var cloned = sut.Clone( Mappings.OnlyProvidedValues() );
			Assert.Null( cloned.PropertyOne );
		}

		[Fact]
		public void Convert()
		{
			var temp = "123";
			var converted = temp.ConvertTo<int>();
			Assert.Equal( 123, converted );
		}

		[Fact]
		public void GetMemberInfo()
		{
			var info = Check( parameter => parameter.Parameter );
			Assert.Equal( nameof(ClassWithParameter.Parameter), info.Name );
		}

		static MemberInfo Check( Expression<Func<ClassWithParameter, object>> expression ) => expression.GetMemberInfo();

		[Theory, Objects.IoC.AutoData]
		void Ignored( ClassWithProperties sut )
		{
			var other = sut.MapInto<ClassWithDifferentProperties>();
			Assert.Equal( 0, other.PropertyOne );
			Assert.Null( other.PropertyTwo );
			Assert.Equal( sut.PropertyThree, other.PropertyThree );
			Assert.Equal( sut.PropertyFour, other.PropertyFour );
		}

		[Theory, Objects.IoC.AutoData]
		void Clone( ClassWithProperties sut )
		{
			var cloned = sut.Clone();
			Assert.NotSame( sut, cloned );
			Assert.Equal( sut.PropertyOne, cloned.PropertyOne );
			Assert.Equal( sut.PropertyTwo, cloned.PropertyTwo );
			Assert.Equal( sut.PropertyThree, cloned.PropertyThree );
			Assert.Equal( sut.PropertyFour, cloned.PropertyFour );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void WithNull( int number )
		{
			var item = new int?( number );
			var count = 0;
			item.With( new Action<int>( i => count++ ) );
			Assert.Equal( 1, count );
			new int?().With( new Action<int>( i => count++ ) );
			Assert.Equal( 1, count );
		}

		[Theory, Objects.IoC.AutoData]
		void Mapper( Objects.ClassWithProperties instance )
		{
			var mapped = instance.MapInto<ClassWithProperties>();
			Assert.Equal( instance.PropertyOne, mapped.PropertyOne );
			Assert.Equal( instance.PropertyTwo, mapped.PropertyTwo );
			Assert.Equal( instance.PropertyThree, mapped.PropertyThree );
			Assert.Equal( instance.PropertyFour, mapped.PropertyFour );
		}

		class ClassWithProperties
		{
			public string PropertyOne { get; set; }
 
			public int PropertyTwo { get; set; }

			public object PropertyThree { get; set; }

			public string PropertyFour { get; set; }

			public string this[ int index ]
			{
				get { return null; }
				set { }
			}

		}

		class ClassWithDifferentProperties
		{
			public int PropertyOne { get; set; }

			public string PropertyTwo { get; set; }

			public object PropertyThree { get; set; }

			public string PropertyFour { get; set; }
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void TryDispose( Disposable sut )
		{
			Assert.False( sut.Disposed );
			sut.TryDispose();
			Assert.True( sut.Disposed );
		}

		[Fact]
		void Null()
		{
			Class @class = null;

			var called = false;
			@class.Null( () => called = true );
			Assert.True( called );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void Enumerate( List<object> sut )
		{
			var items = sut.GetEnumerator().Enumerate().ToList();
			Assert.True( items.Any() && items.All( x => sut.Contains( x ) && sut.ToList().IndexOf( x ) == items.IndexOf( x ) ) );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void GetAllPropertyValuesOf( ClassWithProperties sut )
		{
			var expected = new[] { sut.PropertyOne, sut.PropertyFour };

			var values = sut.GetAllPropertyValuesOf<string>();
			Assert.True( expected.Length == values.Count() && expected.All( x => values.Contains( x ) ) );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void AsValid( Class sut )
		{
			var applied = false;
			var valid = sut.AsValid<IInterface>( i => applied = true );
			Assert.True( applied );
			Assert.IsType<Class>( valid );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void AsInvalid( string sut )
		{
			Assert.Throws<InvalidOperationException>( () => sut.AsValid<int>( i => Assert.True( false ) ) );
		}

		[Fact]
		void DetermineDefault()
		{
			var item = Default<IEnumerable<object>>.Item;
			Assert.IsType<object[]>( item );
			Assert.Empty( item );
			Assert.Same( item, Default<IEnumerable<object>>.Item );
			Assert.Same( Enumerable.Empty<object>(), Enumerable.Empty<object>() );
			var objects = Default<object>.Items;
			Assert.Same( item, objects );

			var ints = Default<int>.Items;
			Assert.Empty( ints );
			Assert.Same( ints, Default<int>.Items );

			Assert.Null( Default<object>.Item );
			Assert.Null( Default<Generic<object>>.Item );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void With( ClassWithParameter sut, string message )
		{
			Assert.Equal( sut.Parameter, sut.With( x => x.Parameter, () => message ) );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void WithNullable( int supplied )
		{
			var item = new int?( supplied );
			var value = 0;
			var result = item.With( i => value = i );
			Assert.Equal( supplied, result );
			Assert.Equal( supplied, value );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void WithSelf( int supplied, string message )
		{
			string item = null;
			Func<int, string> with = i => item = message;
			var result = supplied.WithSelf( with );
			Assert.Equal( message, item );
			Assert.Equal( supplied, result );
		}

		[Fact]
		public void As()
		{
			var called = false;
			Assert.NotNull( new Class().As<IInterface>( x => called = true ) );
			Assert.True( called );
			Assert.NotNull( new Class().As<IInterface>() );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void AsTo( ClassWithParameter sut, string message )
		{
			var value = sut.AsTo<Class, object>( x => x, () => message );
			Assert.Equal( value, message );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void ConvertTo( Class sample )
		{
			Assert.Equal( true, "true".ConvertTo<bool>() );
			Assert.Equal( 6776, "6776".ConvertTo<int>() );

			Assert.Equal( BindingDirection.OneWay, "OneWay".ConvertTo<BindingDirection>() );

			Assert.Equal( sample, sample.ConvertTo( typeof(Class) ) );
			Assert.Null( sample.ConvertTo( typeof(ClassWithParameter) ) );
		}

	}
}