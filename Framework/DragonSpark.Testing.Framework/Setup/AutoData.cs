using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class AutoData : IEquatable<AutoData>
	{
		public AutoData( [Required]IFixture fixture, [Required]MethodBase method )
		{
			Fixture = fixture;
			Method = method;
		}

		public IFixture Fixture { get; }

		public MethodBase Method { get; }

		public bool Equals( AutoData other ) => !ReferenceEquals( null, other ) && ( ReferenceEquals( this, other ) || Equals( Method, other.Method ) );

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( ReferenceEquals( this, obj ) || obj.GetType() == this.GetType() && Equals( (AutoData)obj ) );

		public override int GetHashCode() => Method?.GetHashCode() ?? 0;

		public static bool operator ==( AutoData left, AutoData right ) => Equals( left, right );

		public static bool operator !=( AutoData left, AutoData right ) => !Equals( left, right );
	}
}