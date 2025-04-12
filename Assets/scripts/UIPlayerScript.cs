using UnityEngine;
using TMPro;

public class UIPlayerScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lengthText;

    private void OnEnable()
    {
        PlayerLength.ChangeLengthEvent += ChangeLengthText;
    }

    private void OnDisable()
    {
        PlayerLength.ChangeLengthEvent -= ChangeLengthText;
    }

    private void ChangeLengthText(ushort length)
    {
        lengthText.text = length.ToString();
    }
}
