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


    public void Initialize()
    {
        _signalBus.Subscribe<SignalOnUpdateTick>(OnUpdateTick);

        var allFactions = JsonConfigLoader.LoadAllFromFolder<FactionConfig>("Factions");
        for (int i = 0; i < allFactions.Length; i++)
        {
            Faction faction = new Faction();
            faction.id = i;
            faction.factionConfig = allFactions[i];
            faction.name = allFactions[i].name;
            Debug.Log(faction.name);
            _factions.Add(faction);
        }
    }
    public Faction GetFaction(string name)
    {
        Faction ret = null;
        ret = _factions.Find(x=>x.name == name);
        return ret;
    }
    public void OnUpdateTick()
    {

    }
}