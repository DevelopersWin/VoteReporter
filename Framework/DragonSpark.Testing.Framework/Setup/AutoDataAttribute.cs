using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class AssociatedApplication : AssociatedValue<MethodBase, IApplication>
	{
		public AssociatedApplication( MethodBase instance ) : base( instance, () => null ) {}
	}

	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly Func<IApplication> application;

		public AutoDataAttribute() : this( () => new Application() ) {}

		protected AutoDataAttribute( Func<IApplication> application ) : this( FixtureFactory<DefaultAutoDataCustomization>.Instance.Create, application ) {}

		protected AutoDataAttribute( Func<IFixture> fixture  ) : this( fixture, () => new Application() ) {}

		protected AutoDataAttribute( [Required]Func<IFixture> fixture, [Required]Func<IApplication> application  ) : base( fixture() )
		{
			this.application = application;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( methodUnderTest ) ) )
			{
				var autoData = new AutoData( Fixture, methodUnderTest );
				using ( new ExecuteAutoDataApplicationCommand( application() ).ExecuteWith( autoData ) )
				{
					var result = base.GetData( methodUnderTest );
					return result;
				}
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance>( info => new AspectInstance( info, new AssignExecutionContextAspect() ) ).ToItem();
	}
}