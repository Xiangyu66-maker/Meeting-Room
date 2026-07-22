using UnityEngine;

public class PigRageVisual : MonoBehaviour
{
    [Header("Pig Renderers")]
    [Tooltip("留空时自动寻找猪下面的所有Renderer")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Rage Colour")]
    [Tooltip("最高狂暴状态时的目标颜色")]
    [SerializeField]
    private Color maximumRageColor =
        new Color(1f, 0.05f, 0.05f, 1f);

    [Range(0f, 1f)]
    [Tooltip("原材质与红色混合的最大程度")]
    [SerializeField] private float maximumColourBlend = 0.65f;

    [Header("Optional Emission")]
    [Tooltip("是否增加红色发光效果")]
    [SerializeField] private bool useEmission = false;

    [SerializeField]
    private Color emissionColor =
        new Color(1f, 0f, 0f, 1f);

    [Tooltip("最高狂暴状态的发光强度")]
    [SerializeField] private float maximumEmissionIntensity = 1f;

    private Material[][] runtimeMaterials;
    private Color[][] originalBaseColours;
    private Color[][] originalEmissionColours;

    private static readonly int BaseColorId =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorId =
        Shader.PropertyToID("_Color");

    private static readonly int EmissionColorId =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        /*
         * 没有手动指定时，
         * 自动获取猪及子物体上的所有Renderer。
         */
        if (targetRenderers == null ||
            targetRenderers.Length == 0)
        {
            targetRenderers =
                GetComponentsInChildren<Renderer>(true);
        }

        CacheMaterials();
        SetRageAmount(0f);
    }

    private void CacheMaterials()
    {
        runtimeMaterials =
            new Material[targetRenderers.Length][];

        originalBaseColours =
            new Color[targetRenderers.Length][];

        originalEmissionColours =
            new Color[targetRenderers.Length][];

        for (int rendererIndex = 0;
             rendererIndex < targetRenderers.Length;
             rendererIndex++)
        {
            Renderer targetRenderer =
                targetRenderers[rendererIndex];

            if (targetRenderer == null)
            {
                runtimeMaterials[rendererIndex] =
                    new Material[0];

                originalBaseColours[rendererIndex] =
                    new Color[0];

                originalEmissionColours[rendererIndex] =
                    new Color[0];

                continue;
            }

            /*
             * materials会创建这只猪专用的运行时材质。
             * 不会永久修改资源包中的原始材质。
             */
            runtimeMaterials[rendererIndex] =
                targetRenderer.materials;

            int materialCount =
                runtimeMaterials[rendererIndex].Length;

            originalBaseColours[rendererIndex] =
                new Color[materialCount];

            originalEmissionColours[rendererIndex] =
                new Color[materialCount];

            for (int materialIndex = 0;
                 materialIndex < materialCount;
                 materialIndex++)
            {
                Material material =
                    runtimeMaterials[rendererIndex]
                        [materialIndex];

                if (material == null)
                {
                    originalBaseColours[rendererIndex]
                        [materialIndex] = Color.white;

                    originalEmissionColours[rendererIndex]
                        [materialIndex] = Color.black;

                    continue;
                }

                if (material.HasProperty(BaseColorId))
                {
                    originalBaseColours[rendererIndex]
                        [materialIndex] =
                        material.GetColor(BaseColorId);
                }
                else if (material.HasProperty(ColorId))
                {
                    originalBaseColours[rendererIndex]
                        [materialIndex] =
                        material.GetColor(ColorId);
                }
                else
                {
                    originalBaseColours[rendererIndex]
                        [materialIndex] =
                        Color.white;
                }

                if (material.HasProperty(EmissionColorId))
                {
                    originalEmissionColours[rendererIndex]
                        [materialIndex] =
                        material.GetColor(EmissionColorId);

                    if (useEmission)
                    {
                        material.EnableKeyword("_EMISSION");
                    }
                }
                else
                {
                    originalEmissionColours[rendererIndex]
                        [materialIndex] =
                        Color.black;
                }
            }
        }
    }

    /// <summary>
    /// amount:
    /// 0 = 正常颜色
    /// 1 = 最高狂暴颜色
    /// </summary>
    public void SetRageAmount(float amount)
    {
        amount = Mathf.Clamp01(amount);

        float colourBlend =
            amount * maximumColourBlend;

        if (runtimeMaterials == null)
        {
            return;
        }

        for (int rendererIndex = 0;
             rendererIndex < runtimeMaterials.Length;
             rendererIndex++)
        {
            for (int materialIndex = 0;
                 materialIndex <
                 runtimeMaterials[rendererIndex].Length;
                 materialIndex++)
            {
                Material material =
                    runtimeMaterials[rendererIndex]
                        [materialIndex];

                if (material == null)
                {
                    continue;
                }

                Color originalColour =
                    originalBaseColours[rendererIndex]
                        [materialIndex];

                Color targetColour =
                    maximumRageColor;

                /*
                 * 保留原材质的透明度。
                 */
                targetColour.a =
                    originalColour.a;

                Color finalColour =
                    Color.Lerp(
                        originalColour,
                        targetColour,
                        colourBlend
                    );

                if (material.HasProperty(BaseColorId))
                {
                    material.SetColor(
                        BaseColorId,
                        finalColour
                    );
                }
                else if (material.HasProperty(ColorId))
                {
                    material.SetColor(
                        ColorId,
                        finalColour
                    );
                }

                UpdateEmission(
                    rendererIndex,
                    materialIndex,
                    material,
                    amount
                );
            }
        }
    }

    private void UpdateEmission(
        int rendererIndex,
        int materialIndex,
        Material material,
        float amount)
    {
        if (!material.HasProperty(EmissionColorId))
        {
            return;
        }

        Color originalEmission =
            originalEmissionColours[rendererIndex]
                [materialIndex];

        if (!useEmission)
        {
            material.SetColor(
                EmissionColorId,
                originalEmission
            );

            return;
        }

        material.EnableKeyword("_EMISSION");

        Color targetEmission =
            emissionColor *
            maximumEmissionIntensity;

        targetEmission.a = 1f;

        Color finalEmission =
            Color.Lerp(
                originalEmission,
                targetEmission,
                amount
            );

        material.SetColor(
            EmissionColorId,
            finalEmission
        );
    }

    public void ResetVisual()
    {
        SetRageAmount(0f);
    }
}