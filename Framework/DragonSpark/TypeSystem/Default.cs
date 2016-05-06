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
		public static Func<T, bool> NotNull => t => !t.IsNull();

		public static Func<T, bool> Always => t => true;
	}

	public static class Default<T>
	{
		public static Func<T, T> Self => t => t;

		public static Func<T, object> Boxed => t => t;

		// [Freeze]
		public static T Item => (T)DefaultItemProvider.Instance.Create( typeof(T) );

		// [Freeze]
		public static T[] Items => (T[])DefaultItemProvider.Instance.Create( typeof(T[]) ) /*Enumerable.Empty<T>().Fixed()*/;
	}

	[Synchronized] // http://stackoverflow.com/questions/35976558/is-constructorinfo-getparameters-thread-safe/35976798
	public class InitializeTypeCommand : CommandBase<Type>
	{
		public static InitializeTypeCommand Instance { get; } = new InitializeTypeCommand();

		public InitializeTypeCommand() : this( CanBuildSpecification.Instance ) {}

		public InitializeTypeCommand( ISpecification<Type> specification ) : base( specification ) {}

		[Freeze]
		protected override void OnExecute( Type parameter ) => parameter.GetTypeInfo().DeclaredConstructors.Each( info => info.GetParameters() );
	}

	public static class TypeSupport
	{
		public static Type From( object item ) => item.AsTo<ParameterInfo, Type>( info => info.ParameterType ) ?? item.AsTo<MemberInfo, Type>( info => info.GetMemberType() ) ?? item as Type;
	}
}