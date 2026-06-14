using System.Collections;
using UnityEngine;

/// <summary>
/// 데미지를 입었을 때 일시적으로 오브젝트의 모든 렌더러를 빨간색 단색 마테리얼로 바꿨다가 복구하는 스크립트입니다.
/// </summary>
public class DamageFlash : MonoBehaviour
{
    private static Material flashMaterial;
    [SerializeField] private float defaultFlashDuration = 0.15f;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // 자식 오브젝트들을 포함한 모든 렌더러(MeshRenderer, SkinnedMeshRenderer 등)를 찾습니다.
        renderers = GetComponentsInChildren<Renderer>(true);
        originalMaterials = new Material[renderers.Length][];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalMaterials[i] = renderers[i].sharedMaterials;
            }
        }

        CreateFlashMaterial();
    }

    private void CreateFlashMaterial()
    {
        if (flashMaterial != null) return;

        // URP의 Unlit / Lit 또는 Standard 셰이더를 안전하게 찾습니다.
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Standard");

        if (shader != null)
        {
            flashMaterial = new Material(shader);
            flashMaterial.name = "DamageFlash_Red_Material";
            
            if (flashMaterial.HasProperty("_BaseColor"))
            {
                flashMaterial.SetColor("_BaseColor", Color.red);
            }
            else if (flashMaterial.HasProperty("_Color"))
            {
                flashMaterial.SetColor("_Color", Color.red);
            }
        }
        else
        {
            Debug.LogWarning("DamageFlash: 적합한 셰이더를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 오브젝트를 일시적으로 빨간색으로 플래시합니다.
    /// </summary>
    public void Flash()
    {
        Flash(defaultFlashDuration);
    }

    /// <summary>
    /// 오브젝트를 지정된 시간(초) 동안 빨간색으로 플래시합니다.
    /// </summary>
    public void Flash(float duration)
    {
        if (renderers == null || renderers.Length == 0) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        if (flashMaterial == null) CreateFlashMaterial();
        if (flashMaterial == null) yield break;

        // 모든 렌더러의 마테리얼을 빨간색 플래시 마테리얼로 교체
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null || originalMaterials[i] == null) continue;

            Material[] flashMats = new Material[originalMaterials[i].Length];
            for (int j = 0; j < flashMats.Length; j++)
            {
                flashMats[j] = flashMaterial;
            }
            r.sharedMaterials = flashMats;
        }

        yield return new WaitForSeconds(duration);

        // 원래 마테리얼로 복구
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null || originalMaterials[i] == null) continue;
            r.sharedMaterials = originalMaterials[i];
        }

        flashCoroutine = null;
    }

    private void OnDestroy()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
    }
}
