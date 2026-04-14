namespace RoguelikeTCG.RunMap
{
    public enum NodeType
    {
        Start,
        Combat,
        Elite,
        Boss,
        Event,
        Shop,
        Forge,
        Rest,
        Mystery
    }

    public enum NodeState
    {
        Locked,     // gris — inaccessible
        Visited,    // vert foncé — déjà visité
        Available   // vert clair — accessible maintenant
    }
}
