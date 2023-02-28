using Sandbox;
using System.Linq;

namespace Social
{
    public class SocialGame : GameManager
    {
		public override void ClientJoined( IClient cl )
		{
			base.ClientJoined( cl );

			var player = new SocialPlayer();
			player.Respawn();
			player.DressFromClient( cl );
			cl.Pawn = player;

			var spawnpoints = All.OfType<SpawnPoint>();
			var spawnPoint = spawnpoints.FirstOrDefault();

			if ( spawnPoint != null )
			{
				var tx = spawnPoint.Transform;
				tx.Position += Vector3.Up * 50.0f;
				player.Transform = tx;
			}
		}

	
	}
}
