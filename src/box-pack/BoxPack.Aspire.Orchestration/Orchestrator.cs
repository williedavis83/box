using BoxBottom.Aspire.Orchestration;

using BoxBottom.Auth.Aspire;

using BoxBottom.Auth.ZeroAuth;

using BoxBottom.AzureTable.Aspire;

using BoxBottom.AzureTable.Emulation;

using Bleeb.Aspire;

using Blabber.Emulator;

using BoxBottom.Users.Contract;

using Foo.Primary.Shared.AzureTable;

using Foo.Primary.Shared.Blabber;



namespace BoxPack.Aspire.Orchestration;



public static class Orchestrator

{

    private const string _boxStackName = "box";

    private const string _bobStackName = "bob";

    /// <summary>Vite app folder for the bob stack shell (relative to BoxTop.Aspire).</summary>
    public const string BobWebProjectPath = "../BoxTop.Web.Bob";

    private const string _worldMessageEnvironmentVariable = "World__Message";

    private const string _authProviderEnvironmentVariable = "Auth__Provider";



    public static IReadOnlyDictionary<string, StackResources> Orchestrate(StackOperations stackOperations)

    {

        ArgumentNullException.ThrowIfNull(stackOperations);



        var boxStack = stackOperations.CreateStackDefinition(_boxStackName);



        boxStack.AddApi(new ApiProjectOptions(

            logicalName: "primary-api",

            projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj",

            apiReferences: ["secondary-api"]));



        boxStack.AddApi(new ApiProjectOptions(

            logicalName: "secondary-api",

            projectPath: @"..\..\box-content\Foo.Secondary.Api\Foo.Secondary.Api.csproj",

            grpcOnlyAppChannel: true));



        boxStack.AddApi(new ApiProjectOptions(

            logicalName: "users-api",

            projectPath: @"..\..\box-top\BoxTop.Users.Api\BoxTop.Users.Api.csproj"));



        StackDefinition? boeStack = boxStack with { Name = EntraProofOrchestrationConfiguration.BoeStackName };
        boeStack.AsIntegrationStack();

        boxStack["users-api"].WithEntraEmulation(stackOperations);

        var bobStack = boxStack with { Name = _bobStackName };
        bobStack.AsIntegrationStack();

        // Distinct FE shell — same Aspire shape, different project path / product composition.
        bobStack.Web = bobStack.Web with { ProjectPath = BobWebProjectPath };

        bobStack["secondary-api"].EnvironmentVariables[_worldMessageEnvironmentVariable] = "Bob";

        bobStack["users-api"].EnvironmentVariables[_authProviderEnvironmentVariable] =
            ZeroAuthAuthProvider.Name;



        var bobBlabberEmulation = new BlabberEmulationConfigurationBuilder()

            .OverrideSingleton(BlabberKeys.Foo, "foo")

            .OverrideList(BlabberKeys.ListA, "foo", "bob-fee")

            .OverrideDictionary(

                BlabberKeys.DictA,

                ("foo", "foo"),

                ("bob-fee", "bob-fee"))

            .WithSeedAccounts(

                new Blabber.Emulator.Options.BlabberEmulatorAccount

                {

                    Account = "foo",

                    Bar = "bob-for",

                    Baz = "bob-foz",

                },

                new Blabber.Emulator.Options.BlabberEmulatorAccount

                {

                    Account = "bob-fee",

                    Bar = "bob-fee-bar",

                    Baz = "bob-fee-baz",

                });



        bobStack["primary-api"].WithEmulation(stackOperations, bobBlabberEmulation);

        bobStack["primary-api"].WithAzureTableEmulation(

            stackOperations,

            builder => builder

                .OverrideSingleton(AzureTableKeys.Orders)

                .OverrideSingleton(AzureTableKeys.Analytics)

                .OverrideSingleton(AzureTableKeys.GeoUsEast)

                .OverrideSingleton(AzureTableKeys.GeoEuWest)

                .OverrideDictionary(

                    AzureTableKeys.GeoReplicas,

                    (AzureTableGeoKeys.UsEast, AzureTableKeys.GeoUsEast),

                    (AzureTableGeoKeys.EuWest, AzureTableKeys.GeoEuWest)));



        bobStack["users-api"].WithAzureTableEmulation(

            stackOperations,

            builder => builder.OverrideSingleton(UserTableKeys.Users));



        var stacks = new Dictionary<string, StackResources>(StringComparer.OrdinalIgnoreCase)

        {

            [_boxStackName] = stackOperations.OrchestrateStack(boxStack),

            [_bobStackName] = stackOperations.OrchestrateStack(bobStack),

        };



        if (boeStack is not null)

        {

            stacks[EntraProofOrchestrationConfiguration.BoeStackName] =

                stackOperations.OrchestrateStack(boeStack);

        }



        var bleeb = BleebOrchestrator.OrchestrateApi(stackOperations);

        BleebOrchestrator.WireToPrimaryApis(stackOperations, bleeb, stacks);



        EmulationOrchestrationExtensions.WireOrchestratedEmulationResource(

            stackOperations,

            stacks,

            BleebEmulatorOrchestrationConfiguration.ResourceName,

            BleebOrchestrationConfiguration.PrimaryApiLogicalName,

            BleebEmulatorOrchestrationConfiguration.EmulatorBaseUriEnvironmentVariable,

            _bobStackName);



        AzuriteOrchestrator.WireToApis(

            stackOperations,

            stacks,

            [_bobStackName],

            BleebOrchestrationConfiguration.PrimaryApiLogicalName,

            "users-api");



        KeycloakOrchestrator.WireEntraEmulationToUsersApi(stackOperations, stacks, _boxStackName);

        EntraProofOrchestrator.WireRealEntraToUsersApi(stacks);



        return stacks;

    }

}


