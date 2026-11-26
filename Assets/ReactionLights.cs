using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;   // UI tıklamalarını ayırmak için
using TMPro;

public class ReactionLights : MonoBehaviour
{
    [Header("Lights")]
    public Image[] activeLights;          // 10 eleman: her kolon için 2 alt ışık

    [Header("Textler")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI infoText;          // Uyarılar / mesajlar (false start vs.)
    public TextMeshProUGUI bestTimeText;      // "Best: ..." yazısı
    public TextMeshProUGUI averageTimeText;   // "Avg: ..." yazısı

    [Header("Renkler")]
    public Color offColor = new Color(74f/255f, 74f/255f, 74f/255f, 1f); // #4A4A4A  1f sonradan eklandi
    public Color onColor  = new Color(1f, 0f, 0f, 1f);    // tam kırmızı sondaki 1f sonradan eklendi

    [Header("Ses")]
    public AudioSource beepSource;        // kırmızı ışık sesi
    public AudioSource startBeepSource;   // yeşil / start sesi (istersen boş bırak)

    // Bu satırı sınıfa (class) eklemelisin
    public AdManager adManager;

    // Durum değişkenleri
    private bool waitingForStart = false; // Sekans oynuyor (kırmızı ışıklar / random delay)
    private bool raceStarted = false;     // Yeşil yandı, tepki bekleniyor
    private float startTime;

    // Best time
    private const string BEST_TIME_KEY = "BestTime";
    private float bestTime = -1f; // henüz yok

    // Ortalama için istatistikler
    private const string TOTAL_RUNS_KEY = "TotalRuns";
    private const string TOTAL_TIME_KEY = "TotalReactionTime";
    private int totalRuns = 0;
    private float totalReactionTime = 0f;

    // False start sayacı
    private int consecutiveFalseStarts = 0;
    private Coroutine currentSequenceCoroutine;

    void Start()
{
    // 1) offColor'u SAHNEDEN oku
    if (activeLights != null && activeLights.Length > 0 && activeLights[0] != null)
    {
        // activeLights[0] editörde hangi griyse, offColor o olacak
        offColor = activeLights[0].color;//aktif yaptim deneme olarak
    }
    else
    {
        // Emniyet: activeLights boşsa fallback bir gri
        offColor = new Color(0.45f, 0.45f, 0.45f);
    }

    // 2) Başta tüm ışıkları kapat (o ilk gri renge çek)
    SetAllLights(offColor);

    // Best score'u PlayerPrefs'ten oku
    bestTime = PlayerPrefs.GetFloat(BEST_TIME_KEY, -1f);
    UpdateBestTimeText();

    // Ortalama için kayıtları oku
    totalRuns = PlayerPrefs.GetInt(TOTAL_RUNS_KEY, 0);
    totalReactionTime = PlayerPrefs.GetFloat(TOTAL_TIME_KEY, 0f);
    UpdateAverageTimeText();

    // HintText başta görünsün
    if (hintText != null)
        hintText.gameObject.SetActive(true);

    // infoText'i temizle ve gizle
    if (infoText != null)
    {
        infoText.text = "";
        infoText.gameObject.SetActive(false);
    }

    // Timer başlangıç değeri
    if (timerText != null)
        timerText.text = "00.000";
}

   void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        // 1. ADIM: UI KONTROLÜ (Senin kodun yerine bu yardımcı fonksiyonu çağırıyoruz)
        if (IsPointerOverUI())
            return; 

        // --- Bundan sonrası senin orijinal oyun mantığın, hiçbir şeye dokunmuyoruz ---

        // İlk tıklamada hintText'i gizle
        if (hintText != null && hintText.gameObject.activeSelf)
            hintText.gameObject.SetActive(false);

        // FALSE START: Sekans oynarken tıklarsa
        if (waitingForStart && !raceStarted)
        {
            HandleFalseStart();
        }
        // Henüz sekans başlamamışsa → başlat
        else if (!waitingForStart && !raceStarted)
        {
            currentSequenceCoroutine = StartCoroutine(StartSequence());
        }
        // Işıklar söndükten sonra tıklandıysa
        else if (raceStarted)
        {
            HandleReactionTap();
        }
    }
}

void HandleFalseStart()
{
    // 1. Devam eden sekansı durdur (Donmayı engeller)
    if (currentSequenceCoroutine != null)
    {
        StopCoroutine(currentSequenceCoroutine);
        currentSequenceCoroutine = null;
    }

    // 2. Durumları ve Işıkları sıfırla
    waitingForStart = false;
    raceStarted = false;
    SetAllLights(offColor);

    // 3. Yazıları güncelle
    if (timerText != null) timerText.text = "EARLY";

    // 4. Sayaç ve Reklam Mantığı (Birleştirildi)
    consecutiveFalseStarts++; 

    if (infoText != null)
    {
        infoText.gameObject.SetActive(true);
        // Eğer 3. hataya ulaştıysak reklamı tetikle
        if (consecutiveFalseStarts >= 2)
        {
            infoText.text = "Not started yet!";
            
            if (adManager != null)
            {
                adManager.ShowAd(); // REKLAM BURADA PATLIYOR
            }
            
            consecutiveFalseStarts = 0; // Reklamdan sonra sayacı sıfırla
        }
        else
        {
            infoText.text = "Too early!";
        }
    }

    // Hatalı başlangıçtan sonra da saatin akmaya devam ettiğinden emin olalım
    Time.timeScale = 1f;
}


// 2. ADIM: BU YARDIMCI FONKSİYONU UPDATE'İN DIŞINA (Altına) YAPIŞTIR
private bool IsPointerOverUI()
{
    // EventSystem yoksa hata vermemesi için kontrol
    if (EventSystem.current == null) return false;

    // Mouse için kontrol (Unity Editor'de çalışır)
    if (EventSystem.current.IsPointerOverGameObject()) return true;

    // Touch (Dokunmatik) için kontrol (Emülatör ve Mobilde çalışır)
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        // Parmağın ID'sini göndererek UI üzerinde mi bakıyoruz
        if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return true;
    }
    return false;
}

    void HandleReactionTap()
    {
        float reaction = Time.time - startTime;

        if (timerText != null)
            timerText.text = reaction.ToString("0.000");

        raceStarted = false;

        // Geçerli run olduğuna göre false start sayacını sıfırla
        consecutiveFalseStarts = 0;

        // Best score kontrolü
        if (bestTime < 0f || reaction < bestTime)
        {
            bestTime = reaction;
            PlayerPrefs.SetFloat(BEST_TIME_KEY, bestTime);
            UpdateBestTimeText();
        }

        // Ortalama için istatistik güncelle
        totalRuns++;
        totalReactionTime += reaction;

        PlayerPrefs.SetInt(TOTAL_RUNS_KEY, totalRuns);
        PlayerPrefs.SetFloat(TOTAL_TIME_KEY, totalReactionTime);
        PlayerPrefs.Save();

        UpdateAverageTimeText();
    }

    IEnumerator StartSequence()
    {

        // OYUN SAATİNİ TEKRAR BAŞLAT (Reklamdan kalma donmayı çözer)
        Time.timeScale = 1f;

        waitingForStart = true;
        raceStarted = false;

        if (timerText != null)
            timerText.text = "00.000";

        // infoText'i gizle (önceki uyarılar kalmasın)
        if (infoText != null)
            infoText.gameObject.SetActive(false);

        // Hepsini gri yap
        SetAllLights(offColor);

        // 5 kolon, her kolonda alttaki 2 ışığı yakıyoruz
        int columns = 5;
        for (int col = 0; col < columns; col++)
        {
            int baseIndex = col * 2;

            SetLightColor(baseIndex,     onColor);
            SetLightColor(baseIndex + 1, onColor);

            if (beepSource != null)
                beepSource.Play();

            yield return new WaitForSeconds(0.7f); // kolonlar arası süre
        }

        // Rastgele bekleme (tahmin edilemesin)
        float randomDelay = Random.Range(0.7f, 2.5f);
        yield return new WaitForSeconds(randomDelay);

        // Start anında hepsi söner
        SetAllLights(offColor);

        // Yeşil yani start anında farklı bir ses çalmak istersen:
        if (startBeepSource != null)
            startBeepSource.Play();

        startTime = Time.time;
        raceStarted = true;
        waitingForStart = false;
    }


/*
    void SetAllLights(Color c)
    {
        if (activeLights == null) return;

        foreach (var img in activeLights)
        {
            if (img != null)
            {
                // Ani geçiş
                img.CrossFadeColor(c, 0.0f, false, false);
            }
        }
    }

    // IŞIK GEÇİŞLERİ İÇİN YUMUŞAK FADE
    void SetLightColor(int index, Color c)
    {
        if (activeLights == null) return;
        if (index < 0 || index >= activeLights.Length) return;

        var img = activeLights[index];
        if (img != null)
        {
            // 0.15 saniyede yumuşak renk geçişi
            img.CrossFadeColor(c, 0.15f, false, false);
        }
    }
*/

void SetAllLights(Color c)
{
    if (activeLights == null) return;

    foreach (var img in activeLights)
    {
        if (img == null) continue;

        // Fade yerine direkt set: çakışma/solgunluk biter
        img.color = c;
        // alpha garanti
        var col = img.color;
        col.a = 1f;
        img.color = col;
    }
}

void SetLightColor(int index, Color c)
{
    if (activeLights == null) return;
    if (index < 0 || index >= activeLights.Length) return;

    var img = activeLights[index];
    if (img == null) return;

    img.color = c;
    var col = img.color;
    col.a = 1f;
    img.color = col;
}

// bu kismi ekledim yeni

    void UpdateBestTimeText()
    {
        if (bestTimeText == null) return;

        if (bestTime < 0f)
        {
            bestTimeText.text = "Best: ---.---";
        }
        else
        {
            bestTimeText.text = $"Best: {bestTime:0.000} s";
        }
    }

    void UpdateAverageTimeText()
    {
        if (averageTimeText == null) return;

        if (totalRuns <= 0)
        {
            averageTimeText.text = "Avg: ---.---";
        }
        else
        {
            float avg = totalReactionTime / totalRuns;
            averageTimeText.text = $"Avg: {avg:0.000} s";
        }
    }

    // İleride kullanmak istersen:
    float GetAverageReaction()
    {
        if (totalRuns <= 0) return 0f;
        return totalReactionTime / totalRuns;
    }

    string GetReactionComment(float reaction)
    {
        if (reaction < 0.17f) return "Elite (F1 level)";
        if (reaction < 0.23f) return "Very fast";
        if (reaction < 0.30f) return "Average";
        return "Needs improvement";
    }
}