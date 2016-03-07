using DragonSpark.Extensions;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly Func<Application> application;

		public AutoDataAttribute() : this( () => new Application() ) {}

		protected AutoDataAttribute( Func<Application> application ) : this( FixtureFactory<DefaultAutoDataCustomization>.Instance.Create, application ) {}

		protected AutoDataAttribute( Func<IFixture> fixture  ) : this( fixture, () => new Application() ) {}

		protected AutoDataAttribute( [Required]Func<IFixture> fixture, [Required]Func<Application> application  ) : base( fixture() )
		{
			this.application = application;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			using ( application().ExecuteWith( new AutoData( Fixture, methodUnderTest ) ) )
			{
				var result = base.GetData( methodUnderTest );
				return result;
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance>( info => new AspectInstance( info, new AssignExecutionContextAspect() ) ).ToItem();
	}
}