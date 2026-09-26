using System.Collections.Generic;
using UnityEngine;
using static System.Math;

namespace Logic.Math
{
    public static class Circle
    {
        public static IEnumerable<Vector3> GetEvenlySpacedPoints(Vector3 center, uint radius, uint count)
        {
            if (count == 0) 
                yield break;
            
            for (var i = 0; i < count; i++)
            {
                var angle = i / (double)count * 2 * PI;
                var x = center.x + (float)(Cos(angle) * radius);
                var z = center.z + (float)(Sin(angle) * radius);
                
                yield return new Vector3(x, center.y, z);
            }
        }
        
        public static IEnumerable<Vector3> GetArcPoints(Vector3 center, uint radius, uint count, float stepInDegrees)
        {
            if (count == 0) 
                yield break;
            
            var stepInRadians = stepInDegrees * (PI / 180.0);
            
            for (var i = 0; i < count; i++)
            {
                var angle = i * (double)stepInRadians;
                var x = center.x + (float)(Cos(angle) * radius);
                var z = center.z + (float)(Sin(angle) * radius);
                
                yield return new Vector3(x, center.y, z);
            }
        }
    }
}