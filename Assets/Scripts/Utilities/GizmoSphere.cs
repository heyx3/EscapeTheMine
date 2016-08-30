using UnityEngine;


public class GizmoSphere : MonoBehaviour
{
	public Color Col = Color.white;
	public float Radius = 0.5f;

	public bool OnlyWhenSelected = false;


	void OnDrawGizmos()
	{
		if (!OnlyWhenSelected)
		{
			Gizmos.color = Col;
			Gizmos.DrawSphere(transform.position, Radius);
		}
	}
	void OnDrawGizmosSelected()
	{
		if (OnlyWhenSelected)
		{
			Gizmos.color = Col;
			Gizmos.DrawSphere(transform.position, Radius);
		}
	}
}