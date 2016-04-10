using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Extensibility;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public abstract class AssemblySourceBase : FactoryBase<Assembly[]>
	{
		[Freeze( AttributeInheritance = MulticastInheritance.Multicast, AttributeTargetMemberAttributes = MulticastAttributes.Instance )]
		protected abstract override Assembly[] CreateItem();
	}

	public abstract class AssemblyProviderBase : AssemblySourceBase, IAssemblyProvider
	{
		readonly Assembly[] assemblies;

		protected AssemblyProviderBase( IEnumerable<Type> types, params Assembly[] assemblies ) : this( types.Assemblies().Union( assemblies ).ToArray() ) {}

		protected AssemblyProviderBase( params Type[] types ) : this( types.Assemblies() ) {}

		AssemblyProviderBase( [Required] IEnumerable<Assembly> assemblies )
		{
			this.assemblies = assemblies.NotNull().Distinct().Prioritize().ToArray();
		}

		protected override Assembly[] CreateItem() => assemblies;
	}

	public class AggregateAssemblyFactory : AggregateFactory<Assembly[]>, IAssemblyProvider
	{
		// public AggregateAssemblyFactory( IFactory<Assembly[]> primary, params ITransformer<Assembly[]>[] transformers ) : base( primary, transformers ) {}

		public AggregateAssemblyFactory( Func<Assembly[]> primary, params Func<Assembly[], Assembly[]>[] transformers ) : base( primary, transformers ) {}
	}
}