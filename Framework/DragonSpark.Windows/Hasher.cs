using DragonSpark.Sources.Parameterized;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;

namespace DragonSpark.Windows
{
	public class Hasher : AlterationBase<ImmutableArray<byte>>
	{
		public static Hasher Default { get; } = new Hasher();
		Hasher() {}

		public override ImmutableArray<byte> Get( ImmutableArray<byte> parameter )
		{
			using ( var md5 = MD5.Create() )
			{
				return md5.ComputeHash( parameter.ToArray() ).ToImmutableArray();
			}
		}
	}
}