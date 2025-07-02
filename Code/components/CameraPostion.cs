using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Sandbox;
using Sandbox.Services;

public sealed class CameraPostion : Component
{
	[Property] public float MaxCameraY { get; set; } = 50.0f;
	[Property] private float initialCameraPositionInZ { get; set; } = 200f;
	[Property] private float initialCameraPositionInX { get; set; } = 1000f;
	private GameObject _player { get; set; } = null;
	private Vector3 _playerPosition;

	protected override void OnAwake()
	{
		base.OnAwake();

	}
	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( PlayerController2D.LocalPlayer != null && _player == null )
		{
			_player = PlayerController2D.LocalPlayer;
			PlayerController2D.LocalPlayer.GetComponent<PlayerController2D>().CameraGameObject = GameObject;
		}

		if ( _player == null ) return;

		Vector3 playerPosition = _player.WorldPosition;
		float playerPositionInZ = playerPosition.z;
		float lerpFactor = playerPositionInZ / initialCameraPositionInZ;

		GameObject.WorldPosition = Vector3.Lerp( new Vector3( playerPosition.x + initialCameraPositionInX, playerPosition.y, playerPosition.z + initialCameraPositionInZ ), new Vector3( playerPosition.x + initialCameraPositionInX, playerPosition.y, playerPosition.z ), lerpFactor );
	}
}
