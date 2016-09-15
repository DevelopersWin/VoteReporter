using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
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
		ExtensionPointProfiles() : this( Aspects.Extensions.AutoValidation.Adapters, ExtensionPoints.Default.Get ) {}

		readonly ImmutableArray<TypeAdapter> adapters;
		readonly Func<MethodBase, IExtensionPoint> pointSource;

		ExtensionPointProfiles( ImmutableArray<TypeAdapter> adapters, Func<MethodBase, IExtensionPoint> pointSource )
		{
			this.adapters = adapters;
			this.pointSource = pointSource;
		}

		public IEnumerable<ExtensionPointProfile> Get( Type parameter ) => Yield( parameter ).ToArray();

		IEnumerable<ExtensionPointProfile> Yield( Type parameter )
		{
			/*foreach ( var profile in adapters.Introduce( parameter, tuple => tuple.Item1.IsAssignableFrom( tuple.Item2 ) ) )
			{
				var validation = pointSource( profile.Validation.Get( parameter ) );
				var execution = pointSource( profile.Method.Get( parameter ) );
				yield return new ExtensionPointProfile( validation, execution );
			}*/
			yield break;
		}
	}
}