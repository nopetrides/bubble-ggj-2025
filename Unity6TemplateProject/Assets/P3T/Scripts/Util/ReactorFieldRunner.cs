using System.Collections.Generic;
using BoingKit;
using P3T.Scripts.Gameplay.Survivor;
using UnityEngine;

namespace P3T.Scripts.Util
{
    public class ReactorFieldRunner : MonoBehaviour
    {
        [SerializeField] private PlayerBounds MovementBounds;
        
        [Header("Boing")] 
        [SerializeField] private BoingReactorFieldGPUSampler[] Bush;
        [SerializeField] private int NumBushes;
        [SerializeField] private Vector2 BushScaleRange;
        [SerializeField] private BoingReactorField ReactorField;

        private const int KNumInstancedBushesPerDrawCall = 1000; // Unity 5 doesn't like 1024 and I don't like 1023 *sigh*
        private Matrix4x4 [][] _mAAInstancedBushMatrix;
        private MaterialPropertyBlock _mBushMaterialProps;
        private Mesh[] _bushMesh;
        private Material[] _bushMaterial;
        private List<Matrix4x4[][]> _plantInstances = new ();
        
        private void Start()
        {
            InitBoingField();
        }

        private void InitBoingField()
        {
            var bounds = MovementBounds.PlayerMovementBounds.bounds.extents;
            int length = Bush.Length;
            _bushMesh = new Mesh[length];
            _bushMaterial = new Material[length];
            for (int i = 0; i < length; i++)
            {
                _bushMesh[i] = Bush[i].GetComponent<MeshFilter>().sharedMesh;
                _bushMaterial[i] = Bush[i].GetComponent<MeshRenderer>().sharedMaterial;
                var mAAInstancedBushMatrix = new Matrix4x4[(NumBushes + KNumInstancedBushesPerDrawCall - 1) / KNumInstancedBushesPerDrawCall][];
                for (int j = 0; j < mAAInstancedBushMatrix.Length; ++j)
                {
                    mAAInstancedBushMatrix[j] = new Matrix4x4[KNumInstancedBushesPerDrawCall];
                }
                for (int j = 0; j < NumBushes; ++j)
                {
                    float scale = Random.Range(BushScaleRange.x, BushScaleRange.y);

                    Vector3 position =
                        new Vector3
                        (
                            Random.Range(-0.5f * bounds.x, 0.5f * bounds.x),
                            0.2f * scale,
                            Random.Range(-0.5f * bounds.y, 0.5f * bounds.y)
                        );

                    Quaternion rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

                    mAAInstancedBushMatrix[j / KNumInstancedBushesPerDrawCall][j % KNumInstancedBushesPerDrawCall].SetTRS(position, rotation, scale * Vector3.one);
                }
                
                _plantInstances.Add(mAAInstancedBushMatrix);
            }
            
        }
        

        public void Update()
        {
            for (var i = 0; i < _plantInstances.Count; i++)
            {
                var plantMatrix = _plantInstances[i];
                if (_mBushMaterialProps == null)
                    _mBushMaterialProps = new MaterialPropertyBlock();

                if (ReactorField.UpdateShaderConstants(_mBushMaterialProps))
                {
                    foreach (var aInstancedBushMatrix in plantMatrix)
                    {
                        Graphics.DrawMeshInstanced(_bushMesh[i], 0, _bushMaterial[i], aInstancedBushMatrix,
                            aInstancedBushMatrix.Length, _mBushMaterialProps);
                    }
                }
            }

            if (_mAAInstancedBushMatrix != null)
            {
                
            }
        }
    }
}
