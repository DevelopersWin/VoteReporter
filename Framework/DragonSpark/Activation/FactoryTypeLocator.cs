using DragonSpark.Activation.IoC;
using DragonSpark.Aspects.Validation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public sealed class FactoryTypeLocator<T> : FactoryBase<T, Type>
	{
		readonly Func<T, Type> type;
		readonly Func<T, Type> context;

		public FactoryTypeLocator( Func<T, Type> type, Func<T, Type> context )
		{
			this.type = type;
			this.context = context;
		}

		public override Type Create( T parameter )
		{
			var info = context( parameter ).GetTypeInfo();
			var nestedTypes = info.DeclaredNestedTypes.AsTypes().ToArray();
			var all = nestedTypes.Union( AssemblyTypes.All.Get( info.Assembly ) ).Where( Defaults.ApplicationType ).ToImmutableArray();
			var requests = FactoryTypeRequests.Instance.GetMany( all );
			var candidates = new[] { new FactoryTypes( requests ), FactoryTypes.Instance.Get() };
			var mapped = new LocateTypeRequest( type( parameter ) );
			var result = candidates.Introduce( mapped, tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();
			return result;
		}
	}

	public sealed class FactoryTypeRequests : Cache<Type, FactoryTypeRequest>
	{
		public static ISource<ImmutableArray<FactoryTypeRequest>> Requests { get; } = new ExecutionScope<ImmutableArray<FactoryTypeRequest>>( () => Instance.GetMany( ApplicationParts.Instance.Get().Types ) );

		public static FactoryTypeRequests Instance { get; } = new FactoryTypeRequests();
		FactoryTypeRequests() : base( new Factory().Create ) {}

		[ApplyAutoValidation]
		sealed class Factory : FactoryBase<Type, FactoryTypeRequest>
		{
			public Factory() : base( CanInstantiateSpecification.Instance.And( IsFactorySpecification.Instance, IsExportSpecification.Instance.Cast<Type>( type => type.GetTypeInfo() ), new Specification() ) ) {}

			readonly static Func<Type, Type> ResultLocator = ResultTypeLocator.Instance.ToDelegate();

			public override FactoryTypeRequest Create( Type parameter ) => new FactoryTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), ResultLocator( parameter ) );

			class Specification : GuardedSpecificationBase<Type>
			{
				public override bool IsSatisfiedBy( Type parameter ) => ResultLocator( parameter ) != typeof(object);
			}
		}
	}
}