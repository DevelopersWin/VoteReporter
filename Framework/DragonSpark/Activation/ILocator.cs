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
		readonly CodeContainer<LocateTypeRequest> container;

		public LocateTypeRequest( [Required] Type type, string name = null ) : base( type )
		{
			Name = name;
			container = new CodeContainer<LocateTypeRequest>( type, name );
		}

		public string Name { get; }

		public override int GetHashCode() => container.Code;
	}

	public class ConstructTypeRequest : TypeRequest
	{
		readonly CodeContainer<ConstructTypeRequest> container;

		public ConstructTypeRequest( [Required] Type type, [Required] params object[] arguments ) : base( type )
		{
			Arguments = arguments;
			container = new CodeContainer<ConstructTypeRequest>( Arguments.Prepend( RequestedType ).ToArray()  );
		}

		public object[] Arguments { get; }

		public override int GetHashCode() => container.Code;
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