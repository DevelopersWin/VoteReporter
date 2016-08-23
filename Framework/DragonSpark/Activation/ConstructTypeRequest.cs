using System;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem;

namespace DragonSpark.Activation
{
	public class ConstructTypeRequest : TypeRequest, IEquatable<ConstructTypeRequest>
	{
		readonly static StructuralEqualityComparer<object[]> Comparer = StructuralEqualityComparer<object[]>.Default;

		readonly int code;

		public ConstructTypeRequest( Type type ) : this( type, Items<object>.Default ) {}

		public ConstructTypeRequest( Type type, params object[] arguments ) : base( type )
		{
			Arguments = arguments;

			unchecked
			{
				code = base.GetHashCode() * 397 ^ Comparer.GetHashCode( Arguments );
			}
		}

		public object[] Arguments { get; }

		public bool Equals( ConstructTypeRequest other ) => base.Equals( other ) && Comparer.Equals( Arguments, other.Arguments );

		public override bool Equals( object obj ) => obj is ConstructTypeRequest && Equals( (ConstructTypeRequest)obj );

		public override int GetHashCode() => code;

		public static bool operator ==( ConstructTypeRequest left, ConstructTypeRequest right ) => Equals( left, right );

		public static bool operator !=( ConstructTypeRequest left, ConstructTypeRequest right ) => !Equals( left, right );
	}
}