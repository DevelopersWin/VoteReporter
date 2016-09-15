using DragonSpark.Aspects.Extensions.Build;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Aspects.Extensions
{
	public interface IProfile : IEnumerable<IAspectSource>
	{
		Type DeclaringType { get; }
	}

	public interface IMethodLocator : IParameterizedSource<Type, MethodInfo>
	{
		Type DeclaringType { get; }
	}

	public interface IAspectSource : IParameterizedSource<Type, AspectInstance> {}

	class AspectSource<T> : IAspectSource where T : IAspect
	{
		readonly Func<Type, MethodInfo> methodSource;
		readonly Func<MethodInfo, AspectInstance> inner;

		public AspectSource( IMethodLocator locator ) : this( locator.Get, AspectInstance<T>.Default.Get ) {}

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

	public class Profile : ItemSource<IAspectSource>, IProfile
	{
		protected Profile( Type declaringType, params IAspectSource[] sources ) : base( sources )
		{
			DeclaringType = declaringType;
		}

		public Type DeclaringType { get; }
	}

	public static class Defaults
	{
		public static IMethodLocator Specification { get; } = new MethodDefinition( typeof(ISpecification<>), nameof( ISpecification<object>.IsSatisfiedBy ) );
	}

	public static class AutoValidation
	{
		public static ImmutableArray<IProfile> DefaultProfiles { get; } = 
			ImmutableArray.Create<IProfile>( ParameterizedSourceAutoValidationProfile.Default, GenericCommandAutoValidationProfile.Default, CommandAutoValidationProfile.Default );
			
		public static ImmutableArray<TypeAdapter> Adapters { get; } = DefaultProfiles.Select( profile => profile.DeclaringType.Adapt() ).ToImmutableArray();
	}

	public class AutoValidationProfile : Profile
	{
		protected AutoValidationProfile( Type declaringType, IMethodLocator validation, IMethodLocator execution )
			: base( declaringType, new AspectSource<AutoValidationValidationAspect>( validation ), new AspectSource<AutoValidationExecuteAspect>( execution ) ) {}
	}

	sealed class ParameterizedSourceAutoValidationProfile : AutoValidationProfile
	{
		readonly static Type Type = typeof(IParameterizedSource<,>);

		public static ParameterizedSourceAutoValidationProfile Default { get; } = new ParameterizedSourceAutoValidationProfile();
		ParameterizedSourceAutoValidationProfile() : base( Type, Defaults.Specification, new MethodDefinition( Type, nameof(ISource.Get) ) ) {}
	}

	sealed class CommandAutoValidationProfile : AutoValidationProfile
	{
		readonly static Type Type = typeof(ICommand);
		public static CommandAutoValidationProfile Default { get; } = new CommandAutoValidationProfile();
		CommandAutoValidationProfile() : base( Type, new MethodDefinition( Type, nameof(ICommand.CanExecute) ), new MethodDefinition( Type, nameof(ICommand.Execute) ) ) {}
	}

	sealed class GenericCommandAutoValidationProfile : AutoValidationProfile
	{
		readonly static Type Type = typeof(ICommand<>);

		public static GenericCommandAutoValidationProfile Default { get; } = new GenericCommandAutoValidationProfile();
		GenericCommandAutoValidationProfile() : base( Type, Defaults.Specification, new MethodDefinition( Type, nameof(ICommand.Execute) ) ) {}
	}
}