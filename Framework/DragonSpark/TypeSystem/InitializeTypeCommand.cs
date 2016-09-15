using DragonSpark.Activation;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using System;
using System.Reflection;
using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;

namespace DragonSpark.TypeSystem
{
	// http://stackoverflow.com/questions/35976558/is-constructorinfo-getparameters-thread-safe/35976798
	[ApplyAutoValidation, Specification( typeof(Specification) )]
	public sealed class InitializeTypeCommand : CommandBase<Type>
	{
		public static InitializeTypeCommand Default { get; } = new InitializeTypeCommand();
		InitializeTypeCommand() {}

		public override void Execute( Type parameter ) => parameter.GetTypeInfo().DeclaredConstructors.Each( info => info.GetParameters() );

		class Specification : AllSpecification<Type>
		{
			[UsedImplicitly]
			public static Specification DefaultNested { get; } = new Specification();
			Specification() : base( CanActivateSpecification.Default, new OncePerParameterSpecification<Type>() ) {}
		}
	}
}