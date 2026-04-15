using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoguelikeTCG.Data;

public static class CreateDeVinciCardsEditor
{
    [MenuItem("RoguelikeTCG/Créer deck De Vinci")]
    public static void CreateDeVinciCards()
    {
        // --- Dossier ---
        if (!AssetDatabase.IsValidFolder("Assets/Data/Cards/DeVinci"))
            AssetDatabase.CreateFolder("Assets/Data/Cards", "DeVinci");

        const string folder = "Assets/Data/Cards/DeVinci";

        // --- Cartes de base ---
        var automateBoiteux = CreateUnit(folder, "Automate boiteux",
            "Un automate moderne qui boite inexplicablement. Personne n'a osé demander pourquoi.",
            CardRarity.Common, 1, 2);

        var bouclierTroue = CreateSpell(folder, "Bouclier parapluie troué",
            "Protège un peu. Laisse passer la pluie. Et les épées. Léonard dit que c'est fait exprès.",
            CardRarity.Common, 1, SpellTarget.PlayerHero, EffectType.Heal, 4);

        var joconde = CreateUnit(folder, "Joconde",
            "Un pantin de bois au sourire énigmatique. Même Léonard ne sait pas pourquoi elle sourit.",
            CardRarity.Rare, 2, 2);

        var chienDeGarde = CreateUnit(folder, "Chien de garde",
            "Un chien-robot avec une oreille en moins. L'autre compense largement.",
            CardRarity.Rare, 1, 3);

        var arbaleteGeante = CreateUnit(folder, "Arbalète géante",
            "Sur roues. Longue à recharger. Dévastatrice quand ça marche. Ça marche rarement.",
            CardRarity.Rare, 3, 1);

        var vitruve = CreateUnit(folder, "Vitruve",
            "L'Homme de Vitruve, reconstruit en métal et pierre. Parfaitement proportionné pour vous écraser.",
            CardRarity.Rare, 1, 4);

        var ailesVolantes = CreateSpell(folder, "Ailes volantes",
            "Attacher des ailes à n'importe quoi. Résultat : ça va plus vite et ça fait beaucoup plus mal.",
            CardRarity.Rare, 2, SpellTarget.AllyUnit, EffectType.BuffAttack, 3);

        var revigorant = CreateSpell(folder, "Revigorant",
            "La recette secrète de Léonard. Goût de vinaigre, effets miraculeux. Ne demandez pas les ingrédients.",
            CardRarity.Rare, 2, SpellTarget.PlayerHero, EffectType.Heal, 8);

        var reveilExplosif = CreateSpell(folder, "Réveil explosif",
            "Le réveil de Léonard. N'a jamais fonctionné comme prévu. Très efficace autrement.",
            CardRarity.Rare, 2, SpellTarget.EnemyHero, EffectType.Damage, 5);

        var dragon = CreateUnit(folder, "Dragon",
            "Robot-dragon. Une seule aile, trois pattes. Instable. Terriblement redoutable.",
            CardRarity.Epic, 3, 3);

        var charDAssaut = CreateUnit(folder, "Char d'assaut",
            "En forme de fût de bière, en métaux modernes, couvert de tags. Léonard était très fier.",
            CardRarity.Epic, 2, 5);

        // --- Cartes upgradées ---
        var automateBoiteuxPlus = CreateUnit(folder, "Automate boiteux+",
            "Légèrement moins boiteux. Pour Léonard, c'est une victoire.",
            CardRarity.Common, 2, 2);

        var bouclierTrouePlus = CreateSpell(folder, "Bouclier parapluie troué+",
            "Le trou est plus petit. Léonard affirme que c'est un progrès significatif.",
            CardRarity.Common, 1, SpellTarget.PlayerHero, EffectType.Heal, 6);

        var jocondePlus = CreateUnit(folder, "Joconde+",
            "Renforcée. Le sourire est maintenant franchement inquiétant.",
            CardRarity.Rare, 2, 3);

        var chienDeGardePlus = CreateUnit(folder, "Chien de garde+",
            "L'oreille manquante a été retrouvée. Et fixée du mauvais côté.",
            CardRarity.Rare, 2, 3);

        var arbaleteGeantePlus = CreateUnit(folder, "Arbalète géante+",
            "Nouvelle roue avant. Vise deux fois mieux. Recharge toujours aussi longtemps.",
            CardRarity.Rare, 4, 1);

        var vitruvePlus = CreateUnit(folder, "Vitruve+",
            "Des couches supplémentaires d'acier. Encore plus de perfection géométrique.",
            CardRarity.Rare, 1, 5);

        var ailesVolantesPlus = CreateSpell(folder, "Ailes volantes+",
            "Mieux fixées cette fois. L'unité atteint des vitesses inquiétantes.",
            CardRarity.Rare, 1, SpellTarget.AllyUnit, EffectType.BuffAttack, 3);

        var revigorantPlus = CreateSpell(folder, "Revigorant+",
            "Recette améliorée. Léonard a ajouté un ingrédient secret. Il ne s'en souvient plus.",
            CardRarity.Rare, 1, SpellTarget.PlayerHero, EffectType.Heal, 8);

        var reveilExplosifPlus = CreateSpell(folder, "Réveil explosif+",
            "Encore plus explosif. Léonard lui-même a été surpris du résultat.",
            CardRarity.Rare, 2, SpellTarget.EnemyHero, EffectType.Damage, 7);

        var dragonPlus = CreateUnit(folder, "Dragon+",
            "La deuxième aile a été retrouvée. Deux ailes, trois pattes, dégâts doublés.",
            CardRarity.Epic, 4, 3);

        var charDAssautPlus = CreateUnit(folder, "Char d'assaut+",
            "Renforcé. Plus de tags. Plus d'explosifs. Plus de Léonard.",
            CardRarity.Epic, 3, 5);

        // --- Liens upgrades ---
        automateBoiteux.upgradedVersion = automateBoiteuxPlus;
        bouclierTroue.upgradedVersion   = bouclierTrouePlus;
        joconde.upgradedVersion         = jocondePlus;
        chienDeGarde.upgradedVersion    = chienDeGardePlus;
        arbaleteGeante.upgradedVersion  = arbaleteGeantePlus;
        vitruve.upgradedVersion         = vitruvePlus;
        ailesVolantes.upgradedVersion   = ailesVolantesPlus;
        revigorant.upgradedVersion      = revigorantPlus;
        reveilExplosif.upgradedVersion  = reveilExplosifPlus;
        dragon.upgradedVersion          = dragonPlus;
        charDAssaut.upgradedVersion     = charDAssautPlus;

        EditorUtility.SetDirty(automateBoiteux);
        EditorUtility.SetDirty(bouclierTroue);
        EditorUtility.SetDirty(joconde);
        EditorUtility.SetDirty(chienDeGarde);
        EditorUtility.SetDirty(arbaleteGeante);
        EditorUtility.SetDirty(vitruve);
        EditorUtility.SetDirty(ailesVolantes);
        EditorUtility.SetDirty(revigorant);
        EditorUtility.SetDirty(reveilExplosif);
        EditorUtility.SetDirty(dragon);
        EditorUtility.SetDirty(charDAssaut);

        // --- CardRegistry ---
        var registry = Resources.Load<CardRegistry>("CardRegistry");
        if (registry == null)
        {
            Debug.LogError("CardRegistry introuvable dans Assets/Resources/. Vérifie qu'il existe.");
        }
        else
        {
            registry.allCards = new List<CardData>
            {
                automateBoiteux,  automateBoiteuxPlus,
                bouclierTroue,    bouclierTrouePlus,
                joconde,          jocondePlus,
                chienDeGarde,     chienDeGardePlus,
                arbaleteGeante,   arbaleteGeantePlus,
                vitruve,          vitruvePlus,
                ailesVolantes,    ailesVolantesPlus,
                revigorant,       revigorantPlus,
                reveilExplosif,   reveilExplosifPlus,
                dragon,           dragonPlus,
                charDAssaut,      charDAssautPlus,
            };
            EditorUtility.SetDirty(registry);
            Debug.Log("CardRegistry mis à jour : 22 cartes De Vinci.");
        }

        // --- CharacterData De Vinci ---
        var deVinci = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/LeonardDeVinci.asset");
        if (deVinci == null)
        {
            Debug.LogError("LeonardDeVinci.asset introuvable dans Assets/Data/Characters/.");
        }
        else
        {
            // Deck de départ : 20 cartes
            // 3× Automate boiteux, 3× Bouclier parapluie troué
            // 2× chacune des 4 unités rares, 2× chacun des 3 sorts rares
            deVinci.startingDeck = new List<CardData>
            {
                automateBoiteux, automateBoiteux, automateBoiteux,
                bouclierTroue,   bouclierTroue,   bouclierTroue,
                joconde,         joconde,
                chienDeGarde,    chienDeGarde,
                arbaleteGeante,  arbaleteGeante,
                vitruve,         vitruve,
                ailesVolantes,   ailesVolantes,
                revigorant,      revigorant,
                reveilExplosif,  reveilExplosif,
            };
            EditorUtility.SetDirty(deVinci);
            Debug.Log("CharacterData De Vinci mis à jour : deck de 20 cartes configuré.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Deck De Vinci créé avec succès : 22 assets, registry et CharacterData à jour.");
    }

    // --- Helpers ---

    static CardData CreateUnit(string folder, string name, string description, CardRarity rarity, int atk, int hp)
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = name;
        card.description = description;
        card.cardType    = CardType.Unit;
        card.rarity      = rarity;
        card.manaCost    = 0;
        card.attackPower = atk;
        card.maxHP       = hp;

        AssetDatabase.CreateAsset(card, $"{folder}/{Sanitize(name)}.asset");
        return card;
    }

    static CardData CreateSpell(string folder, string name, string description,
        CardRarity rarity, int cost, SpellTarget target, EffectType effectType, int value)
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = name;
        card.description = description;
        card.cardType    = CardType.Spell;
        card.rarity      = rarity;
        card.manaCost    = cost;
        card.spellTarget = target;
        card.effects     = new List<CardEffect>
        {
            new CardEffect { effectType = effectType, value = value, target = target }
        };

        AssetDatabase.CreateAsset(card, $"{folder}/{Sanitize(name)}.asset");
        return card;
    }

    static string Sanitize(string name) =>
        name.Replace(" ", "").Replace("+", "Plus").Replace("'", "").Replace("'", "");
}
