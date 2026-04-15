using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoguelikeTCG.Data;

public static class CreateMarieCesarCardsEditor
{
    [MenuItem("RoguelikeTCG/Créer decks Marie Curie + Jules César")]
    public static void CreateCards()
    {
        CreateMarieCurieCards();
        CreateJulesCesarCards();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Decks Marie Curie + Jules César créés avec succès.");
    }

    // =========================================================
    // MARIE CURIE
    // =========================================================
    static void CreateMarieCurieCards()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data/Cards/MarieCurie"))
            AssetDatabase.CreateFolder("Assets/Data/Cards", "MarieCurie");

        const string folder = "Assets/Data/Cards/MarieCurie";

        // --- Cartes de base ---
        var unitCommune = CreateUnit(folder, "MC_UnitCommune",
            "Une unité de base. Suffisante pour tenir le front pendant que la chimie opère.",
            CardRarity.Common, 1, 2);

        // Sort commun : -2 ATK unité ennemie (BuffAttack valeur négative sur EnemyUnit)
        var sortCommun = CreateSpell(folder, "MC_SortCommun",
            "Une substance corrosive qui ronge les muscles. Efficace. Légèrement illégale.",
            CardRarity.Common, 1, SpellTarget.EnemyUnit, EffectType.BuffAttack, -2);

        var unitRare1 = CreateUnit(folder, "MC_UnitRare1",
            "Solide. Pas brillante. Exactement ce qu'il faut pour tenir pendant qu'on affaiblit.",
            CardRarity.Rare, 2, 2);

        var unitRare2 = CreateUnit(folder, "MC_UnitRare2",
            "Endurante. Elle encaisse. C'est son seul talent mais il est remarquable.",
            CardRarity.Rare, 1, 3);

        var unitRare3 = CreateUnit(folder, "MC_UnitRare3",
            "Rapide et agressive. Une exception dans un deck qui préfère la patience.",
            CardRarity.Rare, 3, 2);

        var unitRare4 = CreateUnit(folder, "MC_UnitRare4",
            "Équilibrée. Attaque et résiste. Le compromis parfait.",
            CardRarity.Rare, 2, 3);

        // Sort rare 1 : -3 ATK unité ennemie
        var sortRare1 = CreateSpell(folder, "MC_SortRare1",
            "Dose concentrée. L'ennemi ciblé ne frappera plus très fort. Ni très longtemps.",
            CardRarity.Rare, 2, SpellTarget.EnemyUnit, EffectType.BuffAttack, -3);

        // Sort rare 2 : Heal 4 héros allié
        var sortRare2 = CreateSpell(folder, "MC_SortRare2",
            "Un antidote. Pour les autres. Marie n'en a pas besoin.",
            CardRarity.Rare, 1, SpellTarget.PlayerHero, EffectType.Heal, 4);

        // Sort rare 3 : 3 dmg unité ennemie
        var sortRare3 = CreateSpell(folder, "MC_SortRare3",
            "Quand affaiblir ne suffit pas, on dissout. Directement.",
            CardRarity.Rare, 2, SpellTarget.EnemyUnit, EffectType.Damage, 3);

        var unitEpique1 = CreateUnit(folder, "MC_UnitEpique1",
            "Un colosse patient. Attend que les ennemis soient neutralisés pour frapper.",
            CardRarity.Epic, 3, 4);

        var unitEpique2 = CreateUnit(folder, "MC_UnitEpique2",
            "Explosive. Elle n'attend pas, elle. Heureusement qu'il y a des sorts pour compenser.",
            CardRarity.Epic, 4, 3);

        // --- Cartes upgradées ---
        var unitCommunePlus = CreateUnit(folder, "MC_UnitCommunePlus",
            "Améliorée. Elle tient le front encore mieux. Grâce à la chimie, forcément.",
            CardRarity.Common, 2, 2);

        var sortCommunPlus = CreateSpell(folder, "MC_SortCommunPlus",
            "Formule renforcée. L'ennemi ne saura même plus pourquoi il ne frappe plus.",
            CardRarity.Common, 1, SpellTarget.EnemyUnit, EffectType.BuffAttack, -3);

        var unitRare1Plus = CreateUnit(folder, "MC_UnitRare1Plus",
            "Encore plus solide. Encore moins brillante. Parfait.",
            CardRarity.Rare, 3, 2);

        var unitRare2Plus = CreateUnit(folder, "MC_UnitRare2Plus",
            "Endurance maximale. Elle ne mourra probablement pas avant la fin du combat.",
            CardRarity.Rare, 1, 4);

        var unitRare3Plus = CreateUnit(folder, "MC_UnitRare3Plus",
            "Plus rapide, plus tranchante. Marie l'a personnellement calibrée.",
            CardRarity.Rare, 4, 2);

        var unitRare4Plus = CreateUnit(folder, "MC_UnitRare4Plus",
            "Le compromis parfait, en mieux. C'est possible.",
            CardRarity.Rare, 3, 3);

        var sortRare1Plus = CreateSpell(folder, "MC_SortRare1Plus",
            "Dose létale. L'ennemi ciblé ne frappera plus du tout.",
            CardRarity.Rare, 2, SpellTarget.EnemyUnit, EffectType.BuffAttack, -4);

        var sortRare2Plus = CreateSpell(folder, "MC_SortRare2Plus",
            "Antidote purifié. Beaucoup plus efficace. Marie a enfin trouvé la bonne formule.",
            CardRarity.Rare, 1, SpellTarget.PlayerHero, EffectType.Heal, 6);

        var sortRare3Plus = CreateSpell(folder, "MC_SortRare3Plus",
            "Dissolution accélérée. Résultat immédiat, spectaculaire.",
            CardRarity.Rare, 2, SpellTarget.EnemyUnit, EffectType.Damage, 5);

        var unitEpique1Plus = CreateUnit(folder, "MC_UnitEpique1Plus",
            "Un colosse encore plus patient. Et encore plus dévastateur quand il se décide.",
            CardRarity.Epic, 4, 4);

        var unitEpique2Plus = CreateUnit(folder, "MC_UnitEpique2Plus",
            "Améliorée. Elle explose mieux.",
            CardRarity.Epic, 5, 3);

        // --- Liens upgrades ---
        unitCommune.upgradedVersion  = unitCommunePlus;
        sortCommun.upgradedVersion   = sortCommunPlus;
        unitRare1.upgradedVersion    = unitRare1Plus;
        unitRare2.upgradedVersion    = unitRare2Plus;
        unitRare3.upgradedVersion    = unitRare3Plus;
        unitRare4.upgradedVersion    = unitRare4Plus;
        sortRare1.upgradedVersion    = sortRare1Plus;
        sortRare2.upgradedVersion    = sortRare2Plus;
        sortRare3.upgradedVersion    = sortRare3Plus;
        unitEpique1.upgradedVersion  = unitEpique1Plus;
        unitEpique2.upgradedVersion  = unitEpique2Plus;

        foreach (var c in new[] { unitCommune, sortCommun, unitRare1, unitRare2, unitRare3, unitRare4,
                                   sortRare1, sortRare2, sortRare3, unitEpique1, unitEpique2 })
            EditorUtility.SetDirty(c);

        // --- CardRegistry ---
        UpdateRegistry(new List<CardData>
        {
            unitCommune,  unitCommunePlus,
            sortCommun,   sortCommunPlus,
            unitRare1,    unitRare1Plus,
            unitRare2,    unitRare2Plus,
            unitRare3,    unitRare3Plus,
            unitRare4,    unitRare4Plus,
            sortRare1,    sortRare1Plus,
            sortRare2,    sortRare2Plus,
            sortRare3,    sortRare3Plus,
            unitEpique1,  unitEpique1Plus,
            unitEpique2,  unitEpique2Plus,
        }, append: true);

        // --- CharacterData Marie Curie ---
        if (!AssetDatabase.IsValidFolder("Assets/Data/Characters"))
            AssetDatabase.CreateFolder("Assets/Data", "Characters");

        const string charPath = "Assets/Data/Characters/MarieCurie.asset";
        var marie = AssetDatabase.LoadAssetAtPath<CharacterData>(charPath);
        if (marie == null)
        {
            marie = ScriptableObject.CreateInstance<CharacterData>();
            AssetDatabase.CreateAsset(marie, charPath);
        }
        marie.characterName = "Marie Curie";
        marie.description   = "Baronne de la drogue. Son deck neutralise les ennemis avant qu'ils puissent frapper.";
        marie.maxHP         = 80;
        marie.startingDeck  = new List<CardData>
        {
            unitCommune,  unitCommune,  unitCommune,
            sortCommun,   sortCommun,   sortCommun,
            unitRare1,    unitRare1,
            unitRare2,    unitRare2,
            unitRare3,    unitRare3,
            unitRare4,    unitRare4,
            sortRare1,    sortRare1,
            sortRare2,    sortRare2,
            sortRare3,    sortRare3,
        };
        EditorUtility.SetDirty(marie);
        Debug.Log("CharacterData Marie Curie créé/mis à jour.");
    }

    // =========================================================
    // JULES CÉSAR
    // =========================================================
    static void CreateJulesCesarCards()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data/Cards/JulesCesar"))
            AssetDatabase.CreateFolder("Assets/Data/Cards", "JulesCesar");

        const string folder = "Assets/Data/Cards/JulesCesar";

        // --- Cartes de base ---
        var unitCommune = CreateUnit(folder, "JC_UnitCommune",
            "Un légionnaire. Veni. Vidi. Tapé fort.",
            CardRarity.Common, 2, 2);

        // Sort commun : 4 dmg héros ennemi
        var sortCommun = CreateSpell(folder, "JC_SortCommun",
            "Veni Vidi Vendu. Le contrat est signé. Vous perdez des HP.",
            CardRarity.Common, 1, SpellTarget.EnemyHero, EffectType.Damage, 4);

        var unitRare1 = CreateUnit(folder, "JC_UnitRare1",
            "Rapide et brutal. Un parfait agent de César.",
            CardRarity.Rare, 3, 2);

        var unitRare2 = CreateUnit(folder, "JC_UnitRare2",
            "Fragile mais dévastateur. César aime les risques calculés.",
            CardRarity.Rare, 4, 1);

        var unitRare3 = CreateUnit(folder, "JC_UnitRare3",
            "Défensif mais costaud. Pour tenir les lignes pendant que les autres s'occupent du reste.",
            CardRarity.Rare, 2, 4);

        var unitRare4 = CreateUnit(folder, "JC_UnitRare4",
            "Équilibré, puissant. Le préféré de César. Il ne le dira jamais.",
            CardRarity.Rare, 3, 3);

        // Sort rare 1 : 6 dmg héros ennemi
        var sortRare1 = CreateSpell(folder, "JC_SortRare1",
            "Clause pénale. Vous n'avez pas lu le contrat. C'est votre problème.",
            CardRarity.Rare, 2, SpellTarget.EnemyHero, EffectType.Damage, 6);

        // Sort rare 2 : 3 dmg toutes unités ennemies (AllEnemyUnits)
        var sortRare2 = CreateSpell(folder, "JC_SortRare2",
            "Offre de rachat groupée. Tout le monde est concerné. Personne ne survivra.",
            CardRarity.Rare, 2, SpellTarget.AllEnemyUnits, EffectType.Damage, 3);

        // Sort rare 3 : +3 ATK unité alliée
        var sortRare3 = CreateSpell(folder, "JC_SortRare3",
            "Promotion express. Votre unité vient de devenir directrice commerciale. Et elle frappe plus fort.",
            CardRarity.Rare, 1, SpellTarget.AllyUnit, EffectType.BuffAttack, 3);

        var unitEpique1 = CreateUnit(folder, "JC_UnitEpique1",
            "Un général. Vieux, sage, impitoyable. César le loue très cher.",
            CardRarity.Epic, 4, 4);

        var unitEpique2 = CreateUnit(folder, "JC_UnitEpique2",
            "Une machine de guerre. Instable. César a refusé de payer l'assurance.",
            CardRarity.Epic, 5, 3);

        // --- Cartes upgradées ---
        var unitCommunePlus = CreateUnit(folder, "JC_UnitCommunePlus",
            "Légionnaire senior. Il a un badge. Ça ne change rien, il frappe plus fort.",
            CardRarity.Common, 3, 2);

        var sortCommunPlus = CreateSpell(folder, "JC_SortCommunPlus",
            "Avenant au contrat. Les dégâts ont augmenté. Vous n'avez toujours pas lu les CGU.",
            CardRarity.Common, 1, SpellTarget.EnemyHero, EffectType.Damage, 6);

        var unitRare1Plus = CreateUnit(folder, "JC_UnitRare1Plus",
            "Encore plus rapide. Encore plus brutal. César a mis une prime.",
            CardRarity.Rare, 4, 2);

        var unitRare2Plus = CreateUnit(folder, "JC_UnitRare2Plus",
            "Un peu moins fragile. Toujours aussi dévastateur. Progrès.",
            CardRarity.Rare, 5, 1);

        var unitRare3Plus = CreateUnit(folder, "JC_UnitRare3Plus",
            "Blindage renforcé. César a facturé les travaux au contribuable.",
            CardRarity.Rare, 2, 5);

        var unitRare4Plus = CreateUnit(folder, "JC_UnitRare4Plus",
            "Tout est amélioré. César n'est toujours pas satisfait.",
            CardRarity.Rare, 4, 3);

        var sortRare1Plus = CreateSpell(folder, "JC_SortRare1Plus",
            "Pénalité maximale. Vous auriez dû lire le contrat.",
            CardRarity.Rare, 2, SpellTarget.EnemyHero, EffectType.Damage, 8);

        var sortRare2Plus = CreateSpell(folder, "JC_SortRare2Plus",
            "Offre de rachat premium. Les dégâts sont maintenant incluant la TVA.",
            CardRarity.Rare, 2, SpellTarget.AllEnemyUnits, EffectType.Damage, 4);

        var sortRare3Plus = CreateSpell(folder, "JC_SortRare3Plus",
            "Prime exceptionnelle. Votre unité a été promue PDG. Elle frappe encore plus fort.",
            CardRarity.Rare, 1, SpellTarget.AllyUnit, EffectType.BuffAttack, 4);

        var unitEpique1Plus = CreateUnit(folder, "JC_UnitEpique1Plus",
            "Général retraité rappelé. Il était en vacances. Il est furieux.",
            CardRarity.Epic, 5, 4);

        var unitEpique2Plus = CreateUnit(folder, "JC_UnitEpique2Plus",
            "Machine de guerre stabilisée. César a finalement payé l'assurance.",
            CardRarity.Epic, 6, 3);

        // --- Liens upgrades ---
        unitCommune.upgradedVersion  = unitCommunePlus;
        sortCommun.upgradedVersion   = sortCommunPlus;
        unitRare1.upgradedVersion    = unitRare1Plus;
        unitRare2.upgradedVersion    = unitRare2Plus;
        unitRare3.upgradedVersion    = unitRare3Plus;
        unitRare4.upgradedVersion    = unitRare4Plus;
        sortRare1.upgradedVersion    = sortRare1Plus;
        sortRare2.upgradedVersion    = sortRare2Plus;
        sortRare3.upgradedVersion    = sortRare3Plus;
        unitEpique1.upgradedVersion  = unitEpique1Plus;
        unitEpique2.upgradedVersion  = unitEpique2Plus;

        foreach (var c in new[] { unitCommune, sortCommun, unitRare1, unitRare2, unitRare3, unitRare4,
                                   sortRare1, sortRare2, sortRare3, unitEpique1, unitEpique2 })
            EditorUtility.SetDirty(c);

        // --- CardRegistry ---
        UpdateRegistry(new List<CardData>
        {
            unitCommune,  unitCommunePlus,
            sortCommun,   sortCommunPlus,
            unitRare1,    unitRare1Plus,
            unitRare2,    unitRare2Plus,
            unitRare3,    unitRare3Plus,
            unitRare4,    unitRare4Plus,
            sortRare1,    sortRare1Plus,
            sortRare2,    sortRare2Plus,
            sortRare3,    sortRare3Plus,
            unitEpique1,  unitEpique1Plus,
            unitEpique2,  unitEpique2Plus,
        }, append: true);

        // --- CharacterData Jules César ---
        const string charPath = "Assets/Data/Characters/JulesCesar.asset";
        var cesar = AssetDatabase.LoadAssetAtPath<CharacterData>(charPath);
        if (cesar == null)
        {
            cesar = ScriptableObject.CreateInstance<CharacterData>();
            AssetDatabase.CreateAsset(cesar, charPath);
        }
        cesar.characterName = "Jules César";
        cesar.description   = "Agent immobilier. Veni Vidi Vendu. Son deck vous écrase par la puissance brute.";
        cesar.maxHP         = 100;
        cesar.startingDeck  = new List<CardData>
        {
            unitCommune,  unitCommune,  unitCommune,
            sortCommun,   sortCommun,   sortCommun,
            unitRare1,    unitRare1,
            unitRare2,    unitRare2,
            unitRare3,    unitRare3,
            unitRare4,    unitRare4,
            sortRare1,    sortRare1,
            sortRare2,    sortRare2,
            sortRare3,    sortRare3,
        };
        EditorUtility.SetDirty(cesar);
        Debug.Log("CharacterData Jules César créé/mis à jour.");
    }

    // =========================================================
    // HELPERS
    // =========================================================

    static void UpdateRegistry(List<CardData> newCards, bool append)
    {
        var registry = Resources.Load<CardRegistry>("CardRegistry");
        if (registry == null) { Debug.LogError("CardRegistry introuvable dans Assets/Resources/."); return; }

        if (append)
        {
            if (registry.allCards == null) registry.allCards = new List<CardData>();
            registry.allCards.AddRange(newCards);
        }
        else
        {
            registry.allCards = newCards;
        }
        EditorUtility.SetDirty(registry);
        Debug.Log($"CardRegistry mis à jour : {registry.allCards.Count} cartes au total.");
    }

    static CardData CreateUnit(string folder, string id, string description, CardRarity rarity, int atk, int hp)
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = id;
        card.description = description;
        card.cardType    = CardType.Unit;
        card.rarity      = rarity;
        card.manaCost    = 0;
        card.attackPower = atk;
        card.maxHP       = hp;
        AssetDatabase.CreateAsset(card, $"{folder}/{id}.asset");
        return card;
    }

    static CardData CreateSpell(string folder, string id, string description,
        CardRarity rarity, int cost, SpellTarget target, EffectType effectType, int value)
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName    = id;
        card.description = description;
        card.cardType    = CardType.Spell;
        card.rarity      = rarity;
        card.manaCost    = cost;
        card.spellTarget = target;
        card.effects     = new List<CardEffect>
        {
            new CardEffect { effectType = effectType, value = value, target = target }
        };
        AssetDatabase.CreateAsset(card, $"{folder}/{id}.asset");
        return card;
    }
}
