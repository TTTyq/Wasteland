using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class BackpackToggleUI : MonoBehaviour
{
    [SerializeField] private GameObject backpackPanel;
    private bool lastButtonState = false;

    void Start()
    {
        if (backpackPanel != null)
            backpackPanel.SetActive(false); // 启动时隐藏
    }

    void Update()
    {
        // 获取所有右手控制器
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);

        foreach (var device in devices)
        {
            bool buttonValue;
            // secondaryButton 通常是B键（右手）
            if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonValue) && buttonValue && !lastButtonState)
            {
                if (backpackPanel != null)
                    backpackPanel.SetActive(!backpackPanel.activeSelf);
            }
            lastButtonState = buttonValue;
        }
    }
} 