namespace Teniry.Cqrs.Extended.Queryables.Sort;

public struct SortKey {
    public string Property { get; set; }
    public SortDirection Direction { get; set; }

    public SortKey(string property, SortDirection direction) {
        Property = property;
        Direction = direction;
    }

    public SortKey(string property, string direction) {
        Property = property;
        Direction = direction.Equals("asc") ? SortDirection.Asc : SortDirection.Desc;
    }

    public static bool TryParse(string key, out SortKey property) {
        try {
            var sortDirectionAndField = key.Split(SortKeyConfig.SortKeySplitSign);

            if (sortDirectionAndField.Length != 2) {
                property = new("", "");

                return false;
            }

            property = new(sortDirectionAndField[1], sortDirectionAndField[0]);

            return true;
        } catch (Exception) {
            property = new("", "");

            return false;
        }
    }
}

public static class SortKeyConfig {
    public static char SortKeySplitSign = '.';
}