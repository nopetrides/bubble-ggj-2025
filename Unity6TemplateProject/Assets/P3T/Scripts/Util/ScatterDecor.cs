using P3T.Scripts.Gameplay.Survivor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace P3T.Scripts.Util
{
    public class ScatterDecor : MonoBehaviour
    {
        [SerializeField] private PlayerBounds MovementBounds;
        [SerializeField] private GameObject[] Clutter;

        /// <summary>
        /// Performance?
        /// </summary>
        private void Start()
        {
            int amount = Clutter.Length;
            var bounds = MovementBounds.PlayerMovementBounds.bounds.extents;
            for (int i = Mathf.FloorToInt(-0.5f * bounds.x); i < 0.5f * bounds.x; i++)
            {
                for (int j = Mathf.FloorToInt(-0.5f * bounds.y); j < 0.5f * bounds.y; j++)
                {
                    Instantiate(Clutter[Random.Range(0, amount)], new Vector3(i,0,j), Quaternion.Euler(0,Random.Range(0,360),0),  transform);
                }
            }
        }
    }
}
