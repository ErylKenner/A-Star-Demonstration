using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public List<float> Angles = new List<float>();
    public List<float> RadiusAtEachAngle = new List<float>();

    private void Awake()
    {
        
        List<float> temp = new List<float>(RadiusAtEachAngle);
        for (int i = 0; i < temp.Count; ++i)
        {
            temp[i] += RadiusAtEachAngle.ElementAt((i - 1 + temp.Count) % temp.Count) + RadiusAtEachAngle.ElementAt((i + 1) % temp.Count);
            temp[i] /= 3.0f;
        }
        RadiusAtEachAngle = temp;
    }

    public float GetCost(Vector3 nodePosition)
    {
        float relativeAngle = Vector3.SignedAngle(transform.forward, nodePosition - transform.position, Vector3.up);
        if (relativeAngle < 0.0f)
        {
            relativeAngle += 360.0f;
        }
        int index = 0;
        while (index < Angles.Count - 1)
        {
            if (relativeAngle >= Angles.ElementAt(index) && relativeAngle < Angles.ElementAt(index + 1))
            {
                break;
            }
            index++;
        }

        float radius;
        int prevIndex = (index - 1 + RadiusAtEachAngle.Count) % RadiusAtEachAngle.Count;
        int nextIndex = (index + 1) % RadiusAtEachAngle.Count;
        float diff = relativeAngle - Angles.ElementAt(index);
        if (diff < 0.0f)
        {
            diff += 360.0f;
        }
        float range = Angles.ElementAt(nextIndex) - Angles.ElementAt(index);
        if (range < 0.0f)
        {
            range += 360.0f;
        }
        if (diff < 0.5f * range)
        {
            float t = diff / (0.5f * range);
            float midRadius = 0.5f * (RadiusAtEachAngle.ElementAt(prevIndex) + RadiusAtEachAngle.ElementAt(index));
            radius = Mathf.Lerp(midRadius, RadiusAtEachAngle.ElementAt(index), t);
        }
        else
        {
            float t = diff / (0.5f * range) - 1.0f;
            float midRadius = 0.5f * (RadiusAtEachAngle.ElementAt(index) + RadiusAtEachAngle.ElementAt(nextIndex));
            radius = Mathf.Lerp(RadiusAtEachAngle.ElementAt(index), midRadius, t);
        }

        float distance = Vector3.Distance(nodePosition, transform.position);
        if (distance <= radius)
        {
            return 100.0f * (1.0f - distance / radius);
        }
        return 0.0f;
    }
}
