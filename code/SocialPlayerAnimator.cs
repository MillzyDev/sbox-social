using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social
{
	public class SocialPlayerAnimator : EntityComponent<SocialPlayer>, ISingletonComponent
	{
		public void Simulate()
		{
			var helper =  new CitizenAnimationHelper( Entity );
			helper.WithVelocity( Entity.Velocity );
			helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
			helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			helper.IsGrounded = Entity.GroundEntity.IsValid();

			if ( Entity.Controller.HasEvent( "jump" ) )
			{
				helper.TriggerJump();
			}
		}
	}
}
