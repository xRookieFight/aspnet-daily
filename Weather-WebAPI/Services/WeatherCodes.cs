namespace Weather_WebAPI.Services;

internal static class WeatherCodes
{
    private static readonly Dictionary<int, string> Descriptions = new()
    {
        [0] = "Açık",
        [1] = "Çoğunlukla açık",
        [2] = "Parçalı bulutlu",
        [3] = "Kapalı",
        [45] = "Sisli",
        [48] = "Kırağılı sis",
        [51] = "Hafif çisenti",
        [53] = "Çisenti",
        [55] = "Yoğun çisenti",
        [56] = "Hafif donan çisenti",
        [57] = "Yoğun donan çisenti",
        [61] = "Hafif yağmur",
        [63] = "Yağmur",
        [65] = "Şiddetli yağmur",
        [66] = "Hafif donan yağmur",
        [67] = "Şiddetli donan yağmur",
        [71] = "Hafif kar",
        [73] = "Kar",
        [75] = "Yoğun kar",
        [77] = "Kar taneleri",
        [80] = "Hafif sağanak",
        [81] = "Sağanak",
        [82] = "Şiddetli sağanak",
        [85] = "Hafif kar sağanağı",
        [86] = "Yoğun kar sağanağı",
        [95] = "Gök gürültülü fırtına",
        [96] = "Hafif dolulu fırtına",
        [99] = "Şiddetli dolulu fırtına"
    };

    public static string Describe(int code) =>
        Descriptions.TryGetValue(code, out var description) ? description : "Bilinmiyor";
}
