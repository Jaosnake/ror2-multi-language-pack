using System;
using System.Collections.Generic;

namespace PELE.KingDBZFix
{
    public static class DBZLocalizationDictionary
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _languages = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["pt-BR"] = new Dictionary<string, string>
            {
                ["Transformations"] = "Transformações",
                ["KINGMOD_REMOVE"] = "REMOVER",
                ["KINGMOD_TOGGLE_EXTRA"] = "ALTERNAR EXTRAS",
                ["KINGMOD_COSTS_PREFIX"] = "Custa ",
                ["KINGMOD_TOTAL_MASTERY_REQUIRED"] = "Níveis de Maestria Totais Necessários: ",
                ["KINGMOD_TOTAL_MASTERY_LEVELS"] = "Níveis de Maestria Totais",
                ["KINGMOD_KI_DRAIN"] = "Dreno de Ki: ",
                ["KINGMOD_PER_SECOND"] = " Por Segundo",
                ["KINGMOD_CHARGE_POWER"] = "Poder de Carga: ",
                ["KINGMOD_STAT_DAMAGE"] = "Dano: ",
                ["KINGMOD_STAT_ARMOR"] = "Armadura: ",
                ["KINGMOD_STAT_HEALTH_REGEN"] = "Regeneração de Vida: ",
                ["KINGMOD_STAT_ATTACK_SPEED"] = "Velocidade de Ataque: ",
                ["KINGMOD_STAT_MOVE_SPEED"] = "Velocidade de Movimento: ",
                ["KINGMOD_STAT_MAX_HEALTH"] = "Vida Máxima: ",
                ["KINGMOD_MASTERED"] = "Dominado ",
                ["KINGMOD_POWER_LEVEL"] = "Nível de Poder: ",
                ["KINGMOD_NO_STAT_BOOSTS"] = " não possui bônus de atributos...",
                ["KINGMOD_ALL_FORMS_UNLOCKED"] = "Todas as Formas desbloqueadas?",
                ["KINGMOD_UNTIL_NEXT_UNLOCK"] = " nível(is) de Maestria até o próximo desbloqueio",
                ["KINGMOD_COSTS_KI_FORMAT"] = "Custa <color=#03bcff>{0} de Ki</color>",
                ["KINGMOD_DAMAGE_FORMAT"] = "<color=#E5C962>{0}% de Dano</color>",
                ["KINGMOD_STAT_NAME_DAMAGE"] = "Dano",
                ["KINGMOD_STAT_NAME_ARMOR"] = "Armadura",
                ["KINGMOD_STAT_NAME_ATTACKSPEED"] = "Velocidade de Ataque",
                ["KINGMOD_STAT_NAME_MOVESPEED"] = "Velocidade de Movimento",
                ["KINGMOD_STAT_NAME_REGEN"] = "Regeneração de Vida",
                ["KINGMOD_STAT_NAME_MAXHEALTH"] = "Vida Máxima",
                ["KINGMOD_TUTORIAL_F1"] = "Ativar/Desativar Tutoriais pressionando F1",
                ["KINGMOD_TUTORIAL_FORM_MASTERY"] = "Com a Maestria de Forma, ganhe Experiência com o tempo ou causando dano a inimigos. Subir o nível da forma concede o seguinte:\n- Um aumento no bônus de atributos da transformação.\n- Uma redução no dreno de Ki da transformação.\n- Um aumento na regeneração geral de Ki.\nNa Forma Base, regeneração de Ki e Ki máximo são aumentados.",
                ["KINGMOD_TUTORIAL_TRANSFORM_TRIGGER"] = "Quando seu nível de maestria for alto o suficiente, ative uma Transformação Principal tocando no botão de transformação (Padrão 'C') e reverta pressionando (Padrão 'X'). Ao desbloquear mais Transformações, pressione o botão novamente para ascender ainda mais além!",
                ["KINGMOD_TUTORIAL_ULTIMATE_SKILL"] = "Enquanto transformado, você tem acesso a uma Habilidade Suprema. Ative-a pressionando a tecla vinculada abaixo.",
                ["KINGMOD_TUTORIAL_KI_RESOURCE"] = "Ki é um recurso que suas habilidades utilizam. Recupere Ki atingindo inimigos com sua habilidade primária.",
                ["KINGMOD_TUTORIAL_ZENKAI"] = "Ganhe Zenkais morrendo ou ficando abaixo de 20% de vida por 30 segundos. Concede atributos bônus permanentes.",
                ["KINGMOD_TUTORIAL_LIGHT_CHARGE"] = "Ganhe Carga de Luz atingindo inimigos com sua espada. Quando cheia, ative-a segurando o Botão de Transformação. Concede Dano e Chance Crítica bônus enquanto ativa.",
                ["KINGMOD_TUTORIALS_ENABLED"] = "Tutoriais Ativados",
                ["KINGMOD_TUTORIALS_DISABLED"] = "Tutoriais Desativados"
            },
            ["tr"] = new Dictionary<string, string>
            {
                ["Transformations"] = "Dönüşümler",
                ["KINGMOD_REMOVE"] = "KALDIR",
                ["KINGMOD_TOGGLE_EXTRA"] = "EKSTRALARI AÇ/KAPAT",
                ["KINGMOD_COSTS_PREFIX"] = "Gereken: ",
                ["KINGMOD_TOTAL_MASTERY_REQUIRED"] = "Gereken Toplam Ustalık Seviyesi: ",
                ["KINGMOD_TOTAL_MASTERY_LEVELS"] = "Toplam Ustalık Seviyeleri",
                ["KINGMOD_KI_DRAIN"] = "Ki Tüketimi: ",
                ["KINGMOD_PER_SECOND"] = " Saniye Başına",
                ["KINGMOD_CHARGE_POWER"] = "Şarj Gücü: ",
                ["KINGMOD_STAT_DAMAGE"] = "Hasar: ",
                ["KINGMOD_STAT_ARMOR"] = "Zırh: ",
                ["KINGMOD_STAT_HEALTH_REGEN"] = "Can Yenilenmesi: ",
                ["KINGMOD_STAT_ATTACK_SPEED"] = "Saldırı Hızı: ",
                ["KINGMOD_STAT_MOVE_SPEED"] = "Hareket Hızı: ",
                ["KINGMOD_STAT_MAX_HEALTH"] = "Maksimum Can: ",
                ["KINGMOD_MASTERED"] = "Ustalık Kazanıldı ",
                ["KINGMOD_POWER_LEVEL"] = "Güç Seviyesi: ",
                ["KINGMOD_NO_STAT_BOOSTS"] = " özellik takviyesi yok...",
                ["KINGMOD_ALL_FORMS_UNLOCKED"] = "Tüm Formlar açıldı mı?",
                ["KINGMOD_UNTIL_NEXT_UNLOCK"] = " Ustalık seviyesi sonraki açılışa kadar",
                ["KINGMOD_COSTS_KI_FORMAT"] = "<color=#03bcff>{0} Ki</color> Harcar",
                ["KINGMOD_DAMAGE_FORMAT"] = "<color=#E5C962>%{0} Hasar</color>",
                ["KINGMOD_STAT_NAME_DAMAGE"] = "Hasar",
                ["KINGMOD_STAT_NAME_ARMOR"] = "Zırh",
                ["KINGMOD_STAT_NAME_ATTACKSPEED"] = "Saldırı Hızı",
                ["KINGMOD_STAT_NAME_MOVESPEED"] = "Hareket Hızı",
                ["KINGMOD_STAT_NAME_REGEN"] = "Can Yenilenmesi",
                ["KINGMOD_STAT_NAME_MAXHEALTH"] = "Maksimum Can",
                ["KINGMOD_TUTORIAL_F1"] = "F1 tuşuna basarak Öğreticileri Aç/Kapat",
                ["KINGMOD_TUTORIAL_FORM_MASTERY"] = "Form Ustalığı ile zamanla veya düşmanlara hasar vererek Deneyim kazanın. Form seviyesini yükseltmek şunları sağlar:\n- Dönüşümünüzün özellik takviyesinde artış.\n- Dönüşümünüzün Ki tüketiminde azalma.\n- Genel Ki yenilenmenizde artış.\nTemel Formda Ki yenilenmesi ve maksimum Ki kazanılır.",
                ["KINGMOD_TUTORIAL_TRANSFORM_TRIGGER"] = "Ustalık seviyeniz yeterince yükseldiğinde, dönüşüm düğmesine (Varsayılan 'C') dokunarak bir Ana Dönüşümü etkinleştirin ve (Varsayılan 'X') basarak Gücü Azaltın. Daha fazla Dönüşüm açıldığında, daha da öteye yükselmek için dönüşüm düğmesine tekrar basın!",
                ["KINGMOD_TUTORIAL_ULTIMATE_SKILL"] = "Dönüşmüş durumdayken bir Nihai Yeteneğe erişiminiz olur. Aşağıda atanan tuşa basarak etkinleştirin.",
                ["KINGMOD_TUTORIAL_KI_RESOURCE"] = "Ki, yeteneklerinizin kullandığı bir kaynaktır. Birincil yeteneğinizle düşmanlara vurarak bir kısmını geri kazanın.",
                ["KINGMOD_TUTORIAL_ZENKAI"] = "Ölerek veya 30 saniye boyunca %20 canın altında kalarak Zenkai kazanın. Kalıcı bonus özellikler sağlar.",
                ["KINGMOD_TUTORIAL_LIGHT_CHARGE"] = "Kılıcınızla düşmanlara vurarak Işık Şarjı kazanın. Dolduğunda, Dönüşüm Düğmesini basılı tutarak etkinleştirin. Aktifken bonus Hasar ve Kritik Şansı sağlar.",
                ["KINGMOD_TUTORIALS_ENABLED"] = "Öğreticiler Etkinleştirildi",
                ["KINGMOD_TUTORIALS_DISABLED"] = "Öğreticiler Devre Dışı"
            },
            ["en"] = new Dictionary<string, string>
            {
                ["Transformations"] = "Transformations",
                ["KINGMOD_REMOVE"] = "REMOVE",
                ["KINGMOD_TOGGLE_EXTRA"] = "TOGGLE EXTRA",
                ["KINGMOD_COSTS_PREFIX"] = "Costs ",
                ["KINGMOD_TOTAL_MASTERY_REQUIRED"] = "Total Mastery Levels Required: ",
                ["KINGMOD_TOTAL_MASTERY_LEVELS"] = "Total Mastery levels",
                ["KINGMOD_KI_DRAIN"] = "Ki Drain: ",
                ["KINGMOD_PER_SECOND"] = " Per Second",
                ["KINGMOD_CHARGE_POWER"] = "Charge Power: ",
                ["KINGMOD_STAT_DAMAGE"] = "Damage: ",
                ["KINGMOD_STAT_ARMOR"] = "Armor: ",
                ["KINGMOD_STAT_HEALTH_REGEN"] = "Health Regen: ",
                ["KINGMOD_STAT_ATTACK_SPEED"] = "Attack Speed: ",
                ["KINGMOD_STAT_MOVE_SPEED"] = "Movement Speed: ",
                ["KINGMOD_STAT_MAX_HEALTH"] = "Max Health: ",
                ["KINGMOD_MASTERED"] = "Mastered ",
                ["KINGMOD_POWER_LEVEL"] = "Power Level: ",
                ["KINGMOD_NO_STAT_BOOSTS"] = " has no stat boosts...",
                ["KINGMOD_ALL_FORMS_UNLOCKED"] = "All Forms unlocked?",
                ["KINGMOD_UNTIL_NEXT_UNLOCK"] = " Mastery level(s) until next unlock",
                ["KINGMOD_COSTS_KI_FORMAT"] = "Costs <color=#03bcff>{0} Ki</color>",
                ["KINGMOD_DAMAGE_FORMAT"] = "<color=#E5C962>{0}% Damage</color>",
                ["KINGMOD_STAT_NAME_DAMAGE"] = "Damage",
                ["KINGMOD_STAT_NAME_ARMOR"] = "Armor",
                ["KINGMOD_STAT_NAME_ATTACKSPEED"] = "Attack Speed",
                ["KINGMOD_STAT_NAME_MOVESPEED"] = "Movement Speed",
                ["KINGMOD_STAT_NAME_REGEN"] = "Health Regen",
                ["KINGMOD_STAT_NAME_MAXHEALTH"] = "Max Health",
                ["KINGMOD_TUTORIAL_F1"] = "Enable/Disable Tutorials by pressing F1",
                ["KINGMOD_TUTORIAL_FORM_MASTERY"] = "With Form Mastery, gain Experience over time or by damaging enemies. Levelling up the form grants the following:\n- An increase to your transformation's stat boost.\n- A reduction to the Ki drain of your transformation\n- An increase to your overall Ki regen.\nIn Base Form, Ki regen & max Ki is gained.",
                ["KINGMOD_TUTORIAL_TRANSFORM_TRIGGER"] = "Once your mastery level is high enough, activate a Main Transformation by tapping the transformation button (Default 'C') and Power Down by pressing (Default 'X'). When more Transformations unlock, press the transformation button again to ascend further beyond!",
                ["KINGMOD_TUTORIAL_ULTIMATE_SKILL"] = "While transformed, you have access to an Ultimate Skill. Activate it by pressing the key binded below.",
                ["KINGMOD_TUTORIAL_KI_RESOURCE"] = "Ki is a resource that your skills use. Gain some back by hitting enemies with your primary skill.",
                ["KINGMOD_TUTORIAL_ZENKAI"] = "Gain Zenkai's by dying or staying below 20% health for 30 seconds. Grants permanent bonus stats.",
                ["KINGMOD_TUTORIAL_LIGHT_CHARGE"] = "Gain Light Charge by hitting enemies with your sword. Once full, activate it by holding the Transformation Button. Grants bonus Damage and Crit Chance while active.",
                ["KINGMOD_TUTORIALS_ENABLED"] = "Tutorials Enabled",
                ["KINGMOD_TUTORIALS_DISABLED"] = "Tutorials Disabled"
            }
        };

        public static Dictionary<string, string> GetDictionaryForLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return _languages["en"];
            if (_languages.TryGetValue(lang, out var dict)) return dict;

            // Handle prefixes like es-419, es-ES -> check if direct match, else default
            if (lang.StartsWith("pt", StringComparison.OrdinalIgnoreCase)) return _languages["pt-BR"];
            if (lang.StartsWith("tr", StringComparison.OrdinalIgnoreCase)) return _languages["tr"];

            return _languages["en"];
        }
    }
}
