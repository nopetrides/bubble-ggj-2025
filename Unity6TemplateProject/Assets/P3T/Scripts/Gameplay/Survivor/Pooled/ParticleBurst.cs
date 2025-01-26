using DG.Tweening;
using P3T.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class ParticlesBurst : MonoBehaviour
    {
        [SerializeField] private Graphic ParticleTemplate;
        [SerializeField] private Color[] ParticleColors;
        [SerializeField] private AudioClip ParticleSfx;

        void OnEnable()
        {
            ParticleTemplate.gameObject.SetActive(false);
        }

        public void SetColor(Color color)
        {
            ParticleColors = new []{color};
        }

        public void SetColors(Color[] colors)
        {
            ParticleColors = colors;
        }

        public void AnimateParticleFX(Transform sourceTf, float delay = 0f, float duration = 0.3f, float radius = 80, bool parentedToSource = true)
        {
            var partParent = parentedToSource ? sourceTf : transform;
            if (!parentedToSource) transform.position = sourceTf.position;
        
            var particleCount = 20;
            for (var i = 0; i < particleCount; i++)
            {
                var particle = Instantiate(ParticleTemplate, partParent);
                particle.gameObject.SetActive(true);
                var pt = particle.transform;
                pt.localPosition = Vector3.zero;
                pt.localEulerAngles = Random.Range(0, 90) * Vector3.forward;
                pt.localScale = Vector3.zero;
                var pg = pt.GetComponent<Graphic>();
                pg.color = ParticleColors[Random.Range(0, ParticleColors.Length)];

                var rad = Random.Range(radius / 2, radius);
                var ang = Random.Range(0, 360);

                var sequence = DOTween.Sequence();
                sequence.AppendInterval(delay);
                sequence.Append(pt.DOScale(Random.Range(0.1f, 1f), duration * .3f));
                sequence.Append(pt.DOScale(0f, duration * .7f).SetEase(Ease.InOutSine));
                sequence.Insert(delay, pt.DOLocalMove(rad * new Vector3(Mathf.Cos(ang), Mathf.Sin(ang)), duration));
                sequence.OnComplete(() => Destroy(pt.gameObject));
            }

            if(ParticleSfx != null) AudioMgr.Instance.PlaySound(ParticleSfx);
        }
    }
}

