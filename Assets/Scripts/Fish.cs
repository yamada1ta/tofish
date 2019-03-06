using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ToFish
{
    public class Fish : MonoBehaviour
    {
        [SerializeField]
        Material FadeMaterial = null;

        [SerializeField]
        GameObject BubbleSrc = null;

        FishFader fader = null;
        GameObject bubble = null;
        Vector3 targetPos = Vector3.zero;
        Vector3 preAngularVelocity = Vector3.zero;
        static readonly Vector3 preVelocity = new Vector3(0, 0.05f, 0);
        static readonly Vector3 postVelocity = new Vector3(0, 6, 0);

        public bool IsAlive
        {
            get
            {
                if (fader.State == FadeState.Pre)
                {
                    const float topLimit = 1.2f;
                    var viewPos = Camera.main.WorldToViewportPoint(transform.position);

                    return viewPos.y <= topLimit;
                }

                const float nearLimit = 1;
                return Vector3.Distance(transform.position, targetPos) > nearLimit;
            }
        }

        void Start()
        {
            fader = new FishFader(gameObject, FadeMaterial);

            bubble = Instantiate(BubbleSrc);
            bubble.transform.parent = transform;
            bubble.transform.localPosition = Vector3.zero;
            bubble.transform.localScale = Vector3.one;

            System.Func<float> rand = () => Random.Range(1, 6) * (Random.Range(0, 2) == 0 ? 0.1f : -0.1f);
            preAngularVelocity = new Vector3(rand(), 0, rand());

            this.UpdateAsObservable()
                .Where(_ => fader.State == FadeState.Pre)
                .Subscribe(_ =>
                {
                    transform.position += preVelocity;
                    transform.eulerAngles += preAngularVelocity;
                });

            this.UpdateAsObservable()
                .Where(_ => Input.GetMouseButtonDown(0) || Input.touchCount > 0)
                .Take(1)
                .Subscribe(_ =>
                {
                    fader.Start();

                    targetPos = TargetPosition(Input.mousePosition, Camera.main);
                });

            this.UpdateAsObservable()
                .Where(_ => fader.State == FadeState.Progress)
                .Subscribe(_ =>
                {
                    fader.Update();
                });

            this.UpdateAsObservable()
                .Where(_ => fader.State == FadeState.Post)
                .Take(1)
                .Subscribe(_ =>
                {
                    bubble.SetActive(true);
                });

            this.UpdateAsObservable()
                .Where(_ => fader.State == FadeState.Post)
                .Subscribe(_ =>
                {
                    transform.rotation = TargetRotation(transform, targetPos, Time.deltaTime);

                    transform.position += transform.rotation * postVelocity * Time.deltaTime;
                });
        }

        static Vector3 TargetPosition(Vector3 mousePos, Camera camera)
        {
            const float baseDist = 40;
            const float factor = 3;

            var viewPos = camera.ScreenToViewportPoint(mousePos);
            var distFromCenter = Vector3.Distance(new Vector3(0.5f, 0.5f, viewPos.z), viewPos);

            mousePos.z = baseDist - distFromCenter * factor;

            return camera.ScreenToWorldPoint(mousePos);
        }

        static Quaternion TargetRotation(Transform transform, Vector3 targetPos, float delta)
        {
            var diff = targetPos - transform.position;
            var targetRotation = Quaternion.LookRotation(diff) * Quaternion.Euler(90, 0, 0);

            return Quaternion.Slerp(transform.rotation, targetRotation, delta);
        }
    }
}
