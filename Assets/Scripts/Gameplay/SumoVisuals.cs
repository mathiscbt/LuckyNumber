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
    [SerializeField] private SpriteRenderer[] changingColorSprites;


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

    [Header("Stun")]
    [SerializeField] private float blinkInterval = 0.08f;
    [SerializeField] private float stunnedAlpha = 0.25f;
    private bool isBlinking;
    private float blinkTimer;

    [Header("Elimination")]
    [SerializeField] private Vector3 eliminatedSquash = new Vector3(1.3f, 0.15f, 1f);
    [SerializeField] private float squashDuration = 0.25f;
    private Vector3 restScale;
    private bool eliminated;
    private Coroutine squashRoutine;

    private void Awake()
    {
        sumoMovement = GetComponent<SumoMovement>();
        sumoHealth = GetComponent<SumoHealth>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        if (visualsRoot == null) visualsRoot = transform;
        childSprites = visualsRoot.GetComponentsInChildren<SpriteRenderer>(true);
        restScale = visualsRoot.localScale;
    }

    private void OnEnable()
    {
        if (sumoMovement != null)
        {
            sumoMovement.StartDashing += StartGhosting;
            sumoMovement.EndDashing += StopGhosting;
            sumoMovement.OnScaleChange += AdaptToScale;
            sumoMovement.OnEliminated += HandleEliminated;

            sumoMovement.StartWindup += () => animator.SetTrigger("SetWindup");
            sumoMovement.StartDashing += () => animator.SetTrigger("SetDash");
            sumoMovement.EndDashing += () => animator.SetTrigger("SetIdle");
        }

        if (sumoHealth != null)
        {
            sumoHealth.OnHit += ReactToHit;
            sumoHealth.OnStunned += HandleStunned;
            sumoHealth.OnRecovered += HandleRecovered;
        }
    }

    private void HandleStunned(GameObject _)
    {
        if (eliminated) return;
        isBlinking = true;
        blinkTimer = 0f;
    }

    private void HandleRecovered(GameObject _)
    {
        isBlinking = false;
        SetSpriteAlpha(1f);
    }

    private void HandleEliminated(GameObject _)
    {
        eliminated = true;
        isBlinking = false;
        SetSpriteAlpha(1f);

        if (squashRoutine != null) StopCoroutine(squashRoutine);
        squashRoutine = StartCoroutine(SquashRoutine());
    }

    private IEnumerator SquashRoutine()
    {
        Vector3 target = Vector3.Scale(restScale, eliminatedSquash);
        float elapsed = 0f;

        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / squashDuration);
            float eased = 1f - (1f - t) * (1f - t);
            visualsRoot.localScale = Vector3.LerpUnclamped(restScale, target, eased);
            yield return null;
        }

        squashRoutine = null;
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (childSprites == null) return;

        foreach (SpriteRenderer sr in childSprites)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void Update()
    {
        if (!isBlinking) return;

        blinkTimer += Time.deltaTime;
        bool visible = Mathf.FloorToInt(blinkTimer / blinkInterval) % 2 == 0;
        SetSpriteAlpha(visible ? 1f : stunnedAlpha);
    }

    public void SetColor(Color color)
    {
        foreach (SpriteRenderer sr in changingColorSprites)
        {
            sr.color = color;
        }
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

        ghostRoot.AddComponent<DashGhost>().Init(spawnedRenderers, ghostColor, fadeDuration);
    }




    private void AdaptToScale(float newScale)
    {
        eliminated = false;
        isBlinking = false;

        if (squashRoutine != null)
        {
            StopCoroutine(squashRoutine);
            squashRoutine = null;
        }

        if (visualsRoot == transform) restScale = Vector3.one * newScale;
        visualsRoot.localScale = restScale;
        SetSpriteAlpha(1f);
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
            sumoMovement.OnEliminated -= HandleEliminated;
        }

        if (sumoHealth != null)
        {
            sumoHealth.OnHit -= ReactToHit;
            sumoHealth.OnStunned -= HandleStunned;
            sumoHealth.OnRecovered -= HandleRecovered;
        }
    }
}