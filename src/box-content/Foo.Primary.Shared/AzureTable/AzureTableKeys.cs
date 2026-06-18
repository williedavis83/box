namespace Foo.Primary.Shared.AzureTable;

public static class AzureTableKeys
{
    public const string Orders = "table-orders";
    public const string Analytics = "table-analytics";
    public const string GeoReplicas = "table-geo-replicas";
    public const string GeoUsEast = "table-geo-us-east";
    public const string GeoEuWest = "table-geo-eu-west";
}

public static class AzureTableGeoKeys
{
    public const string UsEast = "us-east";
    public const string EuWest = "eu-west";
}
