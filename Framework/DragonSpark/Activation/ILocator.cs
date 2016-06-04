using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using DragonSpark.TypeSystem;

namespace DragonSpark.Activation
{
	public interface IActivator : IFactory<TypeRequest, object> {}

	// public interface IConstructor : IActivator, IFactory<ConstructTypeRequest, object> {}

	// public interface ILocator : IActivator, IFactory<LocateTypeRequest, object> {}

	public class LocateTypeRequest : TypeRequest
	{
		readonly int code;

		public LocateTypeRequest( [Required] Type type, string name = null ) : base( type )
		{
			Name = name;
			code = KeyFactory.Instance.CreateUsing( RequestedType, name );
		}

		public string Name { get; }

		public override int GetHashCode() => code;
	}

	public class ConstructTypeRequest : TypeRequest
	{
		readonly int code;

		public ConstructTypeRequest( [Required] Type type, [Required] params object[] arguments ) : base( type )
		{
			Arguments = arguments;
			code = KeyFactory.Instance.CreateUsing( RequestedType, Arguments );
		}

		public object[] Arguments { get; }

		public override int GetHashCode() => code;
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