using DragonSpark.Extensions;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp;
using PostSharp.Extensibility;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		public AutoDataAttribute() : this( FixtureFactory<DefaultAutoDataCustomization>.Instance.Create ) {}

		protected AutoDataAttribute( [Required]Func<IFixture> fixture ) : base( Testes( fixture ) ) {}

		static IFixture Testes( Func<IFixture> fixture )
		{
			try
			{
				return fixture();
			}
			catch ( Exception e )
			{
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "0001", $"HELLO {e}", null, null, null ) );
				throw;
			}
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( methodUnderTest ) ) )
			{
				using ( var autoData = new AutoData( Fixture, methodUnderTest ) )
				{
					using ( new AssignAutoDataCommand().ExecuteWith( autoData ) )
					{
						var result = base.GetData( methodUnderTest );
						return result;
					}
				}
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance>( info => new AspectInstance( info, new AssignExecutionContextAspect() ) ).ToItem();
	}
}