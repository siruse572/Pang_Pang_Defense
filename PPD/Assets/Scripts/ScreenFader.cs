using System.Collections;
using UnityEngine;

/// <summary>
/// 화면을 검은색으로 페이드 아웃/인 하는 오버레이를 관리합니다.
/// XR 호환을 위해 메인 카메라에 부착된 World Space 캔버스를 사용합니다.
/// (Screen Space Overlay는 VR 헤드셋에서 보이지 않기 때문)
/// 씬에 미리 배치하지 않아도 ScreenFader.Instance 접근 시 자동으로 생성됩니다.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    private static ScreenFader _instance;

    public static ScreenFader Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ScreenFader");
                _instance = go.AddComponent<ScreenFader>();
            }
            return _instance;
        }
    }

    [Tooltip("페이드 아웃/인 각각에 걸리는 시간(초) - 1.5초 + 1.5초 = 총 3초")]
    public float fadeDuration = 1.5f;

    [Tooltip("완전히 검은 상태로 유지하는 시간(초)")]
    public float holdDuration = 1f;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// 검은색 오버레이 캔버스를 (없다면) 생성합니다.
    /// </summary>
    private void EnsureOverlay()
    {
        if (_canvas != null)
            return;

        GameObject canvasGO = new GameObject("ScreenFaderCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.sortingOrder = 32767; // 항상 최상단

        Camera cam = Camera.main;
        if (cam != null)
        {
            // XR/3D 호환: 카메라에 부착된 World Space 오버레이
            _canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.transform.SetParent(cam.transform, false);
            canvasGO.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            canvasGO.transform.localRotation = Quaternion.identity;

            RectTransform rt = _canvas.GetComponent<RectTransform>();
            // 0.3m 앞에서 시야 전체를 덮도록 충분히 크게 설정
            rt.sizeDelta = new Vector2(4f, 4f);
            rt.localScale = Vector3.one;
        }
        else
        {
            // 카메라가 없으면 일반 오버레이로 폴백
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        _canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        // 검은색 풀스크린 이미지
        GameObject imgGO = new GameObject("Black");
        imgGO.transform.SetParent(canvasGO.transform, false);
        UnityEngine.UI.Image img = imgGO.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.black;
        img.raycastTarget = false;

        RectTransform imgRt = img.rectTransform;
        imgRt.anchorMin = Vector2.zero;
        imgRt.anchorMax = Vector2.one;
        imgRt.offsetMin = Vector2.zero;
        imgRt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 페이드 아웃(검게) → midAction 실행 → 잠시 유지 → 페이드 인(밝게) 시퀀스를 실행합니다.
    /// </summary>
    /// <param name="midAction">완전히 검은 상태일 때 실행할 동작(예: 게임 초기화)</param>
    public void FadeOutAndIn(System.Action midAction)
    {
        EnsureOverlay();
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(midAction));
    }

    private IEnumerator FadeRoutine(System.Action midAction)
    {
        // 페이드 아웃 (투명 → 검은색)
        yield return Fade(0f, 1f, fadeDuration);

        // 완전히 검은 상태에서 초기화 동작 실행
        midAction?.Invoke();

        // 검은 화면 유지
        if (holdDuration > 0f)
            yield return new WaitForSeconds(holdDuration);

        // 페이드 인 (검은색 → 투명)
        yield return Fade(1f, 0f, fadeDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (_canvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            _canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        _canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }
}
