using DragonSpark.Aspects.Extensions.Build;
using DragonSpark.Commands;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Aspects.Extensions
{
	public interface IProfile : IEnumerable<IAspectSource>
	{
		Type DeclaringType { get; }
	}

	/*public interface IAutoValidationProfile : IProfile
	{
		IMethodSource Validation { get; }
		IMethodSource Execution { get; }
	}*/

	public interface IMethodSource : IParameterizedSource<Type, MethodInfo> {}

	public interface IAspectSource : IParameterizedSource<Type, AspectInstance> {}

	class AspectSource<T> : IAspectSource where T : IAspect
	{
		readonly Func<Type, MethodInfo> methodSource;
		readonly Func<MethodInfo, AspectInstance> inner;

		public AspectSource( IMethodSource source ) : this( source.Get, AspectInstance<T>.Default.Get ) {}

		public AspectSource( Func<Type, MethodInfo> methodSource, Func<MethodInfo, AspectInstance> inner )
		{
			this.methodSource = methodSource;
			this.inner = inner;
		}

		public AspectInstance Get( Type parameter )
		{
			var method = methodSource( parameter );
			var result = method != null ? inner( method ) : null;
			return result;
		}
	}

	public abstract class ProfileBase : ItemSourceBase<IAspectSource>, IProfile
	{
		protected ProfileBase( Type declaringType )
		{
			DeclaringType = declaringType;
		}

		public Type DeclaringType { get; }
	}

	public abstract class AutoValidationProfileBase : ProfileBase
	{
		readonly IAspectSource validation;
		readonly IAspectSource execution;

		protected AutoValidationProfileBase( Type declaringType, IMethodSource validation, IMethodSource execution )
			: this( declaringType, new AspectSource<AutoValidationValidationAspect>( validation ), new AspectSource<AutoValidationExecuteAspect>( execution ) ) { }

		protected AutoValidationProfileBase( Type declaringType, IAspectSource validation, IAspectSource execution ) : base ( declaringType )
		{
			this.validation = validation;
			this.execution = execution;
		}

		

		protected override IEnumerable<IAspectSource> Yield()
		{
			yield return validation;
			yield return execution;
		}
	}

	public static class AutoValidation
	{
		public static IMethodSource Specification { get; } = new MethodDefinition( typeof(ISpecification<>), nameof( ISpecification<object>.IsSatisfiedBy ) );

		public static ImmutableArray<IProfile> DefaultProfiles { get; } = 
			new IProfile[]
			{
				ParameterizedSourceAutoValidationProfile.Default,
				GenericCommandAutoValidationProfile.Default,
				CommandAutoValidationProfile.Default
			}.ToImmutableArray();
	}

	sealed class ParameterizedSourceAutoValidationProfile : AutoValidationProfileBase
	{
		readonly static Type Type = typeof(IParameterizedSource<,>);

		public static ParameterizedSourceAutoValidationProfile Default { get; } = new ParameterizedSourceAutoValidationProfile();
		ParameterizedSourceAutoValidationProfile() : base( Type, AutoValidation.Specification, new MethodDefinition( Type, nameof(ISource.Get) ) ) {}
	}

	sealed class CommandAutoValidationProfile : AutoValidationProfileBase
	{
		readonly static Type Type = typeof(ICommand);
		public static CommandAutoValidationProfile Default { get; } = new CommandAutoValidationProfile();
		CommandAutoValidationProfile() : base( Type, new MethodDefinition( Type, nameof(ICommand.CanExecute) ), new MethodDefinition( Type, nameof(ICommand.Execute) ) ) {}
	}

	sealed class GenericCommandAutoValidationProfile : AutoValidationProfileBase
	{
		readonly static Type Type = typeof(ICommand<>);

		public static GenericCommandAutoValidationProfile Default { get; } = new GenericCommandAutoValidationProfile();
		GenericCommandAutoValidationProfile() : base( Type, AutoValidation.Specification, new MethodDefinition( Type, nameof(ICommand.Execute) ) ) {}
	}
}