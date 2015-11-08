using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Windows.Markup;

namespace DragonSpark.Windows.Setup
{
	[ContentProperty( "Listener" )]
	public class EventListenerDefinition
	{
		public EventListener Listener { get; set; }

		public IDictionary<Type, EventSourceRegistration> Registrations { get; set; }
	}
}