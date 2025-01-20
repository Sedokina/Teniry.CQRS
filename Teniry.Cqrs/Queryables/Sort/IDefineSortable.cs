namespace Teniry.Cqrs.Queryables.Sort;

public interface IDefineSortable : ISorted {
    protected string[] GetSortKeys();

    public string[] GetSortKeysWithDirection() {
        var keys = GetSortKeys();
        var orders = new[] {
            SortDirection.Asc.ToString().ToLower(),
            SortDirection.Desc.ToString().ToLower()
        };

        return keys.SelectMany(_ => orders, (key, order) => $"{order}{SortKeyConfig.SortKeySplitSign}{key}").ToArray();
    }
}