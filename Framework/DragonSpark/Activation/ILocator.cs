using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Activation
{
	public interface IActivator : IFactory<TypeRequest, object> {}

	public interface IConstructor : IActivator, IFactory<ConstructTypeRequest, object> {}

	public interface ILocator : IActivator, IFactory<LocateTypeRequest, object> {}

	public class LocateTypeRequest : TypeRequest
	{
		public LocateTypeRequest( [Required] Type type, string name = null ) : base( type )
		{
			Name = name;
		}

		public string Name { get; }

		public override int GetHashCode() => KeyFactory.Instance.CreateUsing( RequestedType, Name );
	}

	public class ConstructTypeRequest : TypeRequest
	{
		// public ConstructTypeRequest( Type type ) : this( type, Default<object>.Items ) {}

		public ConstructTypeRequest( [Required] Type type, [Required] params object[] arguments ) : base( type )
		{
			Arguments = arguments;
		}

		public object[] Arguments { get; }

		public override int GetHashCode() => KeyFactory.Instance.Create( RequestedType.Append( Arguments ) );
	}

	public abstract class TypeRequest
	{
		protected TypeRequest( [Required]Type type )
		{
			RequestedType = type;
		}

		public Type RequestedType { get; }
	}

	/*public class TypeRequest
	{
		public TypeRequest( [Required]Type requestedType, string name = null, params object[] parameters )
		{
			RequestedType = requestedType;
			Name = name;
			Parameters = parameters;
		}

		public Type RequestedType { get; }

		public string Name { get; }

		public object[] Parameters { get; }

		
	}*/
}