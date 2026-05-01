#:sdk Aspire.AppHost.Sdk@13.2.2
#:package CommunityToolkit.Aspire.Hosting.Dapr@*
#:package Aspire.Hosting.Python@*

using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var launchProfileName = ShouldUseHttpForEndpoints() ? "http" : "https";

var stateStore = builder.AddDaprStateStore("statestore");
var pubSub = builder.AddDaprPubSub("pubsub");
var secretStore = builder.AddDaprComponent("secretstore", "secretstores.local.file", new DaprComponentOptions
{
    LocalPath = "..\\dapr\\components\\secretstore.yaml"
});


// Redis is used for pubsub and state store by default. So the redis needs to be running at the port 6379.
// Uncomment the below code to add redis as a container if it is not running
// builder.AddContainer("redis", "redis").WithEndpoint(port: 6379, targetPort: 6379);

#pragma warning disable ASPIRECSHARPAPPS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var serviceA = //builder.AddProject<Projects.AspireWithDapr_ServiceA>("service-a", launchProfileName)
    builder.AddCSharpApp("service-a", "../AspireWithDapr.ServiceA", options => 
    { 
        options.LaunchProfileName = launchProfileName;
    })
    .WithDaprSidecar(options =>
    {
        options.WithAnnotation(new EnvironmentCallbackAnnotation("APP_API_TOKEN", () => "secret-dapr-api-token"), ResourceAnnotationMutationBehavior.Append);
        options.WithReference(stateStore);
        options.WithReference(pubSub);
        options.WithReference(secretStore);
    })
    .WithEnvironment("APP_API_TOKEN", "secret-dapr-api-token");
#pragma warning restore ASPIRECSHARPAPPS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


#pragma warning disable ASPIRECSHARPAPPS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var serviceB = //builder.AddProject<Projects.AspireWithDapr_ServiceB>("service-b", launchProfileName)
    builder.AddCSharpApp("service-b", "../AspireWithDapr.ServiceB", options => 
    { 
        options.LaunchProfileName = launchProfileName;
    })
    .WithDaprSidecar(options =>
    {
        options.WithAnnotation(new EnvironmentCallbackAnnotation("APP_API_TOKEN", () => "secret-dapr-api-token"), ResourceAnnotationMutationBehavior.Append);
        options.WithReference(stateStore);
        options.WithReference(pubSub);
        options.WithReference(secretStore);
    })
    .WithEnvironment("APP_API_TOKEN", "secret-dapr-api-token");
#pragma warning restore ASPIRECSHARPAPPS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


#pragma warning disable ASPIRECSHARPAPPS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder //.AddProject<Projects.AspireWithDapr_WebUI>("webfrontend")
       .AddCSharpApp("webfrontend", "../AspireWithDapr.WebUI")
       .WithDaprSidecar(options =>
            options.WithAnnotation(new EnvironmentCallbackAnnotation("APP_API_TOKEN", () => "secret-dapr-api-token"), ResourceAnnotationMutationBehavior.Append))
       .WaitFor(serviceA)
       .WaitFor(serviceB)
       .WithEnvironment("APP_API_TOKEN", "secret-dapr-api-token");
#pragma warning restore ASPIRECSHARPAPPS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// var python = builder.AddPythonApp(
//     name: "python-app1",
//     appDirectory: "../python/python-app1",
//     scriptPath: "main.py")
//     .WithUv();
    //.WithHttpEndpoint(port: 8000, env: "PORT");

var python = builder.AddPythonApp(
    name: "python-app1",
    appDirectory: "../python/python-app1",
    scriptPath: "main.py")
    .WithUv();

var python2 = builder.AddUvicornApp(
    name: "python-app2",
    appDirectory: "../python/python-app2",
    app: "main:app")
    //.WithHttpEndpoint(port: 8000, env: "PORT")
    .WithUv();

// builder.AddProject("python", "../python/python-app1")
//        .WithReference(python);

builder.Build().Run();

static bool ShouldUseHttpForEndpoints()
{
    const string EnvVarName = "USE_HTTP_ENDPOINTS";
    var envValue = Environment.GetEnvironmentVariable(EnvVarName);

    // Attempt to parse the environment variable value; return true if it's exactly "1".
    return int.TryParse(envValue, out int result) && result == 1;
}

