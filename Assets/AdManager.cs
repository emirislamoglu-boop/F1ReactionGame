using UnityEngine;
using GoogleMobileAds.Api; // AdMob kütüphanesi

public class AdManager : MonoBehaviour
{
    // Test ID'si (Gerçek yayına çıkana kadar bunu kullanmalısın!)
    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712"; 
    private InterstitialAd _interstitialAd;

    void Start()
    {
        // SDK'yı başlat
        MobileAds.Initialize(initStatus => { });
        LoadInterstitialAd();
    }

    public void LoadInterstitialAd()
    {
        // Eski reklamı temizle
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        // Yeni reklam isteği oluştur
        var adRequest = new AdRequest();

        // Reklamı yükle
        InterstitialAd.Load(_adUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Reklam yüklenemedi: " + error);
                return;
            }
            _interstitialAd = ad;
        });
    }

    public void ShowAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
            LoadInterstitialAd(); // Bir sonraki ceza için yeni reklamı arkada yükle
        }
        else
        {
            Debug.Log("Reklam henüz hazır değil.");
        }
    }
}