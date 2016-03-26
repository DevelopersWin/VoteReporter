using DragonSpark.Aspects;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;

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

		protected AssemblyProviderBase( IEnumerable<System.Type> types, params Assembly[] assemblies ) : this( types.Assemblies().Concat( assemblies ).ToArray() ) {}

		protected AssemblyProviderBase( params System.Type[] types ) : this( types.Assemblies() ) {}

		protected AssemblyProviderBase( [Required]params Assembly[] assemblies )
		{
			this.assemblies = assemblies;
		}

		protected override Assembly[] CreateItem() => assemblies.NotNull().Distinct().Prioritize().ToArray();
	}

	public class AggregateAssemblyFactory : AggregateFactory<Assembly[]>, IAssemblyProvider
	{
		public AggregateAssemblyFactory( IFactory<Assembly[]> primary, params ITransformer<Assembly[]>[] transformers ) : base( primary, transformers ) {}

		public AggregateAssemblyFactory( Func<Assembly[]> primary, params Func<Assembly[], Assembly[]>[] transformers ) : base( primary, transformers ) {}
	}
}