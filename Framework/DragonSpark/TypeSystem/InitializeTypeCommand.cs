using DragonSpark.Activation;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	// http://stackoverflow.com/questions/35976558/is-constructorinfo-getparameters-thread-safe/35976798
	public sealed class InitializeTypeCommand : CommandBase<Type>
	{
		public static ICommand<Type> Default { get; } = new InitializeTypeCommand().Apply( CanActivateSpecification.Default.And( new OncePerParameterSpecification<Type>() ) );
		InitializeTypeCommand() {}

		public override void Execute( Type parameter ) => parameter.GetTypeInfo().DeclaredConstructors.Each( info => info.GetParameters() );
	}
}