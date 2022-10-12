
using BarracudaTestBot;

namespace PtnGen {
    public class PutinGenerator
    {
        private List<string> wordLast = new List<string>() {
            "підріла",
            "підр",
            "піздабол",
            "лох",
            "підарас",
            "пітух",
            "овцейоб",
            "дегенерат",
            "підофіл",
            "мішок з гівном",
            "пєтушара",
            "виблядок",
            "ублюдок",
            "крисюк",
            "мракобєс",
            "любітєль громкіх басов",
            "довбойоб",
            "валізоходець",
            "гном",
            "хуєсос",
        };

        private List<string> adjectives = new List<string>() {
            "мерзенний",
            "паскудний",
            "в попу йобаний",
            "на пляшці сидівший",
            "хуй сосавший",
            "обдрочений",
            "обісраний",
            "тупорилий",
            "обісцаний",
            "лаптєногий",
            "сучий",
            "пиздливий",
            "огидний",
            "йобаний",
            "паскудний",
            "чмошний",
            "кончений",
            "бридкий",
            "гнилий",
            "хворий",
            "йобнутий",
            "хуйовий",
            "скрєпний",
        };

        public string GenerateName()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder("путін ");
            int adjectivesCount = Utilities.GetRandomNumber(1, 3);
            Random rng = new Random();
            builder.Append(string.Join(" ", adjectives.OrderBy(x => rng.Next()).Take(adjectivesCount)));
            builder.Append($" { wordLast[Utilities.GetRandomNumber(0, wordLast.Count())] }");
            return builder.ToString();
        }
    }
}