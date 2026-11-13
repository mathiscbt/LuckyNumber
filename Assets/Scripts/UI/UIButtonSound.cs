using Ami.BroAudio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    private SoundID hoverSound;
    private SoundID clickSound;

    public void Init(SoundID hover, SoundID click)
    {
        hoverSound = hover;
        clickSound = click;
        GetComponent<Button>().onClick.AddListener(() => BroAudio.Play(clickSound));
    }

    public void OnPointerEnter(PointerEventData eventData) => BroAudio.Play(hoverSound);

    public void OnSelect(BaseEventData eventData)
    {
        if (eventData is PointerEventData) return;
        BroAudio.Play(hoverSound);
    }
}
