using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ADBRuntime
{
    [RequireComponent(typeof(ADBRuntimeController))]
    public class BoneSubdivision : MonoBehaviour
    {
        #region EZConstraint
        private enum SubvisionWeightMode
        {
            ByPointWeight,
            ByPointDistance
        }
        private class EZConstraint
        {
            public int index;
            public Transform pointA;
            public Transform pointB;
            public Transform halfPoint;
            public Vector3 halfPosition;
            public SubvisionWeightMode mode;
            public EZConstraint inverse;
            public int count=0;
            public EZConstraint() { }
            public EZConstraint(ADBConstraintRead aDBConstraintRead)
            {
                pointA = aDBConstraintRead.pointA.trans;
                pointB = aDBConstraintRead.pointB.trans;
                halfPosition = (pointA.position + pointB.position) * 0.5f;
                halfPoint = new GameObject().transform;
                halfPoint.position = halfPosition;
                switch (aDBConstraintRead.constraintRead.type)
                {
                    case ConstraintType.Structural_Vertical:
                        halfPoint.name = GetVerticalName();
                        break;
                    case ConstraintType.Structural_Horizontal:
                        halfPoint.name = GetHorizontalName();
                        break;
                    default:
                        break;
                }
                inverse = GetEZConstraintInverse();
            }
            public EZConstraint(Transform A, Transform B, ConstraintType constraintType)
            {
                pointA = A;
                pointB = B;
                halfPosition = (A.position + B.position) * 0.5f;
                halfPoint = new GameObject().transform;
                switch (constraintType)
                {
                    case ConstraintType.Structural_Vertical:
                        halfPoint.name = GetVerticalName();
                        break;
                    case ConstraintType.Structural_Horizontal:
                        halfPoint.name = GetHorizontalName();
                        break;
                    default:
                        break;
                }
                inverse = GetEZConstraintInverse();
            }
            private EZConstraint GetEZConstraintInverse()
            {
                return new EZConstraint
                {
                    pointA = this.pointB,
                    pointB = this.pointA,
                    halfPoint = this.halfPoint,
                    halfPosition = this.halfPosition,
                    mode = this.mode,
                    inverse = this
                };
            }
            private string GetHorizontalName()
            {
                return "[" + pointA.name + " + " + pointB.name + "]";
            }
            private string GetVerticalName()
            {
                return pointA.name + " .5 ";
            }
            public Hash128 GetHash128()
            {
                return GetHash128(pointA, pointB);
            }
            public static Hash128 GetHash128(Transform pointA, Transform pointB)
            {
                //OYM：为什么被废弃呢,因为这样子是不能区分先后的,必须添加两次
                /*
                 *                 ulong A = (ulong)pointA.GetHashCode();
                ulong B = (ulong)pointB.GetHashCode();
                return A>B?new Hash128(A, B):new Hash128(B,A);
                 */
                return new Hash128((ulong)pointA.GetHashCode(), (ulong)pointB.GetHashCode());
            }
        }

        #endregion

        public string subdivisionKey;
        public bool isSubdivisionhorizontal;
        public bool isSubdivisionvertical;
        public ADBRuntimeController runtimeController;
        List<Transform> subdivisionBone;
        Dictionary<Hash128, EZConstraint> subdivisionDic;
        List<SkinnedMeshRenderer> renders;
        Mesh[] meshs;
        BoneWeight[][] weights;
        List<Transform>[] bones;
        List<Matrix4x4>[] bindPoses;
        public void MakeBoneSubdivision()
        {
            if (runtimeController == null) { return; }
            if (subdivisionKey == null || subdivisionKey.Length == 0) { return; }
            subdivisionKey = subdivisionKey.ToLower();

            renders = new List<SkinnedMeshRenderer>();
            renders.AddRange(gameObject.GetComponentsInChildren<SkinnedMeshRenderer>());

            for (int i = 0; i < renders.Count; i++)//OYM：先大致筛选一遍,排除掉不相干的mesh
            {
                bool isContain = false;
                for (int j = 0; j < renders[i].bones.Length; j++)
                {
                    if (renders[i].bones[j].name.ToLower().Contains(subdivisionKey))
                    {
                        isContain = true;
                        break;
                    }

                }
                if (!isContain)
                {
                    renders.RemoveAt(i);
                    i--;
                }
            }

            int meshCount = renders.Count;
            //OYM：把要修改的数据拉出来
            subdivisionBone = new List<Transform>();
            meshs = new Mesh[meshCount];
            weights = new BoneWeight[meshCount][];
            bones = new List<Transform>[meshCount];
            bindPoses = new List<Matrix4x4>[meshCount];
            //OYM：获取数据
            for (int i = 0; i < meshCount; i++)
            {
                bones[i] = new List<Transform>();
                bindPoses[i] = new List<Matrix4x4>();

                meshs[i] = renders[i].sharedMesh;
                weights[i] = meshs[i].boneWeights;
                bones[i].AddRange(renders[i].bones);
                bindPoses[i].AddRange(meshs[i].bindposes);

            }
            //OYM：准备一个临时的对象
            EZConstraint constraint;

            subdivisionDic = new Dictionary<Hash128, EZConstraint>();
            if (isSubdivisionhorizontal)//OYM：横向分割骨骼
            {
                runtimeController.initializePoint();
                ADBConstraintRead[] horizontal = null;
                Transform lastParent = null;
                if (!runtimeController.GetConstraintByKey(subdivisionKey.ToLower(), ConstraintType.Structural_Horizontal, ref horizontal))
                //OYM：顺便一提,这里提取出来的数据是广度遍历搜索的constraint
                {
                    Debug.LogError("cant find this key word in controller data");
                    return;
                }

                for (int i = 0; i < horizontal.Length; i++)
                {
                    constraint = new EZConstraint(horizontal[i]);

                    if (horizontal[i].pointA.trans.parent == horizontal[i].pointB.trans.parent)//OYM：两个节点的父节点是同一个 
                    {
                        constraint.halfPoint.parent = horizontal[i].pointA.trans.parent;
                    }
                    else
                    {
                        if (horizontal[i].pointA.parent.isFixed && horizontal[i].pointB.parent.isFixed)//如果两个都为固定点,这个时候需要制作一个特殊的constraint出来存给dictionary,不然后面会没法写
                        {
                            EZConstraint parentConstraint = new EZConstraint(horizontal[i].pointA.parent.trans, horizontal[i].pointB.parent.trans, ConstraintType.Structural_Horizontal);

                            parentConstraint.halfPoint.parent = horizontal[i].pointA.trans.parent.parent;//OYM：其实根据搜索规则,这里面无论用A和B,找出来的应该是同一个爹
                            lastParent = parentConstraint.halfPoint;
                            parentConstraint.index = subdivisionBone.Count;//OYM：鬼畜代码,目的是把bone在subdivisionbone的位置记录下来

                            subdivisionDic.Add(parentConstraint.GetHash128(), parentConstraint);
                            //OYM：这里单独注明一下,添加一个inverse是为了避免出现字典不匹配,也为了避免无法确认顺序的情况
                            subdivisionDic.Add(parentConstraint.inverse.GetHash128(), parentConstraint.inverse);
                            subdivisionBone.Add(lastParent);
                        }
                        constraint.halfPoint.parent = lastParent;
                    }
                    lastParent = constraint.halfPoint;
                    constraint.index = subdivisionBone.Count;//OYM：鬼畜代码,目的是把bone在subdivisionbone的位置记录下来

                    subdivisionDic.Add(constraint.GetHash128(), constraint);
                    subdivisionDic.Add(constraint.inverse.GetHash128(), constraint. inverse);
                    subdivisionBone.Add(constraint.halfPoint);
                }
                ComputeWeight();
            }
            if (isSubdivisionvertical)//OYM：分割竖向的骨骼
            {
                runtimeController.initializePoint();
                ADBConstraintRead[] vertical = null;
                if (!runtimeController.GetConstraintByKey(subdivisionKey.ToLower(), ConstraintType.Structural_Vertical, ref vertical))
                {
                    Debug.LogError("cant find this key word in controller data");
                    return;
                }

                for (int i = 0; i < vertical.Length; i++)
                {
                    constraint = new EZConstraint(vertical[i]);

                    constraint.halfPoint.parent = constraint.pointA;
                    constraint.pointB.parent = constraint.halfPoint;
                    constraint.index = subdivisionBone.Count;//OYM：鬼畜代码,目的是把bone在subdivisionbone的位置记录下来

                    subdivisionDic.Add(constraint.GetHash128(), constraint);
                    subdivisionDic.Add(constraint.inverse.GetHash128(), constraint.inverse);
                    subdivisionBone.Add(constraint.halfPoint);
                }
                ComputeWeight();
            }
            if (isSubdivisionhorizontal || isSubdivisionvertical)
            {
                for (int i = 0; i < renders.Count; i++)//OYM：处理mesh
                {
                    meshs[i].boneWeights = weights[i];
                    meshs[i].bindposes = bindPoses[i].ToArray();
                    renders[i].bones = bones[i].ToArray();
                }
            }

        }
        private void ComputeWeight()
        {

            for (int i = 0; i < renders.Count; i++)//OYM：处理mesh
            {
                int offset = bones[i].Count;
                Transform meshRoot = renders[i].transform;
                for (int j = 0; j < subdivisionBone.Count; j++)
                {
                    Matrix4x4 bindPose = subdivisionBone[j].worldToLocalMatrix * meshRoot.localToWorldMatrix;
                    bones[i].Add(subdivisionBone[j]);
                    bindPoses[i].Add(bindPose);
                }

                for (int j = 0; j < weights[i].Length; j++)
                {
                    if (weights[i][j].weight0 != 0 && weights[i][j].weight1 != 0)
                    {
                        SearchAndModifyBoneWeight(bones[i][weights[i][j].boneIndex0], bones[i][weights[i][j].boneIndex1],meshs[i].vertices[j] ,12, offset, ref weights[i][j]);
                    }
                    if (weights[i][j].weight0 != 0 && weights[i][j].weight2 != 0)
                    {
                        SearchAndModifyBoneWeight(bones[i][weights[i][j].boneIndex0], bones[i][weights[i][j].boneIndex2], meshs[i].vertices[j],13, offset, ref weights[i][j]);
                    }
                    if (weights[i][j].weight0 != 0 && weights[i][j].weight3 != 0)
                    {
                        SearchAndModifyBoneWeight(bones[i][weights[i][j].boneIndex0], bones[i][weights[i][j].boneIndex3], meshs[i].vertices[j], 14, offset, ref weights[i][j]);
                    }
                    if (weights[i][j].weight1 != 0 && weights[i][j].weight2 != 0)
                    {
                        SearchAndModifyBoneWeight(bones[i][weights[i][j].boneIndex1], bones[i][weights[i][j].boneIndex2], meshs[i].vertices[j], 23, offset, ref weights[i][j]);
                    }
                    if (weights[i][j].weight1 != 0 && weights[i][j].weight3 != 0)
                    {
                        SearchAndModifyBoneWeight(bones[i][weights[i][j].boneIndex1], bones[i][weights[i][j].boneIndex3], meshs[i].vertices[j], 24, offset, ref weights[i][j]);
                    }
                    if (weights[i][j].weight2 != 0 && weights[i][j].weight3 != 0)
                    {
                        SearchAndModifyBoneWeight(bones[i][weights[i][j].boneIndex2], bones[i][weights[i][j].boneIndex3], meshs[i].vertices[j], 34, offset, ref weights[i][j]);
                    }
                }
            }
        }

        void SearchAndModifyBoneWeight(Transform pointA, Transform pointB,Vector3 position, int combineMode, int indexOffset, ref BoneWeight boneWeight)//OYM：修改weight
        {
            Hash128 hash = EZConstraint.GetHash128(pointA, pointB);
            if (subdivisionDic.TryGetValue(hash, out EZConstraint constraint))
            {
                constraint.count++;
                float weightA, weightB;
                int indexA, indexB;
                switch (combineMode)//OYM：你丫,不能用ref,这里要写的无比的啰嗦了
                {
                    case 12://OYM：看代码的时候看一个就可以了,后面都是重复的
                        weightA = boneWeight.weight0;
                        weightB = boneWeight.weight1;
                        indexA = boneWeight.boneIndex0;
                        indexB = boneWeight.boneIndex1;
                        ModifyAndExchangeWeight(constraint, position, indexOffset + constraint.index, ref weightA, ref weightB, ref indexA, ref indexB);
                        boneWeight.weight0 = weightA;
                        boneWeight.weight1 = weightB;
                        boneWeight.boneIndex0 = indexA;
                        boneWeight.boneIndex1 = indexB;
                        break;
                    case 13:
                        weightA = boneWeight.weight0;
                        weightB = boneWeight.weight2;
                        indexA = boneWeight.boneIndex0;
                        indexB = boneWeight.boneIndex2;
                        ModifyAndExchangeWeight(constraint, position, indexOffset + constraint.index, ref weightA, ref weightB, ref indexA, ref indexB);
                        boneWeight.weight0 = weightA;
                        boneWeight.weight2 = weightB;
                        boneWeight.boneIndex0 = indexA;
                        boneWeight.boneIndex2 = indexB;
                        break;
                    case 14:
                        weightA = boneWeight.weight0;
                        weightB = boneWeight.weight3;
                        indexA = boneWeight.boneIndex0;
                        indexB = boneWeight.boneIndex3;
                        ModifyAndExchangeWeight(constraint, position, indexOffset + constraint.index, ref weightA, ref weightB, ref indexA, ref indexB);
                        boneWeight.weight0 = weightA;
                        boneWeight.weight3 = weightB;
                        boneWeight.boneIndex0 = indexA;
                        boneWeight.boneIndex3 = indexB;
                        break;
                    case 23:
                        weightA = boneWeight.weight1;
                        weightB = boneWeight.weight2;
                        indexA = boneWeight.boneIndex1;
                        indexB = boneWeight.boneIndex2;
                        ModifyAndExchangeWeight(constraint, position, indexOffset + constraint.index, ref weightA, ref weightB, ref indexA, ref indexB);
                        boneWeight.weight1 = weightA;
                        boneWeight.weight2 = weightB;
                        boneWeight.boneIndex1 = indexA;
                        boneWeight.boneIndex2 = indexB;
                        break;
                    case 24:
                        weightA = boneWeight.weight1;
                        weightB = boneWeight.weight3;
                        indexA = boneWeight.boneIndex1;
                        indexB = boneWeight.boneIndex3;
                        ModifyAndExchangeWeight(constraint, position, indexOffset + constraint.index, ref weightA, ref weightB, ref indexA, ref indexB);
                        boneWeight.weight1 = weightA;
                        boneWeight.weight3 = weightB;
                        boneWeight.boneIndex1 = indexA;
                        boneWeight.boneIndex3 = indexB;
                        break;
                    case 34:
                        weightA = boneWeight.weight2;
                        weightB = boneWeight.weight3;
                        indexA = boneWeight.boneIndex2;
                        indexB = boneWeight.boneIndex3;
                        ModifyAndExchangeWeight(constraint,position,indexOffset + constraint.index, ref weightA, ref weightB, ref indexA, ref indexB);
                        boneWeight.weight2 = weightA;
                        boneWeight.weight3 = weightB;
                        boneWeight.boneIndex2 = indexA;
                        boneWeight.boneIndex3 = indexB;
                        break;
                    default:
                        break;
                }
            }
        }

        void ModifyAndExchangeWeight(EZConstraint constraint,Vector3 position, int targetBoneIndex, ref float weightA, ref float weightB, ref int indexA, ref int indexB)//OYM：具体修改,并按照weight的标准交换
        {
            //OYM：替换weight,这里说起来有点啰嗦
            //OYM：首先,新添加的点,是位于两点中间,也就是两点的权重刚好都占到50%
            //OYM：此时,修改之后,该点的权重应该是新点权重1,其余点为0
            //OYM：好了我编不下去了,俺寻思这玩意能跑
            switch (constraint.mode)
            {
                case SubvisionWeightMode.ByPointWeight:
                    {
                        if (weightA > weightB)//OYM：一般而言weightA都会大于weightB,但是不排除有这种情况
                        {
                            //OYM：核心其实就这么一小段,外面一堆都是给这玩意做铺垫
                            weightA = weightA - weightB;
                            weightB = weightB + weightB;
                            indexB = targetBoneIndex;
                        }
                        else
                        {
                            weightB = weightB - weightA;
                            weightA = weightA + weightA;
                            indexA = targetBoneIndex;
                        }
                    }
                    break;
                case SubvisionWeightMode.ByPointDistance:
                    {
                        float pointToA = (constraint.pointA.position - position).sqrMagnitude;
                        float pointToHalf = (constraint.halfPosition - position).sqrMagnitude;
                        float pontToB = (constraint.pointB.position - position).sqrMagnitude;
                        if (pointToA < pointToHalf)
                        {
                            weightA = weightB + weightA;
                            weightB = 0;
                        }
                        else if (pontToB < pointToHalf)
                        {
                            weightB = weightA + weightB;
                            weightA = 0;
                        }
                        else
                        {
                            weightA = weightA + weightB;
                            weightB = 0;
                            indexA = targetBoneIndex;
                        }
                    }
                    break;
                default:
                    break;
            }


            if (weightA < weightB)//OYM：检查顺序,规范是从大到小
            {
                float temp = weightA;
                weightA = weightB;
                weightB = temp;
                temp = indexA;
                indexA = indexB;
                indexB = (int)temp;
            }
            if (weightA == 0)
            {
                indexA = 0;
            }
            if (weightB == 0)
            {
                indexB = 0;
            }

        }

        void FixCannotFindBug()
        {
            
        }
        public void MeshTest()
        {
            gameObject.AddComponent<Animation>();
            gameObject.AddComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer rend = GetComponent<SkinnedMeshRenderer>();
            Animation anim = GetComponent<Animation>();

            int height = 80;
            int width = 8;
            int boneHeight = 8;
            int boneWidth = 0;

            // Build basic mesh
            Mesh mesh = new Mesh();

            int verticesSum = (height + 1) * (width + 1);
            var vertices = new Vector3[verticesSum];
            int index = 0;
            for (int i = 0; i < height + 1; i++)
            {
                for (int j = 0; j < width + 1; j++)
                {
                    vertices[index] = new Vector3(-j, -i, 0);//计算完顶点
                    index++;
                }
            }
            mesh.vertices = vertices;

            int tranglesSum = height * width * 6;
            int[] triangles = new int[tranglesSum];
            index = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int role = width + 1;
                    int self = j + (i * role);
                    int next = j + ((i + 1) * role);
                    //顺时针
                    //第一个三角形
                    triangles[index] = self;
                    triangles[index + 1] = next + 1;
                    triangles[index + 2] = self + 1;
                    //第二个三角形
                    triangles[index + 3] = self;
                    triangles[index + 4] = next;
                    triangles[index + 5] = next + 1;
                    index += 6;
                }
            }

            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign mesh to mesh filter & renderer
            rend.material = new Material(Shader.Find("Diffuse"));

            // Assign bone weights to mesh
            // We use 2 bones. One for the lower vertices, one for the upper vertices.
            int boneSum = (boneHeight + 1) * (boneWidth + 1);
            index = 0;
            Transform[] bones = new Transform[boneSum];
            Matrix4x4[] bindPoses = new Matrix4x4[boneSum];
            float segmentH =boneHeight==0?height: height / (float)boneHeight;
            float segmentW =boneWidth==0?width: width / (float)boneWidth;

            for (int i = 0; i < boneHeight + 1; i++)
            {

                for (int j = 0; j < boneWidth + 1; j++)
                {

                    bones[index] = new GameObject("hair" + index.ToString()).transform;
                    if (boneWidth == 0)
                    {
                        bones[index].position = transform.position + new Vector3(- segmentW*0.5f, -i * segmentH, 0);
                    }
                    else
                    {
                        bones[index].position = transform.position + new Vector3(-j * segmentW, -i * segmentH, 0);
                    }

                    if (i != 0)
                    {
                        bones[index].parent = bones[index - boneWidth - 1];
                    }
                    else
                    {
                        bones[index].parent = transform;
                    }
                    bindPoses[index] = bones[index].worldToLocalMatrix * transform.localToWorldMatrix;
                    index++;
                }
            }
            mesh.bindposes = bindPoses;
            rend.bones = bones;

            BoneWeight[] weights = new BoneWeight[verticesSum];
            index = 0;
            for (int i = 0; i < height + 1; i++)
            {
                float fH = i / segmentH;
                //OYM：保留整数部分,作为boneIndex;
                int iH = Mathf.FloorToInt(fH);
                //OYM：保留小数部分,作为不同点的权重
                fH = fH - iH;

                for (int j = 0; j < width + 1; j++)
                {
                    if (boneWidth == 0)//OYM：只有垂直下来一根的情况
                    {
                        float weight0 = 1 - fH;
                        if (weight0 != 0)//OYM：注意这里,看似没用,实际上有大用途
                        {
                            weights[index].weight0 = weight0;
                            weights[index].boneIndex0 = iH;
                        }
                        float weight1 = fH;
                        if (weight1 != 0)//OYM：左边那个
                        {
                            weights[index].weight1 = weight1;
                            weights[index].boneIndex1 = iH + 1;
                        }
                    }
                    else
                    {
                        float fW = j / segmentW;
                        int iW = Mathf.FloorToInt(fW);
                        fW = fW - iW;

                        float weight0 = (1 - fH) * (1 - fW);
                        if (weight0 != 0)//OYM：注意这里,看似没用,实际上有大用途
                        {
                            weights[index].weight0 = weight0;
                            weights[index].boneIndex0 = iW + iH * (boneWidth + 1);
                        }
                        float weight1 = (1 - fH) * fW;
                        if (weight1 != 0)//OYM：左边那个
                        {
                            weights[index].weight1 = weight1;
                            weights[index].boneIndex1 = iW + 1 + iH * (boneWidth + 1);
                        }
                        float weight2 = fH * (1 - fW);
                        if (weight2 != 0)//OYM：下面那个
                        {
                            weights[index].weight2 = weight2;
                            weights[index].boneIndex2 = iW + (iH + 1) * (boneWidth + 1);
                        }
                        float weight3 = fH * fW;
                        if (weight3 != 0)//OYM：左下那个
                        {
                            weights[index].weight3 = weight3;
                            weights[index].boneIndex3 = iW + 1 + (iH + 1) * (boneWidth + 1);
                        }
                    }
                    index++;
                }
            }            
            mesh.boneWeights = weights;
            rend.sharedMesh = mesh;
        }
    }
}
