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

    //SDF(符号付き距離場)　メッシュ表面までの符号付の距離　外側なら正、内側なら負
    public float SignedDistance(Vector3 worldPoint, out Vector3 closestPoint, out Vector3 normal)
    {
        float minDistSq = float.MaxValue; // これまで見つかった最も近い距離の二乗（初期は十分大きな値にしておく）
        Vector3 bestPoint = worldPoint;
        Vector3 bestNormal = Vector3.up;

        // メッシュを構成する全部の三角形を調べる　trianglesは3要素で一つの三角形なので3ずつ進める
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = trans.TransformPoint(vertices[triangles[i]]);
            Vector3 b = trans.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 c = trans.TransformPoint(vertices[triangles[i + 2]]);

            // worldPoint に最も近い三角形上の点を求める
            Vector3 cp = ClosestPointOnTriangle(worldPoint, a, b, c);
            // 距離の比較
            float distSq = (worldPoint - cp).sqrMagnitude;

            // これまでで最も近い三角形だったら、最も近い点と法線を更新
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                bestPoint = cp;
                bestNormal = Vector3.Cross(b - a, c - a).normalized;
            }
        }
        // 点がメッシュの内側にあるか外側にあるかの判定
        bool inside = IsInside(worldPoint);

        closestPoint = bestPoint;
        normal = inside ? -bestNormal : bestNormal;

        float dist = Mathf.Sqrt(minDistSq);
        // 内側なら負、外側なら正の距離を返す
        return inside ? -dist : dist;
    }

    public bool IsInside(Vector3 worldPoint)
    {
        // 簡易的なレイキャスト　奇遇判定（内外判定）
        Ray ray = new Ray(worldPoint, Vector3.right);
        int hitCount = 0;

        // 全三角形との交差回数を調べる
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
        // 交差回数が奇数なら内側、偶数なら外側
        return (hitCount % 2) == 1;
    }

    // 最も近い点が三角形の内部、辺上、頂点のどこに存在するかを内積を用いて判定
    private Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // 三角形の2辺
        Vector3 ab = b - a;
        Vector3 ac = c - a;

        // 頂点Aから点p（近傍点）へのベクトル
        Vector3 ap = p - a;

        // 点pが頂点Aに最も近い領域にあるか判定
        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0f && d2 <= 0f) return a;

        // 点pが頂点Bに最も近い領域にあるか判定
        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) return b;

        // pの最近傍点が辺AB上にあるか判定
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f)
        {
            float v = d1 / (d1 - d3);
            return a + v * ab;
        }

        // 点pが頂点Cに最も近い領域にあるか判定
        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) return c;

        // pの最近傍点が辺AC上にあるか判定
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f)
        {
            float w = d2 / (d2 - d6);
            return a + w * ac;
        }

        // pの最近傍点が辺AC上にあるか判定
        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + w * (c - b);
        }

        // ここまでのどの領域にも属さない場合、点pは三角形の内部にある
        // 重心座標を用いて三角形の内部の最も近い点を求める
        float denom = 1f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        return a + ab * v2 + ac * w2;
    }

    // レイと三角形が交差するかの判定
    private bool RayIntersectsTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        // 浮動小数点誤差を考慮するための小さな値
        const float epsilon = 0.0000001f;

        // 三角形の2辺を求める
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        // レイ方向とedge2の外積を計算
        Vector3 h = Vector3.Cross(ray.direction, edge2);
        // レイと三角形面が平行に近いかを判定するための値
        float a = Vector3.Dot(edge1, h);

        // aが０に近い場合、レイと三角形面はほぼ平行なので交差しないとみなす
        if (a > -epsilon && a < epsilon) return false;

        float f = 1.0f / a;
        Vector3 s = ray.origin - v0;

        // 三角形内の重心座標 u を求める
        float u = f * Vector3.Dot(s, h);
        if (u < 0.0f || u > 1.0f) return false;

        Vector3 q = Vector3.Cross(s, edge1);

        // 三角形内の重心座標 v を求める
        float v = f * Vector3.Dot(ray.direction, q);
        if (v < 0.0f || u + v > 1.0f) return false;

        float t = f * Vector3.Dot(edge2, q);
        // t が正なら、レイの前方で三角形と交差している
        return t > epsilon;
    }
}