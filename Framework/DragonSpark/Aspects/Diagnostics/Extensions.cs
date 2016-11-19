using DragonSpark.Commands;
using DragonSpark.Runtime;
using PostSharp;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects.Diagnostics
{
	public static class Extensions
	{
		public static void Execute( this ICommand<Message> @this, object element, string format, params object[] arguments )
			=> @this.Execute( MessageLocation.Of( element ), format, arguments );

		public static void Execute( this ICommand<Message> @this, MessageLocation messageLocation, string format, params object[] arguments )
			=> Execute( @this, messageLocation, SeverityType.Info, format, arguments );

		public static void Execute( this ICommand<Message> @this, object element, SeverityType severity, string format, params object[] arguments )
			=> Execute( @this, MessageLocation.Of( element ), severity, format, arguments );

		public static void Execute( this ICommand<Message> @this, MessageLocation messageLocation, SeverityType severity, string format, params object[] arguments ) => 
			@this.Execute( new Message( messageLocation, severity, TextHasher.Default.Get( format ), string.Format( format, arguments ), null, null, null ) );
	}
}