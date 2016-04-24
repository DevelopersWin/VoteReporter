using DragonSpark.Runtime.Values;
using System;

namespace DragonSpark.Testing.Objects
{
	public class ClassCreatedFromDefault
	{
		public ClassCreatedFromDefault( string message )
		{
			var associated = new AssociatedStore<Type, int>( GetType() );
			var count = associated.Value;
			switch ( count )
			{
				case 0:
					associated.Assign( 1 );
					throw new InvalidOperationException( message );
				default:
					Message = message;
					associated.Property.TryDisconnect();
					break;
			}
		}
		public string Message { get; private set; }
	}
}