//The Bind Pose Matrices allow a raw vertex of the mesh (in local coordinates) to be transformed into world-space
// and then to each bone's local coordinate space, after which each bone's animation can be applied to the vertex in question
// (under the influence of the weighting value). The mesh's bind pose takes the vertex from local space to world-space,
// and then each bone's inverse bind pose takes the mesh vertex from world-space to the local space of that bone.
// Once in the bone's local space, the bone's current animated transformation matrix is used to transform (deform)
// the vertex's location. After all the bone influences have been taken into account, the vertex ends up in world-space
// in its final deformed location. In other words, these bind pose matrices relate the location of a vertex in its local mesh space
// to the same location of the vertex relative to each bones' local coordinate systems.

// The mesh's Bind Pose Matrix takes its vertices into world-space, to the location at the time of binding,
// and each bones' Bind Pose Matrix takes the bones from local space to world-space at the time of binding.

using UnityEngine;

class BindPoseTest : MonoBehaviour
{
    void Start()
    {
        var renderer = gameObject.AddComponent<SkinnedMeshRenderer>();

        // Build basic mesh
        var mesh  = new Mesh();

        mesh.vertices = new Vector3[] {
                             new Vector3(-1, 0, 0),
                              new Vector3(1, 0, 0),
                             new Vector3(-1, 5, 0),
                            new Vector3(1, 5, 0)
                        };//OYM：首先你要有定点

        mesh.uv =     new Vector2[]{
                            new Vector2(0, 0),
                            new Vector2(1, 0),
                            new Vector2(0, 1),
                            new Vector2(1, 1)
                            };//OYM：其次你要有UV

        mesh.triangles = new int[]
                            { 0, 1, 2,
                             1, 3, 2,
                             2, 1, 0,
                             2, 3, 1
                        };//OYM：然后你要规定哪些点连成了三角形

        mesh.RecalculateNormals();//OYM：然后你要计算法线

        // Assign mesh to mesh filter  renderer

        renderer.material = new Material(Shader.Find(" Diffuse"));//OYM：然后你要丢一个shader上去

        // BoneWeight[4] : 4 = vertices 0 to 3
        // weights[0] : first (0) vertice
        // boneIndex0 : 0 = first bone
        // weight0 = 1 : 1 = how much influence this bone has on the vertice

        var weights = new BoneWeight[4];//OYM：你要为每个顶点制定一个BoneWeight

        weights[0].boneIndex0 = 0;
        weights[0].weight0 = 1;

        weights[1].boneIndex0 = 0;
        weights[1].weight0 = 1;

        weights[2].boneIndex0 = 0;
        weights[2].weight0 = 1;

        weights[3].boneIndex0 = 0;
        weights[3].weight0 = 1;

        mesh.boneWeights = weights;

        // Create 1 Bone Transform and 1 Bind pose

        var bones = new Transform[2];
        var bindPoses = new Matrix4x4[2];

        // Create a new gameObject

        bones[0] = new GameObject("Lower").transform;

        // Make this gameObject's transform the parent of the bone

        bones[0].parent = transform;

        // Set the position/rotation of the bone

        bones[0].localRotation = Quaternion.identity;
        bones[0].localPosition = Vector3.zero;

        // bones[0] is a Transform mapped to world space. We map it to the local space of its parent,
        // which is the transform "bones[0].worldToLocalMatrix" and afterwards
        // we map it again in world space, keeping the relation child - parent with "* transform.localToWorldMatrix;",
        // thus allowing us 1. to move/rotate/scale it freely in space but also
        //                            2. make all move/rotate/scaling operations on its parent affect it too

        bindPoses[0] = bones[0].worldToLocalMatrix * transform.localToWorldMatrix;

        // Apply bindPoses to the mesh

        mesh.bindposes = bindPoses;

        // Assign bones and bind poses to the SkinnedMeshRenderer

        renderer.bones = bones;
        renderer.sharedMesh = mesh;
    }
}
