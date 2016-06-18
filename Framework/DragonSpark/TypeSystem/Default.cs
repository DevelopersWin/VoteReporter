using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Threading;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public static class Where<T>
	{
		public static Func<T, bool> Assigned => t => t.IsAssigned();

		public static Func<T, bool> Always => t => true;
	}

	[Synchronized] // http://stackoverflow.com/questions/35976558/is-constructorinfo-getparameters-thread-safe/35976798
	public class InitializeTypeCommand : CommandBase<Type>
	{
		public static InitializeTypeCommand Instance { get; } = new InitializeTypeCommand();

		public InitializeTypeCommand() : this( CanInstantiateSpecification.Instance.And( InstantiableTypeSpecification.Instance ) ) {}

		public InitializeTypeCommand( ISpecification<Type> specification ) : base( specification ) {}

		[Freeze]
		public override void Execute( Type parameter ) => parameter.GetTypeInfo().DeclaredConstructors.Each( info => info.GetParameters() );
	}

	public static class TypeSupport
	{
		public static Type From( object item ) => item.AsTo<ParameterInfo, Type>( info => info.ParameterType ) ?? item.AsTo<MemberInfo, Type>( info => info.GetMemberType() ) ?? item as Type ?? item.GetType();
	}
}