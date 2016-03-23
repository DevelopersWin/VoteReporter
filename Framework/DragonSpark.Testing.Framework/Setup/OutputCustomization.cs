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
using Serilog;
using System.Collections;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework.Setup
{
	public abstract class AutoDataCustomization : CustomizationBase, IAutoDataCustomization
	{
		protected override void OnCustomize( IFixture fixture )
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

	public class ListValue<T> : FixedValue<T>
	{
		readonly IList list;

		public ListValue( [Required] IList list )
		{
			this.list = list;
		}

		public override void Assign( T item )
		{
			if ( item == null )
			{
				Remove( Item );
			}
			else if ( !list.Contains( item ) )
			{
				list.Add( item );
			}
			
			base.Assign( item );
		}

		void Remove( T item )
		{
			if ( item != null && list.Contains( item ) )
			{
				list.Remove( item );
			}
		}

		protected override void OnDispose() => Remove( Item );
	}

	public class TraceListenerListValue : ListValue<TraceListener>
	{
		public TraceListenerListValue() : base( Trace.Listeners ) {}

		protected override void OnDispose()
		{
			Item.Dispose();
			base.OnDispose();
		}
	}

	public class OutputCustomization : AutoDataCustomization
	{
		readonly TraceListenerListValue value = new TraceListenerListValue();

		[Service]
		public ILogger Logger { [return: Required]get; set; }

		[Service]
		public RecordingLogEventSink Sink { [return: Required]get; set; }

		protected override void OnInitializing( AutoData context )
		{
			value.Assign( new SerilogTraceListener.SerilogTraceListener( Logger ) );
			base.OnInitializing( context );
		}

		protected override void OnInitialized( AutoData context )
		{
			var declaringType = context.Method.DeclaringType;
			if ( declaringType.GetConstructors().Where( info => info.IsPublic ).Any( info => info.GetParameters().Any( parameterInfo => parameterInfo.ParameterType == typeof(ITestOutputHelper) ) ) )
			{
				var lines = PurgingEventFactory.Instance.Create( Sink );
				new OutputValue( declaringType ).Assign( lines );	
			}
			value.Dispose();
		}
	}
}