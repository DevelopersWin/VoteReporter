using DragonSpark.Activation;
using DragonSpark.Aspects.Extensibility;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	// http://stackoverflow.com/questions/35976558/is-constructorinfo-getparameters-thread-safe/35976798
	[EnableExtensions]
	public sealed class InitializeTypeCommand : ExtensibleCommandBase<Type>
	{
		public static InitializeTypeCommand Default { get; } = new InitializeTypeCommand().ExtendUsing( CanActivateSpecification.Default.And( new OncePerParameterSpecification<Type>() ) );
		InitializeTypeCommand() {}

		public override void Execute( Type parameter ) => parameter.GetTypeInfo().DeclaredConstructors.Each( info => info.GetParameters() );
	}
}