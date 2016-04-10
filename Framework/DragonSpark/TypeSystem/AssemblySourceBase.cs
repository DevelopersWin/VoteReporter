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
	public class FactoryTypeRequest : LocateTypeRequest
	{
		public FactoryTypeRequest( [Required]Type runtimeType, string name, [Required]Type resultType ) :  base( runtimeType, name )
		{
			ResultType = resultType;
		}

		public Type ResultType { get; }
	}

	public class AssembliesFactory : FactoryBase<Type[], Assembly[]>
	{
		public static AssembliesFactory Instance { get; } = new AssembliesFactory();

		[Freeze]
		protected override Assembly[] CreateItem( Type[] parameter ) => parameter.Assemblies();
	}

	public static class AssemblyTypes
	{
		public static AssemblyTypesFactory All { get; } = new AssemblyTypesFactory( assembly => assembly.DefinedTypes.AsTypes() );

		public static AssemblyTypesFactory Public { get; } = new AssemblyTypesFactory( assembly => assembly.ExportedTypes );
	}

	public class AssemblyTypesFactory : FactoryBase<Assembly, Type[]>
	{
		readonly Func<Assembly, IEnumerable<Type>> types;

		public AssemblyTypesFactory( [Required] Func<Assembly, IEnumerable<Type>> types )
		{
			this.types = types;
		}

		[Freeze]
		protected override Type[] CreateItem( Assembly parameter ) => types( parameter ).Where( ApplicationTypeSpecification.Instance.IsSatisfiedBy ).Fixed();
	}

	public class TypesFactory : FactoryBase<Assembly[], Type[]>
	{
		public static TypesFactory Instance { get; } = new TypesFactory();

		[Freeze]
		protected override Type[] CreateItem( Assembly[] parameter ) => parameter.SelectMany( AssemblyTypes.All.Create ).ToArray();
	}

	public abstract class AssemblySourceBase : FactoryBase<Assembly[]>
	{
		[Freeze( AttributeInheritance = MulticastInheritance.Multicast, AttributeTargetMemberAttributes = MulticastAttributes.Instance )]
		protected abstract override Assembly[] CreateItem();
	}

	public abstract class AssemblyProviderBase : AssemblySourceBase, IAssemblyProvider
	{
		readonly Assembly[] assemblies;

		protected AssemblyProviderBase( IEnumerable<Type> types, params Assembly[] assemblies ) : this( AssembliesFactory.Instance.Create( types.Fixed() ).Union( assemblies ).ToArray() ) {}

		protected AssemblyProviderBase( params Type[] types ) : this( AssembliesFactory.Instance.Create( types ) ) {}

		AssemblyProviderBase( [Required] IEnumerable<Assembly> assemblies )
		{
			this.assemblies = assemblies.NotNull().Distinct().Prioritize().Fixed();
		}

		protected override Assembly[] CreateItem() => assemblies;
	}

	public class AggregateAssemblyFactory : AggregateFactory<Assembly[]>, IAssemblyProvider
	{
		// public AggregateAssemblyFactory( IFactory<Assembly[]> primary, params ITransformer<Assembly[]>[] transformers ) : base( primary, transformers ) {}

		public AggregateAssemblyFactory( Func<Assembly[]> primary, params Func<Assembly[], Assembly[]>[] transformers ) : base( primary, transformers ) {}
	}
}