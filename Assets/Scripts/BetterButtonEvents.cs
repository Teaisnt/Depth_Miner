using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UltEvents;
using NaughtyAttributes;
using UnityEngine.VFX;

public class BetterButtonEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler {
    public VisualEffect sparkleVFX;
    public UltEvent OnEnabled;
    public UltEvent OnStart;
    
    [ShowIf(nameof(CheckForButton))]
    public UltEvent OnClick;

    [HideIf(nameof(CheckForButton))]
    public UltEvent OnLeftClick;
    public UltEvent OnLeftDown;
    public UltEvent OnLeftReleased;
    
    public UltEvent OnRightClick;
    public UltEvent OnMiddleClick;

    public UltEvent OnTouchingStart;
    public UltEvent OnTouching;
    public UltEvent OnTouchingEnd;

    public UltEvent OnDisabled;

    private bool touching = false;

    void OnEnable() => OnEnabled.InvokeX();

    // Start is called before the first frame update
    void Start() {
        if (TryGetComponent(out Button button)) {
            button.onClick.AddListener(() => {
                OnClick.InvokeX();
            });
        }

        OnStart.InvokeX();
    }

    // Update is called once per frame
    void Update() {
        if (touching) {
            OnTouching.InvokeX();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OnTouchingStart.InvokeX();
        touching = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        OnTouchingEnd.InvokeX();
        touching = false;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            OnLeftClick.InvokeX();
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right) {
            OnRightClick.InvokeX();
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Middle) {
            OnMiddleClick.InvokeX();
        }
    }

    private bool CheckForButton() {
        return TryGetComponent(out Button button);
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            OnLeftReleased.InvokeX();
            return;
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            OnLeftDown.InvokeX();
            return;
        }
    }
    public void PlaySoundEffect(AudioClip audioClip)
    {
        this.GetComponent<AudioSource>().PlayOneShot(audioClip);
    }
    public void InstantiateSparkles()
    {
        Instantiate(sparkleVFX,new Vector3(650,150,100), new Quaternion(0,0,0,1));
    }

    void OnDisable() {
        if (transform.root.gameObject.activeSelf) {
            OnDisabled.InvokeX();
        }
    }
}
