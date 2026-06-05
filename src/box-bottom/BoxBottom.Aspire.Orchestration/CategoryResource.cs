using Aspire.Hosting.ApplicationModel;

namespace BoxBottom.Aspire.Orchestration;

/// <summary>
/// A dashboard grouping resource with no independent lifecycle.
/// </summary>
public sealed class CategoryResource(string name) : Resource(name), IResourceWithoutLifetime;
