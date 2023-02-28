using Sandbox;
using System;
using System.ComponentModel;

namespace Social
{
	public partial class SocialPlayer : AnimatedEntity
	{
		[ClientInput]
		public Vector3 InputDirection { get; set; }

		[ClientInput]
		public Angles ViewAngles { get; set; }

		[Browsable( false )]
		public Vector3 EyePosition
		{
			get => Transform.PointToWorld( EyeLocalPosition );
			set => EyeLocalPosition = Transform.PointToLocal( value );
		}

		[Net, Predicted, Browsable( false )]
		public Vector3 EyeLocalPosition { get; set; }

		[Browsable( false )]
		public Rotation EyeRotation
		{ 
			get => Transform.RotationToWorld( EyeLocalRotation );
			set => EyeLocalRotation= Transform.RotationToLocal( value );
		}

		[Net, Predicted, Browsable( false )]
		public Rotation EyeLocalRotation { get; set; }

		public BBox Hull
		{
			get => new
			(
				new Vector3( -16, -16, 0 ),
				new Vector3( 16, 16, 64 )
			);
		}

		[BindComponent]
		public SocialPlayerController Controller { get; }

		[BindComponent]
		public SocialPlayerAnimator Animator { get; }

		public override Ray AimRay => new( EyePosition, EyeRotation.Forward );

		public override void Spawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
		}

		public void Respawn()
		{
			Components.Create<SocialPlayerController>();
			Components.Create<SocialPlayerAnimator>();
		}

		public void DressFromClient( IClient cl )
		{
			var c = new ClothingContainer();
			c.LoadFromClient( cl );
			c.DressEntity( this );
		}

		public override void Simulate( IClient cl )
		{
			SimulateRotation();
			Controller?.Simulate( cl );
			Animator?.Simulate();
			EyeLocalPosition = Vector3.Up * ( 64f * Scale );
		}

		public override void BuildInput()
		{
			InputDirection = Input.AnalogMove;

			if ( Input.StopProcessing )
			{
				return;
			}

			var look = Input.AnalogLook;

			if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
			{
				look = look.WithYaw( look.yaw * -1f );
			}

			var viewAngles = ViewAngles;
			viewAngles += look;
			viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
			viewAngles.roll = 0f;
			ViewAngles = viewAngles.Normal;
		}

		public override void FrameSimulate( IClient cl )
		{
			SimulateRotation();

			Camera.Rotation = ViewAngles.ToRotation();
			Camera.Position = EyePosition;
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
			Camera.FirstPersonViewer = this;
		}

		public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
		}

		public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				maxs = maxs.WithZ( maxs.z - liftFeet );
			}

			var tr = Trace.Ray( start, end )
						.Size( mins, maxs )
						.WithAnyTags( "solid", "playerclip", "passbullets" )
						.Ignore( this )
						.Run();

			return tr;
		}

		protected void SimulateRotation()
		{
			EyeRotation = ViewAngles.ToRotation();
			Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
		}
	}
}
