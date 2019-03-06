using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToFish
{
    enum FadeState
    {
        Pre, Progress, Post
    };

    class FishFader
    {
        Material parentMaterial = null;
        Transform parentTransform = null;

        Material fadeMaterial = null;
        float fade = 0;
        readonly float fadeLimit = 0;

        List<MeshRenderer> renderers = new List<MeshRenderer>();

        static readonly Vector3 tailPos = new Vector3(-0.25f, -1.5f, 0);
        static readonly Vector3 tailScale = new Vector3(0.5f, 0.5f, 0.5f);

        static readonly Vector3 fadeBaseLocal = new Vector3(-0.5f, 0, 0);
        Vector3 FadeBase { get { return parentTransform.TransformPoint(fadeBaseLocal); } }

        public FadeState State { get; private set; } = FadeState.Pre;

        public FishFader(GameObject parent, Material fadeSrc)
        {
            parentMaterial = parent.GetComponent<MeshRenderer>().material;
            parentTransform = parent.transform;

            fadeMaterial = CreateFadeMaterial(fadeSrc, parentMaterial, fade);

            var backBody = CreateSubBody(parent, fadeMaterial);
            var tail = CreateSubBody(parent, fadeMaterial);

            backBody.Item1.transform.parent = parentTransform;
            backBody.Item1.transform.Rotate(180, 0, 0);
            renderers.Add(backBody.Item2);

            tail.Item1.transform.parent = parentTransform;
            tail.Item1.transform.localPosition = tailPos;
            tail.Item1.transform.localScale = tailScale;
            renderers.Add(tail.Item2);

            const float factor = 1.5f;
            fadeLimit = Vector3.Distance(FadeBase, tail.Item1.transform.position) * factor;
        }

        static Material CreateFadeMaterial(Material src, Material parent, float initFade)
        {
            var result = GameObject.Instantiate(src);
            result.SetColor("_Color", parent.GetColor("_Color"));
            result.SetTexture("_MainTex", parent.GetTexture("_MainTex"));
            result.SetFloat("_Glossiness", parent.GetFloat("_Glossiness"));
            result.SetFloat("_Metallic", parent.GetFloat("_Metallic"));

            result.SetFloat("_Threshold", initFade);

            return result;
        }

        static Tuple<GameObject, MeshRenderer> CreateSubBody(GameObject src, Material fadeMaterial)
        {
            var result = GameObject.Instantiate(src);
            result.GetComponent<Fish>().enabled = false;

            var renderer = result.GetComponent<MeshRenderer>();
            renderer.material = fadeMaterial;

            return new Tuple<GameObject, MeshRenderer>(result, renderer);
        }

        public void Start()
        {
            if (State != FadeState.Pre)
            {
                return;
            }

            fadeMaterial.SetVector("_BasePos", FadeBase);

            State = FadeState.Progress;
        }

        public void Update()
        {
            if (State != FadeState.Progress)
            {
                return;
            }

            const float fadeSpeed = 0.05f;

            fade += fadeSpeed;
            fadeMaterial.SetFloat("_Threshold", fade);

            if (fade > fadeLimit)
            {
                End();
            }
        }

        void End()
        {
            renderers.ForEach(v =>
            {
                v.material = parentMaterial;
            });

            State = FadeState.Post;
        }
    }
}
