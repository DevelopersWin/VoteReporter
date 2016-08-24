using System;
using System.Collections.Generic;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Testing.Framework.Application.Setup;
using Ploeh.AutoFixture;
using PostSharp.Aspects;

namespace DragonSpark.Testing.Framework.Application
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Default.Get;
		
		public AutoDataAttribute() : this( DefaultFixtureFactory ) {}

		protected AutoDataAttribute( Func<IFixture> fixture ) : base( FixtureContext.Default.WithInstance( fixture() ) ) {}

		protected virtual IApplication ApplicationSource( MethodBase method ) => ApplicationFactory.Default.Get( method );

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var applicationSource = ApplicationSource( methodUnderTest );
			applicationSource.Run( new AutoData( Fixture, methodUnderTest ) );

			var result = base.GetData( methodUnderTest );
			return result;
		}
	}
}