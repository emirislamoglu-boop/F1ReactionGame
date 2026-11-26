using UnityEngine;

public class OpenInstagram : MonoBehaviour
{
    [SerializeField] private string url = "https://instagram.com/emir_islamoglu_";

    public void Open()
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
    }
}