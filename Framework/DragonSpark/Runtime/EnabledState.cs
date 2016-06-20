using System.Collections.Generic;

namespace DragonSpark.Runtime
{
	public class EnabledState
	{
		readonly static int Null = new object().GetHashCode();

		readonly HashSet<int> codes = new HashSet<int>();

		// long store;

		static int GetCode( object item ) => item?.GetHashCode() ?? Null;

		public bool IsEnabled( object item )
		{
			var code = GetCode( item );
			return codes.Contains( code );
		}

		public void Enable( object item, bool on )
		{
			var code = GetCode( item );
			/*
			BitArray bits = new BitArray(System.BitConverter.GetBytes(code));

			bits.*/
			if ( on )
			{
				codes.Add( code );
				// store |= code;
			}
			else
			{
				codes.Remove( code );
				// store &= ~code;
			}
		}
	}

	public static class EnabledStateExtensions
	{
		public static Assignment<object, bool> Assignment( this EnabledState @this, object first, bool second = true ) => 
			new Assignment<object, bool>( new EnabledStateAssign( @this ), Assignments.From( first ), new Value<bool>( second ) );
	}
}