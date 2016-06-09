using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public struct AutoData : IEquatable<AutoData>
	{
		public AutoData( [Required]IFixture fixture, [Required]MethodBase method )
		{
			Fixture = fixture;
			Method = method;
		}

		public IFixture Fixture { get; }

		public MethodBase Method { get; }

		public bool Equals( AutoData other ) => Equals( Fixture, other.Fixture ) && Equals( Method, other.Method );

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is AutoData && Equals( (AutoData)obj ) );

		public override int GetHashCode()
		{
			unchecked
			{
				return ( Fixture?.GetHashCode() ?? 0 ) * 397 ^ ( Method?.GetHashCode() ?? 0 );
			}
		}

		public static bool operator ==( AutoData left, AutoData right ) => left.Equals( right );

		public static bool operator !=( AutoData left, AutoData right ) => !left.Equals( right );
	}
}