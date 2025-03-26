using System.Collections.Generic;

public class BundleLookUpTable
{
    public Dictionary<string, string> asset2Bundle = new Dictionary<string, string>();
    public Dictionary<string, string> asset2FullName = new Dictionary<string, string>();
    public Dictionary<string, string> bundleName2Pack = new Dictionary<string, string>();
    public Dictionary<string, ulong> bundleName2Offset = new Dictionary<string, ulong>();
    public string packName;

    public void MergeTable(BundleLookUpTable table)
    {
        foreach (string asset in table.asset2Bundle.Keys)
        {
            asset2Bundle[asset] = table.asset2Bundle[asset];
        }
        foreach (string asset in table.asset2FullName.Keys)
        {
            asset2FullName[asset] = table.asset2FullName[asset];
        }
        foreach (string bundleName in table.bundleName2Pack.Keys)
        {
            bundleName2Pack[bundleName] = table.bundleName2Pack[bundleName];
        }
        foreach (string bundleName in table.bundleName2Offset.Keys)
        {
            bundleName2Offset[bundleName] = table.bundleName2Offset[bundleName];
        }
    }
}
