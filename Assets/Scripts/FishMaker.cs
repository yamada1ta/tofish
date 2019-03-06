using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ToFish
{
    public class FishMaker : MonoBehaviour
    {
        [SerializeField]
        Fish Src = null;

        List<Fish> fishes = new List<Fish>();

        const int maxNum = 100;
        const int intervalMSec = 100;

        float leftLimit = 0;
        float rightLimit = 0;

        void Start()
        {
            leftLimit = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.transform.position.z)).x;
            rightLimit = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, Camera.main.transform.position.z)).x;

            Observable
                .Interval(System.TimeSpan.FromMilliseconds(intervalMSec))
                .Where(_ => fishes.Count < maxNum)
                .Subscribe(_ =>
                {
                    fishes.Add(CreateFish(Src, leftLimit, rightLimit));
                })
                .AddTo(this);

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    var dead = fishes.Where(v => !v.IsAlive);
                    fishes = fishes.Where(v => v.IsAlive).ToList();

                    foreach (var fish in dead)
                    {
                        Destroy(fish.gameObject);
                    }
                });
        }

        static Fish CreateFish(Fish src, float left, float right)
        {
            var fish = Instantiate(src.gameObject);

            var x = Random.Range(left, right);
            var y = -10;
            var z = Random.Range(-8f, 4f);
            fish.transform.position = new Vector3(x, y, z);

            const float scaleStep = 0.5f;
            var yScale = Random.Range(1, 2) * scaleStep;
            var xzScale = yScale * Random.Range(1, 4) * scaleStep;
            fish.transform.localScale = new Vector3(xzScale, yScale, xzScale);

            var material = fish.GetComponent<MeshRenderer>().material;

            var hue = Mathf.InverseLerp(left, right, x) + 0.5f;
            hue = hue > 1 ? hue - 1 : hue;
            const float saturation = 0.25f;
            const float value = 0.65f;

            material.SetColor("_Color", Color.HSVToRGB(hue, saturation, value));

            fish.SetActive(true);

            return fish.GetComponent<Fish>();
        }
    }
}