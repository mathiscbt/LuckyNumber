using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(SumoMovement), typeof(SumoHealth), typeof(CinemachineImpulseSource))]
public class SumoVisuals : MonoBehaviour
{
    private SumoMovement sumoMovement;
    private SumoHealth sumoHealth;

    [Header("Sprites & Flash")]
    [SerializeField] private SpriteRenderer[] sumoSprites;

    private Coroutine flashCoroutine;


    [Header("Animation")]
    [SerializeField] private Animator animator;
    [Header("Impulse")]
    private CinemachineImpulseSource impulseSource;

    [Header("Ref")]
    [SerializeField] private Transform visualsRoot;

    [Header("Ghost")]
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] private float spawnInterval = 0.03f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private int orderOffset = -5;
    private Coroutine ghostRoutine;

    private SpriteRenderer[] childSprites;

    private void Awake()
    {
        sumoMovement = GetComponent<SumoMovement>();
        sumoHealth = GetComponent<SumoHealth>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        if (visualsRoot == null) visualsRoot = transform;
        childSprites = visualsRoot.GetComponentsInChildren<SpriteRenderer>(true);

    }

    private void OnEnable()
    {
        if (sumoMovement != null)
        {
            sumoMovement.StartDashing += StartGhosting;
            sumoMovement.EndDashing += StopGhosting;
            sumoMovement.OnScaleChange += AdaptToScale;

            sumoMovement.StartWindup += () => animator.SetTrigger("SetWindup");
            sumoMovement.StartDashing += () => animator.SetTrigger("SetDash");
            sumoMovement.EndDashing += () => animator.SetTrigger("SetIdle");
        }

        if (sumoHealth != null)
        {
            sumoHealth.OnHit += ReactToHit;
        }
    }

    public void SetColor(Color color)
    {
        // TODO: appliquer la couleur aux sprites du sumo
    }

    private void Start()
    {
        if (sumoMovement != null && sumoMovement.SumoData != null)
        {
            AdaptToScale(sumoMovement.SumoData.scale);
        }
    }


    public void StartGhosting()
    {
        if (ghostRoutine != null) StopCoroutine(ghostRoutine);
        ghostRoutine = StartCoroutine(SpawnGhosts());
    }

    public void StopGhosting()
    {
        if (ghostRoutine != null) StopCoroutine(ghostRoutine);
    }

    private IEnumerator SpawnGhosts()
    {
        while (true)
        {
            CreateCompositeGhost();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void CreateCompositeGhost()
    {
        GameObject ghostRoot = new GameObject("DashGhost_Composite");
        List<SpriteRenderer> spawnedRenderers = new List<SpriteRenderer>(childSprites.Length);

        foreach (SpriteRenderer source in childSprites)
        {
            if (source == null || !source.gameObject.activeInHierarchy || source.sprite == null)
                continue;

            GameObject part = new GameObject(source.name);
            part.transform.SetParent(ghostRoot.transform, false);
            part.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            part.transform.localScale = source.transform.lossyScale;

            SpriteRenderer sr = part.AddComponent<SpriteRenderer>();
            sr.sprite = source.sprite;
            sr.flipX = source.flipX;
            sr.flipY = source.flipY;
            sr.sortingLayerID = source.sortingLayerID;
            sr.sortingOrder = source.sortingOrder + orderOffset;
            sr.color = ghostColor;

            spawnedRenderers.Add(sr);
        }

        StartCoroutine(FadeAndDestroy(ghostRoot, spawnedRenderers));
    }

    private IEnumerator FadeAndDestroy(GameObject root, List<SpriteRenderer> renderers)
    {
        float elapsed = 0f;
        float startAlpha = ghostColor.a;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            Color c = new Color(ghostColor.r, ghostColor.g, ghostColor.b, currentAlpha);

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null) renderers[i].color = c;
            }

            yield return null;
        }

        Destroy(root);
    }




    private void AdaptToScale(float newScale)
    {

    }


    public void ReactToHit(float value)
    {
        TriggerHitFlash(0.08f);
        impulseSource?.GenerateImpulse(value/100);
    }

    public void TriggerHitFlash(float duration = 0.08f)
    {
        if (sumoSprites == null) return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        //flashCoroutine = StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        Color[] originalColors = new Color[sumoSprites.Length];

        for (int i = 0; i < sumoSprites.Length; i++)
        {
            if (sumoSprites[i] != null)
            {
                originalColors[i] = sumoSprites[i].color;
                sumoSprites[i].color = new Color(10f, 10f, 10f, 1f); 
            }
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < sumoSprites.Length; i++)
        {
            if (sumoSprites[i] != null)
            {
                sumoSprites[i].color = originalColors[i];
            }
        }

        flashCoroutine = null;
    }

    private void OnDisable()
    {
        if (sumoMovement != null)
        {
            sumoMovement.StartDashing -= StartGhosting;
            sumoMovement.EndDashing -= StopGhosting;
            sumoMovement.OnScaleChange -= AdaptToScale;
        }

        if (sumoHealth != null)
        {
            sumoHealth.OnHit -= ReactToHit;
        }
    }
}