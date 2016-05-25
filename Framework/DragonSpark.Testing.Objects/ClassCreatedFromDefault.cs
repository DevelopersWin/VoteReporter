using DragonSpark.Runtime.Values;
using System;

namespace DragonSpark.Testing.Objects
{
	public class ClassCreatedFromDefault
	{
		readonly static IAttachedProperty<Type, int> Property = new AttachedProperty<Type, int>();

		public ClassCreatedFromDefault( string message )
		{
			var instance = GetType();
			switch ( Property.Get( instance ) )
			{
				case 0:
					Property.Set( instance, 1 );
					throw new InvalidOperationException( message );
				default:
					Message = message;
					Property.Clear( instance );
					break;
			}
		}

		public string Message { get; private set; }
	}
}