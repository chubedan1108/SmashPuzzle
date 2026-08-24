using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BoosterButtonUI : MonoBehaviour
{
    [SerializeField] private BoosterType type;
    [SerializeField] private Button button;
    private void Awake()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        bool succes = BoosterManager.Instance.TryActiveBooster(type, 1);
    }
}