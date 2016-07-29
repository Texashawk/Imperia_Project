using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Spacetime Well")]
public class SgtSpacetimeWell : MonoBehaviour
{
	public enum DistributionType
	{
		Gaussian,
		Ripple,
		Twist
	}
	
	// Contains all currently active and enabled wells
	public static List<SgtSpacetimeWell> AllWells = new List<SgtSpacetimeWell>();

	[Tooltip("The method used to deform the spacetime")]
	public DistributionType Distribution = DistributionType.Gaussian;

	[Tooltip("The radius of this spacetime well")]
	public float Radius = 1.0f;

	[Tooltip("The frequency of the ripple")]
	public float Frequency = 1.0f;
	
	[Tooltip("The minimum strength of the well")]
	public float Strength = 1.0f;
	
	[Tooltip("The frequency offset")]
	public float Offset;
	
	[Tooltip("The frequency offset speed per second")]
	public float OffsetSpeed;

	[Tooltip("The size of the twist hole")]
	[SgtRange(0.0f, 0.9f)]
	public float HoleSize;
	
	[Tooltip("The power of the twist hole")]
	public float HolePower = 10.0f;
	
	public static SgtSpacetimeWell Create(SgtSpacetime spacetime)
	{
		if (spacetime != null)
		{
			var gameObject = SgtHelper.CreateGameObject("Well", spacetime.gameObject.layer, spacetime.transform);
			var well       = gameObject.AddComponent<SgtSpacetimeWell>();

			spacetime.Wells.Add(well);

			return well;
		}
		
		return null;
	}

	protected virtual void OnEnable()
	{
		AllWells.Add(this);
	}

	protected virtual void OnDisable()
	{
		AllWells.Remove(this);
	}

	protected virtual void Update()
	{
		Offset += OffsetSpeed * Time.deltaTime;
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(transform.position, Radius);
	}
#endif
}
