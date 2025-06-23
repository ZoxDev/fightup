using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Sandbox;
using Sandbox.Services;

public sealed class CameraPostion : Component
{
	[Property]
	public float MaxCameraY { get; set; } = 50.0f;

	float initialCameraPositionInZ = 0f;
	float initialCameraPositionInX = 0f;

	protected override void OnAwake()
	{
		base.OnAwake();
		Vector3 cameraPosition = GameObject.LocalPosition;

		initialCameraPositionInZ = cameraPosition.z;
		initialCameraPositionInX = cameraPosition.x;
	}
	protected override void OnUpdate()
	{
		Vector3 playerPosition = GameObject.Parent.LocalPosition;
		float playerPositionInZ = playerPosition.z;
		float lerpFactor = playerPositionInZ / initialCameraPositionInZ;

		GameObject.LocalPosition = Vector3.Lerp( new Vector3( initialCameraPositionInX, 0, initialCameraPositionInZ ), new Vector3( initialCameraPositionInX, 0, 0 ), lerpFactor );
	}
}
