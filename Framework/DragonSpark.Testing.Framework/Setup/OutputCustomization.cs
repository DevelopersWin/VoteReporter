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
using System;
using System.Composition;
using System.Linq;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework.Setup
{
	public class Testing : Attribute
	{
		[Service]
		public AutoData AutoData { get; set; }
	}

	[Testing]
	public abstract class AutoDataCustomization : CustomizationBase, IAutoDataCustomization
	{
		[Service]
		public AutoData AutoData { get; set; }

		protected override void OnCustomize( IFixture fixture )
		{
			var autoData = Services.Get<AutoData>().With( OnInitializing );
			var items = autoData?.Items ?? new Items<IAutoDataCustomization>( fixture ).Item;
			items.Ensure( ( IAutoDataCustomization)this );
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
			var type = TypeSupport.From( request );
			object export;
			var result = host.TryGetExport( type, out export ) ? export : new NoSpecimen();
			return result;
		}
	}

	public class OutputCustomization : AutoDataCustomization
	{
		[Service]
		public ILoggerHistory History { [return: Required]get; set; }

		protected override void OnInitialized( AutoData context )
		{
			var declaringType = context.Method.DeclaringType;
			if ( declaringType.GetConstructors().Where( info => info.IsPublic ).Any( info => info.GetParameters().Any( parameterInfo => parameterInfo.ParameterType == typeof(ITestOutputHelper) ) ) )
			{
				var lines = LogEventMessageFactory.Instance.Create( History.Events );
				new OutputValue( declaringType ).Assign( lines );	
			}
		}
	}
}