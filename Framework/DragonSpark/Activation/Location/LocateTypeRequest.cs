using System;

namespace DragonSpark.Activation.Location
{
	public class LocateTypeRequest : TypeRequest, IEquatable<LocateTypeRequest> /*, IEquatable<LocateTypeRequest>*/
	{
		readonly int code;

		public LocateTypeRequest( Type type, string name = null ) : base( type )
		{
			Name = name;

			unchecked
			{
				code = base.GetHashCode() * 397 ^ Name?.GetHashCode() ?? 0;
			}
		}

		public string Name { get; }

		public bool Equals( LocateTypeRequest other ) => base.Equals( other ) && string.Equals( Name, other.Name );

		public override bool Equals( object obj ) => obj is LocateTypeRequest && Equals( (LocateTypeRequest)obj );

		public override int GetHashCode() => code;

		public static bool operator ==( LocateTypeRequest left, LocateTypeRequest right ) => Equals( left, right );

		public static bool operator !=( LocateTypeRequest left, LocateTypeRequest right ) => !Equals( left, right );
	}
}