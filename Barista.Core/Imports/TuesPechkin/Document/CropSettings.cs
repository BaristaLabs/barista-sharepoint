namespace Barista.TuesPechkin
{
    public class CropSettings : ISettings
    {
        [WkhtmltoxSetting("crop.top")]
        public double? Top { get; set; }

        [WkhtmltoxSetting("crop.bottom")]
        public double? Bottom { get; set; }

        [WkhtmltoxSetting("crop.width")]
        public double? Width { get; set; }

        [WkhtmltoxSetting("crop.height")]
        public double? Height { get; set; }
    }
}
