using DragonSpark.Runtime.Data;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using PostSharp.Extensibility;
using System;

namespace DragonSpark.Aspects.Configuration
{
	public sealed class KnownTypesForSerialization : DelegatedParameterizedItemSource<IProject, Type>
	{
		public static KnownTypesForSerialization Default { get; } = new KnownTypesForSerialization();
		KnownTypesForSerialization() : this( KnownTypesForSerializationProperty.Default ) {}

		[UsedImplicitly]
		public KnownTypesForSerialization( IProjectProperty property ) : base( property.To( TypeParser.Default ).GetEnumerable ) {}
	}
}
