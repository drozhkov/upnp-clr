# upnp-clr

### `0.0.1 - (client) discovery`
### `0.0.2 - (client) WANIPConnection:`
- GetExternalIPAddress
- AddPortMapping
- DeletePortMapping
- GetGenericPortMappingEntry

## usage
```c#
var client = new UpnpClient();
await client.AddPortMapping( 80, 8080 );
```

## NuGet
https://www.nuget.org/packages/upnp-clr-client
