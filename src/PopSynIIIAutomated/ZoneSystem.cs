using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PopSynIIIAutomated;

/// <summary>
/// Represents the zone system
/// </summary>
internal sealed class ZoneSystem
{
    private readonly Zone[] _zones;

    private readonly Dictionary<int, int> _tazToFlatZone;

    private readonly Dictionary<int, int> _flatZoneToFlatPD;

    private readonly Dictionary<int, List<int>> _pdIndexToZones;

    /// <summary>
    /// Construct a new zone system from the given file.
    /// </summary>
    /// <param name="fileName">The path to the file to load.</param>
    public ZoneSystem(string fileName)
    {
        // Load Zones
        // TAZ,PD,Region,Ensemble
        _zones = File.ReadLines(fileName)
            .Skip(1)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= Zone.MinimumEntries)
            .Select(parts => Zone.LoadFromParts(parts))
            .OrderBy(zone => zone.TAZ)
            .ToArray();
        // Create the taz lookup
        _tazToFlatZone = _zones
            .Select((zone, i) => (zone.TAZ, Index: i))
            .ToDictionary(entry => entry.TAZ, entry => entry.Index);
        // Get out all of the PDs and map them
        var pdToIndex = _zones
            .Select(zone => zone.PD)
            .Distinct()
            .OrderBy(pd => pd)
            .Select((pd, i) => (PD: pd, Index: i))
            .ToDictionary(entry => entry.PD, entry => entry.Index);
        NumberOfPDs = pdToIndex.Count;
        // Create the flat zone to flat PD
        _flatZoneToFlatPD = _zones
            .Select((zone, i) => (TAZIndex: i, PDIndex: pdToIndex[zone.PD]))
            .ToDictionary(entry => entry.TAZIndex, entry => entry.PDIndex);
        _pdIndexToZones = _flatZoneToFlatPD
            .GroupBy(entry => entry.Value)
            .ToDictionary(group => group.Key, entry => entry.Select(e => e.Value).ToList());
    }

    /// <summary>
    /// Gets the pd index that contains the zone given the zone's index.
    /// </summary>
    /// <param name="zoneIndex">The zone index to lookup.</param>
    /// <returns>The pdindex for the given zone, -1 if the zone does not exist.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ZoneIndexToPDIndex(int zoneIndex) => _flatZoneToFlatPD.TryGetValue(zoneIndex, out var result) ? result : -1;

    /// <summary>
    /// Gets the zone index for a given zone number.
    /// </summary>
    /// <param name="zoneNumber">The zone number to lookup.</param>
    /// <returns>The zone index for the given zone, -1 if the zone does not exist.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ZoneNumberToZoneIndex(int zoneNumber) => _tazToFlatZone.TryGetValue(zoneNumber, out var result) ? result : -1;

    /// <summary>
    /// Gets the zone given a zone index
    /// </summary>
    /// <param name="zoneIndex">The zone index to lookup.</param>
    /// <returns>The zone at the given index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Zone GetZone(int zoneIndex) => _zones[zoneIndex];

    /// <summary>
    /// Gets the zone indexes that belong to this the given pd
    /// </summary>
    /// <param name="flatPD">The pd index to lookup.</param>
    /// <returns>A list of zone indexes contained in this pd.</returns>
    public List<int>? GetZonesInFlatPD(int flatPD) => _pdIndexToZones.TryGetValue(flatPD, out var result) ? result : new();

    /// <summary>
    /// Gets the indexes of all planning districts.
    /// </summary>
    public List<int> FlatPDs => _pdIndexToZones.Keys.ToList();

    /// <summary>
    /// The number of zones in the zone system.
    /// </summary>
    public int NumberOfZones => _zones.Length;

    /// <summary>
    /// The number of planning districts in the zone system.
    /// </summary>
    public int NumberOfPDs { get; private init; }
}

/// <summary>
/// Represents a TAZ
/// </summary>
/// <param name="TAZ">The sparse zone number</param>
/// <param name="PD">The planning district this zone belongs to</param>
/// <param name="PUMA">The logical PUMF index that this zone belongs to.</param>
/// <param name="BaseYearPopulation">The base year's population for the zone.</param>
internal record struct Zone(int TAZ, int PD, int PUMA, float BaseYearPopulation)
{
    internal const int MinimumEntries = 4;

    /// <summary>
    /// Load a zone record from a partitioned string..
    /// </summary>
    /// <param name="parts">The columns of a row to load.</param>
    /// <returns>A zone loaded in from the given string parts.</returns>
    internal static Zone LoadFromParts(string[] parts) => new
        (
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                float.Parse(parts[3])
        );
}
