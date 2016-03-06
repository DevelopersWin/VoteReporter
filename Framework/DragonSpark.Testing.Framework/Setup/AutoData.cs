using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public delegate IEnumerable<object[]> AutoDataFactory( MethodInfo method );

	public class AutoData : IDisposable
	{
		readonly AutoDataFactory factory;

		public AutoData( [Required]IFixture fixture, [Required]MethodInfo method, [Required] AutoDataFactory factory )
		{
			this.factory = factory;
			Fixture = fixture;
			Method = method;
			Items = new List<IAutoDataCustomization>( new object[] { Fixture, Method }.SelectMany( o => new Items<IAutoDataCustomization>( o ).Item.Purge() ) );
		}

		public AutoData Initialize()
		{
			Items.ToArray().Each( customization => customization.Initializing( this ) );
			return this;
		}

		[Freeze]
		public IEnumerable<object[]> Apply() => Results = factory( Method );

		public IEnumerable<object[]> Results { get; private set; }

		public IFixture Fixture { get; }

		public MethodInfo Method { get; }

		public IList<IAutoDataCustomization> Items { get; }

		public void Dispose() => Items.Purge().Each( customization => customization.Initialized( this ) );
	}
}