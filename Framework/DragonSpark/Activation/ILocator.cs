using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Activation
{
	public interface IActivator : IValidatedParameterizedSource<TypeRequest, object> {}

	public class LocateTypeRequest : TypeRequest, IEquatable<LocateTypeRequest> /*, IEquatable<LocateTypeRequest>*/
	{
		readonly int code;

		public LocateTypeRequest( [Required] Type type, string name = null ) : base( type )
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

	public abstract class TypeRequest : IEquatable<TypeRequest>
	{
		readonly int code;

		protected TypeRequest( Type type )
		{
			RequestedType = type;

			code = RequestedType.GetHashCode();
		}

		public Type RequestedType { get; }

		public bool Equals( TypeRequest other ) => ReferenceEquals( this, other ) || RequestedType == other?.RequestedType;

		public override bool Equals( object obj ) => obj is TypeRequest && Equals( (TypeRequest)obj );

		public override int GetHashCode() => code;

		public static bool operator ==( TypeRequest left, TypeRequest right ) => Equals( left, right );

		public static bool operator !=( TypeRequest left, TypeRequest right ) => !Equals( left, right );
	}
}