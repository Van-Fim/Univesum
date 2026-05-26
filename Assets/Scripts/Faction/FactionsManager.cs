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
    }
    public Faction GetFaction(string name)
    {
        Faction ret = null;
        ret = Factions.Find(x=>x.name == name);
        return ret;
    }
    public void OnUpdateTick()
    {

    }
}