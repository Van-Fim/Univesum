using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

[Serializable]
public class Faction
{
    public int id;
    public string name;
    public FactionConfig factionConfig;
    public float GetRelation(string name)
    {
        if (factionConfig.relationships == null) return 0;
        var relation = factionConfig.relationships.FirstOrDefault(r => r.faction == name);
        return relation != null ? relation.relation : 0;
    }
}
public class FactionsManager : IInitializable
{
    [Inject] private Universe _universe;
    [Inject] private DiContainer _container;
    [Inject] private SpaceObjectFactory _shipFactory;
    [Inject] private SignalBus _signalBus;

    private List<Faction> _factions = new List<Faction>();
    public static FactionsManager singleton;

    public List<Faction> Factions { get => _factions; set => _factions = value; }

    public void Initialize()
    {
        singleton = this;
        _signalBus.Subscribe<SignalOnUpdateTick>(OnUpdateTick);

        var allFactions = JsonConfigLoader.LoadAllFromFolder<FactionConfig>("Factions");
        for (int i = 0; i < allFactions.Length; i++)
        {
            Faction faction = new Faction();
            faction.id = i;
            faction.factionConfig = allFactions[i];
            faction.name = allFactions[i].name;
            Factions.Add(faction);
        }
        for (int i = 0; i < Factions.Count; i++)
        {
            Faction f = Factions[i];
            f.factionConfig.relationships = InstallRelationship(f);
        }
        for (int i = 0; i < Factions.Count; i++)
        {
            Faction f = Factions[i];
            FixRelationship(f);
        }
    }
    public Faction GetFaction(string name)
    {
        Faction ret = null;
        ret = Factions.Find(x => x.name == name);
        return ret;
    }
    public List<FactionRelationshipConfig> InstallRelationship(Faction faction)
    {

        List<FactionRelationshipConfig> relationships = faction.factionConfig.relationships;
        List<FactionRelationshipConfig> ret = new List<FactionRelationshipConfig>();
        for (int i = 0; i < Factions.Count; i++)
        {
            Faction f = Factions[i];
            FactionRelationshipConfig rel = relationships.Find(x => x.faction == f.name);
            int relValue = 0;
            if (rel != null)
            {
                relValue = rel.relation;
            }
            ret.Add(new FactionRelationshipConfig { faction = f.name, relation = relValue });
        }
        return ret;
    }
    public List<FactionRelationshipConfig> FixRelationship(Faction faction)
    {

        List<FactionRelationshipConfig> relationships = faction.factionConfig.relationships;
        for (int i = 0; i < Factions.Count; i++)
        {
            Faction f = Factions[i];
            FactionRelationshipConfig rel1 = relationships[f.id];
            FactionRelationshipConfig rel2 = f.factionConfig.relationships[faction.id];
            int relValue1 = rel1.relation;
            int relValue2 = rel2.relation;
            if (relValue1 != 0 && relValue2 == 0)
            {
                relValue2 = relValue1;
            }
            if (relValue1 == 0 && relValue2 != 0)
            {
                relValue1 = relValue2;
            }
            rel1.relation = relValue1;
            rel2.relation = relValue2;
            relationships[f.id] = rel1;
            f.factionConfig.relationships[faction.id] = rel2;
        }
        return relationships;
    }
    public void OnUpdateTick()
    {

    }
}
