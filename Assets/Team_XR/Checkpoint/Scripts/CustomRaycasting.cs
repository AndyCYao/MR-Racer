using UnityEngine;
using System.Collections;

public class CustomRaycasting 
{
    const  int s_MaxRaycastingTrials = 100;

    public static Vector3 RayCastToScene (Vector3 origin) {


        RaycastHit hit;
        Ray ray = new Ray();
        ray.origin = origin;
        float trialsCount = 0;
        // bool hasCreated = false;
        while (trialsCount < s_MaxRaycastingTrials)
        {
            ray.direction = Random.onUnitSphere;
            ray.direction = new Vector3(
                ray.direction.x,
                -Mathf.Abs(ray.direction.y),
                ray.direction.z
            );

            ray.direction.Normalize();

            if (Physics.Raycast(ray, out hit) && hit.transform.gameObject.layer == 8)
            {
                Debug.Log(string.Format("[RaycastSpawner: Hit at point {0}", hit.point.ToString()));


                if (Vector3.Angle(hit.normal, Vector3.up) < 10f && hit.point.y < .2f)    // .2 on y is considered floor height
                {
                    return hit.point;

                }
            }
            trialsCount++;
        }
        Debug.LogWarning("[CustomRayCasting.cs]: Raycasting to scene failed: No present mesh in the scene with layer 'Platform'");
        return Vector3.zero;
    }
}
