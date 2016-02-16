using DragonSpark.ComponentModel;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.Testing.Framework.Setup.Location;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System.Linq;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework.Setup
{
	public class AutoDataCustomization : CustomizationBase, IAutoDataCustomization
	{
		protected override void Customize( IFixture fixture )
		{
			var autoData = new CurrentAutoDataContext().Item.With( OnInitializing );
			var items = autoData?.Items ?? new Items<IAutoDataCustomization>( fixture ).Item;
			items.Ensure( ( IAutoDataCustomization)this );
		}

		void IAutoDataCustomization.Initializing( AutoData data ) => OnInitializing( data );

		protected virtual void OnInitializing( AutoData context ) {}

		void IAutoDataCustomization.Initialized( AutoData data ) => OnInitialized( data );

		protected virtual void OnInitialized( AutoData context ) {}
	}

	public class OutputCustomization : AutoDataCustomization
	{
		[Locate]
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