using System.Globalization;

namespace TP_Product.Models.Help
{
    // Affichage des prix : 3 décimales + devise tunisienne (ex : 25.000 DT)
    public static class FormatPrix
    {
        public static string ToDT(this float prix)
        {
            return prix.ToString("0.000", CultureInfo.InvariantCulture) + " DT";
        }
    }
}
