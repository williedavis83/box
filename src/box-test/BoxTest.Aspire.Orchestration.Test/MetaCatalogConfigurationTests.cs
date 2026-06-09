using BoxBottom.Aspire.Orchestration;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class MetaCatalogConfigurationTests
{
    [Fact]
    public void BuildApiCatalogEnvironmentVariables_IncludesAllApisAndMeta()
    {
        var variables = MetaCatalogConfiguration.BuildApiCatalogEnvironmentVariables(
            [
                ("primary-api", "box-primary-api"),
                ("secondary-api", "box-secondary-api"),
            ],
            "box-meta")
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);

        Assert.Equal("primary-api", variables["Meta__Apis__0__LogicalName"]);
        Assert.Equal("http://box-primary-api", variables["Meta__Apis__0__Url"]);
        Assert.Equal("secondary-api", variables["Meta__Apis__1__LogicalName"]);
        Assert.Equal("http://box-secondary-api", variables["Meta__Apis__1__Url"]);
        Assert.Equal("meta", variables["Meta__Apis__2__LogicalName"]);
        Assert.Equal("http://box-meta", variables["Meta__Apis__2__Url"]);
    }
}
