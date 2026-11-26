using UnityEngine;

public class F1MenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject f1InfoPanel;   // F1InfoPanel

    private bool isOpen = false;

    // Menü butonu tıkladığında çağrılacak
    public void ToggleF1Panel()
    {
        isOpen = !isOpen;

        if (f1InfoPanel != null)
            f1InfoPanel.SetActive(isOpen);
    }

    // Close butonu için ayrı fonksiyon istersen:
    public void CloseF1Panel()
    {
        isOpen = false;
        if (f1InfoPanel != null)
            f1InfoPanel.SetActive(false);
    }
}