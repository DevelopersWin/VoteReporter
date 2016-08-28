using DragonSpark.Commands;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	// [Synchronized] // http://stackoverflow.com/questions/35976558/is-constructorinfo-getparameters-thread-safe/35976798
	public sealed class InitializeTypeCommand : CommandBase<Type>
	{
		public static InitializeTypeCommand Default { get; } = new InitializeTypeCommand();
		InitializeTypeCommand() : base( CanActivateSpecification.Default.And( /*InstantiableTypeSpecification.Default, */new OncePerParameterSpecification<Type>() ) ) {}

		// public InitializeTypeCommand( ISpecification<Type> specification ) : base( specification ) {}

		// [Freeze]
		public override void Execute( Type parameter ) => parameter.GetTypeInfo().DeclaredConstructors.Each( info => info.GetParameters() );
	}
}