using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// 
	/// </summary>
	[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Tank")]
	public class CharacterTank : CharacterAbility
	{
		[Header("demo-Bindings绑定")]
		public List<Transform> Wheels;
		public Transform BodyBase;
		public ParticleSystem LeftSmoke;
		public ParticleSystem RightSmoke;

		[Header("demo-Settings设置")] 
		public float MaxWheelsRotationSpeed = 10f;
        
		protected float _maxSpeed;
		protected Vector3 _rotationSpeed;

		protected CharacterOrientation3D _characterOrientation3D;
		protected ParticleSystem.EmissionModule _leftSmokeEmission;
		protected ParticleSystem.EmissionModule _rightSmokeEmission;
		protected float _initialSmokeEmission;
        
		protected override void Initialization()
		{
			base.Initialization();
			_characterOrientation3D = _character.FindAbility<CharacterOrientation3D>();
			_maxSpeed = _characterMovement.WalkSpeed;
			_leftSmokeEmission = LeftSmoke.emission;
			_rightSmokeEmission = RightSmoke.emission;
			_initialSmokeEmission = _leftSmokeEmission.rateOverTimeMultiplier;
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleWheelsRotation();
			HandleSmoke();
		}

		protected virtual void HandleSmoke()
		{
			float multiplier = MMMaths.Remap(_controller.Speed.magnitude, 0f, _maxSpeed, 0f, 1f);
			multiplier *= _initialSmokeEmission;
			_leftSmokeEmission.rateOverTimeMultiplier = multiplier;
			_rightSmokeEmission.rateOverTimeMultiplier = multiplier;

		}

		protected virtual void HandleWheelsRotation()
		{
			_rotationSpeed.z = MMMaths.Remap(_controller.Speed.magnitude, 0f, _maxSpeed, 0f, MaxWheelsRotationSpeed);

            // 我们让每个轮子都旋转
            foreach (Transform wheel in Wheels)
			{
				wheel.transform.Rotate(_rotationSpeed * Time.deltaTime, Space.Self);
			}
		}
	}
}