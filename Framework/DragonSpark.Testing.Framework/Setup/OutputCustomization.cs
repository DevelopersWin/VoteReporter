using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.Testing.Framework.Setup.Location;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using PostSharp.Patterns.Contracts;
using System.Composition;
using System.Linq;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework.Setup
{
	public class AutoDataCustomization : CustomizationBase, IAutoDataCustomization
	{
		protected override void Customize( IFixture fixture )
		{
			var autoData = Services.Get<AutoData>();
			if ( autoData != null )
			{
				OnInitializing( autoData );
			}
			else
			{
				new Items<IAutoDataCustomization>( fixture ).Item.Ensure( (IAutoDataCustomization)this );
			}
		}

		void IAutoDataCustomization.Initializing( AutoData data ) => OnInitializing( data );

		protected virtual void OnInitializing( AutoData context ) {}

		void IAutoDataCustomization.Initialized( AutoData data ) => OnInitialized( data );

		protected virtual void OnInitialized( AutoData context ) {}
	}

	public class CompositionCustomization : AutoDataCustomization
	{
		[Required, Compose]
		public CompositionRelay Relay { [return: Required]get; set; }

		protected override void OnInitializing( AutoData context )
		{
			base.OnInitializing( context );
			context.Fixture.ResidueCollectors.Add( Relay );
		}
	}

	[Export]
	public class CompositionRelay : ISpecimenBuilder
	{
		readonly CompositionContext host;

		[ImportingConstructor]
		public CompositionRelay( [Required]CompositionContext host )
		{
			this.host = host;
		}

		public object Create( object request, ISpecimenContext context )
		{
			var type = Type.From( request );
			object export;
			var result = host.TryGetExport( type, out export ) ? export : new NoSpecimen();
			return result;
		}
	}

	public class OutputCustomization : AutoDataCustomization
	{
		[Compose]
		public RecordingLogEventSink Logger { [return: Required]get; set; }

		protected override void OnInitialized( AutoData context )
		{
			var declaringType = context.Method.DeclaringType;
			if ( declaringType.GetConstructors().Where( info => info.IsPublic ).Any( info => info.GetParameters().Any( parameterInfo => parameterInfo.ParameterType == typeof(ITestOutputHelper) ) ) )
			{
				var item = Logger.Purge().OrderBy( line => line.Timestamp ).Select( line => line.RenderMessage() ).ToArray();
				new OutputValue( declaringType ).Assign( item );	
			}
		}
	}
}