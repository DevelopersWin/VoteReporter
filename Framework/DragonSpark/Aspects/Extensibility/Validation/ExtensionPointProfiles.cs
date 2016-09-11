using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public sealed class ExtensionPointProfiles : IParameterizedSource<Type, IEnumerable<ExtensionPointProfile>>
	{
		public static IParameterizedSource<Type, IEnumerable<ExtensionPointProfile>> DefaultNested { get; } = new ExtensionPointProfiles().ToCache();
		ExtensionPointProfiles() : this( AutoValidation.DefaultProfiles, ExtensionPoints.Default.Get ) {}

		readonly ImmutableArray<IAspectProfile> profiles;
		readonly Func<MethodBase, IExtensionPoint> pointSource;

		ExtensionPointProfiles( ImmutableArray<IAspectProfile> profiles, Func<MethodBase, IExtensionPoint> pointSource )
		{
			this.profiles = profiles;
			this.pointSource = pointSource;
		}

		public IEnumerable<ExtensionPointProfile> Get( Type parameter ) => Yield( parameter ).ToArray();

		IEnumerable<ExtensionPointProfile> Yield( Type parameter )
		{
			foreach ( var profile in profiles.Introduce( parameter, tuple => tuple.Item1.Method.DeclaringType.Adapt().IsAssignableFrom( tuple.Item2 ) ) )
			{
				var methodInfo = profile.Validation.Find( parameter );
				var validation = pointSource( methodInfo );
				var methodBase = profile.Method.Find( parameter );
				var execution = pointSource( methodBase );
				yield return new ExtensionPointProfile( validation, execution );
			}
		}
	}
}