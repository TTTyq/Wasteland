using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ShowCenterUIOnHover : MonoBehaviour
{
    public GameObject uiCanvas;

    private XRBaseInteractable interactable;

    private void OnEnable()
    {
        interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEnter);
            interactable.hoverExited.AddListener(OnHoverExit);
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEnter);
            interactable.hoverExited.RemoveListener(OnHoverExit);
        }
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        Debug.Log($"{gameObject.name}£ºHover Enter - ÏÔÊ¾ UI");
        uiCanvas?.SetActive(true);
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        Debug.Log($"{gameObject.name}£ºHover Exit - Òþ²Ø UI");
        uiCanvas?.SetActive(false);
    }
}

