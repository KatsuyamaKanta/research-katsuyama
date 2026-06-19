using UnityEngine;

public class MeshSDF
{
    private readonly MeshCollider meshCollider;
    private readonly Mesh mesh;
    private readonly Transform trans;
    private readonly Vector3[] vertices;
    private readonly int[] triangles;

    public MeshSDF(MeshCollider collider)
    {
        meshCollider = collider;
        mesh = collider.sharedMesh;
        trans = collider.transform;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
    }

    public float SignedDistance(Vector3 worldPoint, out Vector3 closestPoint, out Vector3 normal)
    {
        float minDistSq = float.MaxValue;
        Vector3 bestPoint = worldPoint;
        Vector3 bestNormal = Vector3.up;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = trans.TransformPoint(vertices[triangles[i]]);
            Vector3 b = trans.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 c = trans.TransformPoint(vertices[triangles[i + 2]]);

            Vector3 cp = ClosestPointOnTriangle(worldPoint, a, b, c);
            float distSq = (worldPoint - cp).sqrMagnitude;

            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                bestPoint = cp;
                bestNormal = Vector3.Cross(b - a, c - a).normalized;
            }
        }

        bool inside = IsInside(worldPoint);

        closestPoint = bestPoint;
        normal = inside ? -bestNormal : bestNormal;

        float dist = Mathf.Sqrt(minDistSq);
        return inside ? -dist : dist;
    }

    public bool IsInside(Vector3 worldPoint)
    {
        // 簡易レイキャスト奇偶判定
        Ray ray = new Ray(worldPoint, Vector3.right);
        int hitCount = 0;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = trans.TransformPoint(vertices[triangles[i]]);
            Vector3 b = trans.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 c = trans.TransformPoint(vertices[triangles[i + 2]]);

            if (RayIntersectsTriangle(ray, a, b, c))
            {
                hitCount++;
            }
        }

        return (hitCount % 2) == 1;
    }

    private Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = p - a;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0f && d2 <= 0f) return a;

        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) return b;

        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f)
        {
            float v = d1 / (d1 - d3);
            return a + v * ab;
        }

        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) return c;

        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f)
        {
            float w = d2 / (d2 - d6);
            return a + w * ac;
        }

        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + w * (c - b);
        }

        float denom = 1f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        return a + ab * v2 + ac * w2;
    }

    private bool RayIntersectsTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        const float epsilon = 0.0000001f;

        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        Vector3 h = Vector3.Cross(ray.direction, edge2);
        float a = Vector3.Dot(edge1, h);

        if (a > -epsilon && a < epsilon) return false;

        float f = 1.0f / a;
        Vector3 s = ray.origin - v0;
        float u = f * Vector3.Dot(s, h);
        if (u < 0.0f || u > 1.0f) return false;

        Vector3 q = Vector3.Cross(s, edge1);
        float v = f * Vector3.Dot(ray.direction, q);
        if (v < 0.0f || u + v > 1.0f) return false;

        float t = f * Vector3.Dot(edge2, q);
        return t > epsilon;
    }
}